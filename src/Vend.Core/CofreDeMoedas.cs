namespace Vend.Core;

/// <summary>Estoque de moedas guardado na máquina, usado para dar troco.</summary>
public sealed class CofreDeMoedas
{
    private readonly Dictionary<decimal, int> _moedas = [];

    public decimal Total => _moedas.Sum(par => par.Key * par.Value);

    public int Quantidade(decimal moeda) => _moedas.GetValueOrDefault(moeda);

    public void Adicionar(decimal moeda, int quantidade = 1)
    {
        Moedas.GarantirValida(moeda);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(quantidade, 0);

        _moedas[moeda] = Quantidade(moeda) + quantidade;
    }

    /// <summary>Tira as moedas pedidas. Tudo ou nada: se faltar alguma, nada é removido.</summary>
    public void Remover(IReadOnlyDictionary<decimal, int> moedas)
    {
        ArgumentNullException.ThrowIfNull(moedas);

        foreach (var (moeda, quantidade) in moedas)
        {
            if (quantidade > Quantidade(moeda))
            {
                throw new InvalidOperationException($"O cofre não tem {quantidade} moeda(s) de {moeda:0.00}.");
            }
        }

        foreach (var (moeda, quantidade) in moedas)
        {
            _moedas[moeda] = Quantidade(moeda) - quantidade;
        }
    }

    public CofreDeMoedas Clonar()
    {
        var copia = new CofreDeMoedas();
        foreach (var (moeda, quantidade) in _moedas.Where(par => par.Value > 0))
        {
            copia.Adicionar(moeda, quantidade);
        }

        return copia;
    }
}
