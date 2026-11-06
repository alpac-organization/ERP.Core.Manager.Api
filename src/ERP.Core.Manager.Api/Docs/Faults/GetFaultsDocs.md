## Listar Faltas Disciplinarias

Endpoint para listar (con paginación y filtros) las faltas disciplinarias registradas dentro de un módulo de una compañía.

## Información General

| Campo     | Valor |
|-----------|-------|
| **Método**      | `GET` |
| **Endpoint**    | `/api/v1/companies/{company_id}/modules/{module_code}/faults` |
| **Descripción** | Retorna un listado paginado de faltas disciplinarias, con filtros por código de instancia, código de catálogo, colaborador, severidad, estado y origen. |

---

## Parámetros de Ruta (Path Params)

| Parámetro     | Tipo     | Requerido | Descripción |
|:-------------:|:--------:|-----------|-------------|
| `company_id`  | `guid`   | Sí        | Identificador único de la compañía. |
| `module_code` | `string` | Sí        | Código del módulo dentro de la compañía. |

---

## Headers

| Header          | Valor              | Requerido |
|-----------------|--------------------|-----------|
| `Authorization` | `Bearer {token}`   | Sí        |

---

## Query Params

| Parámetro          | Tipo                     | Requerido | Default | Descripción |
|--------------------|--------------------------|-----------|---------|-------------|
| `code`             | `string`                 | No        | `null`  | Filtra por código de instancia de la falta (ej. `FLT-ALP-2026-001`). |
| `catalog_code`     | `string`                 | No        | `null`  | Filtra por código del catálogo de falta (ej. `FL-ALP-01`). |
| `collaborator_id`  | `guid`                   | No        | `null`  | Filtra faltas del colaborador indicado. |
| `severity`          | `enum (FaultSeverity)`   | No        | `null`  | Filtra por severidad. |
| `status`           | `enum (FaultStatus)`     | No        | `null`  | Filtra por estado de la falta. |
| `source`           | `enum (FaultSource)`     | No        | `null`  | Filtra por origen de la falta. |
| `page_size`        | `integer`                | No        | `10`    | Cantidad de registros por página. |
| `page_number`      | `integer`                | No        | `1`     | Número de página a consultar. |

---

## Respuestas

### ✅ 200 OK

Retorna un `PagedResponse<FaultDto>`.

```json
{
  "data": [
    {
      "fault_id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1",
      "code": "FLT-ALP-2026-001",
      "collaborator_id": "ee702059-9d26-4464-9900-c95b616d55c9",
      "collaborator_name": "Juan Perez Gomez",
      "fault_catalog": {
        "fault_catalog_id": "7708d447-1b6a-4194-a877-7e6f49cd80b4",
        "code": "FL-ALP-01",
        "name": "Llegada tarde"
      },
      "occurrence_date": "2026-10-15",
      "severity": "Minor",
      "source": "Manual",
      "status": "Open"
    }
  ],
  "page_number": 1,
  "page_size": 10,
  "total": 1
}
```

### Descripción de Campos

#### Nivel raíz (`PagedResponse`)

| Campo | Tipo | Descripción |
|---|---|---|
| `data` | `array (FaultDto)` | Listado de faltas. |
| `page_number` | `integer` | Página actual. |
| `page_size` | `integer` | Tamaño de página. |
| `total` | `integer` | Total de registros que cumplen los filtros. |

#### `data[]` (`FaultDto`)

| Campo | Tipo | Descripción |
|---|---|---|
| `fault_id` | `guid` | Identificador de la falta. |
| `code` | `string` | Código de instancia de la falta (ej. `FLT-ALP-2026-001`). |
| `collaborator_id` | `guid` | Identificador del colaborador. |
| `collaborator_name` | `string` | Nombre completo del colaborador. |
| `fault_catalog` | `object` | Referencia al catálogo (ver abajo). |
| `occurrence_date` | `date (YYYY-MM-DD)` | Fecha de ocurrencia. |
| `severity` | `string` | Severidad como **texto** del enum `FaultSeverity`. |
| `source` | `string` | Origen como **texto** del enum `FaultSource`. |
| `status` | `string` | Estado como **texto** del enum `FaultStatus`. |

#### `fault_catalog`

| Campo | Tipo | Descripción |
|---|---|---|
| `fault_catalog_id` | `guid` | Identificador del catálogo. |
| `code` | `string` | Código del tipo de falta. |
| `name` | `string` | Nombre del tipo de falta. |

#### Catálogo de Enums

**`FaultSeverity`**

| Valor | Nombre | Traducción |
|---|---|---|
| `1` | Minor | Leve |
| `2` | Serious | Grave |
| `3` | VerySerious | Muy grave |

**`FaultStatus`**

| Valor | Nombre | Traducción |
|---|---|---|
| `1` | Open | Pendiente de sanción |
| `2` | SanctionApplied | Sanción aplicada |
| `3` | Prescribed | Prescrita |
| `4` | Cancelled | Anulada |

**`FaultSource`**

| Valor | Nombre | Traducción |
|---|---|---|
| `1` | Manual | Registro manual |
| `2` | SystemEscalation | Escalamiento por reincidencia |
| `3` | BiometricClock | Reloj biométrico |

### ❌ 400 Bad Request

Usa la entidad `ErrorResponse` (`ERP.Core.Domain.Entities.Errors`):

```json
{
  "status": 400,
  "error": {
    "type_error": "ValidationError",
    "description": "El usuario no tiene acceso a esta compañía o módulo"
  },
  "created_at": "2026-09-09 10:00:00"
}
```

### ❌ 500 Internal Server Error

```json
{
  "status": 500,
  "error": {
    "type_error": "InternalServerError",
    "description": "Ocurrió un error inesperado al procesar la solicitud"
  },
  "created_at": "2026-09-09 10:00:00"
}
```

---

## Códigos de Estado

| Código | Descripción |
|---|---|
| `200` | Listado paginado de faltas obtenido exitosamente. |
| `400` | Error de validación o de acceso (`ErrorResponse`). |
| `500` | Error interno del servidor (`ErrorResponse`). |
