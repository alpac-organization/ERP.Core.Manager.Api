## Cancelar Inasistencia (Recursos Humanos)

Endpoint para descartar o cancelar una inasistencia cuando se llega a un acuerdo o justificación externa fuera del sistema (ej. vacaciones, constancia médica tardía o permiso).

## Información General

| Campo | Valor |
|---|---|
| **Método** | `POST` |
| **Endpoint** | `/api/v1/companies/{company_id}/modules/{module_code}/absences/{absence_id}/cancel` |
| **Descripción** | Pasa la inasistencia a estado `Cancelled`, revierte a `0.00` todas las deducciones calculadas y guarda de forma obligatoria el motivo de cancelación para auditoría. |

---

## Parámetros de Ruta (Path Params)

| Parámetro | Tipo | Requerido | Descripción |
|:---:|:---:|---|---|
| `company_id` | `guid` | Sí | Identificador único de la compañía. |
| `module_code` | `string` | Sí | Código del módulo dentro de la compañía (ej. `PAYROLL`, `RRHH`). |
| `absence_id` | `guid` | Sí | Identificador único de la inasistencia a cancelar. |

---

## Headers

| Header | Valor | Requerido |
|---|---|---|
| `Authorization` | `Bearer {token}` | Sí |
| `Content-Type` | `application/json` | Sí |

---

## Request Body

```json
{
  "cancellation_reason": "El colaborador presentó constancia médica oficial del MINSA al reincorporarse. Se gestionó el trámite como subsidio externo."
}
```

### Descripción de Campos

| Campo | Tipo | Requerido | Descripción |
|---|---|:---:|---|
| `cancellation_reason` | `string` | Sí | Motivo detallado por el cual Recursos Humanos descarta la inasistencia (1 a 500 caracteres). |

---

## Reglas de Negocio

1. **Motivo Obligatorio:** Es estrictamente obligatorio ingresar el `cancellation_reason` para asegurar la transparencia en auditorías laborales.
2. **Ciclo de Nómina:** No se puede cancelar una inasistencia si ya fue liquidada en un ciclo de nómina cerrado (`Closed` o `Completed`).
3. **Reversión Monetaria:** Las deducciones por salario, 7mo día y viáticos se ponen automáticamente en `0.00`.
4. **Registro de Auditoría:** Queda guardado el usuario de RRHH que ejecutó la cancelación (`reviewed_by`) y el momento exacto (`reviewed_at`).

---

## Respuestas

### ✅ 200 OK

Al cancelarse exitosamente, responde con código `200 OK`:

```http
HTTP/1.1 200 OK
```

### ❌ 400 Bad Request

```json
{
  "status": 400,
  "error": {
    "type_error": "ValidationError",
    "description": "El motivo de cancelación es obligatorio."
  },
  "created_at": "2026-10-29 16:00:00"
}
```

```json
{
  "status": 400,
  "error": {
    "type_error": "BadRequest",
    "description": "No se puede cancelar una inasistencia que ya fue liquidada en un ciclo de nómina cerrado."
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
