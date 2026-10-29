# Opciones del formulario de permisos

Devuelve los enums que el formulario de políticas usa como dropdown: tipo, alcance y parentesco.

## Información General

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
    { "value": 10, "name": "MarriageLeave", "label": "Matrimonio" }
  ],
  "beneficiary_scopes": [
    { "value": 1, "name": "Own", "label": "Propio" },
    { "value": 2, "name": "FamilyNucleus", "label": "Núcleo familiar" }
  ],
  "family_relationships": [
    { "value": 0, "name": "None", "label": "Ninguno" },
    { "value": 1, "name": "Spouse", "label": "Conyugue" },
    { "value": 2, "name": "Child", "label": "Hijo" },
    { "value": 3, "name": "Parent", "label": "Padre" },
    { "value": 4, "name": "Sibling", "label": "Hermano" }
  ]
}
```

`None` no aparece en `beneficiary_scopes`. Sí aparece en `family_relationships`, porque matrimonio, nacimiento y cita judicial se registran con parentesco ninguno.

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
