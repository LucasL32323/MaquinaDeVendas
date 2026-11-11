using Vend.Core;
using Xunit;

namespace Vend.Tests;

public class CofreDeMoedasTests
{
    [Fact]
    public void Construtor_SemMoedas_ComecaVazio()
    {
        var cofre = new CofreDeMoedas();

        Assert.Equal(0m, cofre.Total);
        Assert.Equal(0, cofre.Quantidade(0.50m));
    }

    [Fact]
    public void Adicionar_VariasMoedas_AcumulaQuantidadeETotal()
    {
        var cofre = new CofreDeMoedas();

        cofre.Adicionar(1.00m, 3);
        cofre.Adicionar(0.25m);
        cofre.Adicionar(0.25m);

        Assert.Equal(3, cofre.Quantidade(1.00m));
        Assert.Equal(2, cofre.Quantidade(0.25m));
        Assert.Equal(3.50m, cofre.Total);
    }

    [Theory]
    [InlineData(0.02)]
    [InlineData(0.30)]
    [InlineData(2.00)]
    [InlineData(0)]
    public void Adicionar_MoedaInexistente_LancaArgumentException(double valor)
    {
        var cofre = new CofreDeMoedas();
        Action acao = () => cofre.Adicionar((decimal)valor);

        Assert.Throws<ArgumentException>(acao);
    }

    [Fact]
    public void Adicionar_QuantidadeNaoPositiva_LancaArgumentOutOfRange()
    {
        var cofre = new CofreDeMoedas();
        Action acao = () => cofre.Adicionar(1.00m, 0);

        Assert.Throws<ArgumentOutOfRangeException>(acao);
    }

    [Fact]
    public void Remover_MoedasDisponiveis_DiminuiAQuantidade()
    {
        var cofre = new CofreDeMoedas();
        cofre.Adicionar(0.50m, 4);

        cofre.Remover(new Dictionary<decimal, int> { [0.50m] = 3 });

        Assert.Equal(1, cofre.Quantidade(0.50m));
    }

    [Fact]
    public void Remover_MaisMoedasDoQueExistem_LancaInvalidOperationESemAlterarNada()
    {
        var cofre = new CofreDeMoedas();
        cofre.Adicionar(0.50m, 1);
        cofre.Adicionar(0.10m, 5);
        var pedido = new Dictionary<decimal, int> { [0.10m] = 2, [0.50m] = 2 };
        Action acao = () => cofre.Remover(pedido);

        Assert.Throws<InvalidOperationException>(acao);
        Assert.Equal(1, cofre.Quantidade(0.50m));
        Assert.Equal(5, cofre.Quantidade(0.10m));
    }

    [Fact]
    public void Clonar_AlterarACopia_NaoAlteraOOriginal()
    {
        var original = new CofreDeMoedas();
        original.Adicionar(1.00m, 2);

        var copia = original.Clonar();
        copia.Adicionar(1.00m, 5);

        Assert.Equal(2, original.Quantidade(1.00m));
        Assert.Equal(7, copia.Quantidade(1.00m));
    }
}
