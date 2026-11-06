## Anular / Eliminar Inasistencia (Soft Delete)

Endpoint para anular lógicamente un registro de inasistencia que fue ingresado por error humano o por fallo en la captura biométrica.

## Información General

| Campo | Valor |
|---|---|
| **Método** | `DELETE` |
| **Endpoint** | `/api/v1/companies/{company_id}/modules/{module_code}/absences/{absence_id}` |
| **Descripción** | Aplica eliminación lógica (*soft delete*) marcando `deleted_at = UtcNow`, cambia el estado a `Cancelled`, revierte a `0.00` todas las deducciones salariales y, si tenía una falta disciplinaria generada, también se anula lógicamente dicha falta y se revierte cualquier escalamiento automático asociado si no tiene sanción aplicada. |

---

## Parámetros de Ruta (Path Params)

| Parámetro | Tipo | Requerido | Descripción |
|:---:|:---:|---|---|
| `company_id` | `guid` | Sí | Identificador único de la compañía. |
| `module_code` | `string` | Sí | Código del módulo dentro de la compañía (ej. `PAYROLL`, `RRHH`). |
| `absence_id` | `guid` | Sí | Identificador único de la inasistencia a eliminar. |

---

## Headers

| Header | Valor | Requerido |
|---|---|---|
| `Authorization` | `Bearer {token}` | Sí |

---

## Reglas de Negocio

1. **Error de Registro:** Diseñado para cuando un jefe inmediato seleccionó al empleado incorrecto o el reloj biométrico generó una marcación errónea.
2. **Ciclo de Nómina Cerrado:** No se permite anular una inasistencia asociada a un ciclo de nómina cerrado o completado.
3. **Reversión Monetaria y Estado (Defensa en Profundidad):** Las deducciones salariales, de 7mo día y viáticos se restablecen automáticamente en `0.00` y su estado funcional cambia a `Cancelled` para blindar cualquier cálculo de nómina.
4. **Cascada Disciplinaria:** Si la inasistencia ya había sido aplicada y tenía vinculada una falta disciplinaria (`DisciplinaryFault`), la falta disciplinaria también se anula lógicamente (`deleted_at = UtcNow`).
5. **Reversión de Escalamiento:** Si la falta disciplinaria había detonado una falta muy grave (`FMG-*-04`) por alcanzar las 3 inasistencias en el mes, la falta padre se anula lógicamente y las inasistencias hermanas restantes se liberan (`ParentFaultId = null`) como faltas leves abiertas.
6. **Protección de Sanciones Firmes:** Si la falta disciplinaria (o la falta escalada padre) ya cuenta con una sanción disciplinaria aplicada (`FaultStatus.SanctionApplied` o con sanciones registradas), no se permite anular la inasistencia.
7. **Liberación de Fecha:** Al eliminarse lógicamente, la fecha queda liberada permitiendo registrar una inasistencia válida para el colaborador en caso necesario.

---

## Respuestas

### ✅ 204 No Content

Al anularse exitosamente, responde con código `204 No Content` sin cuerpo:

```http
HTTP/1.1 204 No Content
```

### ❌ 400 Bad Request

```json
{
  "status": 400,
  "error": {
    "type_error": "BadRequest",
    "description": "No se puede eliminar una inasistencia asociada a un ciclo de nómina cerrado."
  },
  "created_at": "2026-10-29 16:00:00"
}
```

```json
{
  "status": 400,
  "error": {
    "type_error": "BadRequest",
    "description": "No se puede anular la inasistencia porque su falta disciplinaria ya cuenta con una sanción aplicada."
  },
  "created_at": "2026-10-29 16:00:00"
}
```

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
    "description": "No se encontró el registro de inasistencia especificado o ya fue eliminado."
  },
  "created_at": "2026-10-29 16:00:00"
}
```
