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
  "data": [
    {
      "collaborator_id": "ee702059-9d26-4464-9900-c95b616d55c9",
      "picture_url": null,
      "full_name": "Juan Ramon Calero Velazquez",
      "collaborator_code": "TXX-SFWD",
      "identification_number": "0012708810028T",
      "status": "Active",
      "vacations": 14.64,
      "work_area": "DEPOSITO PUBLICO",
      "cost_center": null,
      "job_position": "Jefe de Almacen"
    },
    {
      "collaborator_id": "b4720a3d-d2ab-4eaf-b2f9-647ee2837177",
      "picture_url": null,
      "full_name": "Erick Francisco Mejicano Cortez",
      "collaborator_code": "HXX-W9BN",
      "identification_number": "5621010790001H",
      "status": "Active",
      "vacations": 45.75,
      "work_area": "AGENCIAS PEÑAS BLANCAS",
      "cost_center": null,
      "job_position": "Responsable Oficina Peñas Blancas"
    },
    {
      "collaborator_id": "7f2f3cbf-2aee-494f-9fb5-1413d13e5518",
      "picture_url": null,
      "full_name": "Ruddy Ramon Jimenez Jalina",
      "collaborator_code": "SXX-5RQ3",
      "identification_number": "0012010730081S",
      "status": "Active",
      "vacations": 12.46,
      "work_area": "DEPOSITO PUBLICO",
      "cost_center": null,
      "job_position": "Grabador RESA"
    },
    {
      "collaborator_id": "02c90344-c403-4dbf-b169-446f0cec909c",
      "picture_url": null,
      "full_name": "Maria Ines Hernandez Vazquez",
      "collaborator_code": "VXX-FRNB",
      "identification_number": "2811901670000V",
      "status": "Active",
      "vacations": 10.22,
      "work_area": "ADMINISTRACION",
      "cost_center": null,
      "job_position": "Conserje"
    },
    {
      "collaborator_id": "7205fb92-b355-4c26-9f5b-e45f44dff2c0",
      "picture_url": null,
      "full_name": "Silvio Ramon Cabrera Moreira",
      "collaborator_code": "YXX-S5WV",
      "identification_number": "0011911730060Y",
      "status": "Active",
      "vacations": 16.04,
      "work_area": "AGENCIA ADUANERA",
      "cost_center": null,
      "job_position": "Conductor - Motorizado"
    },
    {
      "collaborator_id": "e780a6ee-9a75-42be-87b9-96dfc884af05",
      "picture_url": null,
      "full_name": "Carla Patricia Davila",
      "collaborator_code": "WXX-GG7F",
      "identification_number": "0822608760000W",
      "status": "Active",
      "vacations": 34.45,
      "work_area": "AGENCIA ADUANERA",
      "cost_center": null,
      "job_position": "Asistente Agencia Aduanera"
    },
    {
      "collaborator_id": "6d1dc242-d1e0-4f91-b31a-f9d4818179f6",
      "picture_url": null,
      "full_name": "Helga Anel Espinoza Martinez",
      "collaborator_code": "UXX-HHZM",
      "identification_number": "0822004740001U",
      "status": "Active",
      "vacations": 19.18,
      "work_area": "ALMACEN",
      "cost_center": null,
      "job_position": "Asistente de Almacén"
    },
    {
      "collaborator_id": "8824c8b2-e7ab-49f7-8b57-eff21307ee22",
      "picture_url": null,
      "full_name": "Alba Encarnacion Gutierrez Morales",
      "collaborator_code": "KXX-YMDH",
      "identification_number": "0012210790054K",
      "status": "Active",
      "vacations": 6.68,
      "work_area": "DEPOSITO PUBLICO",
      "cost_center": null,
      "job_position": "Asistente Jefe Deposito Publico"
    },
  ],
  "page_size": 116,
  "page_number": 1,
  "total_records": 10,
  "total_active": 113,
  "total_on_vacation": 0,
  "total_on_subsidy": 0,
  "total_collaborators": 116
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