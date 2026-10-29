# Permisos

Flujo de políticas y solicitudes para cita judicial, matrimonio, nacimiento y fallecimiento. También cubre el listado, la aprobación y la cancelación de cualquier solicitud de permiso.

El JSON de la API usa `snake_case`. Los enums se pueden enviar como número o como nombre (`MarriageLeave`). En las respuestas salen como nombre.

La ruta de solicitudes usa `companie_id`. La ruta de políticas usa `company_id`.

## Orden de uso

1. [Opciones del formulario](GetPermitFormOptions.md)
2. [Registrar política](RegisterPermitTypePolicy.md)
3. [Listar políticas](GetPermitTypePolicies.md)
4. [Crear solicitud](CreatePermitApplication.md)
5. [Listar solicitudes](GetPermitApplications.md)
6. [Procesar solicitud](ProcessPermitApplication.md)
7. [Cancelar solicitud](CancelPermitApplication.md)

`PUT .../permit-applications/{permit_application_id}` existe en el controlador y responde `200` sin actualizar la solicitud.
