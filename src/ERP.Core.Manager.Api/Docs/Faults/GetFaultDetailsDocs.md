## Obtener Detalle de Falta Disciplinaria

Endpoint para obtener el detalle completo de una falta disciplinaria específica.

## Información General

| Campo     | Valor |
|-----------|-------|
| **Método**      | `GET` |
| **Endpoint**    | `/api/v1/companies/{company_id}/modules/{module_code}/faults/{fault_id}` |
| **Descripción** | Retorna la información detallada de una falta: colaborador, catálogo, evidencias, falta padre (si hubo escalamiento) y auditoría. |

---

## Parámetros de Ruta (Path Params)

| Parámetro     | Tipo     | Requerido | Descripción |
|:-------------:|:--------:|-----------|-------------|
| `company_id`  | `guid`   | Sí        | Identificador único de la compañía. |
| `module_code` | `string` | Sí        | Código del módulo dentro de la compañía. |
| `fault_id`    | `guid`   | Sí        | Identificador de la falta a consultar. |

---

## Headers

| Header          | Valor              | Requerido |
|-----------------|--------------------|-----------|
| `Authorization` | `Bearer {token}`   | Sí        |

---

## Respuestas

### ✅ 200 OK

Retorna un `FaultDetailDto`.

```json
{
  "fault_id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1",
  "code": "FLT-ALP-2026-001",
  "collaborator_id": "ee702059-9d26-4464-9900-c95b616d55c9",
  "collaborator_name": "Juan Perez Gomez",
  "identification_number": "001-120390-0001A",
  "fault_catalog": {
    "fault_catalog_id": "7708d447-1b6a-4194-a877-7e6f49cd80b4",
    "code": "FL-ALP-01",
    "name": "Llegada tarde"
  },
  "occurrence_date": "2026-10-15",
  "severity": "Minor",
  "source": "Manual",
  "status": "Open",
  "evidences": [
    {
      "file_name": "foto.jpg",
      "content_type": "image/jpeg",
      "image_url": "https://bucket.s3.us-east-1.amazonaws.com/Manager/DisciplinaryFaults/AbCdEfGhIjKl.jpg"
    }
  ],
  "parent_fault": null,
  "registered_by": {
    "user_id": "11111111-1111-1111-1111-111111111111",
    "full_name": "Maria Lopez",
    "email": "maria.lopez@empresa.com",
    "status": "Active"
  },
  "created_at": "2026-10-15T18:30:00Z"
}
```

### Descripción de Campos

#### Nivel raíz

| Campo | Tipo | Descripción |
|---|---|---|
| `fault_id` | `guid` | Identificador de la falta. |
| `code` | `string` | Código de instancia de la falta (ej. `FLT-ALP-2026-001`). |
| `collaborator_id` | `guid` | Identificador del colaborador. |
| `collaborator_name` | `string` | Nombre completo del colaborador. |
| `identification_number` | `string \| null` | Número de identificación del colaborador. |
| `fault_catalog` | `object` | Referencia al catálogo de falta. |
| `occurrence_date` | `date (YYYY-MM-DD)` | Fecha de ocurrencia. |
| `severity` | `string` | Severidad como **texto** del enum `FaultSeverity`. |
| `source` | `string` | Origen como **texto** del enum `FaultSource`. |
| `status` | `string` | Estado como **texto** del enum `FaultStatus`. |
| `evidences` | `array (EvidenceDto)` | Evidencias persistidas (URLs S3). Puede ser `[]`. |
| `parent_fault` | `object \| null` | Falta padre por reincidencia. `null` si no escaló. |
| `registered_by` | `object \| null` | Usuario que registró la falta. |
| `created_at` | `datetime (ISO 8601)` | Fecha/hora de creación (UTC). |

#### `fault_catalog`

| Campo | Tipo | Descripción |
|---|---|---|
| `fault_catalog_id` | `guid` | Identificador del catálogo. |
| `code` | `string` | Código del tipo de falta. |
| `name` | `string` | Nombre del tipo de falta. |

#### `evidences[]`

| Campo | Tipo | Descripción |
|---|---|---|
| `file_name` | `string \| null` | Nombre del archivo. |
| `content_type` | `string \| null` | MIME type (ej. `image/jpeg`). |
| `image_url` | `string \| null` | URL pública en S3. |

#### `parent_fault`

| Campo | Tipo | Descripción |
|---|---|---|
| `fault_id` | `guid` | Identificador de la falta escalada. |
| `code` | `string` | Código de instancia de la falta padre (ej. `FLT-ALP-2026-010`). |
| `occurrence_date` | `date (YYYY-MM-DD)` | Fecha de la falta padre. |
| `severity` | `string` | Severidad de la falta padre. |
| `status` | `string` | Estado de la falta padre. |

#### `registered_by`

| Campo | Tipo | Descripción |
|---|---|---|
| `user_id` | `guid` | Identificador del usuario. |
| `full_name` | `string \| null` | Nombre completo. |
| `email` | `string \| null` | Correo. |
| `status` | `string \| null` | Estado del usuario (texto). |

> ℹ️ En este endpoint de **detalle**, los enums (`severity`, `source`, `status`) vienen como **texto**. Las evidencias ya no incluyen `image_base64`; solo `image_url`.

### ❌ 400 Bad Request

```json
{
  "status": 400,
  "error": {
    "type_error": "ValidationError",
    "description": "La falta disciplinaria no existe."
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
| `200` | Detalle de la falta obtenido exitosamente. |
| `400` | Falta no encontrada o error de validación (`ErrorResponse`). |
| `500` | Error interno del servidor (`ErrorResponse`). |
