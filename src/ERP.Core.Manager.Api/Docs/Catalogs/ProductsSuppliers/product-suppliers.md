# Catálogo Proveedores y Productos (Compras)

Documentación de endpoints para el catálogo N:N proveedor–producto, precios unitarios, precios preferenciales (tiers), historial de precios y exclusividad de proveedores.

## Información general

| Campo | Valor |
|---|---|
| **Base** | `/api/v1/companies/{companie_id}/modules/{module_code}` |
| **Módulo típico** | `PURCHASING` |
| **Auth** | `Authorization: Bearer {token}` (`[HasToken]`) |
| **JSON** | `snake_case` + enums como string |

---

## Modelo de dominio (resumen)

```
Product  ←→  SupplierProduct  ←→  Supplier
                  │
                  ├── UnitPrice + LastPriceUpdate (vigente)
                  ├── TierPrices (preferenciales por volumen)
                  └── HistoryPrices (auditoría unitario / preferencial)
```

- El **código de producto** se genera automáticamente: `{Company.Code}-{Category.Code}-{NNN}` (ej. `ALP-01-001`).
- Si el producto **cambia de categoría**, se regenera el código.
- Al desactivar un vínculo proveedor–producto se cierra el historial activo.

### Enums relevantes

| Enum | Valores |
|---|---|
| `SupplierExclusiveStatus` | `None`, `PendingReview`, `Approved`, `Rejected` |
| `ProductUsageType` | `Insumo`, `OperationalUse` |
| `SupplierPriceHistoryType` | `UnitPrice`, `PreferentialPrice` |

---

## Endpoints – Productos

### 1. Listar productos

| Campo | Valor |
|---|---|
| **Método** | `GET` |
| **Ruta** | `/products` |

**Query params**

| Parámetro | Tipo | Requerido | Descripción |
|---|:---:|:---:|---|
| `category_product_id` | `guid` | No | Filtra por categoría |
| `page_number` | `int` | No | Default `1` |
| `page_size` | `int` | No | Default `10` |

**Respuesta (resumen):** lista paginada con `product_id`, `code`, `product_name`, `category`, `unit_measure_id`, `suppliers_count`.

---

### 2. Detalle de producto

| Campo | Valor |
|---|---|
| **Método** | `GET` |
| **Ruta** | `/products/{product_id}` |

Incluye proveedores activos vinculados, precio unitario vigente y `tier_prices`.

---

### 3. Registrar producto

| Campo | Valor |
|---|---|
| **Método** | `POST` |
| **Ruta** | `/products` |

```json
{
  "product_name": "Lapicero azul",
  "description": "Bolígrafo punta fina",
  "category_id": "00000000-0000-0000-0000-000000000001",
  "unit_measure_id": "00000000-0000-0000-0000-000000000002",
  "product_usage_type": "Insumo",
  "is_tax_exempt": false,
  "suppliers": [
    {
      "supplier_id": "00000000-0000-0000-0000-000000000003",
      "unit_price": 13.19,
      "tier_prices": [
        {
          "min_quantity": 100,
          "preferential_price": 11.50,
          "valid_from": "2026-10-01",
          "valid_to": "2026-12-31"
        }
      ]
    }
  ]
}
```

| Campo | Tipo | Requerido | Descripción |
|---|:---:|:---:|---|
| `product_name` | `string` | Sí | Nombre del producto (máx. 80) |
| `description` | `string` | No | Descripción |
| `category_id` | `guid` | Sí | Categoría activa |
| `unit_measure_id` | `guid` | Sí | Unidad de medida activa |
| `product_usage_type` | `enum` | Sí | `Insumo` / `OperationalUse` |
| `is_tax_exempt` | `bool` | No | Exento de impuestos |
| `suppliers` | `array` | No | Proveedores iniciales + precios |

**Respuesta:** `{ "product_id": "..." }` (el `code` se genera en backend).

**Notas**

- Crea `SupplierProduct` + historial inicial (`UnitPrice` / `PreferentialPrice`).
- No se permite el mismo `supplier_id` duplicado en el payload.
- Los tiers con el mismo `min_quantity` no pueden traslaparse en fechas.

---

### 4. Actualizar producto

| Campo | Valor |
|---|---|
| **Método** | `PATCH` |
| **Ruta** | `/products/{product_id}` |

```json
{
  "product_name": "Lapicero azul punta fina",
  "description": "Actualizado",
  "category_id": "00000000-0000-0000-0000-000000000099",
  "unit_measure_id": null,
  "product_usage_type": "Insumo",
  "is_tax_exempt": false
}
```

Todos los campos del body son opcionales. Si se envía un `category_id` distinto al actual, **se regenera el código**.

---

### 5. Actualizar precios proveedor–producto

| Campo | Valor |
|---|---|
| **Método** | `PATCH` |
| **Ruta** | `/products/{product_id}/suppliers/{supplier_id}/prices` |

```json
{
  "new_unit_price": 14.25,
  "tier_prices": [
    {
      "min_quantity": 200,
      "preferential_price": 10.90,
      "valid_from": "2026-11-01",
      "valid_to": null
    }
  ]
}
```

| Campo | Tipo | Requerido | Descripción |
|---|:---:|:---:|---|
| `new_unit_price` | `decimal` | Condicional | Al menos uno de los dos debe enviarse |
| `tier_prices` | `array` | Condicional | Precios preferenciales a agregar |

**Comportamiento**

- Cierra el historial vigente (`effective_to = now`) y crea uno nuevo.
- Valida traslapes de tiers por `min_quantity`.

---

### 6. Historial de precios

| Campo | Valor |
|---|---|
| **Método** | `GET` |
| **Ruta** | `/products/{product_id}/suppliers/{supplier_id}/price-history` |

Devuelve filas con `price_type`, `price`, `min_quantity?`, `effective_from`, `effective_to`, `is_current`.

---

### 7. Desactivar vínculo proveedor–producto

| Campo | Valor |
|---|---|
| **Método** | `DELETE` |
| **Ruta** | `/products/{product_id}/suppliers/{supplier_id}` |

Soft-delete (`is_active = false`, `deleted_at`). Cierra historiales y tiers activos.

---

## Endpoints – Proveedores

### 1. Listar proveedores

| Campo | Valor |
|---|---|
| **Método** | `GET` |
| **Ruta** | `/suppliers` |

**Query params**

| Parámetro | Tipo | Descripción |
|---|:---:|---|
| `page_number` / `page_size` | `int` | Paginación |
| `exclusive_status` | `enum` | `None` \| `PendingReview` \| `Approved` \| `Rejected` |
| `commercial_name` | `string` | Contiene |
| `identification_number` | `string` | Exacto |
| `constitution_type` | `enum` | Tipo de constitución |

---

### 2. Detalle de proveedor

| Campo | Valor |
|---|---|
| **Método** | `GET` |
| **Ruta** | `/suppliers/{supplier_id}/details` |

Incluye:

- `supplier_details` (crédito, exclusividad, contacto, retenciones)
- `bank_accounts`
- `products` (productos activos vinculados con precio y tiers)

**Ejemplo de nodo `products`:**

```json
{
  "products": [
    {
      "product_id": "...",
      "code": "ALP-01-001",
      "product_name": "Lapicero azul",
      "unit_measure_id": "...",
      "unit_price": 13.19,
      "last_price_update": "2026-10-07T12:00:00Z",
      "tier_prices": [
        {
          "tier_price_id": "...",
          "min_quantity": 100,
          "preferential_price": 11.50,
          "valid_from": "2026-10-01",
          "valid_to": "2026-12-31",
          "unit_measure_id": "..."
        }
      ]
    }
  ]
}
```

---

### 3. Registrar proveedor

| Campo | Valor |
|---|---|
| **Método** | `POST` |
| **Ruta** | `/suppliers` |

```json
{
  "suppliers_legal_name": "Papelería Central S.A.",
  "commercial_name": "Papelería Central",
  "identification_number": "0010909260001A",
  "constitution_type": "Legal",
  "identification_type": "Ruc",
  "supplier_details": {
    "address": "Km 5 Carretera Norte",
    "email_support": "soporte@papeleria.com",
    "contact_name": "Ana López",
    "contact_email": "ana@papeleria.com",
    "contact_phone_number": "+50588889999",
    "has_credit": true,
    "credit_days": 30,
    "credit_limit": 50000,
    "credit_currency": "USD",
    "alert_days_before_due": 5,
    "preferred_payment_method": "ACH",
    "exclusive_status": "PendingReview",
    "exclusive_brands_or_parts": "Marcas de oficina",
    "apply_ir_retention": true,
    "apply_municipal_retention": true,
    "is_tax_exempt": false
  },
  "bank_accounts": [
    {
      "bank_name": "BAC Credomatic",
      "account_number": "9988776655",
      "account_type": "Checking",
      "currency": "USD",
      "account_holder_name": "Papelería Central S.A.",
      "account_holder_identification": "0010909260001A",
      "is_primary": true
    }
  ],
  "products": [
    {
      "product_id": "00000000-0000-0000-0000-000000000010",
      "unit_price": 12.80,
      "tier_prices": []
    }
  ]
}
```

**Notas**

- Al registrar, `exclusive_status` solo admite `None` o `PendingReview`.
- Solo una cuenta bancaria puede ser `is_primary`.
- `products` es opcional: permite vincular productos existentes al crear el proveedor.

**Respuesta:** `{ "supplier_id": "..." }`

---

### 4. Actualizar información del proveedor

| Campo | Valor |
|---|---|
| **Método** | `PATCH` |
| **Ruta** | `/suppliers/{supplier_id}` |

Actualiza datos generales y detalle (crédito, contacto, etc.). **No** aprueba/rechaza exclusividad.

---

### 5. Aprobar / rechazar exclusividad

| Campo | Valor |
|---|---|
| **Método** | `PATCH` |
| **Ruta** | `/suppliers/{supplier_id}/exclusive-status` |

```json
{
  "exclusive_status": "Approved",
  "comments": "Autorizado como proveedor exclusivo"
}
```

| Campo | Tipo | Requerido | Descripción |
|---|:---:|:---:|---|
| `exclusive_status` | `enum` | Sí | Solo `Approved` o `Rejected` |
| `comments` | `string` | No | Comentario de revisión |

**Reglas**

- El proveedor debe estar en `PendingReview`.
- Roles **`Operator`** y **`Manager`** no pueden ejecutar esta acción.

---

### 6. Cuentas bancarias (existentes)

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/suppliers/{supplier_id}/bank-accounts` | Listar |
| `POST` | `/suppliers/{supplier_id}/bank-accounts` | Agregar |
| `PATCH` | `/suppliers/{supplier_id}/bank-accounts/{bank_account_id}` | Actualizar |
| `DELETE` | `/suppliers/{supplier_id}/bank-accounts/{bank_account_id}` | Soft-delete |

---

## Catálogos auxiliares (UI)

| Método | Ruta | Uso en frontend |
|---|---|---|
| `GET` | `/category-products` | Select de categoría al crear/editar producto |
| `GET` | `/units-measurement` | Select de unidad de medida |
| `GET` | `/suppliers` | Multiselect de proveedores al crear producto |
| `GET` | `/products` | Multiselect de productos al crear proveedor |

---

## Flujo recomendado (Frontend)

1. Cargar categorías y unidades de medida.
2. Registrar proveedor (opcional: solicitar exclusividad con `PendingReview`).
3. Registrar producto seleccionando uno o más proveedores + precio.
4. Ver detalle de producto / proveedor para validar la relación N:N.
5. Actualizar precios → consultar historial.
6. Pantalla de revisión: filtrar `exclusive_status=PendingReview` → aprobar/rechazar.
7. Si cambia la categoría del producto, refrescar el detalle para mostrar el nuevo `code`.

---

## Errores comunes

| Caso | Resultado esperado |
|---|---|
| Categoría / U/M inexistente | `400` |
| Proveedor o producto duplicado en el mismo payload | `400` |
| Tiers con mismo `min_quantity` y fechas traslapadas | `400` |
| `exclusive_status` inválido al registrar (`Approved`/`Rejected`) | `400` |
| Aprobar exclusividad sin estar en `PendingReview` | `400` |
| Aprobar exclusividad como Operator/Manager | `400` permiso |
| Vínculo proveedor–producto inexistente al actualizar precio | `400` |

---

## Controllers

- [`ProductsController.cs`](../../Controllers/Catalog/ProductsController.cs)
- [`SupplierController.cs`](../../Controllers/Catalog/SupplierController.cs) (`SuppliersController`)
