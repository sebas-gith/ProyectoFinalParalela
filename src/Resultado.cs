using System.Collections.Concurrent;
namespace AnalizadorTextoParalelo
{
  public class Resultado
  {
    public int TotalPalabras;
    public int TotalLineas;
    public int OcurrenciasTermino;
    public ConcurrentDictionary<string, int> Frecuencias = new ConcurrentDictionary<string,
  int>();
    public int BloquesProcesados = 0;
    public int TareasCreadas = 0;
    public int NivelRecursion = 1;
  }
}














