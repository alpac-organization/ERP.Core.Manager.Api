# Procesar solicitud de permiso

Aprueba o rechaza un paso. Para cita judicial, matrimonio, nacimiento y fallecimiento el cierre aprobado solo descuenta viáticos. No mueve el saldo de vacaciones.

## Procesar un tipo de permiso 

| Campo | Valor |
|---|---|
| **Método** | `POST` |
| **Endpoint** | `/api/v1/companies/{companie_id}/modules/{module_code}/permit-applications/{permit_application_id}/process` |
| **Descripción** | Primer paso: jefe o administrador. Segundo paso: solo administrador. |

---

## Parámetros de Ruta (Path Params)

| Parámetro | Tipo | Requerido | Descripción |
|:---:|:---:|---|---|
| `companie_id` | `guid` | Sí | Identificador de la compañía. |
| `module_code` | `string` | Sí | Código del módulo. |
| `permit_application_id` | `guid` | Sí | `permit_apllication_id` del listado. |

---

## Headers

| Header | Valor | Requerido |
|---|---|---|
| `Authorization` | `Bearer {token}` | Sí |
| `Content-Type` | `application/json` | Sí |

---

## Body (Request)

```json
{
  "is_approved": true
}
```

| Campo | Tipo | Requerido | Descripción |
|---|---|:---:|---|
| `is_approved` | `bool` | Sí | `true` aprueba el paso actual. `false` lo rechaza. |

El identificador no va en el body.

---

## Reglas

- El primer paso lo hacen `Manager` y `Administrator`. Supervisor y operador reciben `400`.
- El segundo paso lo hace solo `Administrator`.
- Si la política pedía un solo paso, el create ya deja el primero aprobado y este endpoint entra al cierre.
- `is_approved: false` en el segundo paso deja la solicitud en `Rejected` y no descuenta viáticos.
- `is_approved: true` en el segundo paso deja la solicitud en `Approved` y llama el descuento de viáticos.
- El descuento usa el rango `start_date` / `end_date`. No usa `max_amount_days`. Domingo no descuenta. Sábado descuenta un día completo solo si el colaborador trabaja sábados.
- Vacaciones, pago de vacaciones y cita médica siguen su propio efecto al aprobarse.

Hay que llamar el endpoint dos veces cuando `required_approval_steps` es `2`: primero con el token del jefe y después con el del administrador.

---

## Respuestas

### 200 OK

No retorna cuerpo.

### 400 Bad Request

```json
{
  "status": 400,
  "error": {
    "type_error": "ERP:01",
    "description": "No tienes permiso para aprobar o rechazar esta solicitud"
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
| `200` | Paso registrado. |
| `400` | Rol no autorizado, solicitud inexistente o salario no encontrado. |
| `500` | Error interno del servidor. |
