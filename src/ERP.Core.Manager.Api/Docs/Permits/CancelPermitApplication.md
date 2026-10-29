# Cancelar solicitud de permiso

Cancela una solicitud que sigue en estado pendiente.

## Información General

| Campo | Valor |
|---|---|
| **Método** | `GET` |
| **Endpoint** | `/api/v1/companies/{companie_id}/modules/{module_code}/permit-applications/{permit_application_id}/abort` |
| **Descripción** | Pasa la solicitud a `Cancelled`. |

---

## Parámetros de Ruta (Path Params)

| Parámetro | Tipo | Requerido | Descripción |
|:---:|:---:|---|---|
| `companie_id` | `guid` | Sí | Identificador de la compañía. |
| `module_code` | `string` | Sí | Código del módulo. |
| `permit_application_id` | `guid` | Sí | Solicitud a cancelar. |

---

## Headers

| Header | Valor | Requerido |
|---|---|---|
| `Authorization` | `Bearer {token}` | Sí |

---

## Query Params

Este endpoint no recibe query params ni body.

---

## Reglas

- La solicitud tiene que estar `Pending`.
- Un operador no puede cancelarla si el primer paso ya fue aprobado.
- Jefe y administrador pueden cancelarla y limpian los nombres de quien revisó.

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
    "description": "No se encontro la solicitud que desea cancelar"
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
| `200` | Solicitud cancelada. |
| `400` | No existe, ya no está pendiente, o el operador no puede cancelarla. |
| `500` | Error interno del servidor. |
