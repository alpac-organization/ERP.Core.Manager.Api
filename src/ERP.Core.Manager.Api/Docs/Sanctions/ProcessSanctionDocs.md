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
Al aprobar una `Suspension` (rango 3–7 **días laborales** ya validado al registrar):

1. Se exige `payroll_id` de una nómina ordinaria en `Progress` de la sucursal del colaborador.
2. Día no pagado = salario mensual fijo / 30. Solo cuentan **días laborales**: no domingo; sábado solo si el colaborador trabaja sábados (`DoesWorkSaturdays`).
3. **Si el rango cabe completo** en el periodo de esa nómina (`suspension_start`/`suspension_end` dentro de `payroll.start_date`–`payroll.end_date`):
   - Se reduce el bruto / ingreso del periodo por los días de suspensión (base para INSS/IR).
   - `Sanction` en deducciones adicionales refleja el monto no pagado (desglose; no se vuelve a restar del neto).
   - Se registra la deducción en `Completed` con historial de pago.
   - Se vinculan en la sanción el `DeductionId` (siempre) y el `PayrollId` de la nómina donde se cobró.
4. **Si no cabe completo** (cruza quincenas, o la nómina enviada no contiene todo el rango; p. ej. quincena ya no aplica y el cobro va a la siguiente):
   - La sanción queda `Applied` disciplinariamente.
   - Se registra la deducción en `Progress` por el monto total (días × diario) y se guarda el `DeductionId` en la sanción.
   - El `PayrollId` de la sanción queda `null` hasta el cobro en una nómina posterior.
   - Al calcular la **siguiente** nómina ordinaria: se cobra el monto diferido en `Sanction` (sí afecta el neto), INSS/IR sobre la base reducida, se cierra la deducción a `Completed`, se setea el `PayrollId` de la sanción y se actualiza el acumulado de IR en coherencia con esa base.
5. Al generar la nómina, los **viáticos** no se pagan en días de suspensión Applied (día no trabajado = sin viático). El viático se suma aparte al neto; no entra a la base salarial.

Consulta del vínculo: el [detalle de sanción](/src/ERP.Core.Manager.Api/Docs/Sanctions/GetSanctionDetailsDocs.md) expone objetos `deduction` y `payroll` (no ids sueltos en la raíz). `deduction` aparece al aplicar; `payroll` solo cuando el cobro ya quedó en una nómina.

Otros tipos de sanción (amonestación, etc.) **no** impactan nómina; no requieren `payroll_id` en el request ni generan `deduction`/`payroll` en el detalle.

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
