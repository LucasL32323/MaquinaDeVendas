using System.Globalization;
using Vend.Core;

var maquina = CriarMaquina();

Console.WriteLine("Máquina de vendas — comandos: moeda <valor> | pedir <código> | cancelar | estado | sair");
ImprimirCardapio(maquina);

string? linha;
while ((linha = Console.ReadLine()) is not null)
{
    var partes = linha.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (partes.Length == 0)
    {
        continue;
    }

    try
    {
        switch (partes[0].ToLowerInvariant())
        {
            case "moeda" when partes.Length == 2:
                maquina.InserirMoeda(LerValor(partes[1]));
                Console.WriteLine($"Crédito: {Dinheiro(maquina.Credito)}");
                break;

            case "pedir" when partes.Length == 2:
                Pedir(maquina, partes[1]);
                break;

            case "cancelar":
                Console.WriteLine($"Devolvido: {Moedas(maquina.Cancelar())}");
                break;

            case "estado":
                Console.WriteLine($"Crédito: {Dinheiro(maquina.Credito)} | Arrecadado: {Dinheiro(maquina.TotalArrecadado)} | Cofre: {Dinheiro(maquina.Cofre.Total)}");
                break;

            case "sair":
                return;

            default:
                Console.WriteLine("Comando não reconhecido.");
                break;
        }
    }
    catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException)
    {
        Console.WriteLine($"Erro: {ex.Message.Split(" (Parameter", StringSplitOptions.None)[0]}");
    }
}

static MaquinaDeVendas CriarMaquina()
{
    var cofre = new CofreDeMoedas();
    foreach (var moeda in Vend.Core.Moedas.Validas)
    {
        cofre.Adicionar(moeda, 10);
    }

    var maquina = new MaquinaDeVendas(cofre);
    maquina.AdicionarProduto("A1", new Produto("Água", 2.50m), 5);
    maquina.AdicionarProduto("A2", new Produto("Refrigerante", 4.50m), 5);
    maquina.AdicionarProduto("B1", new Produto("Salgadinho", 3.75m), 3);
    maquina.AdicionarProduto("B2", new Produto("Chocolate", 4.00m), 0);

    return maquina;
}

static void Pedir(MaquinaDeVendas maquina, string codigo)
{
    var resultado = maquina.Selecionar(codigo);

    if (resultado.Sucesso)
    {
        Console.WriteLine($"Retire seu produto: {resultado.Produto?.Nome}. Troco: {Moedas(resultado.Troco)}");
        return;
    }

    Console.WriteLine(resultado.Motivo switch
    {
        MotivoDaRecusa.ProdutoInexistente => "Código inexistente.",
        MotivoDaRecusa.SemEstoque => "Produto esgotado.",
        MotivoDaRecusa.CreditoInsuficiente => $"Crédito insuficiente: faltam {Dinheiro(resultado.ValorFaltante)}.",
        MotivoDaRecusa.SemTroco => "Sem troco para este valor: insira o valor exato ou cancele.",
        _ => "Não foi possível concluir a venda.",
    });
}

static void ImprimirCardapio(MaquinaDeVendas maquina)
{
    foreach (var (codigo, nome, preco) in new[] { ("A1", "Água", 2.50m), ("A2", "Refrigerante", 4.50m), ("B1", "Salgadinho", 3.75m), ("B2", "Chocolate", 4.00m) })
    {
        Console.WriteLine($"  {codigo}  {nome,-13} {Dinheiro(preco),9}  estoque: {maquina.EstoqueDe(codigo)}");
    }
}

static decimal LerValor(string texto) =>
    decimal.Parse(texto.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);

static string Dinheiro(decimal valor) =>
    "R$ " + valor.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');

static string Moedas(IReadOnlyDictionary<decimal, int> moedas) =>
    moedas.Count == 0
        ? "nenhum"
        : string.Join(" + ", moedas.OrderByDescending(par => par.Key).Select(par => $"{par.Value}×{Dinheiro(par.Key)}"));
