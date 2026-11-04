## Aplicar Inasistencia (Recursos Humanos)

Endpoint para que Recursos Humanos confirme y aplique formalmente una inasistencia no justificada.

## Información General

| Campo | Valor |
|---|---|
| **Método** | `POST` |
| **Endpoint** | `/api/v1/companies/{company_id}/modules/{module_code}/absences/{absence_id}/apply` |
| **Descripción** | Pasa la inasistencia a estado `Applied`, calcula las deducciones legales de nómina (salario diario, 7mo día, viáticos) y registra una falta leve en el módulo disciplinario ejecutando el motor de escalamiento. |

---

## Parámetros de Ruta (Path Params)

| Parámetro | Tipo | Requerido | Descripción |
|:---:|:---:|---|---|
| `company_id` | `guid` | Sí | Identificador único de la compañía. |
| `module_code` | `string` | Sí | Código del módulo dentro de la compañía (ej. `PAYROLL`, `RRHH`). |
| `absence_id` | `guid` | Sí | Identificador único del registro de inasistencia a aplicar. |

---

## Headers

| Header | Valor | Requerido |
|---|---|---|
| `Authorization` | `Bearer {token}` | Sí |
| `Content-Type` | `application/json` | Sí |

---

## Request Body

El cuerpo es opcional. Si no se envía el catálogo de falta, el sistema buscará automáticamente en el catálogo disciplinario la falta leve correspondiente a **"Ausencia injustificada sin respaldo"** para la empresa.

### Opción 1: Automática (Recomendada)
```json
{}
```

### Opción 2: Especificando el catálogo de falta
```json
{
  "fault_catalog_id": "4fe60a26-fbda-4f0d-9de7-c5a9030b0780"
}
```

### Descripción de Campos

| Campo | Tipo | Requerido | Descripción |
|---|---|:---:|---|
| `fault_catalog_id` | `guid?` | No | ID del catálogo de falta disciplinaria (`FaultCatalog`). Si no se provee, se auto-selecciona `Ausencia injustificada sin respaldo` de severidad leve de la empresa. |

---

## Impacto y Reglas de Negocio

Al aplicar la inasistencia, el sistema realiza de forma transaccional:

1. **Cálculo de Deducción de Salario Ordinario:**
   $$\text{SalaryDeduction} = \frac{\text{Salario Base Mensual}}{30}$$
2. **Cálculo de Séptimo Día (Ley Laboral de Nicaragua):**
   $$\text{SeventhDayDeduction} = \frac{\text{Salario Diario}}{6}$$
3. **Cálculo de Viáticos Diarios Asignados:**
   $$\text{TravelExpensesDeduction} = \frac{\sum \text{Viáticos Quincenales Activos}}{15}$$
4. **Registro de Falta Leve Disciplinaria:**
   - Registra una entidad `DisciplinaryFault` (`Severity = Minor`, `Status = Open`).
   - Asocia el ID de la falta a `absence.disciplinary_fault_id`.
   - **Motor de Escalamiento:** Invoca `ExecuteEscalationProcess`. Si el colaborador acumula **3 faltas leves dentro del mismo mes**, el sistema genera automáticamente una **Falta Grave** (`FaultSeverity.Serious`, ej. código `FG-ALP-01` o `FG-AVS-01`) vinculando las tres faltas leves como antecedentes.
5. **Auditoría:** Registra `reviewed_by = {UserId}` y `reviewed_at = UtcNow`.

---

## Respuestas

### ✅ 200 OK

Al aplicarse con éxito, responde con código `200 OK`:

```http
HTTP/1.1 200 OK
```

### ❌ 400 Bad Request

```json
{
  "status": 400,
  "error": {
    "type_error": "BadRequest",
    "description": "Esta inasistencia ya ha sido aplicada previamente."
  },
  "created_at": "2026-10-29 16:00:00"
}
```

```json
{
  "status": 400,
  "error": {
    "type_error": "BadRequest",
    "description": "No se puede aplicar una inasistencia que se encuentra cancelada."
  },
  "created_at": "2026-10-29 16:00:00"
}
```

### ❌ 404 Not Found

```json
{
  "status": 404,
  "error": {
    "type_error": "NotFound",
    "description": "No se encontró el registro de inasistencia especificado."
  },
  "created_at": "2026-10-29 16:00:00"
}
```

```json
{
  "status": 404,
  "error": {
    "type_error": "NotFound",
    "description": "No se encontró en el catálogo de faltas la falta disciplinaria leve por ausencia injustificada para esta empresa."
  },
  "created_at": "2026-10-29 16:00:00"
}
```
