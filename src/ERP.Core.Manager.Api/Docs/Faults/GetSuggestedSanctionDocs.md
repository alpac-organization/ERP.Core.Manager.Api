## Obtener Sanción Sugerida

Endpoint para obtener la sanción sugerida según la severidad de la falta y las ocurrencias en el mes. Si la falta ya fue absorbida por reincidencia, orienta a sancionar la falta padre.

## Información General

| Campo     | Valor |
|-----------|-------|
| **Método**      | `GET` |
| **Endpoint**    | `/api/v1/companies/{company_id}/modules/{module_code}/faults/{fault_id}/suggested-sanction` |
| **Descripción** | Retorna tipo sugerido, tipos permitidos y metadatos de escalamiento para facilitar el registro de la sanción en UI. |

---

## Parámetros de Ruta (Path Params)

| Parámetro     | Tipo     | Requerido | Descripción |
|:-------------:|:--------:|-----------|-------------|
| `company_id`  | `guid`   | Sí        | Identificador único de la compañía. |
| `module_code` | `string` | Sí        | Código del módulo dentro de la compañía. |
| `fault_id`    | `guid`   | Sí        | Identificador de la falta consultada (puede ser hija). |

---

## Headers

| Header          | Valor              | Requerido |
|-----------------|--------------------|-----------|
| `Authorization` | `Bearer {token}`   | Sí        |

---

## Respuestas

### ✅ 200 OK

Retorna un `SuggestedSanctionDto`.

#### Sin padre (sancionar la misma falta)

```json
{
  "fault_id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1",
  "sanction_target_fault_id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1",
  "parent_fault_id": null,
  "parent_fault_code": "",
  "is_covered_by_escalation": false,
  "count_occurrence_in_month": 1,
  "severity": "Minor",
  "suggested_sanction_type": "VerbalWarning",
  "allowed_sanction_types": [
    "VerbalWarning",
    "Memorandum"
  ]
}
```

#### Con padre (reincidencia)

Si la falta consultada está en una cadena (p. ej. leve → grave → muy grave), el target es la **raíz** sancionable (muy grave), no solo el padre inmediato.

```json
{
  "fault_id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3",
  "sanction_target_fault_id": "cccccccc-cccc-cccc-cccc-cccccccccccc",
  "parent_fault_id": "cccccccc-cccc-cccc-cccc-cccccccccccc",
  "parent_fault_code": "FLT-ALP-2026-020",
  "is_covered_by_escalation": true,
  "count_occurrence_in_month": 1,
  "severity": "VerySerious",
  "suggested_sanction_type": "CommitmentLetter",
  "allowed_sanction_types": [
    "CommitmentLetter",
    "Termination"
  ]
}
```

### Descripción de Campos

#### Nivel raíz

| Campo | Tipo | Descripción |
|---|---|---|
| `fault_id` | `guid` | Falta que se consultó (la abierta en UI). |
| `sanction_target_fault_id` | `guid` | Falta raíz a usar en `POST .../sanctions` como `fault_id` (sube toda la cadena de padres). |
| `parent_fault_id` | `guid \| null` | Id de la falta raíz sancionable si hubo escalamiento. |
| `parent_fault_code` | `string` | Código de instancia de esa falta raíz (ej. `FLT-ALP-2026-020`). Vacío si no hay padre. |
| `is_covered_by_escalation` | `boolean` | `true` si la falta consultada no debe sancionarse de forma individual. |
| `count_occurrence_in_month` | `integer` | Cantidad de faltas de la misma severidad en el mes (sobre el target). |
| `severity` | `string` | Severidad del **target** como texto del enum `FaultSeverity`. |
| `suggested_sanction_type` | `string` | Tipo sugerido como texto del enum `SanctionType`. |
| `allowed_sanction_types` | `array (string)` | Tipos que la UI puede ofrecer. |

#### Escalera de sugerencia

| Severidad | Ocurrencias en el mes | Sugerida | Permitidas |
|---|---|---|---|
| Leve (`Minor`) | 1 | `VerbalWarning` | `VerbalWarning`, `Memorandum` |
| Leve (`Minor`) | 2+ | `Memorandum` | `Memorandum` |
| Grave (`Serious`) | 1 | `Memorandum` | `Memorandum`, `Suspension` |
| Grave (`Serious`) | 2+ | `Suspension` | `Suspension` |
| Muy grave (`VerySerious`) | cualquiera | `CommitmentLetter` | `CommitmentLetter`, `Termination` |

#### Catálogo de Enums (`SanctionType`)

| Valor | Nombre | Traducción |
|---|---|---|
| `1` | VerbalWarning | Amonestación verbal |
| `2` | Memorandum | Amonestación escrita |
| `3` | Suspension | Suspensión sin goce (3–7 días) |
| `4` | CommitmentLetter | Carta de compromiso |
| `5` | Termination | Terminación de contrato |

> ℹ️ Si `is_covered_by_escalation` es `true`, al registrar la sanción enviar `fault_id = sanction_target_fault_id`. Ver [Registrar sanción](/src/ERP.Core.Manager.Api/Docs/Sanctions/RegisterSanctionDocs.md).

### ❌ 400 Bad Request

```json
{
  "status": 400,
  "error": {
    "type_error": "ValidationError",
    "description": "Esta falta está vinculada a una reincidencia que no se encontró o fue eliminada."
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
| `200` | Sugerencia obtenida exitosamente. |
| `400` | Falta no encontrada, prescrita, ya sancionada o padre inválido (`ErrorResponse`). |
| `500` | Error interno del servidor (`ErrorResponse`). |

---

## Notas

- El conteo mensual (`count_occurrence_in_month`) solo incluye faltas **sin** `parent_fault_id` (no absorbidas por reincidencia), alineado con el escalado.
- Si la falta consultada es hija (o nieta), la sugerencia se calcula sobre la **raíz** de la cadena (`sanction_target_fault_id`), p. ej. leve → grave → muy grave.
- Al **registrar** la sanción sobre esa raíz queda `Pending`. Al **aprobarla** (Process), la falta raíz pasa a `SanctionApplied` y ese estado se propaga a hijas y nietas.
