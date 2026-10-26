// Lenguaje: C# (version no fijada en estos archivos; define el target en el proyecto de Visual Studio/.NET).
// =====================================================================
// PROGRAMA 1 - CREAR CLIENTE
// Proyecto integrador: Tienda con ERP Dolibarr | Materia: Programación Estructurada | Tarjeta Trello: PE-01
// Módulo de Dolibarr: Terceros
//
// Qué hace:
//   - Pide nombre, teléfono, email y dirección de un cliente nuevo.
//   - Valida el teléfono (7 a 15 dígitos, puede iniciar con +) y el email (opcional).
//   - No deja crear dos clientes con el mismo nombre.
//   - Le asigna un código automático (CU2610-00104, 00105, ...).
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

namespace Programa1_CrearCliente
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

    class Program
    {
        static List<Tercero> terceros = new List<Tercero>();
        static int sigTercero = 104;        // el siguiente código será CU2610-00104
        static int pruebasOk = 0;
        static int pruebasTotal = 0;

        static void Main()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;   // punto decimal
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            CargarTerceros();

            int opcion;
            do
            {
                Console.WriteLine();
                Console.WriteLine("===== PROGRAMA 1: CREAR CLIENTE =====");
                Console.WriteLine(" 1. Crear cliente");
                Console.WriteLine(" 2. Ver clientes");
                Console.WriteLine(" 3. Ejecutar pruebas con resultados esperados");
                Console.WriteLine(" 0. Salir");
                opcion = LeerEntero("Opción", 0, 3);
                switch (opcion)
                {
                    case 1: CrearCliente(); break;
                    case 2: ListarClientes(); break;
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

        // ============================ VALIDACIONES ============================
        static bool TelefonoValido(string tel)
        {
            if (tel == null) return false;
            if (tel.Length < 7 || tel.Length > 15) return false;
            for (int i = 0; i < tel.Length; i++)
            {
                char c = tel[i];
                bool esDigito = c >= '0' && c <= '9';
                bool esMas = (c == '+' && i == 0);
                if (!esDigito && !esMas) return false;
            }
            return true;
        }

        static bool EmailValido(string email)
        {
            if (email == "") return true;   // el email es opcional
            if (string.IsNullOrWhiteSpace(email) || email.Contains(" ")) return false;
            int arroba = email.IndexOf('@');
            if (arroba <= 0 || arroba != email.LastIndexOf('@')) return false;
            string dominio = email.Substring(arroba + 1);
            int punto = dominio.LastIndexOf('.');
            return punto > 0 && punto < dominio.Length - 2 && !dominio.Contains("..");
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


        // ============================ PROGRAMA PRINCIPAL ============================
        // Pide los datos por teclado (repite la pregunta si el dato no es válido)
        static void CrearCliente()
        {
            Console.WriteLine("\n--- Crear cliente ---");
            string nombre = LeerTexto("Nombre");
            if (BuscarTercero(nombre) != null)
            {
                Console.WriteLine("  Ya existe un cliente con ese nombre.");
                return;
            }

            string tel = LeerTexto("Teléfono (7 a 15 dígitos, puede iniciar con +)");
            while (!TelefonoValido(tel))
            {
                Console.WriteLine("  Teléfono no válido.");
                tel = LeerTexto("Teléfono");
            }

            string email = LeerTextoOpcional("Email");
            while (!EmailValido(email))
            {
                Console.WriteLine("  Email no válido.");
                email = LeerTextoOpcional("Email");
            }

            string dir = LeerTextoOpcional("Dirección");

            string error;
            Tercero t = RegistrarCliente(nombre, tel, email, dir, out error);
            if (t == null) Console.WriteLine("  No se pudo crear: " + error);
            else Console.WriteLine("  Cliente creado: " + t.Codigo + " - " + t.Nombre);
        }

        // Valida todo y, si está bien, guarda el cliente. Devuelve null si algo falla.
        static Tercero RegistrarCliente(string nombre, string tel, string email, string dir, out string error)
        {
            error = "";
            if (string.IsNullOrWhiteSpace(nombre)) { error = "El nombre es obligatorio."; return null; }
            if (tel == null) tel = "";
            if (email == null) email = "";
            if (dir == null) dir = "";
            nombre = nombre.Trim();
            tel = tel.Trim();
            email = email.Trim();
            if (BuscarTercero(nombre) != null) { error = "Ya existe un cliente con ese nombre."; return null; }
            if (!TelefonoValido(tel)) { error = "Teléfono no válido."; return null; }
            if (!EmailValido(email)) { error = "Email no válido."; return null; }

            Tercero t = new Tercero();
            t.Codigo = "CU2610-" + sigTercero.ToString("00000");
            sigTercero++;
            t.Nombre = nombre.Trim();
            t.Telefono = tel;
            t.Email = email;
            t.Direccion = dir;
            terceros.Add(t);
            return t;
        }

        static void ListarClientes()
        {
            Console.WriteLine("\n--- Clientes ---");
            foreach (Tercero t in terceros)
                Console.WriteLine("  {0,-13} {1,-28} {2,-14} {3}", t.Codigo, t.Nombre, t.Telefono, t.Activo ? "Activo" : "Inactivo");
        }

        // ============================ DATOS DE EJEMPLO ============================
        // (basados en las capturas de Dolibarr)
        static void CargarDatosEjemplo()
        {
            CargarTerceros();
            sigTercero = 104;
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
            IniciarPruebas("PRUEBAS - CREAR CLIENTE");

            Verificar("Teléfono +59161680120", "True", TelefonoValido("+59161680120").ToString());
            Verificar("Teléfono con letras", "False", TelefonoValido("7000abc1").ToString());
            Verificar("Teléfono muy corto", "False", TelefonoValido("123").ToString());
            Verificar("Email vacío (es opcional)", "True", EmailValido("").ToString());
            Verificar("Email correcto", "True", EmailValido("ana@correo.com").ToString());
            Verificar("Email sin @", "False", EmailValido("anacorreo.com").ToString());

            string error;
            Tercero t = RegistrarCliente("Ana Rojas", "70012345", "ana@correo.com", "Av. Blanco Galindo", out error);
            Verificar("Cliente nuevo: código", "CU2610-00104", (t == null) ? "null" : t.Codigo);
            Verificar("Cantidad de clientes", "5", terceros.Count.ToString());

            Verificar("Nombre repetido (otras mayúsculas)", "True", (RegistrarCliente("ana rojas", "70099999", "", "", out error) == null).ToString());
            Verificar("Teléfono inválido no crea cliente", "True", (RegistrarCliente("Luis Pérez", "12", "", "", out error) == null).ToString());
            Verificar("Email inválido no crea cliente", "True", (RegistrarCliente("Luis Pérez", "70011122", "luis.com", "", out error) == null).ToString());
            Verificar("Cantidad de clientes (sigue igual)", "5", terceros.Count.ToString());

            Tercero t2 = RegistrarCliente("Luis Pérez", "70011122", "", "", out error);
            Verificar("Siguiente código", "CU2610-00105", (t2 == null) ? "null" : t2.Codigo);

            TerminarPruebas();
            CargarDatosEjemplo();     // se vuelven a cargar los datos originales
        }
    }
}
