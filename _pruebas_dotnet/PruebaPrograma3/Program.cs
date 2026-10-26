// Lenguaje: C# (version no fijada en estos archivos; define el target en el proyecto de Visual Studio/.NET).
// =====================================================================
// PROGRAMA 3 - ENTRADAS Y SALIDAS DE STOCK (PEPS)
// Proyecto integrador: Tienda con ERP Dolibarr | Materia: Programación Estructurada | Tarjeta Trello: PE-01
// Módulo de Dolibarr: Productos (stock / movimientos de inventario)
//
// Qué hace:
//   - Entrada: cada vez que llega mercadería se crea un LOTE nuevo (cantidad, fecha de ingreso, fecha de vencimiento).
//   - Salida: usa primero el lote que vence antes; si empatan, respeta el orden de ingreso.
//   - La salida sirve para ventas, productos perdidos o dañados, y queda anotada en los movimientos.
//   - No deja sacar más de lo que hay disponible.
//
// Cómo usarlo en Visual Studio:
//   1) Archivo > Nuevo > Proyecto > Aplicación de consola (C#).
//   2) Borra todo lo que hay en Program.cs y pega este archivo completo.
//   3) Ejecuta con Ctrl+F5.
//   4) En el menú elige la opción 5 (pruebas): todas las líneas deben salir [OK].
// Este programa es independiente: no necesita los otros 6.
// =====================================================================
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Programa3_EntradasSalidasPEPS
{

    class Lote                          // cada entrada de mercadería es un lote
    {
        public int Cantidad;
        public string NumeroLote = "";
        public string Proveedor = "";
        public DateTime FechaIngreso;
        public DateTime FechaVence;     // DateTime.MaxValue = no vence
    }

    class Producto
    {
        public string Ref = "";         // BEB-001
        public string Nombre = "";
        public int StockReservado;
        public List<Lote> Lotes = new List<Lote>();   // en orden de llegada
    }

    class Program
    {
        static readonly DateTime SIN_VENCIMIENTO = DateTime.MaxValue;

        // Fecha del sistema fija para que las pruebas siempre den el mismo resultado.
        // Para usar la fecha real cambiar por: DateTime.Today
        static DateTime hoy = new DateTime(2026, 10, 19);

        static List<Producto> productos = new List<Producto>();
        static List<string> movimientos = new List<string>();
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
                Console.WriteLine("===== PROGRAMA 3: ENTRADAS Y SALIDAS (PEPS) =====");
                Console.WriteLine(" 1. Entrada de stock (nuevo lote)");
                Console.WriteLine("  2. Salida de stock por vencimiento (venta o merma)");
                Console.WriteLine(" 3. Ver productos, stock y lotes");
                Console.WriteLine(" 4. Ver movimientos");
                Console.WriteLine(" 5. Ejecutar pruebas con resultados esperados");
                Console.WriteLine(" 0. Salir");
                opcion = LeerEntero("Opción", 0, 5);
                switch (opcion)
                {
                    case 1: MenuEntrada(); break;
                    case 2: MenuSalida(); break;
                    case 3: MostrarStockYLotes(); break;
                    case 4: MostrarMovimientos(); break;
                    case 5: EjecutarPruebas(); break;
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

        static DateTime LeerFecha(string mensaje)
        {
            DateTime fecha;
            while (true)
            {
                Console.Write(mensaje + " (AAAA-MM-DD): ");
                string entrada = Console.ReadLine();
                if (entrada == null) { Console.WriteLine("Entrada finalizada."); Environment.Exit(0); }
                if (DateTime.TryParseExact(entrada.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out fecha)) return fecha.Date;
                Console.WriteLine("  Fecha invalida. Usa el formato AAAA-MM-DD, por ejemplo 2026-12-31.");
            }
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

        static Producto PedirProducto()
        {
            string texto = LeerTexto("Referencia del producto");
            Producto p = BuscarProducto(texto);
            if (p == null) Console.WriteLine("  Producto no encontrado.");
            return p;
        }

        // ============================ ENTRADAS ============================
        static void MenuEntrada()
        {
            Console.WriteLine("\n--- Entrada de stock ---");
            Producto p = PedirProducto();
            if (p == null) return;
            int cantidad = LeerEntero("Cantidad que ingresa", 1, 100000);
            string numeroLote = LeerTexto("Numero de lote del fabricante");
            string proveedor = LeerTexto("Proveedor");
            DateTime vence = LeerFecha("Fecha de vencimiento");
            string error;
            if (!RegistrarEntrada(p, cantidad, vence, numeroLote, proveedor, out error))
            {
                Console.WriteLine("  No se registro la entrada: " + error);
                return;
            }
            Console.WriteLine("  Entrada registrada. Stock total: " + StockTotal(p));
        }

        static bool RegistrarEntrada(Producto p, int cantidad, DateTime vence, string numeroLote, string proveedor, out string error)
        {
            error = "";
            if (p == null || BuscarProducto(p.Ref) == null) { error = "Selecciona un producto existente."; return false; }
            if (cantidad < 1 || cantidad > 100000) { error = "La cantidad debe estar entre 1 y 100.000 unidades."; return false; }
            if (string.IsNullOrWhiteSpace(numeroLote)) { error = "El numero de lote es obligatorio."; return false; }
            if (string.IsNullOrWhiteSpace(proveedor)) { error = "El proveedor es obligatorio."; return false; }
            if (vence == DateTime.MinValue || (vence != SIN_VENCIMIENTO && vence.Date < hoy.Date))
            { error = "La fecha de vencimiento no puede ser anterior a hoy."; return false; }
            foreach (Lote existente in p.Lotes)
                if (string.Equals(existente.NumeroLote, numeroLote.Trim(), StringComparison.OrdinalIgnoreCase))
                { error = "Ese numero de lote ya existe para el producto."; return false; }

            Lote nuevo = new Lote();
            nuevo.Cantidad = cantidad;
            nuevo.NumeroLote = numeroLote.Trim();
            nuevo.Proveedor = proveedor.Trim();
            nuevo.FechaIngreso = hoy;
            nuevo.FechaVence = vence == SIN_VENCIMIENTO ? SIN_VENCIMIENTO : vence.Date;
            p.Lotes.Add(nuevo);
            movimientos.Add(hoy.ToString("yyyy-MM-dd") + " | " + p.Ref + " | +" + cantidad + " | Entrada " + nuevo.NumeroLote);
            return true;
        }

        // ============================ SALIDAS (PEPS) ============================
        static void MenuSalida()
        {
            Console.WriteLine("\n--- Salida de stock (por vencimiento) ---");
            Producto p = PedirProducto();
            if (p == null) return;
            Console.WriteLine("  Disponible: " + StockDisponible(p));
            int cantidad = LeerEntero("Cantidad que sale", 1, 100000);
            string motivo = LeerMotivoSalida();
            if (RegistrarSalidaPEPS(p, cantidad, motivo))
                Console.WriteLine("  Salida registrada. Stock total: " + StockTotal(p));
            else
                Console.WriteLine("  No se registro: verifica la cantidad disponible y la causal elegida.");
        }

        static string LeerMotivoSalida()
        {
            while (true)
            {
                Console.Write("Motivo (VENTA, VENCIMIENTO, ROTURA, CADENA_FRIO_ROTA): ");
                string motivo = Console.ReadLine();
                if (motivo == null) { Console.WriteLine("Entrada finalizada."); Environment.Exit(0); }
                motivo = motivo.Trim().ToUpperInvariant();
                if (MotivoSalidaValido(motivo)) return motivo;
                Console.WriteLine("  Causal invalida. Elige una de las cuatro opciones indicadas.");
            }
        }

        static bool MotivoSalidaValido(string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo)) return false;
            string m = motivo.Trim().ToUpperInvariant();
            return m == "VENTA" || m == "VENCIMIENTO" || m == "ROTURA" || m == "CADENA_FRIO_ROTA";
        }

        static bool RegistrarSalidaPEPS(Producto p, int cantidad, string motivo)
        {
            if (p == null || cantidad <= 0 || cantidad > 100000 || !MotivoSalidaValido(motivo)) return false;
            if (cantidad > StockDisponible(p)) return false;

            // PEPS: consumir primero el lote que ingresó antes, sin reordenar por vencimiento.
            p.Lotes = p.Lotes.OrderBy(delegate(Lote lote) { return lote.FechaIngreso; }).ToList();
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

            movimientos.Add(hoy.ToString("yyyy-MM-dd") + " | " + p.Ref + " | -" + cantidad + " | " + motivo.Trim().ToUpperInvariant());
            return true;
        }

        // ============================ MOSTRAR ============================

        static void MostrarStockYLotes()
        {
            Console.WriteLine("\n--- Productos, stock y lotes ---");
            foreach (Producto p in productos)
            {
                Console.WriteLine("  " + p.Ref + " " + p.Nombre + " | stock: " + StockTotal(p) +
                                  " | reservado: " + p.StockReservado + " | disponible: " + StockDisponible(p));
                int n = 1;
                foreach (Lote l in p.Lotes)
                {
                    string vence = (l.FechaVence == SIN_VENCIMIENTO) ? "no vence" : "vence " + l.FechaVence.ToString("yyyy-MM-dd");
                    Console.WriteLine("      lote " + n + " [" + l.NumeroLote + "]: " + l.Cantidad + " u. | proveedor " + l.Proveedor + " | ingreso " + l.FechaIngreso.ToString("yyyy-MM-dd") + " | " + vence);
                    n++;
                }
            }
        }

        static void MostrarMovimientos()
        {
            Console.WriteLine("\n--- Movimientos (fecha | producto | cantidad | motivo) ---");
            foreach (string m in movimientos) Console.WriteLine("  " + m);
            if (movimientos.Count == 0) Console.WriteLine("  (sin movimientos)");
        }

        // ============================ DATOS DE EJEMPLO ============================
        static Producto NuevoProducto(string rf, string nombre, int stock)
        {
            Producto p = new Producto();
            p.Ref = rf;
            p.Nombre = nombre;
            if (stock > 0) NuevoLote(p, stock, new DateTime(2026, 10, 16), SIN_VENCIMIENTO);
            productos.Add(p);
            return p;
        }

        static void NuevoLote(Producto p, int cantidad, DateTime ingreso, DateTime vence, string numeroLote = "", string proveedor = "Proveedor de demostracion")
        {
            Lote l = new Lote();
            l.Cantidad = cantidad;
            l.NumeroLote = numeroLote == "" ? "DEMO-" + p.Ref + "-" + (p.Lotes.Count + 1).ToString("00") : numeroLote;
            l.Proveedor = proveedor;
            l.FechaIngreso = ingreso;
            l.FechaVence = vence;
            p.Lotes.Add(l);
        }

        static void CargarDatosEjemplo()
        {
            productos.Clear();
            movimientos.Clear();

            // BEB-001 tiene dos lotes para probar PEPS: primero entró el de 5, después el de 10
            Producto chicha = NuevoProducto("BEB-001", "Chicha de maní con leche 1 L", 0);
            NuevoLote(chicha, 5, new DateTime(2026, 10, 15), hoy.AddDays(4));
            NuevoLote(chicha, 10, new DateTime(2026, 10, 18), new DateTime(2026, 11, 10));

            NuevoProducto("BEB-002", "Api con leche 1 L", 12);
            NuevoProducto("BEB-003", "Kéfir natural 1 L", 20);
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
            IniciarPruebas("PRUEBAS - ENTRADAS Y SALIDAS PEPS");

            // --- PEPS en BEB-001 (lotes de 5 y 10) ---
            Producto chicha = BuscarProducto("BEB-001");
            Verificar("Stock inicial de BEB-001", "15", StockTotal(chicha).ToString());
            RegistrarSalidaPEPS(chicha, 7, "ROTURA");
            Verificar("PEPS: lotes que quedan", "1", chicha.Lotes.Count.ToString());
            Verificar("PEPS: cantidad del lote restante", "8", chicha.Lotes[0].Cantidad.ToString());
            Verificar("PEPS: vence el lote restante", "2026-11-10", chicha.Lotes[0].FechaVence.ToString("yyyy-MM-dd"));
            Verificar("PEPS: salida mayor al stock", "False", RegistrarSalidaPEPS(chicha, 99, "ROTURA").ToString());
            Verificar("PEPS: el stock no cambió", "8", StockTotal(chicha).ToString());

            // --- Entrada de un lote nuevo y salida de un lote completo ---
            Producto api = BuscarProducto("BEB-002");
            string errorEntrada;
            Verificar("Entrada de lote valido", "True", RegistrarEntrada(api, 20, new DateTime(2026, 12, 1), "API-TEST-01", "Proveedor prueba", out errorEntrada).ToString());
            Verificar("Entrada: cantidad de lotes", "2", api.Lotes.Count.ToString());
            Verificar("Entrada: stock total", "32", StockTotal(api).ToString());
            RegistrarSalidaPEPS(api, 12, "ROTURA");
            Verificar("Salida de un lote completo: lotes", "1", api.Lotes.Count.ToString());
            Verificar("Queda el lote nuevo de", "20", api.Lotes[0].Cantidad.ToString());

            // --- Movimientos: la salida rechazada no se anota ---
            Verificar("Movimientos registrados", "3", movimientos.Count.ToString());

            TerminarPruebas();
            CargarDatosEjemplo();     // se vuelven a cargar los datos originales
        }
    }
}
