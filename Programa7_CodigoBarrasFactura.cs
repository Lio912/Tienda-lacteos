// Lenguaje: C# (version no fijada en estos archivos; define el target en el proyecto de Visual Studio/.NET).
// =====================================================================
// PROGRAMA 7 - LECTURA DE CÓDIGO DE BARRAS Y FACTURA
// Proyecto integrador: Tienda con ERP Dolibarr | Materia: Programación Estructurada | Tarjeta Trello: PE-01
// Módulo de Dolibarr: Finanzas (facturas) / TakePOS
//
// Qué hace:
//   - Modo escáner: se escribe (o escanea) el código de barras de cada producto; si el mismo código
//     se lee otra vez, se SUMA la cantidad en la misma línea en vez de crear otra.
//   - Calcula base imponible e IVA; el descuento mayorista de demostracion inicia en 12 unidades.
//   - Acepta efectivo o confirmacion manual de QR; rechaza efectivo insuficiente antes de guardar.
//   - Valida todas las lineas y descuenta primero el lote que vence antes.
//
// Cómo usarlo en Visual Studio:
//   1) Archivo > Nuevo > Proyecto > Aplicación de consola (C#).
//   2) Borra todo lo que hay en Program.cs y pega este archivo completo.
//   3) Ejecuta con Ctrl+F5.
//   4) En el menú elige la opción 4 (pruebas): todas las líneas deben salir [OK].
// Este programa es independiente: no necesita los otros 6.
// =====================================================================
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Programa7_CodigoBarrasFactura
{
    class Tercero
    {
        public string Codigo = "";      // CU2610-00102
        public string Nombre = "";
        public string Telefono = "";
        public string Email = "";
        public string Direccion = "";
        public bool Activo = true;
    }

    class Lote                          // cada entrada de mercadería es un lote
    {
        public int Cantidad;
        public DateTime FechaIngreso;
        public DateTime FechaVence;     // DateTime.MaxValue = no vence
    }

    class Producto
    {
        public string Ref = "";         // BEB-001
        public string Nombre = "";
        public decimal PrecioSinIva;
        public string CodigoBarras = "";
        public int StockReservado;
        public List<Lote> Lotes = new List<Lote>();   // en orden de llegada
    }

    class LineaDoc                      // línea de presupuesto o de factura
    {
        public string RefProducto = "";
        public string Descripcion = "";
        public decimal PrecioUnit;      // sin IVA
        public int Cantidad;
        public decimal DescuentoPct;    // 0 a 100
        public decimal BaseLinea;       // PrecioUnit * Cantidad - descuento
    }

    class Factura
    {
        public string Ref = "";         // FA2610-0007
        public Tercero Cliente;
        public DateTime Fecha;
        public string Estado = "";
        public string MetodoPago = "";
        public List<LineaDoc> Lineas = new List<LineaDoc>();
        public decimal BaseImponible;
        public decimal Iva;
        public decimal Total;
        public decimal Pagado;
        public decimal RestaPorPagar;
        public decimal Cambio;
    }

    class Program
    {
        // ---------- Reglas del negocio (AJUSTAR con el equipo si cambian) ----------
        const decimal IVA = 0.13m;              // 13 %
        const int MIN_MAYORISTA = 12;           // cantidad mínima para precio mayorista
        const decimal DTO_MAYORISTA = 10m;      // % de descuento mayorista
        static readonly DateTime SIN_VENCIMIENTO = DateTime.MaxValue;

        // Fecha del sistema fija para que las pruebas siempre den el mismo resultado.
        // Para usar la fecha real cambiar por: DateTime.Today
        static DateTime hoy = new DateTime(2026, 10, 19);

        static List<Tercero> terceros = new List<Tercero>();
        static List<Producto> productos = new List<Producto>();
        static List<Factura> facturas = new List<Factura>();
        static List<string> movimientos = new List<string>();
        static int sigFactura = 7;              // la siguiente factura será FA2610-0007
        static int pruebasOk = 0;
        static int pruebasTotal = 0;

        static void Main()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;   // punto decimal
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            CargarDatosEjemplo();

            int opcion;
            do
            {
                Console.WriteLine();
                Console.WriteLine("===== PROGRAMA 7: CÓDIGO DE BARRAS Y FACTURA =====");
                Console.WriteLine(" 1. Vender con lector de código de barras");
                Console.WriteLine(" 2. Ver productos, códigos y stock");
                Console.WriteLine(" 3. Ver facturas emitidas");
                Console.WriteLine(" 4. Ejecutar pruebas con resultados esperados");
                Console.WriteLine(" 0. Salir");
                opcion = LeerEntero("Opción", 0, 4);
                switch (opcion)
                {
                    case 1: VenderConEscaner(); break;
                    case 2: ListarProductos(); break;
                    case 3: ListarFacturas(); break;
                    case 4: EjecutarPruebas(); break;
                }
            } while (opcion != 0);

            Console.WriteLine("Hasta luego.");
        }

        // =================== LECTURA Y VALIDACIÓN DE DATOS ===================
        static string LeerTexto(string mensaje)
        {
            string t = "";
            while (t == "")
            {
                Console.Write(mensaje + ": ");
                t = Console.ReadLine();
                if (t == null) { Console.WriteLine("Entrada finalizada."); Environment.Exit(0); }
                t = t.Trim();
            }
            return t;
        }

        static string LeerTextoOpcional(string mensaje)
        {
            Console.Write(mensaje + " (Enter para omitir): ");
            string t = Console.ReadLine();
            if (t == null) { Console.WriteLine("Entrada finalizada."); Environment.Exit(0); }
            return t.Trim();
        }

        static int LeerEntero(string mensaje, int min, int max)
        {
            while (true)
            {
                Console.Write(mensaje + " (" + min + " a " + max + "): ");
                string entrada = Console.ReadLine();
                if (entrada == null) { Console.WriteLine("Entrada finalizada."); Environment.Exit(0); }
                int numero;
                if (int.TryParse(entrada, NumberStyles.Integer, CultureInfo.InvariantCulture, out numero) && numero >= min && numero <= max)
                    return numero;
                Console.WriteLine("  Escribe un numero entero entre " + min + " y " + max + ".");
            }
        }

        static decimal LeerDecimal(string mensaje, decimal min, decimal max)
        {
            while (true)
            {
                Console.Write(mensaje + ": ");
                string entrada = Console.ReadLine();
                if (entrada == null) { Console.WriteLine("Entrada finalizada."); Environment.Exit(0); }
                decimal numero;
                if (decimal.TryParse(entrada, NumberStyles.Number, CultureInfo.InvariantCulture, out numero) && numero >= min && numero <= max)
                    return numero;
                Console.WriteLine("  Usa punto decimal y un valor entre " + min + " y " + max + ".");
            }
        }

        static bool LeerSiNo(string mensaje)
        {
            while (true)
            {
                Console.Write(mensaje + " (s/n): ");
                string r = Console.ReadLine();
                if (r == null) { Console.WriteLine("Entrada finalizada."); Environment.Exit(0); }
                if (r != null)
                {
                    r = r.Trim().ToLower();
                    if (r == "s") return true;
                    if (r == "n") return false;
                }
                Console.WriteLine("  Responde s o n.");
            }
        }

        // ============================ CÁLCULOS ============================
        static decimal Redondear(decimal valor)
        {
            return Math.Round(valor, 2, MidpointRounding.AwayFromZero);
        }

        static decimal ConIva(decimal sinIva)
        {
            return Redondear(sinIva * (1 + IVA));
        }

        static string Bs(decimal valor)
        {
            return valor.ToString("0.00") + " Bs";
        }

        static int StockTotal(Producto p)
        {
            int suma = 0;
            foreach (Lote l in p.Lotes) suma += l.Cantidad;
            return suma;
        }

        static int StockDisponible(Producto p)
        {
            return StockTotal(p) - p.StockReservado;
        }

        // ============================ BÚSQUEDAS ============================
        static Producto BuscarProducto(string refProd)
        {
            foreach (Producto p in productos)
            {
                if (string.Equals(p.Ref, refProd, StringComparison.OrdinalIgnoreCase))
                    return p;
            }
            return null;
        }

        static Producto BuscarProductoPorCodigo(string texto)   // referencia o código de barras
        {
            foreach (Producto p in productos)
            {
                if (string.Equals(p.Ref, texto, StringComparison.OrdinalIgnoreCase) ||
                    (p.CodigoBarras != "" && p.CodigoBarras == texto))
                    return p;
            }
            return null;
        }

        static Producto PedirProducto()
        {
            string texto = LeerTexto("Referencia o código de barras");
            Producto p = BuscarProductoPorCodigo(texto);
            if (p == null) Console.WriteLine("  Producto no encontrado.");
            return p;
        }

        static Tercero BuscarTercero(string texto)      // por código o por nombre
        {
            foreach (Tercero t in terceros)
            {
                if (string.Equals(t.Codigo, texto, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(t.Nombre, texto, StringComparison.OrdinalIgnoreCase))
                    return t;
            }
            return null;
        }

        static Tercero PedirCliente()
        {
            string texto = LeerTexto("Código o nombre del cliente");
            Tercero t = BuscarTercero(texto);
            if (t == null) Console.WriteLine("  Cliente no encontrado.");
            return t;
        }

        static void NuevoTercero(string codigo, string nombre, string tel, string email, string dir)
        {
            Tercero t = new Tercero();
            t.Codigo = codigo;
            t.Nombre = nombre;
            t.Telefono = tel;
            t.Email = email;
            t.Direccion = dir;
            terceros.Add(t);
        }

        static void CargarTerceros()
        {
            terceros.Clear();
            NuevoTercero("CU2610-00001", "Cliente genérico TakePOS", "", "", "");
            NuevoTercero("CU2610-00102", "Cliente Prueba Web", "70000000", "prueba.tienda@correo.example", "Av. Heroínas 100, Cochabamba, Bolivia");
            NuevoTercero("CU2610-00103", "Mark Flores", "+59161680120", "", "");
            NuevoTercero("CU2610-00050", "Guardería Pequeños Pasos", "", "", "");
        }

        // Descuento a aplicar: el mayor entre el manual y el mayorista (si llega a la cantidad mínima)
        static decimal CalcularDescuentoPct(int cantidad, decimal descuentoManual)
        {
            decimal dto = descuentoManual;
            if (cantidad >= MIN_MAYORISTA && dto < DTO_MAYORISTA) dto = DTO_MAYORISTA;
            if (dto < 0) dto = 0;
            if (dto > 100) dto = 100;
            return dto;
        }

        // Si el producto se repite (por ejemplo al leer dos veces el mismo código), se suman las cantidades
        static bool AgregarOSumarLinea(List<LineaDoc> lineas, Producto p, int cantidad)
        {
            if (lineas == null || p == null || cantidad <= 0 || cantidad > 100000) return false;
            foreach (LineaDoc l in lineas)
            {
                if (l.RefProducto == p.Ref)
                {
                    if (l.Cantidad > 100000 - cantidad) return false;
                    l.Cantidad += cantidad;
                    return true;
                }
            }
            LineaDoc nueva = new LineaDoc();
            nueva.RefProducto = p.Ref;
            nueva.Descripcion = p.Nombre;
            nueva.PrecioUnit = p.PrecioSinIva;
            nueva.Cantidad = cantidad;
            lineas.Add(nueva);
            return true;
        }

        static void RecalcularLineas(List<LineaDoc> lineas, decimal descuentoManual)
        {
            foreach (LineaDoc l in lineas)
            {
                l.DescuentoPct = CalcularDescuentoPct(l.Cantidad, descuentoManual);
                l.BaseLinea = Redondear(l.PrecioUnit * l.Cantidad * (1 - l.DescuentoPct / 100m));
            }
        }

        static void CalcularTotales(List<LineaDoc> lineas, out decimal baseImp, out decimal iva, out decimal total)
        {
            baseImp = 0m;
            foreach (LineaDoc l in lineas) baseImp += l.BaseLinea;
            baseImp = Redondear(baseImp);
            iva = Redondear(baseImp * IVA);
            total = Redondear(baseImp + iva);
        }

        static void MostrarLineas(List<LineaDoc> lineas)
        {
            Console.WriteLine("  {0,-9} {1,-30} {2,8} {3,5} {4,5} {5,10}", "Ref", "Descripción", "P.U.", "Cant", "Dto%", "Base");
            foreach (LineaDoc l in lineas)
            {
                Console.WriteLine("  {0,-9} {1,-30} {2,8:0.00} {3,5} {4,5:0.#} {5,10:0.00}",
                    l.RefProducto, l.Descripcion, l.PrecioUnit, l.Cantidad, l.DescuentoPct, l.BaseLinea);
            }
        }

        // Prioriza el vencimiento mas cercano; a igualdad, conserva primero el lote ingresado antes.
        static bool RegistrarSalidaPEPS(Producto p, int cantidad, string motivo)
        {
            if (p == null || cantidad <= 0 || cantidad > 100000 || string.IsNullOrWhiteSpace(motivo)) return false;
            if (cantidad > StockDisponible(p)) return false;
            p.Lotes = p.Lotes.OrderBy(delegate(Lote lote) { return lote.FechaVence; }).ThenBy(delegate(Lote lote) { return lote.FechaIngreso; }).ToList();

            int pendiente = cantidad;
            int i = 0;
            while (pendiente > 0 && i < p.Lotes.Count)
            {
                Lote l = p.Lotes[i];
                if (l.Cantidad <= pendiente)
                {
                    pendiente -= l.Cantidad;    // se consume todo el lote
                    l.Cantidad = 0;
                }
                else
                {
                    l.Cantidad -= pendiente;    // se consume solo una parte
                    pendiente = 0;
                }
                i++;
            }

            // quitar los lotes que quedaron vacíos (recorriendo de atrás hacia adelante)
            for (int k = p.Lotes.Count - 1; k >= 0; k--)
            {
                if (p.Lotes[k].Cantidad == 0) p.Lotes.RemoveAt(k);
            }

            movimientos.Add(hoy.ToString("yyyy-MM-dd") + " | " + p.Ref + " | -" + cantidad + " | " + motivo);
            return true;
        }

        static decimal CalcularCambio(decimal total, decimal pagado)
        {
            if (pagado <= total) return 0m;
            return Redondear(pagado - total);
        }

        static int CantidadEnLineas(List<LineaDoc> lineas, string refProd)
        {
            foreach (LineaDoc l in lineas)
            {
                if (l.RefProducto == refProd) return l.Cantidad;
            }
            return 0;
        }

        // ============================ PROGRAMA PRINCIPAL ============================
        // Lee códigos de barras uno por uno. Cada lectura suma 1 unidad; si el producto
        // ya está en la cuenta, se suma a la misma línea.
        static void VenderConEscaner()
        {
            Console.WriteLine("\n--- Venta con lector de código de barras ---");
            Tercero cliente = PedirCliente();
            if (cliente == null) return;

            List<LineaDoc> lineas = new List<LineaDoc>();
            Console.WriteLine("Escribe o escanea el código de barras (o la referencia). Escribe FIN para terminar.");
            while (true)
            {
                Console.Write("Código: ");
                string texto = Console.ReadLine();
                if (texto == null) { Console.WriteLine("Entrada finalizada."); return; }
                texto = texto.Trim();

                if (texto.ToUpper() == "FIN") break;
                if (texto == "") continue;

                Producto p = BuscarProductoPorCodigo(texto);
                if (p == null)
                {
                    Console.WriteLine("  Producto no encontrado.");
                    continue;
                }
                if (!AgregarOSumarLinea(lineas, p, 1))
                {
                    Console.WriteLine("  No se agrego: se alcanzo el maximo de 100.000 unidades por producto.");
                    continue;
                }
                Console.WriteLine("  + " + p.Nombre + " (cantidad en la cuenta: " + CantidadEnLineas(lineas, p.Ref) + ")");
            }

            if (lineas.Count == 0)
            {
                Console.WriteLine("  No se leyó ningún producto.");
                return;
            }

            decimal dto = LeerDecimal("Descuento manual % (0 si no hay)", 0m, 100m);
            int opcionPago = LeerEntero("Metodo de pago: 1 efectivo, 2 QR", 1, 2);
            string metodoPago = opcionPago == 1 ? "EFECTIVO" : "QR";
            decimal pagado = 0m;
            bool qrConfirmado = false;
            if (metodoPago == "EFECTIVO")
                pagado = LeerDecimal("Monto recibido en efectivo", 0.01m, 10000000m);
            else
            {
                Console.WriteLine("QR estatico: verifica el pago en la aplicacion bancaria antes de confirmar.");
                qrConfirmado = LeerSiNo("Confirmas que recibiste el pago QR?");
                if (!qrConfirmado) { Console.WriteLine("  Venta cancelada: confirma el pago o elige otro metodo."); return; }
            }

            string error;
            Factura f = EmitirFactura(cliente, lineas, dto, pagado, metodoPago, qrConfirmado, out error);
            if (f == null) Console.WriteLine("  No se pudo emitir: " + error);
            else MostrarFactura(f);
        }

        static Factura EmitirFactura(Tercero cliente, List<LineaDoc> lineas, decimal descuentoManual,
                                     decimal pagado, out string error)
        {
            return EmitirFactura(cliente, lineas, descuentoManual, pagado, "EFECTIVO", false, out error);
        }

        static Factura EmitirFactura(Tercero cliente, List<LineaDoc> lineas, decimal descuentoManual,
                                     decimal pagado, string metodoPago, bool qrConfirmado, out string error)
        {
            error = "";
            if (cliente == null || BuscarTercero(cliente.Codigo) == null) { error = "Selecciona un cliente registrado."; return null; }
            if (lineas == null || lineas.Count == 0) { error = "La factura no tiene productos."; return null; }
            if (descuentoManual < 0m || descuentoManual > 100m) { error = "El descuento debe estar entre 0 y 100%."; return null; }
            if (metodoPago != "EFECTIVO" && metodoPago != "QR") { error = "Selecciona efectivo o QR."; return null; }
            if (metodoPago == "EFECTIVO" && (pagado < 0m || pagado > 10000000m))
            { error = "El pago en efectivo debe ser mayor que 0 y no superar 10.000.000 Bs."; return null; }
            if (metodoPago == "QR" && !qrConfirmado) { error = "Confirma el pago QR antes de registrar la venta."; return null; }

            Dictionary<string, int> cantidades = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (LineaDoc linea in lineas)
            {
                if (linea == null || linea.Cantidad <= 0 || linea.Cantidad > 100000)
                { error = "Cada cantidad debe ser un entero entre 1 y 100.000."; return null; }
                Producto producto = BuscarProducto(linea.RefProducto);
                if (producto == null) { error = "Producto no encontrado: " + linea.RefProducto + "."; return null; }
                int acumulada;
                cantidades.TryGetValue(producto.Ref, out acumulada);
                if (acumulada > 100000 - linea.Cantidad)
                { error = "La cantidad acumulada supera 100.000 unidades para " + producto.Ref + "."; return null; }
                cantidades[producto.Ref] = acumulada + linea.Cantidad;
            }
            foreach (KeyValuePair<string, int> solicitud in cantidades)
            {
                Producto producto = BuscarProducto(solicitud.Key);
                if (producto == null || solicitud.Value > StockDisponible(producto))
                { error = "Stock insuficiente de " + solicitud.Key + ". Revisa la cantidad disponible."; return null; }
            }

            // Rebuild from the product catalog so duplicate input lines merge and prices cannot be forged.
            List<LineaDoc> consolidadas = new List<LineaDoc>();
            foreach (LineaDoc linea in lineas)
                AgregarOSumarLinea(consolidadas, BuscarProducto(linea.RefProducto), linea.Cantidad);
            lineas = consolidadas;
            RecalcularLineas(lineas, descuentoManual);
            Factura f = new Factura();
            f.Cliente = cliente;
            f.Fecha = hoy;
            f.Lineas = lineas;
            CalcularTotales(lineas, out f.BaseImponible, out f.Iva, out f.Total);
            if (metodoPago == "EFECTIVO" && pagado < f.Total)
            { error = "Pago insuficiente. Total a pagar: " + Bs(f.Total) + ". No se registro la venta."; return null; }

            f.Ref = "FA2610-" + sigFactura.ToString("0000");
            f.MetodoPago = metodoPago;
            f.Cambio = metodoPago == "EFECTIVO" ? CalcularCambio(f.Total, pagado) : 0m;
            f.Pagado = f.Total;
            f.RestaPorPagar = 0m;
            f.Estado = "Pagada";

            // El stock se modifica solo cuando todas las lineas y el pago son validos.
            foreach (KeyValuePair<string, int> solicitud in cantidades)
                if (!RegistrarSalidaPEPS(BuscarProducto(solicitud.Key), solicitud.Value, "Venta " + f.Ref))
                { error = "No se pudo descontar el stock de " + solicitud.Key + ". No se registro la factura."; return null; }

            sigFactura++;
            facturas.Add(f);
            return f;
        }

        // ============================ MOSTRAR / LISTAR ============================
        static void MostrarFactura(Factura f)
        {
            Console.WriteLine("\n=== FACTURA " + f.Ref + " ===");
            Console.WriteLine("Cliente: " + f.Cliente.Nombre);
            Console.WriteLine("Fecha: " + f.Fecha.ToString("yyyy-MM-dd") + " | Estado: " + f.Estado + " | Pago: " + f.MetodoPago);
            MostrarLineas(f.Lineas);
            Console.WriteLine("Base imponible: " + Bs(f.BaseImponible));
            Console.WriteLine("IVA 13%:        " + Bs(f.Iva));
            Console.WriteLine("TOTAL:          " + Bs(f.Total));
            Console.WriteLine("Pagado:         " + Bs(f.Pagado));
            Console.WriteLine("Resta por pagar:" + " " + Bs(f.RestaPorPagar));
            Console.WriteLine("CAMBIO:         " + Bs(f.Cambio));
        }

        static void ListarProductos()
        {
            Console.WriteLine("\n--- Productos, códigos de barras y stock ---");
            Console.WriteLine("  {0,-8} {1,-30} {2,-14} {3,8} {4,6}", "Ref", "Nombre", "Cód. barras", "Con IVA", "Stock");
            foreach (Producto p in productos)
                Console.WriteLine("  {0,-8} {1,-30} {2,-14} {3,8:0.00} {4,6}", p.Ref, p.Nombre, p.CodigoBarras, ConIva(p.PrecioSinIva), StockDisponible(p));
        }

        static void ListarFacturas()
        {
            Console.WriteLine("\n--- Facturas emitidas en esta sesión ---");
            foreach (Factura f in facturas)
                Console.WriteLine("  " + f.Ref + " | " + f.Cliente.Nombre + " | " + Bs(f.Total) + " | " + f.Estado);
            if (facturas.Count == 0) Console.WriteLine("  (sin facturas)");
        }

        static Producto NuevoProducto(string rf, string nombre, decimal precioSinIva, string barras, int stock)
        {
            Producto p = new Producto();
            p.Ref = rf;
            p.Nombre = nombre;
            p.PrecioSinIva = precioSinIva;
            p.CodigoBarras = barras;
            if (stock > 0) NuevoLote(p, stock, new DateTime(2026, 10, 16), SIN_VENCIMIENTO);
            productos.Add(p);
            return p;
        }

        static void NuevoLote(Producto p, int cantidad, DateTime ingreso, DateTime vence)
        {
            Lote l = new Lote();
            l.Cantidad = cantidad;
            l.FechaIngreso = ingreso;
            l.FechaVence = vence;
            p.Lotes.Add(l);
        }

        // ============================ DATOS DE EJEMPLO ============================
        static void CargarDatosEjemplo()
        {
            CargarTerceros();
            productos.Clear();
            movimientos.Clear();
            facturas.Clear();
            sigFactura = 7;

            // BEB-001 tiene dos lotes para ver el PEPS al vender: primero el de 5, después el de 10
            Producto chicha = NuevoProducto("BEB-001", "Chicha de maní con leche 1 L", 8.85m, "7770000000012", 0);
            NuevoLote(chicha, 5, new DateTime(2026, 10, 15), hoy.AddDays(4));
            NuevoLote(chicha, 10, new DateTime(2026, 10, 18), new DateTime(2026, 11, 10));

            NuevoProducto("BEB-002", "Api con leche 1 L", 7.96m, "7770000000029", 12);
            NuevoProducto("BEB-003", "Kéfir natural 1 L", 13.27m, "7770000000036", 20);
            NuevoProducto("YOG-024", "Yogur bebible canela 1 L", 10.62m, "7770000000821", 30);
            NuevoProducto("YOG-029", "Yogur bebible maracuyá 1 L", 10.62m, "7770000000838", 150);
            NuevoProducto("HEL-006", "Paleta de leche unidad", 2.65m, "7770000000869", 2);
        }

        // ============================ PRUEBAS ============================
        static void Verificar(string nombre, string esperado, string obtenido)
        {
            pruebasTotal++;
            bool ok = (esperado == obtenido);
            if (ok) pruebasOk++;
            Console.WriteLine((ok ? "  [OK]    " : "  [FALLA] ") + nombre + " -> esperado: " + esperado + " | obtenido: " + obtenido);
        }

        static void IniciarPruebas(string titulo)
        {
            Console.WriteLine("\n########## " + titulo + " ##########");
            pruebasOk = 0;
            pruebasTotal = 0;
        }

        static void TerminarPruebas()
        {
            Console.WriteLine("\nResultado: " + pruebasOk + " de " + pruebasTotal + " pruebas correctas.");
        }

        static void EjecutarPruebas()
        {
            CargarDatosEjemplo();     // se reinician los datos para que todo salga igual
            IniciarPruebas("PRUEBAS - CÓDIGO DE BARRAS Y FACTURA");
            string error;

            // --- Lectura de código de barras ---
            Producto leido = BuscarProductoPorCodigo("7770000000036");
            Verificar("Código de barras 7770000000036", "BEB-003", (leido == null) ? "null" : leido.Ref);
            Verificar("Código de barras que no existe", "True", (BuscarProductoPorCodigo("0000000000000") == null).ToString());

            // --- Factura de Mark Flores: 1 chicha + 1 kéfir, paga 30 Bs ---
            List<LineaDoc> lf = new List<LineaDoc>();
            AgregarOSumarLinea(lf, BuscarProducto("BEB-001"), 1);
            AgregarOSumarLinea(lf, BuscarProducto("BEB-003"), 1);
            Factura f = EmitirFactura(BuscarTercero("Mark Flores"), lf, 0m, 30m, out error);
            MostrarFactura(f);
            Verificar("Factura: referencia", "FA2610-0007", f.Ref);
            Verificar("Factura: base imponible", "22.12", f.BaseImponible.ToString("0.00"));
            Verificar("Factura: IVA", "2.88", f.Iva.ToString("0.00"));
            Verificar("Factura: total", "25.00", f.Total.ToString("0.00"));
            Verificar("Factura: cambio", "5.00", f.Cambio.ToString("0.00"));
            Verificar("Factura: estado", "Pagada", f.Estado);
            Verificar("Stock de BEB-003 tras vender 1", "19", StockTotal(BuscarProducto("BEB-003")).ToString());
            Verificar("PEPS: el lote más antiguo de BEB-001 baja a", "4", BuscarProducto("BEB-001").Lotes[0].Cantidad.ToString());

            // --- Producto repetido (se lee dos veces el mismo código) y factura sin pago ---
            List<LineaDoc> lr = new List<LineaDoc>();
            AgregarOSumarLinea(lr, BuscarProductoPorCodigo("7770000000821"), 1);
            AgregarOSumarLinea(lr, BuscarProductoPorCodigo("7770000000821"), 1);   // mismo código: se suma
            Verificar("Repetidos: líneas", "1", lr.Count.ToString());
            Verificar("Repetidos: cantidad", "2", lr[0].Cantidad.ToString());
            int stockAntesSinPago = StockTotal(BuscarProducto("YOG-024"));
            Factura f2 = EmitirFactura(BuscarTercero("Mark Flores"), lr, 0m, 0m, out error);
            Verificar("Sin pago: no registra venta", "True", (f2 == null).ToString());
            Verificar("Sin pago: mensaje claro", "True", error.Contains("Pago insuficiente").ToString());
            Verificar("Sin pago: conserva stock", stockAntesSinPago.ToString(), StockTotal(BuscarProducto("YOG-024")).ToString());

            // --- Pago insuficiente (1 api = 8.99, paga 5) ---
            List<LineaDoc> l3 = new List<LineaDoc>();
            AgregarOSumarLinea(l3, BuscarProducto("BEB-002"), 1);
            int stockAntesPagoInsuficiente = StockTotal(BuscarProducto("BEB-002"));
            Factura f3 = EmitirFactura(BuscarTercero("Mark Flores"), l3, 0m, 5m, out error);
            Verificar("Pago insuficiente: no registra venta", "True", (f3 == null).ToString());
            Verificar("Pago insuficiente: stock intacto", stockAntesPagoInsuficiente.ToString(), StockTotal(BuscarProducto("BEB-002")).ToString());

            // --- Precio mayorista en la factura: 24 yogures ---
            List<LineaDoc> l4 = new List<LineaDoc>();
            AgregarOSumarLinea(l4, BuscarProducto("YOG-029"), 24);
            Factura f4 = EmitirFactura(BuscarTercero("Guardería Pequeños Pasos"), l4, 0m, 300m, out error);
            Verificar("Mayorista: descuento de la línea", "10", f4.Lineas[0].DescuentoPct.ToString("0.##"));
            Verificar("Mayorista: base imponible", "229.39", f4.BaseImponible.ToString("0.00"));
            Verificar("Mayorista: total", "259.21", f4.Total.ToString("0.00"));
            Verificar("Mayorista: cambio de 300", "40.79", f4.Cambio.ToString("0.00"));

            // --- Casos que no deben facturar ---
            List<LineaDoc> l5 = new List<LineaDoc>();
            AgregarOSumarLinea(l5, BuscarProducto("HEL-006"), 3);     // solo hay 2
            Factura f5 = EmitirFactura(BuscarTercero("Mark Flores"), l5, 0m, 10m, out error);
            Verificar("Stock insuficiente: no factura", "True", (f5 == null).ToString());
            Verificar("Stock insuficiente: mensaje", "Stock insuficiente de HEL-006. Revisa la cantidad disponible.", error);
            Verificar("Stock insuficiente: el stock no cambia", "2", StockTotal(BuscarProducto("HEL-006")).ToString());
            Verificar("Sin cliente: no factura", "True", (EmitirFactura(null, lf, 0m, 0m, out error) == null).ToString());
            Verificar("Sin productos: no factura", "True", (EmitirFactura(BuscarTercero("Mark Flores"), new List<LineaDoc>(), 0m, 0m, out error) == null).ToString());

            TerminarPruebas();
            CargarDatosEjemplo();     // se vuelven a cargar los datos originales
        }
    }
}
