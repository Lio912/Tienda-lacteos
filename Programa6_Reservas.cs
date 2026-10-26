// Lenguaje: C# (version no fijada en estos archivos; define el target en el proyecto de Visual Studio/.NET).
// =====================================================================
// PROGRAMA 6 - RESERVAS DE STOCK
// Proyecto integrador: Tienda con ERP Dolibarr | Materia: Programación Estructurada | Tarjeta Trello: PE-01
// Módulo de Dolibarr: Productos (stock reservado)
//
// Qué hace:
//   - Crear reserva: aparta unidades para un cliente sin venderlas todavía.
//   - Disponible = stock total - stock reservado. No deja reservar más de lo disponible.
//   - Cancelar reserva: devuelve las unidades al stock disponible.
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

namespace Programa6_Reservas
{

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
        public int StockReservado;
        public List<Lote> Lotes = new List<Lote>();   // en orden de llegada
    }

    class Reserva
    {
        public int Id;
        public string RefProducto = "";
        public string Cliente = "";
        public int Cantidad;
        public bool Activa = true;
        public string Estado = "ACTIVA";
        public DateTime FechaInicio;
        public DateTime FechaLimite;
    }

    class Program
    {
        static readonly DateTime SIN_VENCIMIENTO = DateTime.MaxValue;

        static List<Producto> productos = new List<Producto>();
        static List<Reserva> reservas = new List<Reserva>();
        static int sigReserva = 1;
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
                Console.WriteLine("===== PROGRAMA 6: RESERVAS =====");
                Console.WriteLine(" 1. Crear reserva");
                Console.WriteLine(" 2. Cancelar reserva");
                Console.WriteLine(" 3. Ver reservas");
                Console.WriteLine(" 4. Ver productos y stock");
                Console.WriteLine(" 5. Ejecutar pruebas con resultados esperados");
                Console.WriteLine(" 0. Salir");
                opcion = LeerEntero("Opción", 0, 5);
                switch (opcion)
                {
                    case 1: MenuCrearReserva(); break;
                    case 2: MenuCancelarReserva(); break;
                    case 3: ListarReservas(); break;
                    case 4: MostrarStockYLotes(); break;
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

        // ============================ PROGRAMA PRINCIPAL ============================
        static void MenuCrearReserva()
        {
            Console.WriteLine("\n--- Crear reserva ---");
            Producto p = PedirProducto();
            if (p == null) return;
            Console.WriteLine("  Disponible: " + StockDisponible(p));
            int cantidad = LeerEntero("Cantidad a reservar", 1, 100000);
            string cliente = LeerTexto("Nombre del cliente");
            int dias = LeerEntero("Vigencia de la reserva en dias (1 a 365)", 1, 365);
            Reserva r = CrearReserva(p, cantidad, cliente, dias);
            if (r == null) Console.WriteLine("  No hay stock disponible suficiente.");
            else Console.WriteLine("  Reserva N° " + r.Id + " creada. Disponible ahora: " + StockDisponible(p));
        }

        static void MenuCancelarReserva()
        {
            Console.WriteLine("\n--- Cancelar reserva ---");
            int id = LeerEntero("N° de reserva", 1, 100000);
            if (CancelarReserva(id)) Console.WriteLine("  Reserva cancelada: el stock vuelve a estar disponible.");
            else Console.WriteLine("  Esa reserva no existe o ya estaba cancelada.");
        }

        // Devuelve null si pide más de lo disponible
        static Reserva CrearReserva(Producto p, int cantidad, string cliente)
        {
            return CrearReserva(p, cantidad, cliente, 1);
        }

        static Reserva CrearReserva(Producto p, int cantidad, string cliente, int diasVigencia)
        {
            ActualizarReservasExpiradas();
            if (p == null || BuscarProducto(p.Ref) == null || string.IsNullOrWhiteSpace(cliente) || cantidad <= 0 || cantidad > 100000 ||
                diasVigencia < 1 || diasVigencia > 365 || cantidad > StockDisponible(p)) return null;
            p.StockReservado += cantidad;
            Reserva r = new Reserva();
            r.Id = sigReserva;
            sigReserva++;
            r.RefProducto = p.Ref;
            r.Cliente = cliente;
            r.Cantidad = cantidad;
            r.FechaInicio = DateTime.Now;
            r.FechaLimite = r.FechaInicio.AddDays(diasVigencia);
            r.Estado = "ACTIVA";
            reservas.Add(r);
            return r;
        }

        static void ActualizarReservasExpiradas()
        {
            DateTime ahora = DateTime.Now;
            foreach (Reserva r in reservas)
            {
                if (r.Activa && ahora >= r.FechaLimite)
                {
                    Producto p = BuscarProducto(r.RefProducto);
                    if (p != null) p.StockReservado -= r.Cantidad;
                    r.Activa = false;
                    r.Estado = "EXPIRADA";
                }
            }
        }

        static bool CancelarReserva(int id)
        {
            ActualizarReservasExpiradas();
            foreach (Reserva r in reservas)
            {
                if (r.Id == id && r.Activa)
                {
                    Producto p = BuscarProducto(r.RefProducto);
                    if (p == null) return false;
                    p.StockReservado -= r.Cantidad;   // devuelve el stock al disponible
                    r.Activa = false;
                    r.Estado = "CANCELADA";
                    return true;
                }
            }
            return false;
        }

        static int ContarActivas()
        {
            ActualizarReservasExpiradas();
            int n = 0;
            foreach (Reserva r in reservas)
            {
                if (r.Activa) n++;
            }
            return n;
        }

        static void ListarReservas()
        {
            ActualizarReservasExpiradas();
            Console.WriteLine("\n--- Reservas ---");
            foreach (Reserva r in reservas)
                Console.WriteLine("  Reserva " + r.Id + " | " + r.RefProducto + " | " + r.Cantidad + " u. | " + r.Cliente + " | " + r.Estado + " | vence " + r.FechaLimite.ToString("yyyy-MM-dd HH:mm"));
            if (reservas.Count == 0) Console.WriteLine("  (sin reservas)");
        }

        static void MostrarStockYLotes()
        {
            ActualizarReservasExpiradas();
            Console.WriteLine("\n--- Productos, stock y lotes ---");
            foreach (Producto p in productos)
            {
                Console.WriteLine("  " + p.Ref + " " + p.Nombre + " | stock: " + StockTotal(p) +
                                  " | reservado: " + p.StockReservado + " | disponible: " + StockDisponible(p));
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
            l.FechaIngreso = ingreso;
            l.FechaVence = vence;
            p.Lotes.Add(l);
        }

        static void CargarDatosEjemplo()
        {
            productos.Clear();
            reservas.Clear();
            sigReserva = 1;
            NuevoProducto("BEB-002", "Api con leche 1 L", 12);
            NuevoProducto("YOG-024", "Yogur bebible canela 1 L", 30);
            NuevoProducto("HEL-006", "Paleta de leche unidad", 2);
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
            IniciarPruebas("PRUEBAS - RESERVAS");

            Producto yog = BuscarProducto("YOG-024");      // tiene 30 unidades
            Verificar("Disponible al inicio", "30", StockDisponible(yog).ToString());

            // --- Crear y cancelar ---
            Reserva r = CrearReserva(yog, 10, "Mark Flores", 30);
            Verificar("N° de la primera reserva", "1", (r == null) ? "null" : r.Id.ToString());
            Verificar("Disponible tras reservar 10", "20", StockDisponible(yog).ToString());
            Verificar("El stock total no cambia", "30", StockTotal(yog).ToString());
            Verificar("Cancelar la reserva", "True", CancelarReserva(1).ToString());
            Verificar("Disponible tras cancelar", "30", StockDisponible(yog).ToString());
            Verificar("Cancelar otra vez", "False", CancelarReserva(1).ToString());
            Reserva expira = CrearReserva(yog, 1, "Prueba expiracion", 30);
            expira.FechaLimite = DateTime.Now.AddSeconds(-1);
            ContarActivas();
            Verificar("Estado de reserva expirada", "EXPIRADA", expira.Estado);
            Verificar("Stock disponible tras expirar", "30", StockDisponible(yog).ToString());

            // --- Límites ---
            Verificar("Reservar más que el stock", "True", (CrearReserva(yog, 999, "X") == null).ToString());
            Reserva todo = CrearReserva(yog, 30, "Y");
            Verificar("Reservar todo el stock", "0", StockDisponible(yog).ToString());
            Verificar("Reservar 1 más (ya no hay disponible)", "True", (CrearReserva(yog, 1, "Z") == null).ToString());
            Verificar("Cancelar una reserva que no existe", "False", CancelarReserva(99).ToString());
            Verificar("Reservas activas", "1", ContarActivas().ToString());

            TerminarPruebas();
            CargarDatosEjemplo();     // se vuelven a cargar los datos originales
        }
    }
}
