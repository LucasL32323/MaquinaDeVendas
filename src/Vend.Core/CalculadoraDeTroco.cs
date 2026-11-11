namespace Vend.Core;

/// <summary>
/// Calcula o troco com o menor número de moedas possível respeitando o que existe no cofre.
/// Um algoritmo guloso falharia quando faltasse alguma moeda (0,30 com 0,25 ×1 e 0,10 ×3, por exemplo),
/// por isso a escolha é feita por programação dinâmica sobre as denominações.
/// </summary>
public static class CalculadoraDeTroco
{
    private const int Impossivel = int.MaxValue;

    /// <returns>As moedas do troco, ou <c>null</c> quando não dá para formar o valor com o cofre.</returns>
    public static IReadOnlyDictionary<decimal, int>? Calcular(decimal valor, CofreDeMoedas cofre)
    {
        ArgumentNullException.ThrowIfNull(cofre);
        ArgumentOutOfRangeException.ThrowIfLessThan(valor, 0m);

        var emCentavos = valor * 100m;
        if (emCentavos != decimal.Truncate(emCentavos))
        {
            return null;
        }

        var alvo = (int)emCentavos;
        if (alvo == 0)
        {
            return new Dictionary<decimal, int>();
        }

        var denominacoes = Moedas.Validas.OrderByDescending(moeda => moeda).ToArray();
        var minimo = new int[denominacoes.Length + 1][];
        var usadas = new int[denominacoes.Length + 1][];

        minimo[0] = Enumerable.Repeat(Impossivel, alvo + 1).ToArray();
        minimo[0][0] = 0;
        usadas[0] = new int[alvo + 1];

        for (var i = 1; i <= denominacoes.Length; i++)
        {
            var moeda = denominacoes[i - 1];
            var centavos = (int)(moeda * 100m);
            var disponiveis = cofre.Quantidade(moeda);

            minimo[i] = new int[alvo + 1];
            usadas[i] = new int[alvo + 1];

            for (var parcial = 0; parcial <= alvo; parcial++)
            {
                var melhor = Impossivel;
                var melhorQuantidade = 0;

                for (var k = 0; k <= Math.Min(disponiveis, parcial / centavos); k++)
                {
                    var anterior = minimo[i - 1][parcial - (k * centavos)];
                    if (anterior != Impossivel && anterior + k <= melhor)
                    {
                        melhor = anterior + k;
                        melhorQuantidade = k;
                    }
                }

                minimo[i][parcial] = melhor;
                usadas[i][parcial] = melhorQuantidade;
            }
        }

        if (minimo[denominacoes.Length][alvo] == Impossivel)
        {
            return null;
        }

        var troco = new Dictionary<decimal, int>();
        var restante = alvo;
        for (var i = denominacoes.Length; i >= 1; i--)
        {
            var quantidade = usadas[i][restante];
            if (quantidade > 0)
            {
                troco[denominacoes[i - 1]] = quantidade;
                restante -= quantidade * (int)(denominacoes[i - 1] * 100m);
            }
        }

        return troco;
    }
}
