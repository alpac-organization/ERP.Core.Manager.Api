## Listar Inasistencias

Endpoint para consultar y listar las inasistencias registradas en la compañía con paginación del servidor y múltiples filtros (Bandeja de RRHH).

## Información General

| Campo | Valor |
|---|---|
| **Método** | `GET` |
| **Endpoint** | `/api/v1/companies/{company_id}/modules/{module_code}/absences` |
| **Descripción** | Retorna un listado paginado y liviano (`AbsenceDto`) de las inasistencias activas para la grilla o tabla principal de gestión. |

---

## Parámetros de Ruta (Path Params)

| Parámetro | Tipo | Requerido | Descripción |
|:---:|:---:|---|---|
| `company_id` | `guid` | Sí | Identificador único de la compañía. |
| `module_code` | `string` | Sí | Código del módulo dentro de la compañía (ej. `PAYROLL`, `RRHH`). |

---

## Headers

| Header | Valor | Requerido |
|---|---|---|
| `Authorization` | `Bearer {token}` | Sí |

---

## Query Params (Filtros de Búsqueda)

| Parámetro | Tipo | Requerido | Default | Descripción |
|---|---|:---:|:---:|---|
| `page_number` | `integer` | No | `1` | Número de página a consultar (mayor a 0). |
| `page_size` | `integer` | No | `10` | Cantidad de registros por página (1 a 100). |
| `status` | `enum (AbsenceStatus)` | No | `null` | Filtra por estado de la inasistencia (`1 = Pending`, `2 = Applied`, `3 = Cancelled`). |
| `source` | `enum (AbsenceSource)` | No | `null` | Filtra por origen (`1 = Manual`, `2 = BiometricClock`). |
| `start_date` | `date (YYYY-MM-DD)` | No | `null` | Fecha inicial de búsqueda. |
| `end_date` | `date (YYYY-MM-DD)` | No | `null` | Fecha final de búsqueda. |
| `collaborator_id` | `guid` | No | `null` | Filtra por un colaborador específico. |
| `branch_id` | `guid` | No | `null` | Filtra colaboradores por sucursal. |
| `identification_number` | `string` | No | `null` | Filtra por cédula o número de identificación. |
| `payroll_id` | `guid` | No | `null` | Filtra por ciclo de nómina asociado. |

---

## Catálogo de Enums

### `AbsenceStatus` (Estado de la inasistencia)

| Valor | Nombre | Descripción |
|:---:|---|---|
| `1` | `Pending` | Pendiente de revisión por Recursos Humanos. |
| `2` | `Applied` | Aprobada y aplicada formalmente (descuenta en nómina y genera falta leve). |
| `3` | `Cancelled` | Cancelada por acuerdo o justificación externa (vacaciones, permiso, constancia médica). |

### `AbsenceSource` (Origen)

| Valor | Nombre | Descripción |
|:---:|---|---|
| `1` | `Manual` | Registrada manualmente por el jefe o supervisor. |
| `2` | `BiometricClock` | Registrada por el reloj biométrico. |

---

## Respuestas

### ✅ 200 OK

Retorna una estructura paginada `PagedResponse<AbsenceDto>` con la información esencial de cada registro para la grilla:

```json
{
  "data": [
    {
      "absence_id": "50ee2c80-130e-4abb-b221-630f5a98a719",
      "collaborator_id": "92efea04-e76d-4f43-b367-bdf21394695d",
      "fullname": "Donald José Munguía Guadamuz",
      "identification_number": "0011401950054W",
      "branch_name": "Almacenadora del Pacífico S.A.",
      "date": "2026-10-29",
      "status": "Pending",
      "source": "Manual"
    },
    {
      "absence_id": "44c418a0-e81a-44a4-b13b-184d3788fe3e",
      "collaborator_id": "8824c8b2-e7ab-49f7-8b57-eff21307ee22",
      "fullname": "Alba Encarnacion Gutierrez Morales",
      "identification_number": "0012210790054K",
      "branch_name": "Almacenadora del Pacífico S.A.",
      "date": "2026-10-29",
      "status": "Applied",
      "source": "BiometricClock"
    }
  ],
  "page_number": 1,
  "page_size": 10,
  "total": 2
}
```

### ❌ 400 Bad Request

```json
{
  "status": 400,
  "error": {
    "type_error": "ValidationError",
    "description": "El número de página debe ser mayor a 0."
  },
  "created_at": "2026-10-29 16:00:00"
}
```

### ❌ 401 Unauthorized

```json
{
  "status": 401,
  "error": {
    "type_error": "Unauthorized",
    "description": "Token de autenticación inválido o expirado"
  },
  "created_at": "2026-10-29 16:00:00"
}
```
