// Los tests de integración comparten un único contenedor PostgreSQL y la misma base de datos.
// Ejecutarlos en paralelo provocaría condiciones de carrera sobre los mismos datos.
[assembly: NonParallelizable]
