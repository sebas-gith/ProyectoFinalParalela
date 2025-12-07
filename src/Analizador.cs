using System.Diagnostics;
using System.Text;
using AnalizadorTextoParalelo;

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
        var secuencial = ProcesarSecuencial(textoTotal, termino, 1);
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

        //presentacion de metricas
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
}