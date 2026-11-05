## Registrar Sanción Disciplinaria

Endpoint para **registrar/proponer** una sanción sobre una falta disciplinaria. La sanción queda en estado `Pending`. La falta permanece `Open` hasta que RRHH la procese con [Procesar sanción](/src/ERP.Core.Manager.Api/Docs/Sanctions/ProcessSanctionDocs.md).

## Información General

| Campo     | Valor |
|-----------|-------|
| **Método**      | `POST` |
| **Endpoint**    | `/api/v1/companies/{company_id}/modules/{module_code}/sanctions` |
| **Descripción** | Registra sanción pendiente. Roles: `Administrator` (Gerente General / RRHH) o `Manager` (Jefe Inmediato). |

> 🔗 Antes de registrar, consultar [Sugerir sanción](/src/ERP.Core.Manager.Api/Docs/Faults/GetSuggestedSanctionDocs.md) y usar `sanction_target_fault_id` como `fault_id` del body.

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
  "fault_id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1",
  "sanction_type": 2,
  "reason": "Segunda falta leve en el mes",
  "suspension_start_date": null,
  "suspension_end_date": null,
  "evidences": [
    {
      "file_name": "evidencia.jpg",
      "content_type": "image/jpeg",
      "image_base64": "/9j/4AAQSkZJRgABAQAAAQABAAD..."
    }
  ]
}
```

Ejemplo de suspensión (3 a 7 días):

```json
{
  "fault_id": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
  "sanction_type": 3,
  "reason": "Segunda falta grave en el mes",
  "suspension_start_date": "2026-10-20",
  "suspension_end_date": "2026-10-24",
  "evidences": [
    {
      "file_name": "acta.jpg",
      "content_type": "image/jpeg",
      "image_base64": "/9j/4AAQ..."
    }
  ]
}
```

### Descripción de Campos

#### Nivel raíz

| Campo | Tipo | Requerido | Descripción |
|---|---|---|---|
| `fault_id` | `guid` | Sí | Falta a sancionar. No debe tener `parent_fault_id` (usar el target del suggested). |
| `sanction_type` | `enum (SanctionType)` | Sí | Tipo de sanción. |
| `reason` | `string` | Sí | Motivo (máx. 500 caracteres). |
| `suspension_start_date` | `date \| null` | Condicional | Obligatorio si `sanction_type = Suspension`. |
| `suspension_end_date` | `date \| null` | Condicional | Obligatorio si suspensión; rango **3–7 días** inclusive. |
| `evidences` | `array` | Condicional | Obligatorias salvo `VerbalWarning`. |

#### `evidences[]`

| Campo | Tipo | Requerido | Descripción |
|---|---|---|---|
| `file_name` | `string` | Sí* | Nombre del archivo. |
| `content_type` | `string` | Sí* | `image/jpeg`, `image/jpg`, `image/png` o `image/webp`. |
| `image_base64` | `string` | Sí* | Contenido en base64 (se sube a S3). |

\* Obligatorios si el ítem existe / si el tipo exige evidencias.

#### Catálogo de Enums

**`SanctionType`**

| Valor | Nombre | Traducción | Evidencia |
|---|---|---|---|
| `1` | VerbalWarning | Amonestación verbal | Opcional |
| `2` | Memorandum | Amonestación escrita | Requerida |
| `3` | Suspension | Suspensión sin goce | Requerida |
| `4` | CommitmentLetter | Carta de compromiso | Requerida |
| `5` | Termination | Terminación de contrato | Requerida |

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
    "description": "Esta falta ya fue absorbida por una falta escalada. Debe sancionar la falta padre."
  },
  "created_at": "2026-09-09 10:00:00"
}
```

Otros mensajes frecuentes:

- `No se puede sancionar una falta prescrita, aplicada o cancelada.`
- `Ya existe una sanción pendiente o aplicada para esta falta.`
- `La suspensión debe durar entre 3 y 7 días.`

### ❌ 401 Unauthorized

```json
{
  "status": 401,
  "error": {
    "type_error": "Unauthorized",
    "description": "Solo el Gerente General, Gerente de Recursos Humanos o Jefe Inmediato pueden registrar sanciones."
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
| `201` | Sanción registrada exitosamente (`Pending`). |
| `400` | Error de validación, falta inválida, suspensión fuera de rango, etc. (`ErrorResponse`). |
| `401` | Rol no autorizado (`ErrorResponse`). |
| `500` | Error interno del servidor (`ErrorResponse`). |

---

## Notas

- Cada sanción registrada recibe un código de instancia (`SAN-{CODE}-{YEAR}-###`).
- La sanción queda `Pending`. Para aplicar o cancelar usar [Procesar sanción](/src/ERP.Core.Manager.Api/Docs/Sanctions/ProcessSanctionDocs.md).
- No se permite otra sanción `Pending` o `Applied` activa sobre la misma falta.
- No se marca la falta como `SanctionApplied` en este paso; eso ocurre al aprobar en Process.
- Evidencias: carpeta S3 `Manager/DisciplinarySanctions/`.
- Efectos de nómina (`payroll_id`) pendientes de implementación.
