using Vend.Core;
using Xunit;

namespace Vend.Tests;

public class MaquinaDeVendasTests
{
    [Fact]
    public void InserirMoeda_VariasMoedas_AcumulaOCredito()
    {
        var maquina = MaquinaComTroco();

        maquina.InserirMoeda(1.00m);
        maquina.InserirMoeda(0.50m);
        maquina.InserirMoeda(0.25m);

        Assert.Equal(1.75m, maquina.Credito);
    }

    [Theory]
    [InlineData(0.02)]
    [InlineData(2.00)]
    public void InserirMoeda_MoedaInvalida_LancaArgumentExceptionESemCredito(double valor)
    {
        var maquina = MaquinaComTroco();
        Action acao = () => maquina.InserirMoeda((decimal)valor);

        Assert.Throws<ArgumentException>(acao);
        Assert.Equal(0m, maquina.Credito);
    }

    [Fact]
    public void Selecionar_CreditoExato_VendeSemTroco()
    {
        var maquina = MaquinaComTroco();
        InserirTudo(maquina, 1.00m, 1.00m, 0.50m);

        var resultado = maquina.Selecionar("A1");

        Assert.True(resultado.Sucesso);
        Assert.Equal("Água", resultado.Produto?.Nome);
        Assert.Equal("<vazio>", DescricaoDeMoedas.De(resultado.Troco));
        Assert.Equal(0m, maquina.Credito);
        Assert.Equal(2, maquina.EstoqueDe("A1"));
        Assert.Equal(2.50m, maquina.TotalArrecadado);
    }

    [Fact]
    public void Selecionar_CreditoMaiorQueOPreco_DevolveOTroco()
    {
        var maquina = MaquinaComTroco();
        InserirTudo(maquina, 1.00m, 1.00m, 1.00m);

        var resultado = maquina.Selecionar("A1");

        Assert.True(resultado.Sucesso);
        Assert.Equal("0.50x1", DescricaoDeMoedas.De(resultado.Troco));
        Assert.Equal(0m, maquina.Credito);
    }

    [Fact]
    public void Selecionar_VendaComTroco_AtualizaOCofre()
    {
        var maquina = MaquinaComTroco();
        var totalAntes = maquina.Cofre.Total;
        InserirTudo(maquina, 1.00m, 1.00m, 1.00m);

        maquina.Selecionar("A1");

        // Entraram 3,00 e saíram 0,50 de troco.
        Assert.Equal(totalAntes + 2.50m, maquina.Cofre.Total);
        Assert.Equal(13, maquina.Cofre.Quantidade(1.00m));
        Assert.Equal(9, maquina.Cofre.Quantidade(0.50m));
    }

    [Fact]
    public void Selecionar_CreditoInsuficiente_RecusaEInformaOQueFalta()
    {
        var maquina = MaquinaComTroco();
        InserirTudo(maquina, 1.00m, 1.00m);

        var resultado = maquina.Selecionar("A1");

        Assert.False(resultado.Sucesso);
        Assert.Equal(MotivoDaRecusa.CreditoInsuficiente, resultado.Motivo);
        Assert.Equal(0.50m, resultado.ValorFaltante);
        Assert.Equal(2.00m, maquina.Credito);
        Assert.Equal(3, maquina.EstoqueDe("A1"));
    }

    [Fact]
    public void Selecionar_CodigoInexistente_Recusa()
    {
        var maquina = MaquinaComTroco();
        maquina.InserirMoeda(1.00m);

        var resultado = maquina.Selecionar("Z9");

        Assert.False(resultado.Sucesso);
        Assert.Equal(MotivoDaRecusa.ProdutoInexistente, resultado.Motivo);
        Assert.Equal(1.00m, maquina.Credito);
    }

    [Fact]
    public void Selecionar_ProdutoEsgotado_RecusaMesmoComCreditoDeSobra()
    {
        var maquina = MaquinaComTroco();
        InserirTudo(maquina, 1.00m, 1.00m, 1.00m, 1.00m, 1.00m);

        var resultado = maquina.Selecionar("B2");

        Assert.False(resultado.Sucesso);
        Assert.Equal(MotivoDaRecusa.SemEstoque, resultado.Motivo);
        Assert.Equal(5.00m, maquina.Credito);
    }

    [Fact]
    public void Selecionar_SemMoedasParaOTroco_RecusaEMantemTudoComoEstava()
    {
        var maquina = new MaquinaDeVendas(new CofreDeMoedas());
        maquina.AdicionarProduto("A1", new Produto("Água", 2.50m), 3);
        InserirTudo(maquina, 1.00m, 1.00m, 1.00m);

        var resultado = maquina.Selecionar("A1");

        Assert.False(resultado.Sucesso);
        Assert.Equal(MotivoDaRecusa.SemTroco, resultado.Motivo);
        Assert.Equal(3.00m, maquina.Credito);
        Assert.Equal(3, maquina.EstoqueDe("A1"));
        Assert.Equal(0m, maquina.Cofre.Total);
        Assert.Equal(0m, maquina.TotalArrecadado);
    }

    [Fact]
    public void Selecionar_TrocoPodeUsarMoedasInseridasNaPropriaCompra()
    {
        var maquina = new MaquinaDeVendas(new CofreDeMoedas());
        maquina.AdicionarProduto("A1", new Produto("Água", 2.50m), 3);
        InserirTudo(maquina, 1.00m, 1.00m, 0.50m, 0.50m);

        var resultado = maquina.Selecionar("A1");

        Assert.True(resultado.Sucesso);
        Assert.Equal("0.50x1", DescricaoDeMoedas.De(resultado.Troco));
        Assert.Equal(2.50m, maquina.Cofre.Total);
    }

    [Fact]
    public void Cancelar_ComCredito_DevolveAsMesmasMoedasInseridas()
    {
        var maquina = MaquinaComTroco();
        var totalAntes = maquina.Cofre.Total;
        InserirTudo(maquina, 0.25m, 0.25m, 1.00m);

        var devolvido = maquina.Cancelar();

        Assert.Equal("1.00x1+0.25x2", DescricaoDeMoedas.De(devolvido));
        Assert.Equal(0m, maquina.Credito);
        Assert.Equal(totalAntes, maquina.Cofre.Total);
    }

    [Fact]
    public void Cancelar_SemCredito_DevolveNada()
    {
        var maquina = MaquinaComTroco();

        var devolvido = maquina.Cancelar();

        Assert.Equal("<vazio>", DescricaoDeMoedas.De(devolvido));
    }

    [Fact]
    public void Selecionar_DuasVendasSeguidas_SomaOTotalArrecadado()
    {
        var maquina = MaquinaComTroco();
        InserirTudo(maquina, 1.00m, 1.00m, 1.00m, 0.50m, 0.25m);

        maquina.Selecionar("A1");
        InserirTudo(maquina, 1.00m, 1.00m, 1.00m, 1.00m);
        maquina.Selecionar("B2");
        maquina.Selecionar("B1");

        Assert.Equal(2.50m + 3.75m, maquina.TotalArrecadado);
    }

    [Fact]
    public void AdicionarProduto_CodigoRepetido_LancaInvalidOperation()
    {
        var maquina = MaquinaComTroco();
        Action acao = () => maquina.AdicionarProduto("A1", new Produto("Outro", 1.00m), 1);

        Assert.Throws<InvalidOperationException>(acao);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(2.52)]
    public void Produto_PrecoInvalido_LancaArgumentOutOfRange(double preco)
    {
        Action acao = () => _ = new Produto("Teste", (decimal)preco);

        Assert.Throws<ArgumentOutOfRangeException>(acao);
    }

    [Fact]
    public void AdicionarProduto_QuantidadeNegativa_LancaArgumentOutOfRange()
    {
        var maquina = MaquinaComTroco();
        Action acao = () => maquina.AdicionarProduto("C1", new Produto("Bala", 0.50m), -1);

        Assert.Throws<ArgumentOutOfRangeException>(acao);
    }

    private static MaquinaDeVendas MaquinaComTroco()
    {
        var cofre = new CofreDeMoedas();
        foreach (var moeda in Moedas.Validas)
        {
            cofre.Adicionar(moeda, 10);
        }

        var maquina = new MaquinaDeVendas(cofre);
        maquina.AdicionarProduto("A1", new Produto("Água", 2.50m), 3);
        maquina.AdicionarProduto("B1", new Produto("Salgadinho", 3.75m), 1);
        maquina.AdicionarProduto("B2", new Produto("Chocolate", 4.00m), 0);

        return maquina;
    }

    private static void InserirTudo(MaquinaDeVendas maquina, params decimal[] moedas)
    {
        foreach (var moeda in moedas)
        {
            maquina.InserirMoeda(moeda);
        }
    }
}
