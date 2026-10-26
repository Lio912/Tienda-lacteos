// Lenguaje: C# (version no fijada en estos archivos; define el target en el proyecto de Visual Studio/.NET).
// =====================================================================
// PROGRAMA 4 - ALERTAS DE STOCK Y VENCIMIENTO
// Proyecto integrador: Tienda con ERP Dolibarr | Materia: Programación Estructurada | Tarjeta Trello: PE-01
// Módulo de Dolibarr: Productos (stock)
//
// Qué hace:
//   - Avisa los productos con stock BAJO: 5 unidades o menos.
//   - Avisa los lotes POR VENCER: faltan 5 días o menos para su vencimiento (o ya vencieron).
//   - Permite simular que pasan unos días para ver cómo cambian las alertas.
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

namespace Programa4_Alertas
{

    class Lote                          // cada entrada de mercadería es un lote
    {
        public int Cantidad;
        public string NumeroLote = "";
        public DateTime FechaIngreso;
        public DateTime FechaVence;     // DateTime.MaxValue = no vence
    }

    class Producto
    {
        public string Ref = "";         // BEB-001
        public string Nombre = "";
        public int StockReservado;
        public int UmbralStock = 5;
        public List<Lote> Lotes = new List<Lote>();   // en orden de llegada
    }

    class Program
    {
        // ---------- Reglas del negocio (AJUSTAR con el equipo si cambian) ----------
        const int STOCK_MINIMO = 5;             // alerta si el stock es 5 o menos
        const int DIAS_ALERTA = 5;              // alerta si vence dentro de 5 días
        static readonly DateTime SIN_VENCIMIENTO = DateTime.MaxValue;

        // Fecha del sistema fija para que las pruebas siempre den el mismo resultado.
        // Para usar la fecha real cambiar por: DateTime.Today
        static DateTime hoy = new DateTime(2026, 10, 19);

        static List<Producto> productos = new List<Producto>();
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
                Console.WriteLine("===== PROGRAMA 4: ALERTAS =====");
                Console.WriteLine(" Fecha del sistema: " + hoy.ToString("yyyy-MM-dd"));
                Console.WriteLine(" 1. Ver alertas (stock bajo / por vencer)");
                Console.WriteLine(" 2. Ver productos, stock y lotes");
                Console.WriteLine(" 3. Simular que pasan unos días");
                Console.WriteLine(" 4. Ejecutar pruebas con resultados esperados");
                Console.WriteLine("  5. Configurar umbral de stock de un producto");
                Console.WriteLine(" 0. Salir");
                opcion = LeerEntero("Opcion", 0, 5);
                switch (opcion)
                {
                    case 1: MostrarAlertas(true); break;
                    case 2: MostrarStockYLotes(); break;
                    case 3: SimularDias(); break;
                    case 4: EjecutarPruebas(); break;
                    case 5: ConfigurarUmbral(); break;
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

        static Producto BuscarProducto(string referencia)
        {
            if (string.IsNullOrWhiteSpace(referencia)) return null;
            foreach (Producto p in productos)
            {
                if (string.Equals(p.Ref, referencia.Trim(), StringComparison.OrdinalIgnoreCase))
                    return p;
            }
            return null;
        }

        // ============================ REGLAS DE ALERTA ============================
        static bool EsStockBajo(int stock) { return stock <= STOCK_MINIMO; }

        static bool EsStockBajo(int stock, Producto p)
        {
            return stock <= p.UmbralStock;
        }

        static string EstadoSemaforo(Producto p)
        {
            int stock = StockTotal(p);
            bool vencido = false;
            foreach (Lote lote in p.Lotes)
                if (lote.Cantidad > 0 && lote.FechaVence != SIN_VENCIMIENTO && lote.FechaVence.Date < hoy.Date) vencido = true;
            if (stock == 0 || vencido) return "ROJO";
            return stock <= p.UmbralStock ? "AMARILLO" : "VERDE";
        }

        static void ConfigurarUmbral()
        {
            Console.WriteLine("--- Configurar umbral de stock ---");
            Console.Write("Referencia del producto: ");
            string referencia = Console.ReadLine();
            if (referencia == null) { Console.WriteLine("Entrada finalizada."); Environment.Exit(0); }
            Producto p = BuscarProducto(referencia.Trim());
            if (p == null) { Console.WriteLine("  Producto no encontrado; revisa la referencia."); return; }
            p.UmbralStock = LeerEntero("Umbral de stock (0 a 100.000)", 0, 100000);
            Console.WriteLine("  Umbral actualizado para " + p.Ref + ": " + p.UmbralStock + " unidades.");
        }

        static bool EsPorVencer(DateTime vence)
        {
            if (vence == SIN_VENCIMIENTO) return false;     // este lote no vence
            int dias = (vence - hoy).Days;                   // si es negativo, ya venció
            return dias <= DIAS_ALERTA;
        }

        // ============================ PROGRAMA PRINCIPAL ============================
        // Recorre todos los productos y lotes. Devuelve cuántas alertas encontró.
        // Si imprimir es false, solo cuenta (sirve para las pruebas).
        static int MostrarAlertas(bool imprimir)
        {
            if (imprimir) Console.WriteLine("\n--- Alertas ---");
            int total = 0;
            foreach (Producto p in productos)
            {
                int stock = StockTotal(p);
                if (EsStockBajo(stock, p))
                {
                    if (imprimir) Console.WriteLine("  [" + EstadoSemaforo(p) + "] " + p.Ref + " " + p.Nombre + ": " + stock + "/" + p.UmbralStock);
                    total++;
                }
                foreach (Lote l in p.Lotes)
                {
                    if (l.Cantidad > 0 && EsPorVencer(l.FechaVence))
                    {
                        int dias = (l.FechaVence - hoy).Days;
                        string estado = dias < 0 ? "VENCIDO" : "vence en " + dias + " día(s)";
                        if (imprimir) Console.WriteLine("  [VENCIMIENTO] " + p.Ref + " lote " + l.NumeroLote + " de " + l.Cantidad + " u., vence " + l.FechaVence.ToString("yyyy-MM-dd") + ": " + estado);
                        total++;
                    }
                }
            }
            if (imprimir && total == 0) Console.WriteLine("  Sin alertas.");
            return total;
        }

        static void SimularDias()
        {
            int n = LeerEntero("¿Cuántos días pasan?", 1, 365);
            hoy = hoy.AddDays(n);
            Console.WriteLine("  Fecha del sistema ahora: " + hoy.ToString("yyyy-MM-dd"));
            MostrarAlertas(true);
        }

        static void MostrarStockYLotes()
        {
            Console.WriteLine("\n--- Productos, stock y lotes ---");
            foreach (Producto p in productos)
            {
                Console.WriteLine("  [" + EstadoSemaforo(p) + "] " + p.Ref + " " + p.Nombre + " | stock: " + StockTotal(p) +
                                  " | umbral: " + p.UmbralStock + " | reservado: " + p.StockReservado + " | disponible: " + StockDisponible(p));
                int n = 1;
                foreach (Lote l in p.Lotes)
                {
                    string vence = (l.FechaVence == SIN_VENCIMIENTO) ? "no vence" : "vence " + l.FechaVence.ToString("yyyy-MM-dd");
                    Console.WriteLine("      lote " + n + ": " + l.Cantidad + " u. | ingreso " + l.FechaIngreso.ToString("yyyy-MM-dd") + " | " + vence);
                    n++;
                }
            }
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

        static void NuevoLote(Producto p, int cantidad, DateTime ingreso, DateTime vence)
        {
            Lote l = new Lote();
            l.Cantidad = cantidad;
            l.NumeroLote = "DEMO-" + p.Ref + "-" + (p.Lotes.Count + 1).ToString("00");
            l.FechaIngreso = ingreso;
            l.FechaVence = vence;
            p.Lotes.Add(l);
        }

        static void CargarDatosEjemplo()
        {
            hoy = new DateTime(2026, 10, 19);     // se vuelve a la fecha fija
            productos.Clear();

            // BEB-001 tiene dos lotes: el primero vence en 4 días (alerta de vencimiento)
            Producto chicha = NuevoProducto("BEB-001", "Chicha de maní con leche 1 L", 0);
            NuevoLote(chicha, 5, new DateTime(2026, 10, 15), hoy.AddDays(4));
            NuevoLote(chicha, 10, new DateTime(2026, 10, 18), new DateTime(2026, 11, 10));

            NuevoProducto("BEB-002", "Api con leche 1 L", 12);
            NuevoProducto("BEB-003", "Kéfir natural 1 L", 20);
            NuevoProducto("YOG-024", "Yogur bebible canela 1 L", 30);
            NuevoProducto("YOG-029", "Yogur bebible maracuyá 1 L", 150);
            NuevoProducto("LEC-011", "Leche frutilla 1 L", 60);
            NuevoProducto("LEC-002", "Leche entera 2 L", 40);
            NuevoProducto("HEL-006", "Paleta de leche unidad", 2);       // stock bajo
            NuevoProducto("LEC-013", "Leche en polvo 800 g", 0);         // stock bajo (agotado)
            NuevoProducto("YOG-009", "Yogur bebible mora 1 L", 2);       // stock bajo
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
            IniciarPruebas("PRUEBAS - ALERTAS");

            // --- Alertas con los datos de ejemplo: 3 de stock bajo + 1 por vencer ---
            Verificar("Cantidad de alertas", "4", MostrarAlertas(true).ToString());

            // --- Límites de la regla de stock (5 o menos) ---
            Verificar("Stock 5 -> hay alerta", "True", EsStockBajo(5).ToString());
            Verificar("Stock 6 -> no hay alerta", "False", EsStockBajo(6).ToString());
            Verificar("Stock 0 -> hay alerta", "True", EsStockBajo(0).ToString());

            // --- Límites de la regla de vencimiento (5 días o menos) ---
            Verificar("Vence en 5 días -> hay alerta", "True", EsPorVencer(hoy.AddDays(5)).ToString());
            Verificar("Vence en 6 días -> no hay alerta", "False", EsPorVencer(hoy.AddDays(6)).ToString());
            Verificar("Venció ayer -> hay alerta", "True", EsPorVencer(hoy.AddDays(-1)).ToString());
            Verificar("No vence nunca -> no hay alerta", "False", EsPorVencer(SIN_VENCIMIENTO).ToString());

            // --- Pasan 18 días (hoy = 2026-11-06): lote de BEB-001 vencido + otro lote que vence en 4 días ---
            hoy = hoy.AddDays(18);
            Verificar("Alertas 18 días después", "5", MostrarAlertas(false).ToString());

            TerminarPruebas();
            CargarDatosEjemplo();     // se vuelven a cargar los datos originales (y la fecha fija)
        }
    }
}
