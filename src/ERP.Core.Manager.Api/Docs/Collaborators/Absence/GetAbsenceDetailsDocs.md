## Obtener Detalle de Inasistencia

Endpoint para consultar la ficha completa y detallada de un registro de inasistencia mediante su identificador único (`absence_id`).

## Información General

| Campo | Valor |
|---|---|
| **Método** | `GET` |
| **Endpoint** | `/api/v1/companies/{company_id}/modules/{module_code}/absences/{absence_id}` |
| **Descripción** | Retorna la entidad plana completa `AbsenceDetailDto`, que incluye desglose financiero, motivos, auditoría y faltas asociadas. |

---

## Parámetros de Ruta (Path Params)

| Parámetro | Tipo | Requerido | Descripción |
|:---:|:---:|---|---|
| `company_id` | `guid` | Sí | Identificador único de la compañía. |
| `module_code` | `string` | Sí | Código del módulo dentro de la compañía (ej. `PAYROLL`, `RRHH`). |
| `absence_id` | `guid` | Sí | Identificador único del registro de inasistencia. |

---

## Headers

| Header | Valor | Requerido |
|---|---|---|
| `Authorization` | `Bearer {token}` | Sí |

---

## Respuestas

### ✅ 200 OK

Retorna el DTO de detalle plano `AbsenceDetailDto`:

```json
{
  "absence_id": "50ee2c80-130e-4abb-b221-630f5a98a719",
  "collaborator_id": "92efea04-e76d-4f43-b367-bdf21394695d",
  "fullname": "Donald José Munguía Guadamuz",
  "identification_number": "0011401950054W",
  "branch_name": "Almacenadora del Pacífico S.A.",
  "date": "2026-10-29",
  "status": "Applied",
  "source": "Manual",
  "collaborator_code": "WXX-XDVE",
  "reason": "El colaborador no se presentó a su turno matutino y no notificó previamente.",
  "registered_by": "7293b6b6-3070-402d-8594-34321cfabf07",
  "reviewed_by": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "reviewed_at": "2026-10-29T20:30:15Z",
  "cancellation_reason": null,
  "salary_deduction": 500.00,
  "seventh_day_deduction": 83.33,
  "travel_expenses_deduction": 40.00,
  "total_deduction": 623.33,
  "payroll_id": null,
  "ordinary_payroll_id": null,
  "disciplinary_fault_id": "4fe60a26-fbda-4f0d-9de7-c5a9030b0780",
  "created_at": "2026-10-29T20:15:09.356171Z",
  "deleted_at": null
}
```

### Descripción de Campos en la Respuesta

| Campo | Tipo | Descripción |
|---|---|---|
| `absence_id` | `guid` | Identificador único de la inasistencia. |
| `collaborator_id` | `guid` | Identificador único del colaborador. |
| `fullname` | `string` | Nombre completo del colaborador (nombres y apellidos). |
| `identification_number` | `string` | Cédula o número de documento del colaborador. |
| `branch_name` | `string?` | Nombre de la sucursal a la que pertenece. |
| `date` | `date` | Fecha en la que ocurrió la inasistencia. |
| `status` | `enum` | Estado actual (`Pending`, `Applied`, `Cancelled`). |
| `source` | `enum` | Origen de registro (`Manual`, `BiometricClock`). |
| `collaborator_code` | `string` | Código interno de empleado (ej. `WXX-XDVE`). |
| `reason` | `string?` | Observaciones o motivo inicial de la inasistencia. |
| `registered_by` | `guid` | ID del usuario que registró la inasistencia. |
| `reviewed_by` | `guid?` | ID del usuario de RRHH que aplicó o canceló la inasistencia. |
| `reviewed_at` | `datetime?` | Fecha y hora en que RRHH revisó el registro. |
| `cancellation_reason` | `string?` | Motivo de cancelación si fue descartada. |
| `salary_deduction` | `decimal` | Descuento del salario ordinario diario ($\text{Salario Base} / 30$). |
| `seventh_day_deduction` | `decimal` | Descuento proporcional del séptimo día ($\text{Salario Diario} / 6$). |
| `travel_expenses_deduction`| `decimal` | Descuento de viáticos diarios asignados ($\text{Viáticos Quincenales} / 15$). |
| `total_deduction` | `decimal` | Suma total de las deducciones calculadas. |
| `payroll_id` | `guid?` | ID del ciclo de nómina al que fue liquidada. |
| `ordinary_payroll_id` | `guid?` | ID del detalle de nómina ordinaria del empleado. |
| `disciplinary_fault_id` | `guid?` | ID de la falta disciplinaria leve generada en el módulo disciplinario. |
| `created_at` | `datetime` | Fecha y hora de creación del registro. |
| `deleted_at` | `datetime?` | Fecha de anulación lógica (soft delete). |

---

### ❌ 401 Unauthorized

```json
{
  "status": 401,
  "error": {
    "type_error": "Unauthorized",
    "description": "Token de autenticación inválido o expirado"
  },
  "created_at": "2026-10-29 16:00:00"
}
```

### ❌ 404 Not Found

```json
{
  "status": 404,
  "error": {
    "type_error": "NotFound",
    "description": "No se encontró el registro de inasistencia especificado."
  },
  "created_at": "2026-10-29 16:00:00"
}
```
