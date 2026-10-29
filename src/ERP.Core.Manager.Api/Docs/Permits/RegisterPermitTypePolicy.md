# Registrar política de permiso

Crea una fila de `PermitTypePolicy`. El alta de una solicitud de cita judicial, matrimonio, nacimiento o fallecimiento exige una política activa para la misma combinación de tipo, alcance y parentesco.

## Información General

| Campo | Valor |
|---|---|
| **Método** | `POST` |
| **Endpoint** | `/api/v1/companies/{company_id}/modules/{module_code}/permit-type-policies` |
| **Descripción** | Registra la regla de evidencia, pasos de aprobación y tope de días. |

---

## Parámetros de Ruta (Path Params)

| Parámetro | Tipo | Requerido | Descripción |
|:---:|:---:|---|---|
| `company_id` | `guid` | Sí | Identificador de la compañía. Solo se usa para validar el acceso. La política no guarda `company_id`. |
| `module_code` | `string` | Sí | Código del módulo. |

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
  "permit_application_type": 10,
  "permit_beneficiary_scope": 1,
  "family_relationship": 0,
  "is_paid": true,
  "is_active": true,
  "requires_evidence": true,
  "consumes_vacation_balance": false,
  "min_evidence_count": 1,
  "max_evidence_count": 3,
  "required_approval_steps": 2,
  "max_amount_days": 3
}
```

| Campo | Tipo | Requerido | Descripción |
|---|---|:---:|---|
| `permit_application_type` | `enum` | Sí | Tipo de permiso. |
| `permit_beneficiary_scope` | `enum` | Sí | `1` propio, `2` núcleo familiar. `0` no es válido. |
| `family_relationship` | `enum` | Sí | `0` si el alcance es propio. Distinto de `0` si es núcleo familiar. |
| `is_paid` | `bool` | Sí | Indica si el día se paga en salario. |
| `is_active` | `bool` | No | El handler guarda la política como activa. |
| `requires_evidence` | `bool` | Sí | Si es `true`, `min_evidence_count` debe ser al menos 1. |
| `consumes_vacation_balance` | `bool` | Sí | Para estos cuatro tipos se envía `false`. El process no descuenta vacaciones. |
| `min_evidence_count` | `int` | Sí | Mínimo de imágenes. No puede ser negativo. |
| `max_evidence_count` | `int` | Sí | Máximo de imágenes. No puede ser menor al mínimo. |
| `required_approval_steps` | `int` | Sí | `1` o `2`. |
| `max_amount_days` | `decimal` | No | Tope de días. Omitir el campo o enviar `null` deja la política sin tope. `0` se rechaza. |

### Tipos

| Valor | Nombre |
|---|---|
| `9` | `JudicialAppointment` |
| `10` | `MarriageLeave` |
| `11` | `BirthLeave` |
| `12` | `BereavementLeave` |

Fallecimiento necesita una fila por parentesco: `1` cónyuge, `2` hijo, `3` padre, `4` hermano.

---

## Autorización

- Requiere token.
- Solo el rol `Administrator` puede registrar. Otro rol recibe `400`.
- No se puede repetir la combinación tipo + alcance + parentesco, aunque la fila anterior esté inactiva.

---

## Respuestas

### 201 Created

No retorna cuerpo.

### 400 Bad Request

```json
{
  "status": 400,
  "error": {
    "type_error": "ERP:02",
    "description": "Ya existe una politica para este tipo de permiso"
  },
  "created_at": "2026-10-29 08:00:00"
}
```

Si el JSON va en camelCase, las propiedades no se asignan y la validación responde que el tipo no es válido, que el alcance es obligatorio y que los pasos deben ser 1 o 2.

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
| `201` | Política registrada. |
| `400` | Validación, acceso o combinación duplicada. |
| `500` | Error interno del servidor. |
