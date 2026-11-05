# Opciones del formulario de permisos

Devuelve los enums que el formulario de políticas usa como dropdown: tipo, alcance y parentesco.

## Obtener opciones de permisos para registrar  una politica

| Campo | Valor |
|---|---|
| **Método** | `GET` |
| **Endpoint** | `/api/v1/companies/{company_id}/modules/{module_code}/permit-form-options` |
| **Descripción** | Lista de opciones para registrar una política. No consulta la tabla de políticas. |

---

## Parámetros de Ruta (Path Params)

| Parámetro | Tipo | Requerido | Descripción |
|:---:|:---:|---|---|
| `company_id` | `guid` | Sí | Identificador de la compañía. |
| `module_code` | `string` | Sí | Código del módulo. |

---

## Headers

| Header | Valor | Requerido |
|---|---|---|
| `Authorization` | `Bearer {token}` | Sí |

---

## Query Params

Este endpoint no recibe query params.

---

## Respuestas

### 200 OK

```json
{
  "permit_types": [
    { "name": "JudicialAppointment", "value": 9 },
    { "name": "MarriageLeave", "value": 10 },
    { "name": "BirthLeave", "value": 11 },
    { "name": "BereavementLeave", "value": 12 }
  ],
  "beneficiary_scopes": [
    { "name": "Own", "value": 1 },
    { "name": "FamilyNucleus", "value": 2 }
  ],
  "family_relationships": [
    { "name": "None", "value": 0 },
    { "name": "Spouse", "value": 1 },
    { "name": "Child", "value": 2 },
    { "name": "Parent", "value": 3 },
    { "name": "Sibling", "value": 4 },
    { "name": "Other", "value": 5 }
  ]
}
```

Cada opción solo trae `name` (nombre del enum) y `value` (entero). `permit_types` solo incluye cita judicial, matrimonio, nacimiento y fallecimiento. `None` no aparece en `beneficiary_scopes`. Sí aparece en `family_relationships`, porque un permiso propio se registra con parentesco ninguno. `Other` (`5`) es un parentesco de núcleo familiar y exige su propia política.

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
| `200` | Opciones obtenidas. |
| `400` | Error de acceso a la compañía o al módulo. |
| `500` | Error interno del servidor. |
