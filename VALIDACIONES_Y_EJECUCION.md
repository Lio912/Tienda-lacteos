# Guia de cambios y ejecucion

**Materia:** Lenguajes de Programacion  
**Responsable:** Julio Cossio

## Para que sirve esta guia

Se revisaron y modificaron los siete archivos C# de esta carpeta para que pidan nuevamente los datos incorrectos y expliquen el motivo. Cada archivo es un programa independiente. Esta guia indica que problema se encontro, que se cambio y como ejecutar cada programa.

## Que estaba mal y que se corrigio

| Archivo | Que estaba mal o faltaba | Que se cambio |
|---|---|---|
| `Programa1_CrearCliente.cs` | Se podia ingresar informacion de cliente con formato incorrecto o dejar datos necesarios vacios. | Se valida que los campos requeridos tengan contenido, que el telefono tenga formato numerico y que el correo tenga una estructura basica. Tambien se evita registrar el mismo cliente dos veces. |
| `Programa2_RegistrarProducto.cs` | El precio solo se comprobaba por el limite minimo. El codigo de barras podia no cumplir el formato esperado o repetirse. | El precio debe ser mayor que cero y no superar Bs 1.000.000. El codigo de barras, si se ingresa, debe tener 8, 12, 13 o 14 digitos y no estar ya registrado. |
| `Programa3_EntradasSalidasPEPS.cs` | Las entradas y salidas no comprobaban suficientemente la cantidad, el producto, el lote ni la fecha. Una salida podia pedir mas unidades que las disponibles. | Se comprueba que el producto exista, que la cantidad este entre 1 y 100.000, que lote y proveedor esten informados y que el lote no se repita para ese producto. La fecha debe existir, escribirse como `AAAA-MM-DD` y no ser anterior a hoy. La salida pide un motivo permitido y no se realiza si falta stock. |
| `Programa4_Alertas.cs` | La alerta de stock era limitada y no mostraba claramente los niveles ni los lotes proximos a vencer. | Se agrego un semaforo: rojo si no hay stock o hay lote vencido; amarillo si el stock esta en el umbral o por debajo; verde si lo supera. El umbral se puede cambiar. Tambien se muestran lotes que vencen dentro de los proximos cinco dias. |
| `Programa5_Presupuesto.cs` | Una cotizacion podia usar cantidades, productos o descuentos incorrectos. | Se valida que haya cliente y productos validos, que las cantidades sean positivas y que el descuento manual este entre 0 y 100%. Los datos del producto se obtienen del catalogo para no aceptar precios o descripciones alterados. El precio mayorista se aplica desde 12 unidades. |
| `Programa6_Reservas.cs` | Faltaban controles suficientes para cliente, producto, cantidad, stock y vencimiento de la reserva. | Se valida la reserva contra el stock disponible. Se puede indicar una vigencia de 1 a 365 dias. Al cancelar o vencer una reserva, se libera la cantidad reservada. |
| `Programa7_CodigoBarrasFactura.cs` | La venta podia tener cantidades invalidas o superar el stock; los productos repetidos podian hacer que la comprobacion de stock fuera insuficiente. | Se suman las cantidades repetidas y se comprueba el total antes de descontar stock. Se rechaza la venta si falta stock o el pago en efectivo no alcanza. El precio usado corresponde al producto del catalogo. Para QR, el programa solicita confirmacion manual. |

En los programas que venden o retiran mercaderia se evita cambiar el stock cuando la operacion se rechaza. Los calculos de negocio existentes se conservaron. El umbral mayorista se dejo en 12 unidades y se mantuvo el 10% que ya tenia el codigo; PE debe confirmar ese porcentaje para que todos los modulos apliquen la misma regla.

## Datos que se aceptan y se rechazan

| Dato | Se acepta | Se rechaza |
|---|---|---|
| Opcion de menu | Un numero entero que aparece como opcion | Letras, decimales o numeros fuera de las opciones. El programa vuelve a preguntar. |
| Cantidad | Numero entero de 1 a 100.000, sujeto al stock disponible | Cero, negativos, decimales, texto, mas de 100.000 o mas unidades que el stock. |
| Precio | Numero mayor que cero y hasta Bs 1.000.000 | Cero, negativos, texto o un monto mayor al limite. |
| Codigo de barras | Campo vacio o solo digitos con 8, 12, 13 o 14 caracteres, sin duplicados | Letras, espacios, otra cantidad de digitos o un codigo ya registrado. |
| Cliente y proveedor | Nombre con contenido | Campo obligatorio vacio; en cliente, tambien un registro duplicado. |
| Telefono y correo | Telefono de 7 a 15 digitos (puede comenzar con `+`); correo vacio o con estructura basica `usuario@dominio.tld` | Telefono con letras o longitud incorrecta; correo con espacios o sin la estructura indicada. |
| Fecha de lote | Fecha real en formato `AAAA-MM-DD`, hoy o posterior | Fecha imposible, otro formato o fecha anterior a hoy. |
| Motivo de salida | Venta, vencimiento, rotura o cadena de frio rota | Otro motivo o cantidad superior a la disponible. |
| Descuento | Numero entre 0% y 100% | Texto, porcentaje negativo o mayor a 100%. |
| Efectivo | Monto positivo, suficiente para cubrir el total y hasta Bs 10.000.000 | Monto no numerico, cero, negativo, mayor al limite o menor al total. La venta no se registra. |
| Pago QR | Confirmacion afirmativa en pantalla | Si no se confirma, la venta se cancela. El programa no verifica el pago con un banco. |
| Reserva | Cantidad disponible y vigencia de 1 a 365 dias | Cliente o producto faltante, cantidad no valida o plazo fuera del rango. |

Cuando un dato no cumple estas reglas, el programa muestra un mensaje y vuelve a pedirlo o cancela esa operacion sin aplicarla.

## Como ejecutar los archivos

Cada archivo tiene su propio menu y punto de inicio. Ejecuta **un archivo por proyecto**; no juntes los siete en un mismo proyecto.

### Opcion A: Visual Studio

1. Crea un proyecto nuevo de tipo **Aplicacion de consola C#**.
2. Abre el archivo `Program.cs` del proyecto y reemplaza su contenido por el de uno de los archivos de esta carpeta.
3. Ejecuta con **Ctrl+F5**.
4. Para usar otro programa, repite los pasos en otro proyecto con el siguiente archivo.

### Opcion B: .NET SDK desde PowerShell

Abre PowerShell en esta carpeta. Para ejecutar el programa 1, copia y pega:

```powershell
dotnet new console -n Programa1
Copy-Item .\Programa1_CrearCliente.cs .\Programa1\Program.cs
dotnet run --project .\Programa1
```

Para los demas archivos, usa una carpeta distinta para cada proyecto:

| Proyecto | Archivo que debes copiar |
|---|---|
| `Programa2` | `Programa2_RegistrarProducto.cs` |
| `Programa3` | `Programa3_EntradasSalidasPEPS.cs` |
| `Programa4` | `Programa4_Alertas.cs` |
| `Programa5` | `Programa5_Presupuesto.cs` |
| `Programa6` | `Programa6_Reservas.cs` |
| `Programa7` | `Programa7_CodigoBarrasFactura.cs` |

Por ejemplo, para el programa 2, cambia `Programa1` por `Programa2` y `Programa1_CrearCliente.cs` por `Programa2_RegistrarProducto.cs` en los tres comandos. Repite de la misma manera con los otros nombres de la tabla. Si `dotnet new console` pregunta si se debe sobrescribir un proyecto existente, usa un nombre de carpeta nuevo.

## Lo que esta entrega no integra

Los programas son ejemplos independientes y guardan los datos en memoria mientras estan ejecutandose. Todavia no comparten una base de datos ni estan conectados entre si, con Dolibarr, FastAPI o Electron. Por ejemplo, una cotizacion del programa 5 no crea automaticamente una reserva en el programa 6. Esa integracion corresponde coordinarla con los otros modulos.

Los archivos y esta guia se pueden compartir con LP-02 para pruebas y con ADS2-01 para integrar. Esta guia no envia los archivos por si misma.
