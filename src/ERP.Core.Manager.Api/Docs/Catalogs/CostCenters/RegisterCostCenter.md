# Registrar Centro de Costos

Endpoint para registrar un nuevo centro de costos dentro del área de un módulo y compañía especificados.

## Información General

| Campo | Valor |
|---|---|
| **Método** | `POST` |
| **Endpoint** | `/api/v1/companies/{company_id}/modules/{module_code}/areas/{area_id}/cost-centers` |
| **Descripción** | Permite registrar un nuevo centro de costos asociado a un área específica. |

---

## Parámetros de Ruta (Path Params)

| Parámetro | Tipo | Requerido | Descripción |
|:---:|:---:|-----------|-------------|
| `company_id` | `guid` | Sí | Identificador único de la compañía. |
| `module_code` | `string` | Sí | Código del módulo dentro de la compañía. |
| `area_id` | `guid` | Sí | Identificador único del área a la que pertenecerá el centro de costos. |

---

## Headers

| Header | Valor | Requerido |
|---|---|---|
| `Authorization` | `Bearer {token}` | Sí |

---

## Body (Request Body)

```json
{
  "coil_code": 101,
  "cost_center_name": "Centro de Costo - Almacén Central",
  "description": "Centro de costos para operaciones logísticas y almacenamiento."
}
```

---

| Campo | Tipo | Requerido | Descripción |
|---|:---:|:---:|---|
| `coil_code` | `integer` | **Sí** | Código numérico del centro de costo (`CoilCode`). |
| `cost_center_name` | `string` | **Si** | Nombre opcional del centro de costo (`CostCenterName`). |
| `description` | `string` | **No** | Descripción general opcional del centro de costo. |

---

## Respuestas

### ✅ 201 Created

No retorna cuerpo (`CreatedResult`).

### ❌ 400 Bad Request

```json
{
  "status": 400,
  "error": {
    "type_error": "ValidationError",
    "description": "El usuario no tiene acceso a esta compañía o módulo"
  },
  "created_at": "2026-09-11 10:00:00"
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
  "created_at": "2026-09-11 10:00:00"
}
```

---

## Códigos de Estado

| Código | Descripción |
|---|---|
| `200` | Listado de puestos de trabajo obtenido exitosamente. |
| `400` | Error de validación o de acceso del usuario a la compañía/módulo (`ErrorResponse`). |
| `500` | Error interno del servidor (`ErrorResponse`). |