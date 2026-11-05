## Registrar Faltas Disciplinarias

Endpoint para registrar una o más faltas disciplinarias dentro del módulo de nómina. Tras el registro se evalúa el escalamiento por reincidencia.

## Información General

| Campo     | Valor |
|-----------|-------|
| **Método**      | `POST` |
| **Endpoint**    | `/api/v1/companies/{company_id}/modules/{module_code}/faults` |
| **Descripción** | Registra faltas asociadas a colaboradores. Roles permitidos: `Administrator` (Gerente General / RRHH) o `Manager` (Jefe Inmediato). |

---

## Catálogos requeridos

| Campo | Catálogo | Documentación |
|---|---|---|
| `fault_catalog_id` | Catálogo de faltas | [Listar](/src/ERP.Core.Manager.Api/Docs/Faults/GetFaultCatalogDocs.md) |
| `collaborator_id` | Colaboradores | [Listar](/src/ERP.Core.Manager.Api/Docs/Collaborators/GetCollaboratorsDocs.md) |

---

## Parámetros de Ruta (Path Params)

| Parámetro     | Tipo     | Requerido | Descripción |
|:-------------:|:--------:|-----------|-------------|
| `company_id`  | `guid`   | Sí        | Identificador único de la compañía. |
| `module_code` | `string` | Sí        | Código del módulo dentro de la compañía. |

---

## Headers

| Header          | Valor              | Requerido |
|-----------------|--------------------|-----------|
| `Authorization` | `Bearer {token}`   | Sí        |
| `Content-Type`  | `application/json` | Sí        |

---

## Request Body

```json
{
  "faults": [
    {
      "collaborator_id": "ee702059-9d26-4464-9900-c95b616d55c9",
      "fault_catalog_id": "7708d447-1b6a-4194-a877-7e6f49cd80b4",
      "occurrence_date": "2026-10-15",
      "source": 1,
      "parent_fault_id": null,
      "evidences": [
        {
          "file_name": "foto.jpg",
          "content_type": "image/jpeg",
          "image_base64": "/9j/4AAQSkZJRgABAQAAAQABAAD..."
        }
      ]
    }
  ]
}
```

### Descripción de Campos

#### Nivel raíz

| Campo | Tipo | Requerido | Descripción |
|---|---|---|---|
| `faults` | `array` | Sí | Lista de faltas a registrar (al menos una). |

#### `faults[]`

| Campo | Tipo | Requerido | Descripción |
|---|---|---|---|
| `collaborator_id` | `guid` | Sí | Colaborador al que se imputa la falta. |
| `fault_catalog_id` | `guid` | Sí | Tipo de falta del catálogo. |
| `occurrence_date` | `date (YYYY-MM-DD)` | Sí | Fecha de ocurrencia (no puede ser futura). |
| `source` | `enum (FaultSource)` | Sí | Origen de la falta. |
| `parent_fault_id` | `guid \| null` | No | Falta padre si ya está vinculada a reincidencia. |
| `evidences` | `array` | No | Evidencias opcionales. Default: `[]`. |

#### `evidences[]`

| Campo | Tipo | Requerido | Descripción |
|---|---|---|---|
| `file_name` | `string` | Sí* | Nombre del archivo (extensión acorde al content type). |
| `content_type` | `string` | Sí* | `image/jpeg`, `image/jpg`, `image/png` o `image/webp`. |
| `image_base64` | `string` | Sí* | Contenido en base64 (se sube a S3; no se guarda el base64 en BD). |

\* Obligatorios **si** se envía un ítem en `evidences`.

#### Catálogo de Enums

Los enums en el **request** pueden enviarse como valor numérico o como nombre según la serialización del API (`JsonStringEnumConverter` + snake_case). Se documentan ambos.

**`FaultSource`**

| Valor | Nombre | Traducción |
|---|---|---|
| `1` | Manual | Registro manual |
| `2` | SystemEscalation | Escalamiento por reincidencia |
| `3` | BiometricClock | Reloj biométrico |

---

## Respuestas

### ✅ 201 Created

Al crear exitosamente, el endpoint responde únicamente con el código **`201 Created`**, sin body.

```
HTTP/1.1 201 Created
```

### ❌ 400 Bad Request

Usa la entidad `ErrorResponse` (`ERP.Core.Domain.Entities.Errors`):

```json
{
  "status": 400,
  "error": {
    "type_error": "ValidationError",
    "description": "Uno o más catalogos de falta disciplinaria no existen o no están activos"
  },
  "created_at": "2026-09-09 10:00:00"
}
```

### ❌ 401 Unauthorized

```json
{
  "status": 401,
  "error": {
    "type_error": "Unauthorized",
    "description": "Solo el Gerente General, Gerente de Recursos Humanos o Jefe Inmediato pueden registrar faltas disciplinarias."
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
| `201` | Falta(s) registrada(s) exitosamente. |
| `400` | Error de validación (`ErrorResponse`). |
| `401` | Rol no autorizado (`ErrorResponse`). |
| `500` | Error interno del servidor (`ErrorResponse`). |

---

## Notas

- Cada falta registrada recibe un código de instancia (`FLT-{CODE}-{YEAR}-###`).
- No se pueden registrar manualmente catálogos de marcación biométrica (p. ej. `FL-*-01`, `FL-*-03`, `FMG-*-04` para ALP, AVS, TMN, VGM, AMS). Esos códigos no aparecen en el listado de catálogo.
- Tras el registro, el sistema puede crear una falta por reincidencia (`SystemEscalation`) con su propio código `FLT-...`, enlazar orígenes con `parent_fault_id` y reevaluar el siguiente nivel (leve → grave → muy grave) si se cumple el umbral.
- Evidencias: bucket S3 configurado; carpeta `Manager/DisciplinaryFaults/`.
