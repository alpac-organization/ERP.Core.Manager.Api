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

### Aprobar / aplicar (amonestación u otros tipos sin suspensión)

```json
{
  "is_approved": true
}
```

### Aprobar / aplicar suspensión

`payroll_id` es **obligatorio** cuando el tipo de la sanción es `Suspension`.

```json
{
  "is_approved": true,
  "payroll_id": "dddddddd-dddd-dddd-dddd-dddddddddddd"
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
| `payroll_id` | `guid \| null` | Condicional | **Obligatorio** si se aprueba una `Suspension`. Nómina ordinaria en `Progress` de la sucursal del colaborador, usada para evaluar si el cobro cabe en el periodo. |
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

### Suspensión e impacto en nómina
Al aprobar una `Suspension` (rango 3–7 días ya validado al registrar):

1. Se exige `payroll_id` de una nómina ordinaria en progreso de la sucursal del colaborador.
2. Se calcula el monto = días totales × (salario mensual fijo / 30).
3. **Si el rango de suspensión cabe completo** en el periodo de esa nómina (`start`/`end` dentro de `payroll.start_date`–`payroll.end_date`):
   - Se aplica de inmediato en la nómina ordinaria del colaborador (`Sanction` en deducciones adicionales).
   - Se registra la deducción en estado `Completed` con historial de pago.
   - Se guarda `payroll_id` en la sanción (nómina donde se cobró).
4. **Si no cabe completo** (p. ej. inicia cerca del cierre de quincena):
   - La sanción queda `Applied` disciplinariamente.
   - Solo se **registra** la deducción en `Progress` (pendiente).
   - `payroll_id` de la sanción queda `null` hasta el cobro (vínculo `deduction_id` pendiente de implementación).
   - Al abrir/calcular la **siguiente** nómina ordinaria, el monto se suma a `Sanction`, se genera historial y la deducción pasa a `Completed`.
5. Un recálculo de la misma nómina repone sanciones ya cobradas en ese periodo vía historial de pagos, y sigue aplicando las pendientes `Progress`.

Otros tipos de sanción (amonestación, etc.) **no** impactan nómina; no requieren `payroll_id`.

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
- `El id de la nómina es requerido al aplicar una suspensión.`
- Nómina inválida (no existe, no está en progreso, no es ordinaria o no es de la sucursal).
- Sin salario fijo activo / sin registro de nómina ordinaria del colaborador (cuando cabe y se aplica ya).
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
