namespace Vend.Core;

/// <summary>As moedas de real aceitas pela máquina.</summary>
public static class Moedas
{
    public static IReadOnlyList<decimal> Validas { get; } = [0.05m, 0.10m, 0.25m, 0.50m, 1.00m];

    public static bool EhValida(decimal valor) => Validas.Contains(valor);

    internal static void GarantirValida(decimal valor)
    {
        if (!EhValida(valor))
        {
            throw new ArgumentException($"Moeda não aceita: {valor:0.00}.", nameof(valor));
        }
    }
}
