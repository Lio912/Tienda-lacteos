// Lenguaje: C# (version no fijada en estos archivos; define el target en el proyecto de Visual Studio/.NET).
// =====================================================================
// PROGRAMA 2 - REGISTRAR PRODUCTO
// Proyecto integrador: Tienda con ERP Dolibarr | Materia: Programación Estructurada | Tarjeta Trello: PE-01
// Módulo de Dolibarr: Productos
//
// Qué hace:
//   - Pide referencia, nombre, precio sin IVA y código de barras (opcional).
//   - No deja repetir la referencia ni el código de barras.
//   - Muestra el precio con IVA (13%).
//
// Cómo usarlo en Visual Studio:
//   1) Archivo > Nuevo > Proyecto > Aplicación de consola (C#).
//   2) Borra todo lo que hay en Program.cs y pega este archivo completo.
//   3) Ejecuta con Ctrl+F5.
//   4) En el menú elige la opción 3 (pruebas): todas las líneas deben salir [OK].
// Este programa es independiente: no necesita los otros 6.
// =====================================================================
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Programa2_RegistrarProducto
{
    class Producto
    {
        public string Ref = "";         // BEB-001
        public string Nombre = "";
        public decimal PrecioSinIva;
        public string CodigoBarras = "";
    }

    class Program
    {
        const decimal IVA = 0.13m;      // 13 %

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
                Console.WriteLine("===== PROGRAMA 2: REGISTRAR PRODUCTO =====");
                Console.WriteLine(" 1. Registrar producto");
                Console.WriteLine(" 2. Ver productos");
                Console.WriteLine(" 3. Ejecutar pruebas con resultados esperados");
                Console.WriteLine(" 0. Salir");
                opcion = LeerEntero("Opción", 0, 3);
                switch (opcion)
                {
                    case 1: MenuRegistrarProducto(); break;
                    case 2: ListarProductos(); break;
                    case 3: EjecutarPruebas(); break;
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

        // ============================ BÚSQUEDAS ============================
        static bool CodigoBarrasValido(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return true;
            if (codigo.Length != 8 && codigo.Length != 12 && codigo.Length != 13 && codigo.Length != 14) return false;
            for (int i = 0; i < codigo.Length; i++)
                if (codigo[i] < '0' || codigo[i] > '9') return false;
            return true;
        }

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

        // ============================ PROGRAMA PRINCIPAL ============================
        static void MenuRegistrarProducto()
        {
            Console.WriteLine("\n--- Registrar producto ---");
            string rf = LeerTexto("Referencia (ej. BEB-004)").ToUpper();
            if (BuscarProducto(rf) != null)
            {
                Console.WriteLine("  Esa referencia ya existe.");
                return;
            }
            string nombre = LeerTexto("Nombre");
            decimal precio = LeerDecimal("Precio sin IVA", 0.01m, 1000000m);
            string barras = LeerTextoOpcional("Codigo de barras (8, 12, 13 o 14 digitos)");

            string error;
            Producto p = RegistrarProducto(rf, nombre, precio, barras, out error);
            if (p == null) Console.WriteLine("  No se pudo registrar: " + error);
            else Console.WriteLine("  Producto registrado. Precio con IVA: " + Bs(ConIva(p.PrecioSinIva)));
        }

        // Valida todo y, si está bien, guarda el producto. Devuelve null si algo falla.
        static Producto RegistrarProducto(string rf, string nombre, decimal precio, string barras, out string error)
        {
            error = "";
            if (rf == null) rf = "";
            if (nombre == null) nombre = "";
            if (barras == null) barras = "";
            rf = rf.Trim().ToUpper();
            barras = barras.Trim();
            if (rf == "") { error = "La referencia es obligatoria."; return null; }
            if (BuscarProducto(rf) != null) { error = "Esa referencia ya existe."; return null; }
            if (nombre.Trim() == "") { error = "El nombre es obligatorio."; return null; }
            if (precio <= 0 || precio > 1000000m) { error = "El precio debe ser mayor que 0 y no superar 1.000.000 Bs."; return null; }
            if (!CodigoBarrasValido(barras)) { error = "El codigo debe tener 8, 12, 13 o 14 digitos, sin letras ni espacios."; return null; }
            if (barras != "" && BuscarProductoPorCodigo(barras) != null)
            {
                error = "Ese código de barras ya está en otro producto.";
                return null;
            }

            Producto p = new Producto();
            p.Ref = rf;
            p.Nombre = nombre.Trim();
            p.PrecioSinIva = precio;
            p.CodigoBarras = barras;
            productos.Add(p);
            return p;
        }

        static void ListarProductos()
        {
            Console.WriteLine("\n--- Productos ---");
            Console.WriteLine("  {0,-8} {1,-30} {2,8} {3,8}  {4}", "Ref", "Nombre", "Sin IVA", "Con IVA", "Código de barras");
            foreach (Producto p in productos)
                Console.WriteLine("  {0,-8} {1,-30} {2,8:0.00} {3,8:0.00}  {4}", p.Ref, p.Nombre, p.PrecioSinIva, ConIva(p.PrecioSinIva), p.CodigoBarras);
        }

        // ============================ DATOS DE EJEMPLO ============================
        static void NuevoProducto(string rf, string nombre, decimal precioSinIva, string barras)
        {
            Producto p = new Producto();
            p.Ref = rf;
            p.Nombre = nombre;
            p.PrecioSinIva = precioSinIva;
            p.CodigoBarras = barras;
            productos.Add(p);
        }

        static void CargarDatosEjemplo()
        {
            productos.Clear();
            NuevoProducto("BEB-001", "Chicha de maní con leche 1 L", 8.85m, "7770000000012");
            NuevoProducto("BEB-002", "Api con leche 1 L", 7.96m, "7770000000029");
            NuevoProducto("BEB-003", "Kéfir natural 1 L", 13.27m, "7770000000036");
            NuevoProducto("YOG-024", "Yogur bebible canela 1 L", 10.62m, "7770000000821");
            NuevoProducto("YOG-029", "Yogur bebible maracuyá 1 L", 10.62m, "7770000000838");
            NuevoProducto("LEC-011", "Leche frutilla 1 L", 8.85m, "7770000000845");
            NuevoProducto("LEC-002", "Leche entera 2 L", 11.95m, "7770000000852");
            NuevoProducto("HEL-006", "Paleta de leche unidad", 2.65m, "7770000000869");
            NuevoProducto("LEC-013", "Leche en polvo 800 g", 45.00m, "7770000000876");
            NuevoProducto("YOG-009", "Yogur bebible mora 1 L", 10.62m, "7770000000883");
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
            IniciarPruebas("PRUEBAS - REGISTRAR PRODUCTO");

            // --- Precio con IVA (13 %) ---
            Verificar("Precio con IVA de 8.85", "10.00", ConIva(8.85m).ToString("0.00"));
            Verificar("Precio con IVA de 13.27", "15.00", ConIva(13.27m).ToString("0.00"));
            Verificar("Precio con IVA de 9.00", "10.17", ConIva(9.00m).ToString("0.00"));

            // --- Registro correcto ---
            string error;
            Producto p = RegistrarProducto("beb-004", "Jugo de naranja 1 L", 9.00m, "7770000000890", out error);
            Verificar("Producto nuevo: referencia (pasa a mayúsculas)", "BEB-004", (p == null) ? "null" : p.Ref);
            Verificar("Cantidad de productos", "11", productos.Count.ToString());

            // --- Registros que deben rechazarse ---
            Verificar("Referencia repetida", "True", (RegistrarProducto("BEB-001", "Otro", 5m, "", out error) == null).ToString());
            Verificar("Referencia repetida (minúsculas)", "True", (RegistrarProducto("beb-001", "Otro", 5m, "", out error) == null).ToString());
            Verificar("Código de barras repetido", "True", (RegistrarProducto("BEB-005", "Otro", 5m, "7770000000012", out error) == null).ToString());
            Verificar("Precio 0", "True", (RegistrarProducto("BEB-006", "Otro", 0m, "", out error) == null).ToString());
            Verificar("Nombre vacío", "True", (RegistrarProducto("BEB-007", "", 5m, "", out error) == null).ToString());
            Verificar("Cantidad de productos (sigue igual)", "11", productos.Count.ToString());

            // --- Sin código de barras también es válido ---
            Producto q = RegistrarProducto("BEB-008", "Soda 2 L", 6.50m, "", out error);
            Verificar("Producto sin código de barras", "True", (q != null).ToString());

            TerminarPruebas();
            CargarDatosEjemplo();     // se vuelven a cargar los datos originales
        }
    }
}
