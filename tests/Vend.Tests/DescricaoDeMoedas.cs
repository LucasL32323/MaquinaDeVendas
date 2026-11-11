using System.Globalization;

namespace Vend.Tests;

/// <summary>Transforma um conjunto de moedas em texto estável ("0.50x1+0.10x2") para comparar nos testes.</summary>
internal static class DescricaoDeMoedas
{
    public static string De(IReadOnlyDictionary<decimal, int>? moedas)
    {
        if (moedas is null)
        {
            return "<nulo>";
        }

        if (moedas.Count == 0)
        {
            return "<vazio>";
        }

        return string.Join(
            "+",
            moedas
                .Where(par => par.Value > 0)
                .OrderByDescending(par => par.Key)
                .Select(par => $"{par.Key.ToString("0.00", CultureInfo.InvariantCulture)}x{par.Value}"));
    }
}
