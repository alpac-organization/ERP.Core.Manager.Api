## Registrar Inasistencia

Endpoint para registrar una inasistencia de un colaborador, reportada por el jefe inmediato o por el sistema de reloj biométrico.

## Información General

| Campo | Valor |
|---|---|
| **Método** | `POST` |
| **Endpoint** | `/api/v1/companies/{company_id}/modules/{module_code}/absences` |
| **Descripción** | Registra una nueva inasistencia en estado `Pending` (Pendiente) para que Recursos Humanos la revise y aplique o cancele. |

---

## Parámetros de Ruta (Path Params)

| Parámetro | Tipo | Requerido | Descripción |
|:---:|:---:|---|---|
| `company_id` | `guid` | Sí | Identificador único de la compañía. |
| `module_code` | `string` | Sí | Código del módulo dentro de la compañía (ej. `PAYROLL`, `RRHH`). |

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
  "collaborator_id": "92efea04-e76d-4f43-b367-bdf21394695d",
  "date": "2026-10-29",
  "reason": "El colaborador no se presentó a su turno matutino y no notificó previamente.",
  "source": 1
}
```

### Descripción de Campos

| Campo | Tipo | Requerido | Descripción |
|---|---|---|---|
| `collaborator_id` | `guid` | Sí | Identificador único del colaborador. |
| `date` | `date (YYYY-MM-DD)` | Sí | Fecha de la inasistencia. |
| `reason` | `string` | No | Motivo u observación inicial del registro (máx. 500 caracteres). |
| `source` | `enum (AbsenceSource)` | No | Origen del registro. Default: `1` (*Manual*). |

---

## Catálogo de Enums

### `AbsenceSource` (Origen de la inasistencia)

| Valor | Nombre | Descripción |
|:---:|---|---|
| `1` | `Manual` | Registrada manualmente por el Jefe Inmediato o RRHH. |
| `2` | `BiometricClock` | Registrada automáticamente por el sistema de reloj biométrico. |

---

## Validaciones y Reglas de Negocio

1. **Unicidad por fecha:** No se permite registrar más de una inasistencia activa o pendiente para el mismo colaborador en la misma fecha.
2. **Permisos Aprobados:** Valida que el colaborador no cuente con una solicitud de permiso aprobada (`PermitApplication`) que cubra la fecha indicada.
3. **Estado Inicial:** Toda inasistencia recién registrada nace con estado `Pending` (`1`), deducciones en `0.00` y sin falta disciplinaria asociada, a la espera de la resolución de RRHH.

---

## Respuestas

### ✅ 201 Created

Al registrarse exitosamente, el endpoint responde con código HTTP `201 Created` sin cuerpo:

```http
HTTP/1.1 201 Created
```

### ❌ 400 Bad Request

```json
{
  "status": 400,
  "error": {
    "type_error": "BadRequest",
    "description": "Ya existe un registro de inasistencia para este colaborador en la fecha especificada."
  },
  "created_at": "2026-10-29 16:00:00"
}
```

```json
{
  "status": 400,
  "error": {
    "type_error": "BadRequest",
    "description": "El colaborador cuenta con un permiso aprobado en la fecha indicada."
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
    "description": "No se encontró el colaborador especificado en esta empresa."
  },
  "created_at": "2026-10-29 16:00:00"
}
```
