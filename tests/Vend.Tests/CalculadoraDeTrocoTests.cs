using Vend.Core;
using Xunit;

namespace Vend.Tests;

public class CalculadoraDeTrocoTests
{
    public static TheoryData<decimal, string> ComEstoqueFarto => new()
    {
        { 0.05m, "0.05x1" },
        { 0.30m, "0.25x1+0.05x1" },
        { 0.65m, "0.50x1+0.10x1+0.05x1" },
        { 0.90m, "0.50x1+0.25x1+0.10x1+0.05x1" },
        { 1.45m, "1.00x1+0.25x1+0.10x2" },
        { 3.00m, "1.00x3" },
    };

    [Theory]
    [MemberData(nameof(ComEstoqueFarto))]
    public void Calcular_EstoqueFarto_UsaOMenorNumeroDeMoedas(decimal valor, string esperado)
    {
        var cofre = CofreCheio();

        var troco = CalculadoraDeTroco.Calcular(valor, cofre);

        Assert.Equal(esperado, DescricaoDeMoedas.De(troco));
    }

    [Fact]
    public void Calcular_ValorZero_RetornaTrocoVazio()
    {
        var troco = CalculadoraDeTroco.Calcular(0m, CofreCheio());

        Assert.Equal("<vazio>", DescricaoDeMoedas.De(troco));
    }

    [Fact]
    public void Calcular_FaltaMoedaDeCinco_CombinaOutrasMoedasEmVezDeFalhar()
    {
        // Guloso escolheria 0,25 e travaria nos 0,05 que faltam; o correto é 3 × 0,10.
        var cofre = new CofreDeMoedas();
        cofre.Adicionar(0.25m, 1);
        cofre.Adicionar(0.10m, 3);

        var troco = CalculadoraDeTroco.Calcular(0.30m, cofre);

        Assert.Equal("0.10x3", DescricaoDeMoedas.De(troco));
    }

    [Fact]
    public void Calcular_ExistemDuasCombinacoes_EscolheAQueUsaMenosMoedas()
    {
        var cofre = new CofreDeMoedas();
        cofre.Adicionar(0.25m, 1);
        cofre.Adicionar(0.10m, 5);
        cofre.Adicionar(0.05m, 5);

        var troco = CalculadoraDeTroco.Calcular(0.30m, cofre);

        Assert.Equal("0.25x1+0.05x1", DescricaoDeMoedas.De(troco));
    }

    [Fact]
    public void Calcular_NaoHaMoedasSuficientes_RetornaNulo()
    {
        var cofre = new CofreDeMoedas();
        cofre.Adicionar(1.00m, 5);

        var troco = CalculadoraDeTroco.Calcular(0.50m, cofre);

        Assert.Null(troco);
    }

    [Fact]
    public void Calcular_ValorQueNaoFechaComAsMoedas_RetornaNulo()
    {
        var troco = CalculadoraDeTroco.Calcular(0.03m, CofreCheio());

        Assert.Null(troco);
    }

    [Fact]
    public void Calcular_ValorNegativo_LancaArgumentOutOfRange()
    {
        var cofre = CofreCheio();
        Action acao = () => CalculadoraDeTroco.Calcular(-0.50m, cofre);

        Assert.Throws<ArgumentOutOfRangeException>(acao);
    }

    [Fact]
    public void Calcular_AposCalcular_NaoMexeNoCofre()
    {
        var cofre = CofreCheio();
        var totalAntes = cofre.Total;

        _ = CalculadoraDeTroco.Calcular(1.45m, cofre);

        Assert.Equal(totalAntes, cofre.Total);
    }

    private static CofreDeMoedas CofreCheio()
    {
        var cofre = new CofreDeMoedas();
        foreach (var moeda in Moedas.Validas)
        {
            cofre.Adicionar(moeda, 10);
        }

        return cofre;
    }
}
