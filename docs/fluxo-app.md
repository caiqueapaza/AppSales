# Fluxo desenhavel do app

Este arquivo reflete o fluxo atual do sistema: venda com produtos, entrada de itens para reparo, servicos por item, acompanhamento de status, pagamentos parciais e recibo em PDF.

Voce pode colar os blocos Mermaid em:

- https://mermaid.live
- diagrams.net / draw.io, usando `Insert > Advanced > Mermaid`
- Notion, GitHub ou Markdown Preview com suporte a Mermaid

## Visao geral atual

```mermaid
flowchart LR
    Login["Login"]
    Perfil["Meu perfil"]
    Admin["Administrador"]
    Operacao["Funcionario / Operacao"]

    Login --> Perfil
    Login --> Admin
    Login --> Operacao

    Admin --> Funcionarios["Funcionarios"]
    Admin --> Acessos["Acessos ao app"]
    Admin --> Categorias["Categorias de itens"]
    Admin --> Produtos["Produtos"]
    Admin --> Servicos["Servicos por tipo/categoria"]
    Admin --> ClientesAdmin["Clientes"]
    Admin --> VendasAdmin["Vendas e cancelamentos"]

    Operacao --> Clientes["Cadastrar/consultar cliente"]
    Operacao --> Ordem["Criar ordem/venda"]
    Operacao --> StatusServico["Atualizar status dos servicos"]
    Operacao --> Pagamentos["Registrar pagamentos"]
    Operacao --> Recibo["Gerar recibo PDF"]

    Categorias --> Ordem
    Produtos --> Ordem
    Servicos --> Ordem
    Funcionarios --> Ordem
    Clientes --> Ordem
    Ordem --> StatusServico
    Ordem --> Pagamentos
    Ordem --> Recibo
```

## Fluxo principal da ordem

```mermaid
flowchart TD
    Inicio["Cliente chega com pedido"]
    ClienteExiste{"Cliente ja cadastrado?"}
    CadastrarCliente["Cadastrar cliente"]
    AbrirOrdem["Abrir ordem/venda"]
    EscolherTipo{"O pedido tem o que?"}

    ProdutosVenda["Adicionar produtos vendidos"]
    EntradaReparo["Adicionar item recebido para reparo"]
    Categoria["Selecionar categoria do item"]
    DescricaoItem["Descrever item e estado de entrada"]
    Sugestoes["Buscar servicos sugeridos pela categoria"]
    ServicosItem["Adicionar um ou mais servicos ao item"]
    Medidas["Informar unidade: uni, cm ou m"]
    Executor["Selecionar executor, se houver"]

    Valores["Calcular produtos + servicos"]
    Desconto["Aplicar desconto"]
    PagamentoInicial{"Teve pagamento inicial?"}
    RegistrarInicial["Criar pagamento inicial no historico"]
    Salvar["Salvar ordem"]
    Acompanhar["Acompanhar servicos, pagamento e entrega"]

    Inicio --> ClienteExiste
    ClienteExiste -- "Nao" --> CadastrarCliente --> AbrirOrdem
    ClienteExiste -- "Sim" --> AbrirOrdem

    AbrirOrdem --> EscolherTipo
    EscolherTipo -- "Produto simples" --> ProdutosVenda --> Valores
    EscolherTipo -- "Reparo/conserto" --> EntradaReparo
    EscolherTipo -- "Ambos" --> ProdutosVendaAmbos["Adicionar produtos vendidos"] --> EntradaReparo

    EntradaReparo --> Categoria --> DescricaoItem --> Sugestoes --> ServicosItem --> Medidas --> Executor --> Valores
    Valores --> Desconto --> PagamentoInicial
    PagamentoInicial -- "Sim" --> RegistrarInicial --> Salvar
    PagamentoInicial -- "Nao" --> Salvar
    Salvar --> Acompanhar
```

## Detalhe do reparo por item

```mermaid
flowchart TD
    Item["Item recebido para reparo"]
    Categoria["Categoria: calca, camisa, vestido, etc."]
    Descricao["Descricao do item"]
    Estado["Estado/observacao de entrada"]
    Servico1["Servico 1"]
    Servico2["Servico 2"]
    ServicoN["Outros servicos"]

    Item --> Categoria
    Item --> Descricao
    Item --> Estado
    Item --> Servico1
    Item --> Servico2
    Item --> ServicoN

    Servico1 --> Tipo1["Servico cadastrado"]
    Servico1 --> Medida1["Quantidade + unidade"]
    Servico1 --> Valor1["Valor unitario"]
    Servico1 --> Status1["Status do servico"]
    Servico1 --> Executor1["Executor opcional"]

    Servico2 --> Tipo2["Servico cadastrado"]
    Servico2 --> Medida2["Quantidade + unidade"]
    Servico2 --> Valor2["Valor unitario"]
    Servico2 --> Status2["Status do servico"]
    Servico2 --> Executor2["Executor opcional"]
```

## Sugestao de servicos por categoria

```mermaid
flowchart LR
    Categoria["Categoria ativa"]
    NomeCategoria["Nome da categoria normalizado"]
    ServicosAtivos["Servicos ativos"]
    TipoServico["ServiceType do servico"]
    Filtrar["Seleciona servicos em que ServiceType = categoria"]
    Sugestoes["Retorna sugestoes"]

    Sugestoes --> Barra["Se nome contem barra: pede cm"]
    Sugestoes --> Gola["Se nome contem gola: pede cm"]
    Sugestoes --> Geral["Outros: unidade padrao uni"]

    Categoria --> NomeCategoria --> Filtrar
    ServicosAtivos --> TipoServico --> Filtrar
    Filtrar --> Sugestoes
```

## Regras atuais da criacao da ordem

```mermaid
flowchart TD
    Criar["Criar ordem/venda"]
    Cliente["Cliente precisa existir"]
    Vendedor["Vendedor precisa existir"]
    Conteudo{"Tem produto ou item de reparo?"}

    Produto["Produto vendido"]
    ProdutoRegra["Produto deve informar ProductId"]
    ProdutoSemServico["Produto nao informa ServiceItemId"]
    ProdutoSemExecutor["Produto nao informa executor"]

    Reparo["Item de reparo"]
    CategoriaExiste["Categoria precisa existir"]
    TemServico["Cada item precisa ter pelo menos 1 servico"]
    ServicoExiste["Servico precisa existir"]
    ExecutorValido["Executor precisa existir, se informado"]
    UnidadeValida["Unidade deve ser uni, cm ou m"]

    Totais["Calcula subtotal: produtos + servicos"]
    Desconto["Desconto nao passa do subtotal"]
    Pago["Valor pago nao passa do total final"]
    OK["Ordem pode ser salva"]

    Criar --> Cliente --> Vendedor --> Conteudo
    Conteudo -- "Nao" --> ErroConteudo["Erro: incluir produto ou reparo"]
    Conteudo -- "Produto" --> Produto --> ProdutoRegra --> ProdutoSemServico --> ProdutoSemExecutor --> Totais
    Conteudo -- "Reparo" --> Reparo --> CategoriaExiste --> TemServico --> ServicoExiste --> ExecutorValido --> UnidadeValida --> Totais
    Conteudo -- "Ambos" --> ProdutoAmbos["Produto vendido"] --> ProdutoRegraAmbos["Produto deve informar ProductId"] --> ProdutoSemServicoAmbos["Produto nao informa ServiceItemId"] --> ProdutoSemExecutorAmbos["Produto nao informa executor"] --> Reparo
    Totais --> Desconto --> Pago --> OK
```

## Status do servico de reparo

```mermaid
stateDiagram-v2
    [*] --> Received: recebido
    Received --> InRepair: iniciou reparo
    InRepair --> Ready: pronto
    Ready --> Delivered: entregue
    Received --> Canceled: cancelado
    InRepair --> Canceled: cancelado
    Ready --> Canceled: cancelado

    note right of Delivered
      Para entregar, precisa informar:
      nome de quem recebeu
      funcionario que entregou
      observacao opcional
    end note
```

## Status geral da venda

```mermaid
stateDiagram-v2
    [*] --> Received: ordem registrada
    Received --> InProgress: em andamento
    InProgress --> Ready: pronta
    Ready --> Delivered: entregue
    Received --> Canceled: cancelada
    InProgress --> Canceled: cancelada
    Ready --> Canceled: cancelada

    note right of Canceled
      Cancelar a venda continua exigindo ADM
      quando ela ainda nao estava cancelada.
    end note
```

## Pagamentos

```mermaid
flowchart TD
    Total["Total da ordem"]
    Inicial{"Valor pago no cadastro > 0?"}
    HistoricoInicial["Cria pagamento inicial no historico"]
    NovoPagamento["Adicionar novo pagamento"]
    Somar["Soma todos os pagamentos"]
    AtualizarPago["Atualiza AmountPaid"]
    Status{"Quanto foi pago?"}
    Pending["Pending"]
    Partial["Partial"]
    Paid["Paid"]

    Total --> Inicial
    Inicial -- "Sim" --> HistoricoInicial --> NovoPagamento
    Inicial -- "Nao" --> NovoPagamento
    NovoPagamento --> Somar --> AtualizarPago --> Status
    Status -- "0" --> Pending
    Status -- "Menor que total" --> Partial
    Status -- "Maior ou igual ao total" --> Paid
```

## Recibo PDF

```mermaid
flowchart TD
    Pedido["Abrir ordem"]
    Pdf["Gerar recibo PDF"]
    Loja["Dados da loja no appsettings"]
    Cliente["Dados do cliente"]
    Reparo["Resumo dos itens de reparo"]
    Servicos["Servicos, descricoes e valores"]
    Entrega["Nome de quem recebeu, se ja entregue"]
    Arquivo["PDF: ordem + cliente + id"]

    Pedido --> Pdf
    Loja --> Pdf
    Cliente --> Pdf
    Reparo --> Pdf
    Servicos --> Pdf
    Entrega --> Pdf
    Pdf --> Arquivo
```

## Mapa dos dados atual

```mermaid
erDiagram
    CUSTOMER ||--o{ SALE : "faz"
    EMPLOYEE ||--o{ SALE : "vende"
    SALE ||--o{ SALE_ITEM : "tem produto vendido"
    PRODUCT ||--o{ SALE_ITEM : "produto vendido"
    SALE ||--o{ SALE_ENTRY_ITEM : "tem item recebido"
    CATEGORY ||--o{ SALE_ENTRY_ITEM : "classifica item"
    SALE_ENTRY_ITEM ||--o{ SALE_ENTRY_ITEM_SERVICE : "tem servico"
    SERVICE_ITEM ||--o{ SALE_ENTRY_ITEM_SERVICE : "servico aplicado"
    EMPLOYEE ||--o{ SALE_ENTRY_ITEM_SERVICE : "executa"
    EMPLOYEE ||--o{ SALE_ENTRY_ITEM_SERVICE : "entrega"
    SALE ||--o{ SALE_PAYMENT : "recebe pagamento"
    EMPLOYEE ||--o{ SALE_PAYMENT : "recebeu pagamento"
    EMPLOYEE ||--o| USER : "pode ter acesso"

    SALE {
        int id
        int customerId
        int sellerEmployeeId
        string orderStatus
        string paymentStatus
        decimal subTotalAmount
        decimal discountAmount
        decimal totalAmount
        decimal amountPaid
    }

    SALE_ENTRY_ITEM {
        int id
        int saleId
        int categoryId
        string description
        string conditionNotes
    }

    SALE_ENTRY_ITEM_SERVICE {
        int id
        int saleEntryItemId
        int serviceItemId
        int executorEmployeeId
        int quantity
        string measurementUnit
        decimal unitPrice
        string itemStatus
        string deliveredToName
        int deliveredByEmployeeId
    }

    SALE_PAYMENT {
        int id
        int saleId
        decimal amount
        string method
        string note
        int receivedByEmployeeId
        datetime paidAt
    }

    SERVICE_ITEM {
        int id
        string name
        string serviceType
        decimal price
        bool isActive
    }
```

## Quem pode fazer o que

```mermaid
flowchart LR
    ADM["ADM"]
    Func["Funcionario com acesso"]

    ADM --> Produtos["Criar/editar/remover produtos"]
    ADM --> Servicos["Criar/editar/remover servicos"]
    ADM --> Categorias["Criar/editar/remover categorias"]
    ADM --> Funcionarios["Criar/editar/remover funcionarios"]
    ADM --> Acessos["Liberar acesso de funcionarios"]
    ADM --> ApagarVenda["Apagar venda"]
    ADM --> CancelarVenda["Cancelar venda"]

    Func --> VerCatalogo["Consultar catalogo"]
    Func --> Clientes["Cadastrar/editar clientes"]
    Func --> CriarOrdem["Criar ordem/venda"]
    Func --> AtualizarOrdem["Atualizar dados gerais da ordem"]
    Func --> StatusServico["Atualizar status dos servicos"]
    Func --> Pagamento["Adicionar pagamento"]
    Func --> Recibo["Gerar recibo PDF"]
    Func --> Perfil["Editar meu perfil"]
```

## Pontos para decidir agora

- A palavra na interface deve ser "venda", "ordem", "ordem de servico" ou "atendimento"?
- Produto vendido e item de reparo devem aparecer na mesma tela ou em abas separadas?
- O status geral da venda deve ser automatico conforme os servicos avancam?
- Entrega parcial precisa existir quando uma ordem tem varios itens?
- O pagamento deve bloquear entrega se ainda estiver pendente/parcial?
- O recibo deve incluir produtos vendidos tambem, ou so reparos como esta hoje?
- Quem pode alterar status de servico: qualquer funcionario ou somente o executor/ADM?
- As unidades `uni`, `cm` e `m` sao suficientes?
- As sugestoes por categoria devem ser configuradas pelo nome da categoria ou por um campo fixo?

## Roteiro de apresentacao

1. Fazer login.
2. Mostrar cliente.
3. Mostrar categorias e servicos configurados por tipo.
4. Criar uma ordem com item recebido para reparo.
5. Selecionar categoria e ver servicos sugeridos.
6. Adicionar medidas, executor, desconto e pagamento inicial.
7. Atualizar status do servico: recebido, em reparo, pronto, entregue.
8. Registrar pagamento parcial ou final.
9. Gerar recibo PDF.
10. Perguntar o que ficou estranho para o uso real da loja.
