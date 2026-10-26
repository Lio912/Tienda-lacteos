# Lenguajes de Programación

Programas de consola C# de la materia, responsable Julio Cossio. Cada archivo se compila y ejecuta en un proyecto independiente.

## Resultado de la última revalidación

Se recompilaron y ejecutaron las suites integradas de los siete programas con .NET SDK 10.0.402. Todas las compilaciones finalizaron correctamente y todas las suites pasaron:

| Archivo | Opción de pruebas | Resultado |
|---|---:|---:|
| `Programa1_CrearCliente.cs` | 3 | 13/13 correctas |
| `Programa2_RegistrarProducto.cs` | 3 | 12/12 correctas |
| `Programa3_EntradasSalidasPEPS.cs` | 5 | 12/12 correctas |
| `Programa4_Alertas.cs` | 4 | 9/9 correctas |
| `Programa5_Presupuesto.cs` | 4 | 19/19 correctas |
| `Programa6_Reservas.cs` | 5 | 14/14 correctas |
| `Programa7_CodigoBarrasFactura.cs` | 4 | 26/26 correctas |

Los logs de compilación y de cada suite están en [`_pruebas_dotnet/evidencia/`](<./_pruebas_dotnet/evidencia/>). Las capturas manuales aportadas están en [`Pruebas_manuales/`](<./Pruebas_manuales/>). El registro de casos, correcciones y repeticiones está en [`INFORME_PRUEBAS.md`](INFORME_PRUEBAS.md).

## Compilar y probar un programa

Abre PowerShell en esta carpeta. Por ejemplo, para el programa 1:

```powershell
Copy-Item .\Programa1_CrearCliente.cs .\_pruebas_dotnet\PruebaPrograma1\Program.cs -Force
dotnet build .\_pruebas_dotnet\PruebaPrograma1
dotnet run --project .\_pruebas_dotnet\PruebaPrograma1
```

En el menú, elige la opción de pruebas indicada en la tabla y luego `0` para salir. La guía [`VALIDACIONES_Y_EJECUCION.md`](VALIDACIONES_Y_EJECUCION.md) contiene el detalle de validaciones y proyectos.

> Se realizaron todos los casos manuales. Las capturas disponibles documentan ejemplos representativos; el detalle de lo que se ve en cada imagen está en `INFORME_PRUEBAS.md`.
