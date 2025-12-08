using AnalizadorTextoParalelo;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

public class Analizador
{
    private readonly int sizeBloque;
    private readonly int maxParalelismo;

    public Analizador(int sizeBloque)
    {
        this.sizeBloque = sizeBloque;
        this.maxParalelismo = Environment.ProcessorCount;
    }


    public async Task<Resultado> AnalizarArchivos(List<string> archivos, string termino)
    {
        Stopwatch timeLectura = Stopwatch.StartNew();

        long totalSize = 0;

        StringBuilder data = new StringBuilder();

        foreach(string archivo in archivos)
        {
            string texto = await File.ReadAllTextAsync(archivo);
            data.AppendLine(texto);
            totalSize += new FileInfo(archivo).Length * 1024 * 1024;
        }

        timeLectura.Stop();
        string textoTotal = data.ToString();

        //tiempo secuencial
        Stopwatch secuencialTime = Stopwatch.StartNew();
        var secuencial = ProcesarSecuencial(textoTotal, termino);
        secuencialTime.Stop();

        //tiempo paralelo/recursivo
        Stopwatch paraleloTime = Stopwatch.StartNew();
        var recursivo = ProcesarRecursivo(textoTotal, termino, 1);
        paraleloTime.Stop();

        //speedup y eficiencia
        double secTime = secuencialTime.ElapsedMilliseconds;
        double recurTime = paraleloTime.ElapsedMilliseconds;

        double speedup = secTime/recurTime;
        double eficiencia = (speedup/maxParalelismo)*100;
        double mejoraPorciento = ((secTime - recurTime) / secTime) * 100;

        Console.WriteLine("---------- METRICAS DE RENDIMIENTO --------------");
        Console.WriteLine($"Tiempo total: {timeLectura.ElapsedMilliseconds + paraleloTime.ElapsedMilliseconds} ms");
        Console.WriteLine($"Tiempo de lectura: {timeLectura.ElapsedMilliseconds} ms");
        Console.WriteLine($"Tiempo paralelo: {paraleloTime.ElapsedMilliseconds} ms");
        Console.WriteLine($"Tiempo secuencial: {secuencialTime.ElapsedMilliseconds} ms");
        Console.WriteLine($"Speedup: {speedup}");
        Console.WriteLine($"Eficiencia %: {eficiencia}%");
        Console.WriteLine($"Mejora %: {mejoraPorciento}%");
        Console.WriteLine($"Tamano/Size: {totalSize}");
        Console.WriteLine($"Tareas: {recursivo.TareasCreadas}");
        Console.WriteLine($"Bloques: {recursivo.BloquesProcesados}");
        Console.WriteLine($"Nivel maximo de recursion: {recursivo.NivelRecursion}");
        Console.WriteLine($"Nucleos: {maxParalelismo}");

        return recursivo;
    }
    private async Task<Resultado> ProcesarRecursivo(string texto, string termino, int nivel)
    {
        if (texto.Length <= sizeBloque)
        {
            var rBase = ProcesarSecuencial(texto, termino);
            rBase.BloquesProcesados = 1;
            rBase.TareasCreadas = 1;
            rBase.NivelRecursion = nivel;
            return rBase;
        }

        int medio = texto.Length / 2;

        while (medio < texto.Length && !char.IsWhiteSpace(texto[medio]))
            medio++;

        string izq = texto.Substring(0, medio);
        string der = texto.Substring(medio);

        var t1 = Task.Run(() => ProcesarRecursivo(izq, termino, nivel + 1));
        var t2 = Task.Run(() => ProcesarRecursivo(der, termino, nivel + 1));

        await Task.WhenAll(t1, t2);
        return Combinar(t1.Result, t2.Result);
    }
    private Resultado ProcesarSecuencial(string texto, string termino)
    {
        var r = new Resultado();

        r.TotalLineas = texto.Count(c => c == '\n');

        var palabras = Regex.Matches(texto, @"\b[\w']+\b")
            .Cast<Match>()
            .Select(m => m.Value.ToLower())
            .ToList();

        r.TotalPalabras = palabras.Count;

        foreach (var p in palabras)
            r.Frecuencias.AddOrUpdate(p, 1, (_, v) => v + 1);

        if (!string.IsNullOrEmpty(termino))
            r.OcurrenciasTermino = palabras.Count(p => p == termino.ToLower());

        return r;
    }

}