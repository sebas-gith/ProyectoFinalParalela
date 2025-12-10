using System.Text;

namespace AnalizadorTextoParalelo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("CALCULADORA PARALELA DE ESTADISTICAS DE TEXTO");
            Console.WriteLine("Descomposicion Recursiva (Divide y Venceras) \n");

            string carpeta = @"../tests";

            var archivos = Directory.GetFiles(carpeta, "*.txt").ToList();

            Console.WriteLine($"Archivos encontrados: {archivos.Count}");
            foreach (var a in archivos)
                Console.WriteLine($"   - {Path.GetFileName(a)}");

            if (archivos.Count == 0)
            {
                Console.WriteLine("No hay archivos .txt");
                return;
            }

            Console.Write("\n¿Buscas un termino en especifico? (enter para omitir):");
            string termino = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(termino))
                termino = null;

            int tamMB = 8;
            Console.WriteLine($"\nProcesando con Bloque Base = {tamMB} MB...\n");

            var analizador = new Analizador(tamMB * 1024 * 1024);
            var resultado = await analizador.AnalizarArchivos(archivos, termino);

            Console.WriteLine("\n---RESUMEN DEL ANALISIS DEL TEXTO---");

            Console.WriteLine($"Total de Lineas: {resultado.TotalLineas:N0}");
            Console.WriteLine($"Toral de palabras: {resultado.TotalPalabras:N0}");
            Console.WriteLine($"Palabras unicas: {resultado.Frecuencias.Count:N0}");

            if (termino != null)
                Console.WriteLine($"Ocurrencias de '{termino}': {resultado.OcurrenciasTermino:N0}");
            else
                Console.WriteLine("No se busco ningun termino.");

            Console.WriteLine("\n---Top 10 palabras mas usadas:---");
            foreach (var kv in resultado.Frecuencias.OrderByDescending(x => x.Value).Take(10))
                Console.WriteLine($"    {kv.Key,-20} → {kv.Value:N0}");

            Console.WriteLine("\nPresiona una tecla para salir...");
            Console.ReadKey();
        }



    }


}
