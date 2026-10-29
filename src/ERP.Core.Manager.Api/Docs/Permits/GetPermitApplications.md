# Listar solicitudes de permiso

Devuelve las solicitudes de la compañía, paginadas. El identificador para aprobar o cancelar es `permit_apllication_id`.

## Obtener permisos 

| Campo | Valor |
|---|---|
| **Método** | `GET` |
| **Endpoint** | `/api/v1/companies/{companie_id}/modules/{module_code}/permit-applications` |
| **Descripción** | Lista solicitudes con filtros opcionales. |

---

## Parámetros de Ruta (Path Params)

| Parámetro | Tipo | Requerido | Descripción |
|:---:|:---:|---|---|
| `companie_id` | `guid` | Sí | Identificador de la compañía. |
| `module_code` | `string` | Sí | Código del módulo. |

---

## Headers

| Header | Valor | Requerido |
|---|---|---|
| `Authorization` | `Bearer {token}` | Sí |

---

## Query Params

| Parámetro | Tipo | Requerido | Default | Descripción |
|---|---|:---:|---|---|
| `page_size` | `int` | No | `10` | Tamaño de página. |
| `page_number` | `int` | No | `1` | Número de página. |
| `payroll_id` | `guid` | No | | Filtra por nómina. |
| `identification_number` | `string` | No | | Filtra por cédula. |
| `type` | `enum` | No | | Por ejemplo `MarriageLeave` o `10`. |
| `status` | `enum` | No | | Estado de la solicitud. |

---

## Respuestas

### 200 OK

```json
{
  "data": [
    {
      "payroll_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "collaborator_id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
      "permit_apllication_id": "c6319544-db83-4bd5-b786-f5f811edc238",
      "description": "Permiso por matrimonio",
      "requested_by": "Nombre del colaborador",
      "additional_data": "{\"MedicalAppointmentData\":{\"IsFullDay\":false,\"ImagesAttached\":[]},\"LeaveEvidenceData\":{\"IsPaid\":true,\"Evidence\":[],\"RequireApprovalSteps\":2,\"ConsumesVacationBalance\":false}}",
      "collaborator_code": "KXX-YMDH",
      "fullname": "Nombre del colaborador",
      "first_step_status": {
        "is_approved": null,
        "reviewed_by": null
      },
      "second_step_status": {
        "is_approved": null,
        "reviewed_by": null
      },
      "amount_days": 3,
      "end_time": null,
      "start_time": null,
      "end_date": "2026-11-05",
      "start_date": "2026-11-03",
      "created_at": "2026-10-29T08:00:00",
      "status": "Pending",
      "type": "MarriageLeave",
      "related_person_name": null,
      "beneficiary_scope": "Own",
      "family_relationship": "None"
    }
  ],
  "page_number": 1,
  "page_size": 10,
  "total": 1
}
```

`additional_data` es un string JSON. Las evidencias de estos tipos van en `leaveEvidenceData.evidence`.

### 400 Bad Request

```json
{
  "status": 400,
  "error": {
    "type_error": "ValidationError",
    "description": "No tienes acceso a este módulo"
  },
  "created_at": "2026-10-29 08:00:00"
}
```

### 500 Internal Server Error

```json
{
  "status": 500,
  "error": {
    "type_error": "InternalServerError",
    "description": "Ocurrió un error inesperado al procesar la solicitud"
  },
  "created_at": "2026-10-29 08:00:00"
}
```

---

## Códigos de Estado

| Código | Descripción |
|---|---|
| `200` | Página de solicitudes obtenida. |
| `400` | Error de acceso o validación. |
| `500` | Error interno del servidor. |
