# Registrar política de permiso

Crea una fila de `PermitTypePolicy`. El alta de una solicitud de cita judicial, matrimonio, nacimiento o fallecimiento exige una política activa para la misma combinación de tipo, alcance y parentesco.

## Registrar un tipo de politica de permiso 

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
| `permit_application_type` | `enum` | Sí | Solo `9`, `10`, `11` o `12`. Otro tipo se rechaza. |
| `permit_beneficiary_scope` | `enum` | Sí | `1` propio, `2` núcleo familiar. `0` no es válido. |
| `family_relationship` | `enum` | Sí | `0` si el alcance es propio. Distinto de `0` si es núcleo familiar, incluido `5` otro. |
| `is_active` | `bool` | Sí | Se guarda el valor enviado. |
| `requires_evidence` | `bool` | Sí | Si es `false`, el mínimo y el máximo se guardan en `0`. Si es `true`, el mínimo debe ser al menos 1 y el máximo no puede ser menor. |
| `consumes_vacation_balance` | `bool` | Sí | Si es `true`, el alta exige saldo y la aprobación descuenta esos días. |
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

Fallecimiento necesita una fila por parentesco: `1` cónyuge, `2` hijo, `3` padre, `4` hermano, `5` otro. Vacaciones, pago de vacaciones y cita médica no usan esta política.

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
