# Listar políticas de permiso

Devuelve las políticas registradas. Con `only_active=true` es el dropdown que usa el colaborador al solicitar un permiso.

## Listar politicas de permisos  

| Campo | Valor |
|---|---|
| **Método** | `GET` |
| **Endpoint** | `/api/v1/companies/{company_id}/modules/{module_code}/permit-type-policies` |
| **Descripción** | Lista políticas activas o todas. |

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

| Parámetro | Tipo | Requerido | Default | Descripción |
|---|---|:---:|---|---|
| `only_active` | `bool` | No | `true` | `false` incluye políticas inactivas. |
| `permit_application_type` | `enum` | No | | Filtra por tipo. Acepta el nombre o el valor, por ejemplo `MarriageLeave` o `10`. |
| `page_number` | `int` | No | `1` | Número de página. Debe ser mayor que 0. |
| `page_size` | `int` | No | `10` | Tamaño de página. Debe ser mayor que 0. |

---

## Respuestas

### 200 OK

`max_amount_days` es `decimal` porque se compara con días fraccionarios (`0.5` un sábado, `0.1` a `1` por horas). Si el valor guardado es `3`, la respuesta puede traer `3.0000` por la escala numérica de la base. `null` significa que la política no tiene tope.

```json
{
  "data": [
    {
      "permit_type_policy_id": "c6319544-db83-4bd5-b786-f5f811edc238",
      "permit_application_type": "MarriageLeave",
      "permit_beneficiary_scope": "Own",
      "family_relationship": "None",
      "max_amount_days": 3.0000,
      "max_evidence_count": 3,
      "min_evidence_count": 1,
      "is_active": true,
      "consumes_vacation_balance": false,
      "required_approval_steps": 2
    }
  ],
  "page_number": 1,
  "page_size": 10,
  "total": 1
}
```

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
| `200` | Listado obtenido. |
| `400` | Error de acceso. |
| `500` | Error interno del servidor. |
