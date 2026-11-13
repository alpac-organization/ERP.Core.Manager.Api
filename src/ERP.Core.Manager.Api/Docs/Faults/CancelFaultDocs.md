## Anular Falta Disciplinaria

Endpoint para anular una falta disciplinaria pendiente de sanción (`DeletedAt` + estado `Cancelled` + motivo).

## Información General

| Campo     | Valor |
|-----------|-------|
| **Método**      | `DELETE` |
| **Endpoint**    | `/api/v1/companies/{company_id}/modules/{module_code}/faults/{fault_id}` |
| **Descripción** | Anula la falta indicada con un motivo obligatorio. Si es falta padre de reincidencia, anula también las faltas hijas vinculadas (mismo motivo). No permite anular faltas hijas, sancionadas, prescritas, ni con sanción `Pending`/`Applied`. Roles: `Administrator` o `Manager`. |

---

## Parámetros de Ruta (Path Params)

| Parámetro     | Tipo     | Requerido | Descripción |
|:-------------:|:--------:|-----------|-------------|
| `company_id`  | `guid`   | Sí        | Identificador único de la compañía. |
| `module_code` | `string` | Sí        | Código del módulo dentro de la compañía. |
| `fault_id`    | `guid`   | Sí        | Identificador de la falta a anular (padre o independiente). |

---

## Headers

| Header          | Valor              | Requerido |
|-----------------|--------------------|-----------|
| `Authorization` | `Bearer {token}`   | Sí        |
| `Content-Type`  | `application/json` | Sí        |

---

## Request Body

```json
{
  "cancellation_reason": "Error de captura: la falta corresponde a otro colaborador."
}
```

### Descripción de Campos

| Campo | Tipo | Requerido | Descripción |
|---|---|---|---|
| `cancellation_reason` | `string` | Sí | Motivo de anulación (máx. 500 caracteres). Se guarda en la falta y se replica a las hijas si hay cascada. |

---

## Reglas de anulación

| Caso | Comportamiento |
|---|---|
| Falta independiente (`parent_fault_id` null, sin hijas) | Se anula solo esa falta (`DeletedAt` + `Cancelled` + motivo). |
| Falta padre (reincidencia) | Se anula el padre y, en cascada, las hijas activas vinculadas (mismo motivo). |
| Falta hija (`parent_fault_id` con valor) | **Bloqueado.** Debe anularse la falta padre (el error indica el código del padre). |
| Estado `SanctionApplied` o `Prescribed` | **Bloqueado.** |
| Tiene sanción `Pending` o `Applied` | **Bloqueado.** Debe procesar o cancelar la sanción primero ([Procesar sanción](/src/ERP.Core.Manager.Api/Docs/Sanctions/ProcessSanctionDocs.md)). Una `Applied` (p. ej. suspensión) ya puede haber impactado nómina; no se anula la falta por este endpoint. |
| Sin `cancellation_reason` | **Bloqueado** (validación). |

---

## Respuestas

### ✅ 204 No Content

Al anular exitosamente, el endpoint responde únicamente con el código **`204 No Content`**, sin body.

```
HTTP/1.1 204 No Content
```

### ❌ 400 Bad Request

Usa la entidad `ErrorResponse` (`ERP.Core.Domain.Entities.Errors`):

```json
{
  "status": 400,
  "error": {
    "type_error": "ValidationError",
    "description": "El motivo de anulación es requerido."
  },
  "created_at": "2026-09-09 10:00:00"
}
```

Otros mensajes frecuentes:
- `Esta falta está absorbida por una reincidencia. Debe anular la falta padre con código: FLT-ALP-2026-010.`
- `No se puede anular una falta que ya tiene sanción aplicada.`
- `No se puede anular la falta: tiene una sanción pendiente o aplicada. Debe procesar o cancelar la sanción primero.`
- `No se puede anular una falta prescrita.`
- `La falta disciplinaria no existe o ya fue anulada.`

### ❌ 401 Unauthorized

```json
{
  "status": 401,
  "error": {
    "type_error": "Unauthorized",
    "description": "Solo el Gerente General, Gerente de Recursos Humanos o Jefe Inmediato pueden anular faltas disciplinarias."
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
| `204` | Falta (y hijas, si aplica) anulada exitosamente. |
| `400` | Motivo faltante, falta no encontrada, hija absorbida, sancionada, prescrita u otro error de validación (`ErrorResponse`). |
| `401` | Rol no autorizado (`ErrorResponse`). |
| `500` | Error interno del servidor (`ErrorResponse`). |

---

## Notas

- Anular falta ≠ cancelar sanción. Si existe sanción `Pending`, hay que rechazarla en Process antes de poder anular la falta.
- Si la sanción ya está `Applied` (incluida suspensión con cobro en nómina), la falta no se puede anular por este endpoint.
