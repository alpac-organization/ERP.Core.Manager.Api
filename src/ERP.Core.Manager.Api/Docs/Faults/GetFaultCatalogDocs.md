## Listar Catálogo de Faltas Disciplinarias

Endpoint para listar (con paginación y filtros) el catálogo de faltas disciplinarias de una compañía.

## Información General

| Campo     | Valor |
|-----------|-------|
| **Método**      | `GET` |
| **Endpoint**    | `/api/v1/companies/{company_id}/modules/{module_code}/fault-catalogs` |
| **Descripción** | Retorna un listado paginado de tipos de falta (leves, graves, muy graves) asociados a la compañía. Excluye catálogos de registro automático por marcación biométrica. |

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

| Parámetro     | Tipo                        | Requerido | Default | Descripción |
|---------------|-----------------------------|-----------|---------|-------------|
| `severity`     | `enum (FaultSeverity)`      | No        | `null`  | Filtra por severidad. |
| `code`         | `string`                    | No        | `""`    | Filtra por código de catálogo (ej. `FG-ALP-01`). |
| `page_size`    | `integer`                   | No        | `null`  | Cantidad de registros por página. Si no se envía, usa el total. |
| `page_number`  | `integer`                   | No        | `1`     | Número de página a consultar. |

---

## Respuestas

### ✅ 200 OK

Retorna un `PagedResponse<FaultCatalogDto>`.

```json
{
  "data": [
    {
      "fault_catalog_id": "7708d447-1b6a-4194-a877-7e6f49cd80b4",
      "company_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "code": "FL-ALP-01",
      "name": "Llegada tarde",
      "description": "Incumplimiento del horario de ingreso",
      "is_active": true,
      "fault_severity": "Minor"
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
| `data` | `array (FaultCatalogDto)` | Listado de catálogos de falta. |
| `page_number` | `integer` | Página actual. |
| `page_size` | `integer` | Tamaño de página aplicado. |
| `total` | `integer` | Total de registros que cumplen el filtro (antes de paginar). |

#### `data[]` (`FaultCatalogDto`)

| Campo | Tipo | Descripción |
|---|---|---|
| `fault_catalog_id` | `guid` | Identificador del catálogo de falta. |
| `company_id` | `guid` | Compañía a la que pertenece el catálogo. |
| `code` | `string` | Código del tipo de falta (ej. `FL-ALP-01`, `FG-ALP-01`). |
| `name` | `string` | Nombre del tipo de falta. |
| `description` | `string` | Descripción / fundamento. |
| `is_active` | `boolean` | Indica si el catálogo está activo. |
| `fault_severity` | `string` | Severidad como **texto** del enum `FaultSeverity` (ej. `"Minor"`). |

> ℹ️ Los enums en respuesta vienen como **texto** (nombre del enum), no como número.

#### Catálogo de Enums (`FaultSeverity`)

| Valor | Nombre | Traducción |
|---|---|---|
| `1` | Minor | Leve |
| `2` | Serious | Grave |
| `3` | VerySerious | Muy grave |

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
| `200` | Catálogo de faltas obtenido exitosamente. |
| `400` | Error de validación o de acceso (`ErrorResponse`). |
| `500` | Error interno del servidor (`ErrorResponse`). |

---

## Notas

- Se ocultan códigos de faltas automáticas por biométrico según compañía:
  - **ALP:** `FL-ALP-01`, `FL-ALP-03`, `FMG-ALP-04`
  - **AVS:** `FL-AVS-01`, `FL-AVS-03`, `FMG-AVS-04`
  - **TMN:** `FL-TMN-01`, `FL-TMN-03`, `FMG-TMN-04`
  - **VGM:** `FL-VGM-01`, `FL-VGM-03`, `FMG-VGM-04`
  - **AMS:** `FL-AMS-01`, `FL-AMS-03`, `FMG-AMS-04`
- Esos mismos códigos tampoco pueden registrarse por el endpoint de registro manual de faltas.
