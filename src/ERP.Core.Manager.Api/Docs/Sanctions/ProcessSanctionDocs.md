## Procesar Sanción Disciplinaria

Endpoint para **aprobar** (aplicar) o **cancelar/rechazar** una sanción en estado `Pending`. Una sanción `Applied` no se puede retractar.

Corresponde a `ProcessSanctionCommand` / `ProcessSanctionHandler`.

## Información General

| Campo | Valor |
|-------|-------|
| **Método** | `POST` |
| **Endpoint** | `/api/v1/companies/{company_id}/modules/{module_code}/sanctions/{sanction_id}/process` |
| **Descripción** | `is_approved=true` aplica; `is_approved=false` cancela/rechaza (solo `Pending`). |

---

## Parámetros de Ruta (Path Params)

| Parámetro | Tipo | Requerido | Descripción |
|:---------:|:----:|-----------|-------------|
| `company_id` | `guid` | Sí | Identificador de la compañía. |
| `module_code` | `string` | Sí | Código del módulo. |
| `sanction_id` | `guid` | Sí | Identificador de la sanción. |

---

## Headers

| Header | Valor | Requerido |
|--------|-------|-----------|
| `Authorization` | `Bearer {token}` | Sí |
| `Content-Type` | `application/json` | Sí |

---

## Request Body

### Aprobar / aplicar

```json
{
  "is_approved": true
}
```

### Cancelar / rechazar (solo Pending)

```json
{
  "is_approved": false,
  "cancellation_reason": "No procede la sanción propuesta"
}
```

### Descripción de Campos

| Campo | Tipo | Requerido | Descripción |
|---|---|:---:|---|
| `is_approved` | `bool` | Sí | `true` aplica la sanción. `false` la cancela/rechaza. |
| `cancellation_reason` | `string` | Condicional | **Obligatorio** si `is_approved` es `false`. Máx. 500 caracteres. |

---

## Reglas

### Aprobar (`is_approved: true`)
- Solo `Administrator` (Recursos Humanos / Gerente General).
- La sanción debe estar en `Pending`.
- La falta no debe ser hija (`parent_fault_id`); si lo es, debe sancionarse la falta padre.
- La falta no debe estar `Prescribed` ni `Cancelled`.
- Resultado: sanción `Applied`; se setean `approved_by` y `approved_at`.
- La falta pasa a `SanctionApplied` y se propagan las faltas hijas absorbidas.

### Cancelar / rechazar (`is_approved: false`)
- Solo si la sanción está en `Pending`.
- Requiere `cancellation_reason`.
- Roles: `Administrator` o `Manager`.
- Resultado: `Cancelled` + motivo. La falta permanece `Open`.
- No se permite si la sanción ya está `Applied`.

---

## Respuestas

### ✅ 200 OK

```
HTTP/1.1 200 OK
```

### ❌ 400 Bad Request

Ejemplos:

- Motivo de cancelación faltante cuando `is_approved` es `false`.
- Sanción no pendiente al intentar aplicar o cancelar.
- `No se puede cancelar una sanción ya aplicada.`
- Falta absorbida: debe sancionarse la falta padre.
- Sanción ya cancelada.

### ❌ 401 Unauthorized

- Intentar **aplicar** sin rol `Administrator`: *"Solo Recursos Humanos puede aplicar la sanción."*

---

## Códigos de Estado

| Código | Descripción |
|---|---|
| `200` | Procesada correctamente. |
| `400` | Validación o regla de negocio. |
| `401` | Rol no autorizado. |
| `500` | Error interno. |
