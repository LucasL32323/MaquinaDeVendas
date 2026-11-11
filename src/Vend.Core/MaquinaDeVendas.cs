namespace Vend.Core;

/// <summary>
/// Máquina de vendas: recebe moedas, vende produtos por código e devolve troco com as moedas do cofre.
/// Uma recusa nunca altera o estado: o crédito continua lá e o cliente pode completar o valor ou cancelar.
/// </summary>
public sealed class MaquinaDeVendas
{
    private readonly Dictionary<string, Slot> _slots = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<decimal, int> _inseridas = [];

    public MaquinaDeVendas(CofreDeMoedas cofre)
    {
        ArgumentNullException.ThrowIfNull(cofre);

        Cofre = cofre;
    }

    public CofreDeMoedas Cofre { get; }

    public decimal Credito => _inseridas.Sum(par => par.Key * par.Value);

    public decimal TotalArrecadado { get; private set; }

    public void AdicionarProduto(string codigo, Produto produto, int quantidade)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentNullException.ThrowIfNull(produto);
        ArgumentOutOfRangeException.ThrowIfNegative(quantidade);

        if (!_slots.TryAdd(codigo, new Slot(produto, quantidade)))
        {
            throw new InvalidOperationException($"O código {codigo} já está em uso.");
        }
    }

    public int EstoqueDe(string codigo) =>
        _slots.TryGetValue(codigo, out var slot)
            ? slot.Quantidade
            : throw new KeyNotFoundException($"Código inexistente: {codigo}.");

    public void InserirMoeda(decimal moeda)
    {
        Moedas.GarantirValida(moeda);

        _inseridas[moeda] = _inseridas.GetValueOrDefault(moeda) + 1;
    }

    public ResultadoDaVenda Selecionar(string codigo)
    {
        if (!_slots.TryGetValue(codigo, out var slot))
        {
            return ResultadoDaVenda.Recusado(MotivoDaRecusa.ProdutoInexistente);
        }

        if (slot.Quantidade == 0)
        {
            return ResultadoDaVenda.Recusado(MotivoDaRecusa.SemEstoque);
        }

        var preco = slot.Produto.Preco;
        if (Credito < preco)
        {
            return ResultadoDaVenda.Recusado(MotivoDaRecusa.CreditoInsuficiente, preco - Credito);
        }

        // As moedas do cliente entram no cofre antes do troco; se não houver troco, nada disso é confirmado.
        var cofreAposInserir = Cofre.Clonar();
        foreach (var (moeda, quantidade) in _inseridas)
        {
            cofreAposInserir.Adicionar(moeda, quantidade);
        }

        var troco = CalculadoraDeTroco.Calcular(Credito - preco, cofreAposInserir);
        if (troco is null)
        {
            return ResultadoDaVenda.Recusado(MotivoDaRecusa.SemTroco);
        }

        foreach (var (moeda, quantidade) in _inseridas)
        {
            Cofre.Adicionar(moeda, quantidade);
        }

        Cofre.Remover(troco);
        _inseridas.Clear();
        slot.Quantidade--;
        TotalArrecadado += preco;

        return ResultadoDaVenda.Vendido(slot.Produto, troco);
    }

    /// <summary>Devolve exatamente as moedas que o cliente inseriu e zera o crédito.</summary>
    public IReadOnlyDictionary<decimal, int> Cancelar()
    {
        var devolvidas = new Dictionary<decimal, int>(_inseridas);
        _inseridas.Clear();

        return devolvidas;
    }

    private sealed class Slot(Produto produto, int quantidade)
    {
        public Produto Produto { get; } = produto;

        public int Quantidade { get; set; } = quantidade;
    }
}
