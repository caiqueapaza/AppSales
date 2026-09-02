# APISales - API de Vendas e Ordens de Serviço

API REST desenvolvida em **ASP.NET Core 8** para gerenciar o fluxo operacional de uma loja de vendas e serviços de reparo. O projeto contempla autenticação, controle de acesso, cadastro de clientes, funcionários, produtos, categorias, serviços, ordens de serviço, pagamentos parciais e geração de recibo em PDF.

Este repositório foi organizado para demonstrar práticas de desenvolvimento backend com .NET, Entity Framework Core, PostgreSQL, JWT, Swagger, AutoMapper e Docker.

## Funcionalidades

- Autenticação com JWT e controle de perfil administrativo.
- Cadastro e manutenção de usuários, funcionários, clientes, produtos, categorias e serviços.
- Gestão de acesso de funcionários ao aplicativo.
- Criação de vendas e ordens de serviço com produtos e itens para reparo.
- Sugestão de serviços por categoria do item.
- Controle de status dos serviços: recebido, em reparo, pronto, entregue e cancelado.
- Registro de pagamentos iniciais, parciais e finais.
- Cálculo de subtotal, desconto, total e status de pagamento.
- Geração de recibo em PDF com dados da loja, cliente, itens e serviços.
- Documentação interativa via Swagger em ambiente de desenvolvimento.
- Suporte a Docker para publicação da API.

## Tecnologias

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Npgsql
- JWT Bearer Authentication
- BCrypt
- AutoMapper
- QuestPDF
- Swagger / Swashbuckle
- Docker

## Arquitetura

O projeto segue uma separação em camadas simples, mantendo responsabilidades bem definidas:

```text
Application/
  Controllers/    Endpoints HTTP da API
  DTOs/           Objetos de entrada e saída
  Mappings/       Configuração do AutoMapper
  Services/       Serviços de autenticação e geração de PDF

Domain/
  Customers/      Entidades de clientes
  Employees/      Entidades de funcionários
  Products/       Produtos e categorias
  Sales/          Vendas, itens, serviços e pagamentos
  ServiceItens/   Serviços cadastráveis
  Users/          Usuários da aplicação

Infrastructure/
  Data/           DbContext e mapeamentos do EF Core
  Repositories/   Repositórios de acesso a dados

Migrations/       Histórico de migrations do banco
scripts/          Scripts SQL auxiliares para carga e consulta
docs/             Documentação complementar do fluxo da aplicação
assets/           Logos utilizadas na geração de recibos
```

## Principais Endpoints

| Recurso | Endpoint base | Descrição |
| --- | --- | --- |
| Autenticação | `POST /User/login` | Realiza login e retorna token JWT |
| Usuário atual | `GET /User/me` | Retorna dados do usuário autenticado |
| Acessos | `/User/employee-access` | Gerencia acesso de funcionários |
| Clientes | `/Customer` | CRUD de clientes |
| Funcionários | `/Employee` | CRUD de funcionários |
| Produtos | `/Product` | CRUD de produtos |
| Categorias | `/Category` | CRUD de categorias |
| Serviços | `/ServiceItem` | CRUD de serviços |
| Sugestões | `GET /ServiceItem/suggestions?categoryId=1&audienceType=Adult&actionType=Replacement` | Lista serviços gerais com valor médio sugerido |
| Vendas / Ordens | `/Sale` | Criação e gestão de vendas/ordens |
| Status do serviço | `PUT /Sale/entry-service/{id}/status` | Atualiza o status de um serviço da ordem |
| Pagamentos | `POST /Sale/{id}/payments` | Registra pagamento em uma ordem |
| Recibo PDF | `GET /Sale/{id}/receipt-pdf` | Gera recibo em PDF |

## Regras de Negócio

- Uma ordem precisa ter pelo menos um produto vendido ou um item de reparo.
- Itens de reparo precisam ter categoria e ao menos um serviço vinculado.
- Serviços podem usar unidade `uni`, `cm` ou `m`.
- Itens de reparo exigem público `Adult` ou `Child`.
- Serviços aplicados exigem ação `Adjustment`, `Replacement`, `Addition` ou `Removal`.
- O desconto não pode ser maior que o subtotal.
- O valor pago não pode ultrapassar o total final da ordem.
- Cancelamento e exclusões administrativas exigem perfil `ADM`.
- Entrega de serviço exige informar quem recebeu e qual funcionário realizou a entrega.
- O status de pagamento é recalculado a partir do histórico de pagamentos.

## Como Executar Localmente

### Pré-requisitos

- .NET SDK 8
- PostgreSQL
- Entity Framework Core Tools

Instalação do EF Core Tools, caso necessário:

```bash
dotnet tool install --global dotnet-ef
```

### Configuração

Configure a connection string e a chave JWT usando variáveis de ambiente, user secrets ou um arquivo de configuração local não versionado.

Exemplo de configuração:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=apisales;Username=postgres;Password=sua_senha"
  },
  "Jwt": {
    "Key": "sua-chave-jwt-com-tamanho-seguro"
  },
  "Store": {
    "Name": "Nome da Loja",
    "ReceiptTitle": "Recibo da Ordem de Serviço",
    "Address": "Endereço da loja",
    "Phone": "(00) 00000-0000",
    "Email": "contato@loja.com"
  }
}
```

> Antes de publicar este repositório, remova credenciais reais do `appsettings.json` e rotacione qualquer senha, token ou connection string que já tenha sido commitada.

### Banco de Dados

Execute as migrations:

```bash
dotnet ef database update
```

Os scripts SQL auxiliares ficam em `scripts/` e podem ser usados para criar dados iniciais, como usuário administrador e categorias.

### Executar a API

```bash
dotnet restore
dotnet run
```

Por padrão, a aplicação usa a porta definida na variável `PORT` ou `5150` quando a variável não existe.

Swagger em desenvolvimento:

```text
http://localhost:5150/swagger
```

## Executar com Docker

Build da imagem:

```bash
docker build -t apisales .
```

Execução do container:

```bash
docker run -p 10000:10000 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=apisales;Username=postgres;Password=sua_senha" \
  -e Jwt__Key="sua-chave-jwt-com-tamanho-seguro" \
  apisales
```

## Fluxo da Aplicação

O arquivo [`docs/fluxo-app.md`](docs/fluxo-app.md) contém diagramas Mermaid com o fluxo completo da aplicação, incluindo:

- visão geral do sistema;
- fluxo de criação de ordem;
- status de reparo;
- pagamentos;
- recibo PDF;
- mapa das entidades principais.

## Destaques Técnicos

- Uso de DTOs para separar contrato da API das entidades de domínio.
- Mapeamento automático com AutoMapper.
- Relacionamentos configurados via Fluent API no `AppDbContext`.
- Autenticação stateless com JWT.
- Restrições por perfil usando `[Authorize(Roles = "ADM")]`.
- Tratamento global de exceções via filtro.
- Logger customizado.
- Geração de PDF com layout profissional usando QuestPDF.
- Preparação para deploy em ambientes que fornecem porta por variável `PORT`.

## Status do Projeto

Projeto backend funcional, em evolução, com foco em um cenário real de loja que trabalha com venda de produtos e serviços de reparo.

Próximos pontos de melhoria possíveis:

- adicionar testes automatizados;
- criar `docker-compose` com API e PostgreSQL;
- mover configurações sensíveis para user secrets ou variáveis de ambiente;
- padronizar respostas de erro;
- expandir documentação dos exemplos de payload.

## Autor

Desenvolvido por **Caique** como projeto de portfólio backend.
