## Obtener Detalle de Sanción Disciplinaria

Endpoint para obtener el detalle completo de una sanción disciplinaria específica.

## Información General

| Campo     | Valor |
|-----------|-------|
| **Método**      | `GET` |
| **Endpoint**    | `/api/v1/companies/{company_id}/modules/{module_code}/sanctions/{sanction_id}` |
| **Descripción** | Retorna la información detallada de una sanción: falta asociada, colaborador, evidencias y quien la emitió. |

---

## Parámetros de Ruta (Path Params)

| Parámetro      | Tipo     | Requerido | Descripción |
|:--------------:|:--------:|-----------|-------------|
| `company_id`   | `guid`   | Sí        | Identificador único de la compañía. |
| `module_code`  | `string` | Sí        | Código del módulo dentro de la compañía. |
| `sanction_id`  | `guid`   | Sí        | Identificador de la sanción a consultar. |

---

## Headers

| Header          | Valor              | Requerido |
|-----------------|--------------------|-----------|
| `Authorization` | `Bearer {token}`   | Sí        |

---

## Respuestas

### ✅ 200 OK

Retorna un `SanctionDetailDto`.

```json
{
  "sanction_id": "cccccccc-cccc-cccc-cccc-cccccccccccc",
  "code": "SAN-ALP-2026-001",
  "fault_id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1",
  "collaborator_id": "ee702059-9d26-4464-9900-c95b616d55c9",
  "collaborator_name": "Juan Perez Gomez",
  "identification_number": "001-120390-0001A",
  "fault_catalog": {
    "fault_catalog_id": "7708d447-1b6a-4194-a877-7e6f49cd80b4",
    "code": "FL-ALP-01",
    "name": "Llegada tarde"
  },
  "occurrence_date": "2026-10-15",
  "fault_severity": "Minor",
  "sanction_type": "Memorandum",
  "sanction_status": "Applied",
  "reason": "Segunda falta leve en el mes",
  "suspension_start_date": null,
  "suspension_end_date": null,
  "payroll_id": null,
  "issued_by": "11111111-1111-1111-1111-111111111111",
  "issued_by_user": {
    "user_id": "11111111-1111-1111-1111-111111111111",
    "full_name": "Maria Lopez",
    "email": "maria.lopez@empresa.com",
    "status": "Active"
  },
  "evidences": [
    {
      "file_name": "evidencia.jpg",
      "content_type": "image/jpeg",
      "image_url": "https://bucket.s3.us-east-1.amazonaws.com/Manager/DisciplinarySanctions/XyZ123AbCdEf.jpg"
    }
  ],
  "created_at": "2026-10-16T14:00:00Z"
}
```

### Descripción de Campos

#### Nivel raíz

| Campo | Tipo | Descripción |
|---|---|---|
| `sanction_id` | `guid` | Identificador de la sanción. |
| `code` | `string` | Código de instancia de la sanción (ej. `SAN-ALP-2026-001`). |
| `fault_id` | `guid` | Falta sancionada. |
| `collaborator_id` | `guid` | Colaborador sancionado. |
| `collaborator_name` | `string` | Nombre completo. |
| `identification_number` | `string \| null` | Cédula / identificación. |
| `fault_catalog` | `object` | Catálogo de la falta asociada. |
| `occurrence_date` | `date (YYYY-MM-DD)` | Fecha de la falta. |
| `fault_severity` | `string` | Severidad de la falta (texto enum). |
| `sanction_type` | `string` | Tipo de sanción (texto enum). |
| `sanction_status` | `string` | Estado de la sanción (texto enum). |
| `reason` | `string` | Motivo. |
| `suspension_start_date` | `date \| null` | Inicio de suspensión. |
| `suspension_end_date` | `date \| null` | Fin de suspensión. |
| `payroll_id` | `guid \| null` | Nómina vinculada (si aplica; efectos pendientes). |
| `issued_by` | `guid` | Usuario que emitió la sanción. |
| `issued_by_user` | `object \| null` | Detalle del emisor. |
| `evidences` | `array (EvidenceDto)` | Evidencias con URL S3. |
| `created_at` | `datetime (ISO 8601)` | Fecha/hora de creación. |

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
| `content_type` | `string \| null` | MIME type. |
| `image_url` | `string \| null` | URL pública en S3. |

#### `issued_by_user`

| Campo | Tipo | Descripción |
|---|---|---|
| `user_id` | `guid` | Identificador del usuario. |
| `full_name` | `string \| null` | Nombre completo. |
| `email` | `string \| null` | Correo. |
| `status` | `string \| null` | Estado del usuario. |

> ℹ️ En este endpoint de **detalle**, los enums vienen como **texto**. Las evidencias exponen `image_url`, no `image_base64`.

### ❌ 400 Bad Request

```json
{
  "status": 400,
  "error": {
    "type_error": "ValidationError",
    "description": "La sanción disciplinaria no existe."
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
| `200` | Detalle de la sanción obtenido exitosamente. |
| `400` | Sanción no encontrada o error de validación (`ErrorResponse`). |
| `500` | Error interno del servidor (`ErrorResponse`). |
