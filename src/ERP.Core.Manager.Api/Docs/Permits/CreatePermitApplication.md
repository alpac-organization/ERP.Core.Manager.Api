# Crear solicitud de permiso

Registra una solicitud. Para cita judicial, matrimonio, nacimiento y fallecimiento el cuerpo es `permit_application_leave`. Vacaciones, pago de vacaciones y cita médica siguen usando su propio bloque.

## Información General

| Campo | Valor |
|---|---|
| **Método** | `POST` |
| **Endpoint** | `/api/v1/companies/{companie_id}/modules/{module_code}/permit-applications` |
| **Descripción** | Crea la solicitud contra la política activa. |

---

## Parámetros de Ruta (Path Params)

| Parámetro | Tipo | Requerido | Descripción |
|:---:|:---:|---|---|
| `companie_id` | `guid` | Sí | Identificador de la compañía. El nombre de la ruta es `companie_id`. |
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
  "payroll_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "channel": 1,
  "description": "Permiso por matrimonio",
  "identification_number": "0010000000000A",
  "permit_application_type": 10,
  "permit_application_leave": {
    "start_date": "2026-11-03",
    "end_date": "2026-11-05",
    "start_time": null,
    "end_time": null,
    "beneficiary_scope": 1,
    "family_relationship": 0,
    "related_person": null,
    "images": [
      {
        "image_base64": "/9j/4AAQSkZJRgABAQAAAQABAAD...",
        "content_type": "image/jpeg"
      }
    ]
  }
}
```

| Campo | Tipo | Requerido | Descripción |
|---|---|:---:|---|
| `payroll_id` | `guid` | Sí | Nómina. Si no existe, la solicitud se guarda como pendiente. |
| `channel` | `enum` | Sí | `1` panel personal, `2` panel administrativo. |
| `description` | `string` | No | Máximo 500 caracteres. |
| `identification_number` | `string` | Sí | Cédula del colaborador. |
| `permit_application_type` | `enum` | Sí | `9`, `10`, `11` o `12` para estos tipos. |
| `permit_application_leave` | `object` | Sí | Datos del permiso. Obligatorio solo para esos cuatro tipos. |

### `permit_application_leave`

| Campo | Tipo | Requerido | Descripción |
|---|---|:---:|---|
| `start_date` | `date` | Sí | Fecha de inicio. |
| `end_date` | `date` | Sí | Fecha de fin. No puede ser menor a la de inicio. |
| `start_time` | `time` | No | Hora de inicio. En el mismo día, junto con `end_time`, calcula fracción de día. |
| `end_time` | `time` | No | Debe ser posterior a `start_time`. |
| `beneficiary_scope` | `enum` | Sí | `1` propio o `2` núcleo familiar. |
| `family_relationship` | `enum` | Sí | `0` si es propio. Obligatorio si es núcleo familiar. |
| `related_person` | `string` | Condicional | Obligatorio cuando el alcance es núcleo familiar. |
| `images` | `array` | Condicional | La cantidad la define la política (`min_evidence_count`, `max_evidence_count`). |

---

## Reglas

- Debe existir una política activa para tipo + alcance + parentesco.
- Si la política exige evidencia, la cantidad de imágenes con `image_base64` debe estar entre el mínimo y el máximo.
- Si `max_amount_days` de la política tiene valor, los días calculados no pueden superarlo. Si es `null`, no hay tope.
- Domingo no cuenta. Sábado cuenta `0.5` solo si el colaborador trabaja sábados.
- No se permite otra solicitud pendiente del mismo colaborador, ni solapamiento de fechas con una pendiente o aprobada.
- El colaborador no puede registrar su propia solicitud desde el panel administrativo.
- `required_approval_steps = 1` deja el primer paso aprobado al crear. `2` usa el flujo de jefe y administrador.
- Estos tipos no descuentan vacaciones al crearse.

---

## Respuestas

### 201 Created

No retorna cuerpo.

### 400 Bad Request

```json
{
  "status": 400,
  "error": {
    "type_error": "ERP:POLICY",
    "description": "No existe un tipo de politica para este tipo de permiso"
  },
  "created_at": "2026-10-29 08:00:00"
}
```

Otros códigos de negocio: `ERP:EVIDENCE`, `ERP:DAYS`, `ERP:LEAVE`, `ERP:DATE_OVERLAP`.

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
| `201` | Solicitud creada. |
| `400` | Validación, política, evidencia, días, solapamiento o acceso. |
| `500` | Error interno del servidor. |
