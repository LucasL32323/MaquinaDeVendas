namespace Vend.Core;

/// <summary>Produto à venda. O preço precisa ser múltiplo de R$ 0,05 para que sempre exista troco possível.</summary>
public sealed record Produto
{
    public Produto(string nome, decimal preco)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(preco, 0m);

        if (preco % 0.05m != 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(preco), preco, "O preço deve ser múltiplo de R$ 0,05.");
        }

        Nome = nome;
        Preco = preco;
    }

    public string Nome { get; }

    public decimal Preco { get; }
}
