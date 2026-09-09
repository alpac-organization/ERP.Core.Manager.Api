## Listar Colaboradores Disponibles

Endpoint para listar (con paginación y filtros) los colaboradores registrados dentro de un módulo de una compañía.

## Información General

| Campo     | Valor |
|-----------|-------|
| **Método**      | `GET` |
| **Endpoint**    | `/api/v1/companies/{companie_id}/modules/{module_code}/collaborators` |
| **Descripción** | Retorna un listado paginado de colaboradores de una compañía/módulo, con contadores por estado y filtros opcionales. |

---

## Parámetros de Ruta (Path Params)

| Parámetro     | Tipo     | Requerido | Descripción |
|:-------------:|:--------:|-----------|-------------|
| `companie_id` | `guid`   | Sí        | Identificador único de la compañía. |
| `module_code` | `string` | Sí        | Código del módulo dentro de la compañía. |

---

## Headers

| Header          | Valor              | Requerido |
|-----------------|--------------------|-----------|
| `Authorization` | `Bearer {token}`   | Sí        |

---

## Query Params

| Parámetro                | Tipo                        | Requerido | Default | Descripción |
|---------------------------|-----------------------------|-----------|---------|-------------|
| `area_id`                 | `guid`                      | No        | `null`  | Filtra colaboradores por área de trabajo. |
| `branch_id`                | `guid`                      | No        | `null`  | Filtra colaboradores por sucursal. |
| `status`                   | `enum (CollaboratorStatus)` | No        | `null`  | Filtra colaboradores por estado. |
| `identification_number`    | `string`                    | No        | `null`  | Filtra por número de identificación exacto. |
| `page_size`                | `integer`                   | No        | `10`    | Cantidad de registros por página. |
| `page_number`              | `integer`                   | No        | `1`     | Número de página a consultar. |

---


## Respuestas

### ✅ 200 OK

Retorna un `PagedResponse<CollaboratorDto>`.

```json
{
  "collaborator_id": "5f8d0d55-6c8a-4a2b-9d3f-000000000099",
  "collaborator_code": "COL-0001",
  "full_name": "Juan Carlos Pérez Gómez",
  "work_position": "Analista de Nómina",
  "status": "Active",
  "profile_picture_url": "https://cdn.tuservicio.com/profiles/col_001.png",
 
  "cost_centers": [
    {
      "id": "5f8d0d55-6c8a-4a2b-9d3f-000000000011",
      "name": "Administración"
    }
  ],
 
  "personal_information": {
    "gender": 1,
    "identification_number": "001-090926-0001A",
    "address": "Barrio El Progreso, Managua",
    "personal_email": "juanperez@gmail.com",
    "personal_phone_number": "88887777",
    "departament": "Managua",
    "marital_status": 1,
    "birthdate": "1995-04-12T00:00:00"
  },
 
  "working_information": {
    "inss_number": "1234567",
    "work_phone_number": "22551234",
    "work_email": "juan.perez@empresa.com",
    "bank_account_number": "1234567890",
    "bank_name": "BAC Nicaragua",
    "work_area": "Recursos Humanos",
    "work_position": "Analista de Nómina",
    "branch_name": "Sucursal Managua",
    "entry_date": "2026-09-09"
  },
 
  "salary_information": {
    "salary": 15000.00,
    "currency": "NIO",
    "salary_type": "Fixed"
  },
 
  "vacation_information": {
    "available_vacations": 8.5
  }
}
```

### Notas sobre los contadores

| Campo (inferido) | Descripción |
|---|---|
| `total_collaborators` | Total de colaboradores de la compañía, sin aplicar filtros de query. |
| `total_active` | Total de colaboradores con `status = Active`, sin aplicar filtros de query. |
| `total_on_vacation` | Total de colaboradores con `status = Vacation`, sin aplicar filtros de query. |
| `total_on_subsidy` | Total de colaboradores con `status = Subsidy`, sin aplicar filtros de query. |
| `total_records` | Total de registros que cumplen los filtros de query (`area_id`, `branch_id`, `status`, `identification_number`), antes de paginar. |

> Estos contadores confirman 3 de los valores conocidos de `CollaboratorStatus`: `Active`, `Vacation` y `Subsidy`. Aún falta el catálogo completo del enum.

### ❌ 400 Bad Request

Usa la entidad `ErrorResponse` (`ERP.Core.Domain.Entities.Errors`):

```json
{
  "status": 400,
  "error": {
    "type_error": "ValidationError",
    "description": "El usuario no tiene acceso a esta compañía o módulo"
  },
  "created_at": "2026-09-09 10:00:00"
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
  "created_at": "2026-09-09 10:00:00"
}
```

---

## Códigos de Estado

| Código | Descripción |
|---|---|
| `200` | Listado paginado de colaboradores obtenido exitosamente. |
| `400` | Error de validación o de acceso del usuario a la compañía/módulo (`ErrorResponse`). |
| `500` | Error interno del servidor (`ErrorResponse`). |