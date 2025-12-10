
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using AnalizadorTextoParalelo;

namespace ProyectoFinalParalela.Tests;

public class AnalizadorSecuencialTests
{
    [Fact]
    public void ProcesarSecuencial_CuentaLineasYPalabras()
    {
        var texto = "Hola mundo\nEsta es otra linea";
        var analizador = new Analizador(1024 * 1024);

        var resultado = InvokeProcesarSecuencial(analizador, texto, null);

        Assert.Equal(1, resultado.TotalLineas);
        Assert.Equal(6, resultado.TotalPalabras);
    }

    private Resultado InvokeProcesarSecuencial(Analizador analizador, string texto, string termino)
    {
        var mi = typeof(Analizador)
            .GetMethod("ProcesarSecuencial", BindingFlags.NonPublic | BindingFlags.Instance);

        return (Resultado)mi!.Invoke(analizador, new object[] { texto, termino! })!;
    }
}
