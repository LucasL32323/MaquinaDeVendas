namespace Vend.Core;

public enum MotivoDaRecusa
{
    Nenhum,
    ProdutoInexistente,
    SemEstoque,
    CreditoInsuficiente,
    SemTroco,
}

/// <summary>Resultado de pedir um produto: venda concluída (com troco) ou recusada (com o motivo).</summary>
public sealed record ResultadoDaVenda
{
    private ResultadoDaVenda(
        bool sucesso,
        MotivoDaRecusa motivo,
        Produto? produto,
        IReadOnlyDictionary<decimal, int> troco,
        decimal valorFaltante)
    {
        Sucesso = sucesso;
        Motivo = motivo;
        Produto = produto;
        Troco = troco;
        ValorFaltante = valorFaltante;
    }

    public bool Sucesso { get; }

    public MotivoDaRecusa Motivo { get; }

    public Produto? Produto { get; }

    public IReadOnlyDictionary<decimal, int> Troco { get; }

    public decimal ValorFaltante { get; }

    internal static ResultadoDaVenda Vendido(Produto produto, IReadOnlyDictionary<decimal, int> troco) =>
        new(true, MotivoDaRecusa.Nenhum, produto, troco, 0m);

    internal static ResultadoDaVenda Recusado(MotivoDaRecusa motivo, decimal valorFaltante = 0m) =>
        new(false, motivo, null, new Dictionary<decimal, int>(), valorFaltante);
}
