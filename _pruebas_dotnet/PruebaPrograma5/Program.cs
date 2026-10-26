// Lenguaje: C# (version no fijada en estos archivos; define el target en el proyecto de Visual Studio/.NET).
// =====================================================================
// PROGRAMA 5 - DESCUENTOS, PRECIO MAYORISTA Y PRESUPUESTO (COTIZACIÓN)
// Proyecto integrador: Tienda con ERP Dolibarr | Materia: Programación Estructurada | Tarjeta Trello: PE-01
// Módulo de Dolibarr: Comercial
//
// Qué hace:
//   - Arma un presupuesto para un cliente con varios productos.
//   - Precio mayorista: desde 12 unidades de un mismo producto se aplica 10% de descuento.
//     (SUPUESTO: confirmar esa regla con el equipo; se cambia en las constantes MIN_MAYORISTA y DTO_MAYORISTA).
//   - También acepta un descuento manual; se aplica el mayor entre el manual y el mayorista.
//   - Calcula base imponible, IVA (13%) y total. El presupuesto vale 15 días.
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

namespace Programa5_Presupuesto
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

    class Producto
    {
        public string Ref = "";         // BEB-001
        public string Nombre = "";
        public decimal PrecioSinIva;
        public string CodigoBarras = "";
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

    class Presupuesto
    {
        public string Ref = "";         // COT-0101
        public Tercero Cliente;
        public DateTime Fecha;
        public DateTime FinValidez;
        public string Estado = "";
        public List<LineaDoc> Lineas = new List<LineaDoc>();
        public decimal BaseImponible;
        public decimal Iva;
        public decimal Total;
    }

    class Program
    {
        // ---------- Reglas del negocio (AJUSTAR con el equipo si cambian) ----------
        const decimal IVA = 0.13m;              // 13 %
        const int MIN_MAYORISTA = 12;           // cantidad mínima para precio mayorista
        const decimal DTO_MAYORISTA = 10m;      // % de descuento mayorista
        const int DIAS_VALIDEZ = 15;            // validez de un presupuesto

        // Fecha del sistema fija para que las pruebas siempre den el mismo resultado.
        // Para usar la fecha real cambiar por: DateTime.Today
        static DateTime hoy = new DateTime(2026, 10, 19);

        static List<Tercero> terceros = new List<Tercero>();
        static List<Producto> productos = new List<Producto>();
        static List<Presupuesto> presupuestos = new List<Presupuesto>();
        static int sigPresupuesto = 101;
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
                Console.WriteLine("===== PROGRAMA 5: PRESUPUESTO (COTIZACIÓN) =====");
                Console.WriteLine(" 1. Generar presupuesto");
                Console.WriteLine(" 2. Ver productos y precios");
                Console.WriteLine(" 3. Ver presupuestos creados");
                Console.WriteLine(" 4. Ejecutar pruebas con resultados esperados");
                Console.WriteLine(" 0. Salir");
                opcion = LeerEntero("Opción", 0, 4);
                switch (opcion)
                {
                    case 1: MenuPresupuesto(); break;
                    case 2: ListarProductos(); break;
                    case 3: ListarPresupuestos(); break;
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

        static List<LineaDoc> LeerLineas()
        {
            List<LineaDoc> lineas = new List<LineaDoc>();
            bool seguir = true;
            while (seguir)
            {
                Producto p = PedirProducto();
                if (p != null)
                {
                    int cantidad = LeerEntero("Cantidad", 1, 100000);
                    if (!AgregarOSumarLinea(lineas, p, cantidad))
                        Console.WriteLine("  No se agrego: verifica la cantidad y el maximo acumulado de 100.000 unidades.");
                }
                seguir = LeerSiNo("¿Agregar otro producto?");
            }
            return lineas;
        }

        // ============================ PROGRAMA PRINCIPAL ============================
        static void MenuPresupuesto()
        {
            Console.WriteLine("\n--- Generar presupuesto ---");
            Tercero cliente = PedirCliente();
            if (cliente == null) return;
            List<LineaDoc> lineas = LeerLineas();
            decimal dto = LeerDecimal("Descuento manual % (0 si no hay)", 0m, 100m);
            Presupuesto q = GenerarPresupuesto(cliente, lineas, dto);
            if (q == null) Console.WriteLine("  No se pudo generar: faltan datos.");
            else MostrarPresupuesto(q);
        }

        static Presupuesto GenerarPresupuesto(Tercero cliente, List<LineaDoc> lineas, decimal descuentoManual)
        {
            if (cliente == null || BuscarTercero(cliente.Codigo) == null || lineas == null || lineas.Count == 0) return null;
            if (descuentoManual < 0m || descuentoManual > 100m) return null;
            foreach (LineaDoc linea in lineas)
                if (linea == null || linea.Cantidad <= 0 || linea.Cantidad > 100000 || BuscarProducto(linea.RefProducto) == null)
                    return null;
            foreach (LineaDoc linea in lineas)
            {
                Producto producto = BuscarProducto(linea.RefProducto);
                linea.RefProducto = producto.Ref;
                linea.Descripcion = producto.Nombre;
                linea.PrecioUnit = producto.PrecioSinIva;
            }

            Presupuesto q = new Presupuesto();
            q.Ref = "COT-" + sigPresupuesto.ToString("0000");
            sigPresupuesto++;
            q.Cliente = cliente;
            q.Fecha = hoy;
            q.FinValidez = hoy.AddDays(DIAS_VALIDEZ);
            q.Estado = "Validado (presupuesto abierto)";
            q.Lineas = lineas;

            RecalcularLineas(lineas, descuentoManual);
            CalcularTotales(lineas, out q.BaseImponible, out q.Iva, out q.Total);

            presupuestos.Add(q);
            return q;
        }

        // ============================ MOSTRAR / LISTAR ============================
        static void MostrarPresupuesto(Presupuesto q)
        {
            Console.WriteLine("\n=== PRESUPUESTO " + q.Ref + " ===");
            Console.WriteLine("Cliente: " + q.Cliente.Nombre);
            Console.WriteLine("Fecha: " + q.Fecha.ToString("yyyy-MM-dd") + " | Válido hasta: " + q.FinValidez.ToString("yyyy-MM-dd"));
            Console.WriteLine("Estado: " + q.Estado);
            MostrarLineas(q.Lineas);
            Console.WriteLine("Base imponible: " + Bs(q.BaseImponible));
            Console.WriteLine("IVA 13%:        " + Bs(q.Iva));
            Console.WriteLine("TOTAL:          " + Bs(q.Total));
        }

        static void ListarProductos()
        {
            Console.WriteLine("\n--- Productos y precios ---");
            Console.WriteLine("  {0,-8} {1,-30} {2,8} {3,8}", "Ref", "Nombre", "Sin IVA", "Con IVA");
            foreach (Producto p in productos)
                Console.WriteLine("  {0,-8} {1,-30} {2,8:0.00} {3,8:0.00}", p.Ref, p.Nombre, p.PrecioSinIva, ConIva(p.PrecioSinIva));
        }

        static void ListarPresupuestos()
        {
            Console.WriteLine("\n--- Presupuestos creados en esta sesión ---");
            foreach (Presupuesto q in presupuestos)
                Console.WriteLine("  " + q.Ref + " | " + q.Cliente.Nombre + " | " + Bs(q.Total) + " | " + q.Estado);
            if (presupuestos.Count == 0) Console.WriteLine("  (sin presupuestos)");
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
            CargarTerceros();
            productos.Clear();
            presupuestos.Clear();
            sigPresupuesto = 101;

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
            IniciarPruebas("PRUEBAS - DESCUENTOS Y PRESUPUESTO");

            // --- Descuentos y precio mayorista ---
            Verificar("Descuento con 11 u.", "0", CalcularDescuentoPct(11, 0m).ToString("0.##"));
            Verificar("Descuento con 12 u. (mayorista)", "10", CalcularDescuentoPct(12, 0m).ToString("0.##"));
            Verificar("Descuento manual 15% con 5 u.", "15", CalcularDescuentoPct(5, 15m).ToString("0.##"));
            Verificar("Manual 5% con 30 u. (gana el mayorista)", "10", CalcularDescuentoPct(30, 5m).ToString("0.##"));
            Verificar("Manual 15% con 30 u. (gana el manual)", "15", CalcularDescuentoPct(30, 15m).ToString("0.##"));

            // --- Presupuesto de la Guardería Pequeños Pasos (todas las líneas son mayoristas) ---
            List<LineaDoc> lc = new List<LineaDoc>();
            AgregarOSumarLinea(lc, BuscarProducto("LEC-011"), 36);
            AgregarOSumarLinea(lc, BuscarProducto("YOG-029"), 100);
            AgregarOSumarLinea(lc, BuscarProducto("LEC-002"), 24);
            Presupuesto q = GenerarPresupuesto(BuscarTercero("Guardería Pequeños Pasos"), lc, 0m);
            MostrarPresupuesto(q);
            Verificar("Presupuesto: referencia", "COT-0101", q.Ref);
            Verificar("Presupuesto: primera línea (36 x 8.85 con 10%)", "286.74", q.Lineas[0].BaseLinea.ToString("0.00"));
            Verificar("Presupuesto: base imponible", "1500.66", q.BaseImponible.ToString("0.00"));
            Verificar("Presupuesto: IVA", "195.09", q.Iva.ToString("0.00"));
            Verificar("Presupuesto: total", "1695.75", q.Total.ToString("0.00"));
            Verificar("Presupuesto: válido hasta", "2026-11-03", q.FinValidez.ToString("yyyy-MM-dd"));

            // --- Presupuesto sin descuento (2 kéfir) ---
            List<LineaDoc> l2 = new List<LineaDoc>();
            AgregarOSumarLinea(l2, BuscarProducto("BEB-003"), 1);
            AgregarOSumarLinea(l2, BuscarProducto("BEB-003"), 1);     // repetido: se suma
            Presupuesto q2 = GenerarPresupuesto(BuscarTercero("Mark Flores"), l2, 0m);
            Verificar("Repetidos: una sola línea", "1", q2.Lineas.Count.ToString());
            Verificar("Sin descuento: base", "26.54", q2.BaseImponible.ToString("0.00"));
            Verificar("Sin descuento: IVA", "3.45", q2.Iva.ToString("0.00"));
            Verificar("Sin descuento: total", "29.99", q2.Total.ToString("0.00"));

            // --- Descuento manual del 10% sobre 1 leche entera ---
            List<LineaDoc> l3 = new List<LineaDoc>();
            AgregarOSumarLinea(l3, BuscarProducto("LEC-002"), 1);
            Presupuesto q3 = GenerarPresupuesto(BuscarTercero("Mark Flores"), l3, 10m);
            Verificar("Descuento manual: base", "10.76", q3.BaseImponible.ToString("0.00"));
            Verificar("Descuento manual: total", "12.16", q3.Total.ToString("0.00"));

            // --- Casos que no deben generar presupuesto ---
            Verificar("Sin cliente", "True", (GenerarPresupuesto(null, l2, 0m) == null).ToString());
            Verificar("Sin productos", "True", (GenerarPresupuesto(BuscarTercero("Mark Flores"), new List<LineaDoc>(), 0m) == null).ToString());

            TerminarPruebas();
            CargarDatosEjemplo();     // se vuelven a cargar los datos originales
        }
    }
}
