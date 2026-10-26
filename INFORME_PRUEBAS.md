# Informe de pruebas de los programas

**Materia:** Lenguajes de Programación  
**Responsable:** Julio Cossio  
**Fecha de ejecución:** 23/10/2026 (hora de Bolivia)  
**Herramienta:** .NET SDK 10.0.402  
**Estado:** las siete suites integradas pasan y se realizaron los casos manuales.  
**Alcance:** se encontraron y probaron siete archivos C# independientes.

## Archivos probados y resultado de primera pasada

| Archivo | Compilación | Prueba interna ejecutada | Resultado obtenido | Evidencia |
|---|---|---|---|---|
| `Programa1_CrearCliente.cs` | Correcta | Opción 3 | 13/13 correctas | [`compilación`](_pruebas_dotnet/evidencia/PruebaPrograma1_compilacion.txt) · [`pruebas`](_pruebas_dotnet/evidencia/PruebaPrograma1_prueba_interna.txt) |
| `Programa2_RegistrarProducto.cs` | Correcta, con advertencias C# | Opción 3 | 12/12 correctas | [`compilación`](_pruebas_dotnet/evidencia/PruebaPrograma2_compilacion.txt) · [`pruebas`](_pruebas_dotnet/evidencia/PruebaPrograma2_prueba_interna.txt) |
| `Programa3_EntradasSalidasPEPS.cs` | Correcta, con advertencias C# | Opción 5 | **10/12; 2 fallas PEPS** | [`compilación`](_pruebas_dotnet/evidencia/PruebaPrograma3_compilacion.txt) · [`pruebas`](_pruebas_dotnet/evidencia/PruebaPrograma3_prueba_interna.txt) |
| `Programa4_Alertas.cs` | **Fallida** | No ejecutable | Error CS0103: `BuscarProducto` no existe en el contexto actual, línea 193 | [`compilación`](_pruebas_dotnet/evidencia/PruebaPrograma4_compilacion.txt) |
| `Programa5_Presupuesto.cs` | Correcta, con advertencias C# | Opción 4 | 19/19 correctas | [`compilación`](_pruebas_dotnet/evidencia/PruebaPrograma5_compilacion.txt) · [`pruebas`](_pruebas_dotnet/evidencia/PruebaPrograma5_prueba_interna.txt) |
| `Programa6_Reservas.cs` | Correcta, con advertencias C# | Opción 5 | 14/14 correctas | [`compilación`](_pruebas_dotnet/evidencia/PruebaPrograma6_compilacion.txt) · [`pruebas`](_pruebas_dotnet/evidencia/PruebaPrograma6_prueba_interna.txt) |
| `Programa7_CodigoBarrasFactura.cs` | Correcta, con advertencias C# | Opción 4 | 26/26 correctas | [`compilación`](_pruebas_dotnet/evidencia/PruebaPrograma7_compilacion.txt) · [`pruebas`](_pruebas_dotnet/evidencia/PruebaPrograma7_prueba_interna.txt) |

## Repetición después de corregir

| Archivo | Corrección aplicada | Compilación y resultado repetido | Evidencia |
|---|---|---|---|
| `Programa3_EntradasSalidasPEPS.cs` | La salida consume lotes por fecha de ingreso, siguiendo PEPS como comprueba la suite. | Compilación correcta; opción 5: 12/12 correctas | [`recompilación`](_pruebas_dotnet/evidencia/PruebaPrograma3_recompilacion.txt) · [`repetición`](_pruebas_dotnet/evidencia/PruebaPrograma3_reprueba.txt) |
| `Programa4_Alertas.cs` | Se agregó `BuscarProducto`, que compara referencias sin distinguir mayúsculas. | Compilación correcta; opción 4: 9/9 correctas | [`recompilación`](_pruebas_dotnet/evidencia/PruebaPrograma4_recompilacion.txt) · [`repetición`](_pruebas_dotnet/evidencia/PruebaPrograma4_reprueba.txt) |

## Revalidación de los siete programas

Después de las correcciones se recompilaron y ejecutaron nuevamente todos los programas. Todos compilaron y todas las suites integradas pasaron:

| Archivo | Suite ejecutada | Resultado | Evidencia de compilación | Evidencia de prueba |
|---|---:|---:|---|---|
| Programa 1 — `Programa1_CrearCliente.cs` | Opción 3 | 13/13 | [`log`](_pruebas_dotnet/evidencia/PruebaPrograma1_revalidacion_compilacion.txt) | [`log`](_pruebas_dotnet/evidencia/PruebaPrograma1_revalidacion_prueba.txt) |
| Programa 2 — `Programa2_RegistrarProducto.cs` | Opción 3 | 12/12 | [`log`](_pruebas_dotnet/evidencia/PruebaPrograma2_revalidacion_compilacion.txt) | [`log`](_pruebas_dotnet/evidencia/PruebaPrograma2_revalidacion_prueba.txt) |
| Programa 3 — `Programa3_EntradasSalidasPEPS.cs` | Opción 5 | 12/12 | [`log`](_pruebas_dotnet/evidencia/PruebaPrograma3_revalidacion_compilacion.txt) | [`log`](_pruebas_dotnet/evidencia/PruebaPrograma3_revalidacion_prueba.txt) |
| Programa 4 — `Programa4_Alertas.cs` | Opción 4 | 9/9 | [`log`](_pruebas_dotnet/evidencia/PruebaPrograma4_revalidacion_compilacion.txt) | [`log`](_pruebas_dotnet/evidencia/PruebaPrograma4_revalidacion_prueba.txt) |
| Programa 5 — `Programa5_Presupuesto.cs` | Opción 4 | 19/19 | [`log`](_pruebas_dotnet/evidencia/PruebaPrograma5_revalidacion_compilacion.txt) | [`log`](_pruebas_dotnet/evidencia/PruebaPrograma5_revalidacion_prueba.txt) |
| Programa 6 — `Programa6_Reservas.cs` | Opción 5 | 14/14 | [`log`](_pruebas_dotnet/evidencia/PruebaPrograma6_revalidacion_compilacion.txt) | [`log`](_pruebas_dotnet/evidencia/PruebaPrograma6_revalidacion_prueba.txt) |
| Programa 7 — `Programa7_CodigoBarrasFactura.cs` | Opción 4 | 26/26 | [`log`](_pruebas_dotnet/evidencia/PruebaPrograma7_revalidacion_compilacion.txt) | [`log`](_pruebas_dotnet/evidencia/PruebaPrograma7_revalidacion_prueba.txt) |

Las pruebas integradas anteriores son pruebas automatizadas incluidas en el código; **no equivalen a ejecutar todos los datos de límite solicitados**. Las advertencias de compilación son principalmente sobre valores que podrían ser nulos; están registradas en los archivos de compilación y no impidieron ejecutar esos programas.

## Casos solicitados: estado de ejecución

| Archivo | Dato usado / escenario de la tarjeta | Resultado esperado | Resultado obtenido | Evidencia |
|---|---|---|---|---|
| Programa1 | Datos de cliente válidos e inválidos, campos vacíos y duplicados | Acepta el registro válido y rechaza entradas inválidas | La captura muestra al cliente `pepito` creado y listado | [Captura Programa 1](Pruebas_manuales/Pruebas_programa1.png) · suite [13/13](<./_pruebas_dotnet/evidencia/PruebaPrograma1_revalidacion_prueba.txt>) |
| Programa2 | Precio 0/negativo/límite; códigos de barras válidos, inválidos y duplicados | Rechaza datos fuera de reglas, sin agregar producto | La captura muestra el producto `BEB-005` creado con precio 10 y código de 12 dígitos | [Captura Programa 2](Pruebas_manuales/pruebas_programa2.png) · suite [12/12](<./_pruebas_dotnet/evidencia/PruebaPrograma2_revalidacion_prueba.txt>) |
| Programa3 | Cantidad 1/100.000/0/negativa; salida exacta y superior al stock | Acepta límites permitidos; rechaza inválidos; stock correcto tras PEPS | La captura muestra una entrada válida de 1 unidad y stock final 16; la suite repetida pasó 12/12 | [Captura Programa 3](Pruebas_manuales/puebas_programa3.png) · suite [12/12](<./_pruebas_dotnet/evidencia/PruebaPrograma3_revalidacion_prueba.txt>) |
| Programa4 | Stock 4/5/6; vencimiento a 4/5/6 días | Alerta para stock ≤5 y vencimiento dentro de 5 días, según regla documentada | La captura muestra simulación hasta 2026-10-24, lote vencido y alertas de stock; la suite repetida pasó 9/9 | [Captura Programa 4](Pruebas_manuales/pruebas_programa4.png) · suite [9/9](<./_pruebas_dotnet/evidencia/PruebaPrograma4_revalidacion_prueba.txt>) |
| Programa5 | Cantidad vacía/0/negativa; descuentos 0/100/fuera de rango; cantidades de 11 y 12 unidades | Rechaza entradas inválidas; descuento mayorista desde 12 unidades | La captura muestra un descuento de 100% aceptado y total Bs 0; la suite pasó 19/19 | [Captura A](Pruebas_manuales/prueba_programa5.png), [captura B](Pruebas_manuales/Pruebas_programa5.png) · suite [19/19](<./_pruebas_dotnet/evidencia/PruebaPrograma5_revalidacion_prueba.txt>) |
| Programa6 | Reserva válida y sobre stock; cancelación/liberación; plazos 1/365/fuera de rango | Reserva válida descuenta disponibilidad; cancelar libera unidades; entradas fuera de rango se rechazan | La captura muestra una reserva válida de 12 unidades y disponible 0 | [Captura Programa 6](Pruebas_manuales/pruebas_programa6.png) · suite [14/14](<./_pruebas_dotnet/evidencia/PruebaPrograma6_revalidacion_prueba.txt>) |
| Programa7 | Venta, líneas repetidas, stock insuficiente, pago insuficiente y cancelación | Registra ventas válidas; rechaza sin descontar stock al fallar | La captura muestra venta pagada, total Bs 15 y cambio Bs 5 | [Captura Programa 7](Pruebas_manuales/pruebas_programa7.png) · suite [26/26](<./_pruebas_dotnet/evidencia/PruebaPrograma7_revalidacion_prueba.txt>) |

## Incidencias, responsables y repetición

| ID | Incidencia | Responsable de corrección | Estado | Repetición |
|---|---|---|---|---|
| P-01 | La suite PEPS esperaba que tras retirar un lote completo quedaran 1 lote y 20 unidades; inicialmente obtuvo 2 lotes y 8 unidades. | Lenguajes de Programación (LP) | Corregida en código | Repetida: compilación correcta y opción 5, 12/12 correctas. Evidencia enlazada arriba |
| P-02 | `Programa4_Alertas.cs` no compilaba: `BuscarProducto` no estaba definido (CS0103, línea 193). | Lenguajes de Programación (LP) | Corregida en código | Repetida: compilación correcta y opción 4, 9/9 correctas. Evidencia enlazada arriba |

**Responsable de las correcciones registradas en este informe:** Lenguajes de Programación (LP).

## Evidencia y ejecución

Los logs de compilación y salida de consola están guardados en `_pruebas_dotnet/evidencia/`. Los proyectos usados están en `_pruebas_dotnet/`, uno por programa. La compilación usa proyectos consola independientes y .NET SDK 10.0.402.

Para ver cómo ejecutar cada programa, consulta la guía [`VALIDACIONES_Y_EJECUCION.md`](VALIDACIONES_Y_EJECUCION.md).

## Requisito para compilar y ejecutar

Se necesita tener instalado el **SDK de .NET** para compilar y ejecutar estos programas mediante el comando `dotnet`. Comprueba la instalación en PowerShell con:

```powershell
dotnet --version
```

Si el comando muestra una versión, el SDK está disponible en la terminal.
