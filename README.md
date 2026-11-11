# MaquinaDeVendas

[![CI](https://github.com/LucasL32323/MaquinaDeVendas/actions/workflows/ci.yml/badge.svg)](https://github.com/LucasL32323/MaquinaDeVendas/actions/workflows/ci.yml)

Máquina de vendas em C# (.NET 10): recebe moedas de real, vende produtos por código e devolve o troco **com o menor número de moedas possível, respeitando o que existe no cofre**. Se não houver moedas para o troco, a venda é recusada e nada muda.

## Como o TDD foi aplicado

Cada regra nasceu de um teste que falhava (Red), ganhou o código para passar (Green) e só então foi limpa (Refactor).

| Etapa | Exemplo real do projeto |
| --- | --- |
| **Red** | Os testes foram escritos antes de existir qualquer classe de `Vend.Core`; o projeto de testes nem compilava. Entre eles, `Calcular_FaltaMoedaDeCinco_CombinaOutrasMoedasEmVezDeFalhar`. |
| **Green** | A `CalculadoraDeTroco` usa programação dinâmica sobre as denominações. Um algoritmo guloso falharia exatamente nesse teste: para R$ 0,30 com uma moeda de 0,25 e três de 0,10, ele pega a de 0,25 e trava nos 0,05 que faltam. |
| **Refactor** | Validar a moeda ficou concentrado em `Moedas.GarantirValida`, usado pelo cofre e pela máquina, sem mudar nenhum teste. |

Dois testes protegem o comportamento mais delicado: `Selecionar_SemMoedasParaOTroco_RecusaEMantemTudoComoEstava` garante que uma recusa não mexe em crédito, estoque, cofre nem total arrecadado, e `Selecionar_TrocoPodeUsarMoedasInseridasNaPropriaCompra` cobre o caso em que o cliente paga com moedas que a máquina ainda não tinha.

## Arquitetura

```
MaquinaDeVendas/
├── src/
│   ├── Vend.Core/   regras de negócio, sem dependências
│   └── Vend.Cli/    console interativo
└── tests/
    └── Vend.Tests/  xUnit
```

```
Vend.Cli ──▶ Vend.Core ◀── Vend.Tests

Vend.Core:  MaquinaDeVendas ──▶ CalculadoraDeTroco ──▶ CofreDeMoedas ──▶ Moedas
                  │
                  └──▶ Produto, ResultadoDaVenda
```

## Regras de negócio

1. Só existem moedas de R$ 0,05, 0,10, 0,25, 0,50 e 1,00. Qualquer outra lança `ArgumentException` e não gera crédito.
2. O preço de todo produto é maior que zero e múltiplo de R$ 0,05, assim o troco sempre é matematicamente possível.
3. A venda é avaliada nesta ordem: código existe → há estoque → crédito suficiente → há troco.
4. Recusa por código inexistente, esgotado, crédito insuficiente (informando o valor que falta) ou falta de troco **não altera nada**: o cliente pode inserir mais moedas ou cancelar.
5. As moedas do cliente entram no cofre antes do troco ser calculado, então podem ser usadas no próprio troco. Se não houver troco, elas não entram.
6. O troco usa o menor número de moedas possível entre as que existem no cofre.
7. Venda concluída: o cofre recebe as moedas inseridas e perde as do troco, o estoque cai em 1, o crédito zera e o total arrecadado soma o preço.
8. `Cancelar` devolve exatamente as moedas inseridas e zera o crédito, sem mexer no cofre.
9. Códigos de produto são únicos e não diferenciam maiúsculas de minúsculas.

## Decisões técnicas

- **`decimal` para dinheiro**, com o troco calculado em centavos inteiros dentro da `CalculadoraDeTroco`.
- **Programação dinâmica em vez de guloso**: o sistema de moedas é "canônico", então o guloso funcionaria com cofre ilimitado, mas deixa de funcionar quando o cofre tem estoque limitado.
- **A venda é confirmada só no fim**: o troco é calculado sobre uma cópia do cofre (`Clonar`), e o estado real só muda se tudo deu certo. Assim não existe estado intermediário inconsistente.
- **`ResultadoDaVenda` em vez de exceções** para as recusas: não ter troco ou crédito é um resultado esperado do negócio, não uma falha de programação.
- `TreatWarningsAsErrors` ligado nos projetos de produção. No projeto de testes ele fica desligado, porque os analisadores do xUnit mudam entre versões.

## Como rodar

```bash
dotnet test
dotnet run --project src/Vend.Cli
```

Comandos do console: `moeda <valor>`, `pedir <código>`, `cancelar`, `estado`, `sair`. Exemplo:

```
moeda 1
moeda 1
pedir A1
Crédito insuficiente: faltam R$ 0,50.
moeda 1
pedir A1
Retire seu produto: Água. Troco: 1×R$ 0,50
```
