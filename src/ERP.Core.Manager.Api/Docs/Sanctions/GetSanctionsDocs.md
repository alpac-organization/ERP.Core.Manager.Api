## Listar Sanciones Disciplinarias

Endpoint para listar (con paginación y filtros) las sanciones disciplinarias registradas dentro de un módulo de una compañía.

## Información General

| Campo     | Valor |
|-----------|-------|
| **Método**      | `GET` |
| **Endpoint**    | `/api/v1/companies/{company_id}/modules/{module_code}/sanctions` |
| **Descripción** | Retorna un listado paginado de sanciones, con filtros por colaborador, falta, tipo y estado. |

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

| Parámetro           | Tipo                       | Requerido | Default | Descripción |
|---------------------|----------------------------|-----------|---------|-------------|
| `collaborator_id`   | `guid`                     | No        | `null`  | Filtra por colaborador. |
| `fault_id`          | `guid`                     | No        | `null`  | Filtra por falta. |
| `sanction_type`     | `enum (SanctionType)`      | No        | `null`  | Filtra por tipo de sanción. |
| `sanction_status`   | `enum (SanctionStatus)`    | No        | `null`  | Filtra por estado (`Pending`, `Applied`, `Cancelled`). |
| `page_size`         | `integer`                  | No        | `10`    | Cantidad de registros por página. |
| `page_number`       | `integer`                  | No        | `1`     | Número de página a consultar. |

---

## Respuestas

### ✅ 200 OK

Retorna un `PagedResponse<SanctionDto>`.

```json
{
  "data": [
    {
      "sanction_id": "cccccccc-cccc-cccc-cccc-cccccccccccc",
      "code": "SAN-ALP-2026-001",
      "fault_id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1",
      "collaborator_id": "ee702059-9d26-4464-9900-c95b616d55c9",
      "collaborator_name": "Juan Perez Gomez",
      "sanction_type": "Memorandum",
      "sanction_status": "Pending",
      "reason": "Segunda falta leve en el mes",
      "suspension_start_date": null,
      "suspension_end_date": null,
      "created_at": "2026-10-16T14:00:00Z"
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
| `data` | `array (SanctionDto)` | Listado de sanciones. |
| `page_number` | `integer` | Página actual. |
| `page_size` | `integer` | Tamaño de página. |
| `total` | `integer` | Total de registros que cumplen los filtros. |

#### `data[]` (`SanctionDto`)

| Campo | Tipo | Descripción |
|---|---|---|
| `sanction_id` | `guid` | Identificador de la sanción. |
| `code` | `string` | Código de instancia de la sanción (ej. `SAN-ALP-2026-001`). |
| `fault_id` | `guid` | Falta asociada. |
| `collaborator_id` | `guid` | Colaborador. |
| `collaborator_name` | `string` | Nombre completo del colaborador. |
| `sanction_type` | `string` | Tipo como **texto** del enum `SanctionType`. |
| `sanction_status` | `string` | Estado como **texto** del enum `SanctionStatus`. |
| `reason` | `string` | Motivo de la sanción. |
| `suspension_start_date` | `date \| null` | Inicio de suspensión (solo si aplica). |
| `suspension_end_date` | `date \| null` | Fin de suspensión (solo si aplica). |
| `created_at` | `datetime (ISO 8601)` | Fecha/hora de registro. |

#### Catálogo de Enums

**`SanctionType`**

| Valor | Nombre | Traducción |
|---|---|---|
| `1` | VerbalWarning | Amonestación verbal |
| `2` | Memorandum | Amonestación escrita |
| `3` | Suspension | Suspensión sin goce |
| `4` | CommitmentLetter | Carta de compromiso |
| `5` | Termination | Terminación de contrato |

**`SanctionStatus`**

| Valor | Nombre | Traducción |
|---|---|---|
| `1` | Applied | Aplicada (aprobada por RRHH) |
| `2` | Cancelled | Cancelada / rechazada |
| `3` | Pending | Pendiente de aprobación de RRHH |

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
| `200` | Listado paginado de sanciones obtenido exitosamente. |
| `400` | Error de validación o de acceso (`ErrorResponse`). |
| `500` | Error interno del servidor (`ErrorResponse`). |
