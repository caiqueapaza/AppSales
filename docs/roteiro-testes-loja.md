# Roteiro de Testes - APISales na loja

Este roteiro serve para validar a API em um teste acompanhado na loja, cobrindo os fluxos mais importantes: login, cadastros, venda, ordem de servico, pagamentos, alteracao de status e recibo em PDF.

## 1. Dados do ambiente

Preencha antes de iniciar:

| Item | Valor |
| --- | --- |
| URL da API | `https://...` |
| Swagger | `https://.../swagger` |
| Usuario ADM |  |
| Usuario funcionario |  |
| Data do teste |  |
| Responsavel pelo teste |  |

## 2. Preparacao

- Confirmar que a API esta online.
- Confirmar que o banco Neon esta acessivel.
- Abrir o Swagger ou Postman/Insomnia.
- Fazer login com usuario ADM.
- Copiar o token JWT retornado.
- Usar o token nas chamadas protegidas com `Authorization: Bearer TOKEN_AQUI`.

Teste rapido:

```http
POST /User/login
Content-Type: application/json

{
  "email": "admin@email.com",
  "password": "senha"
}
```

Resultado esperado:

- Status `200 OK`.
- Retorno com token.
- O token permite chamar `GET /User/me`.

## 3. Checklist geral

| # | Fluxo | Resultado esperado | Status | Observacoes |
| --- | --- | --- | --- | --- |
| 1 | Login ADM | Entra e retorna token |  |  |
| 2 | Login funcionario | Entra e retorna token |  |  |
| 3 | Dados do usuario atual | Retorna dados corretos |  |  |
| 4 | Cadastro de cliente | Cliente criado e listado |  |  |
| 5 | Cadastro de funcionario | Funcionario criado e listado |  |  |
| 6 | Cadastro de categoria | Categoria criada e listada |  |  |
| 7 | Cadastro de produto | Produto criado e listado |  |  |
| 8 | Cadastro de servico | Servico criado e listado |  |  |
| 9 | Sugestao de servico | Retorna sugestoes por categoria |  |  |
| 10 | Venda simples de produto | Venda criada com total correto |  |  |
| 11 | Ordem de servico | Ordem criada com item para reparo |  |  |
| 12 | Pagamento parcial | Valor pago e status atualizados |  |  |
| 13 | Pagamento final | Status de pagamento fica quitado |  |  |
| 14 | Mudanca de status do reparo | Status atualizado corretamente |  |  |
| 15 | Entrega do servico | Exige recebedor e funcionario |  |  |
| 16 | Recibo PDF | PDF abre com dados corretos |  |  |
| 17 | Bloqueio sem token | API retorna `401 Unauthorized` |  |  |
| 18 | Bloqueio de permissao | Funcionario nao acessa acoes ADM |  |  |

Use `OK`, `Falhou` ou `Pendente` na coluna Status.

## 4. Cadastros basicos

### 4.1 Cliente

```http
POST /Customer
Authorization: Bearer TOKEN
Content-Type: application/json

{
  "name": "Cliente Teste Loja",
  "document": "00000000000",
  "email": "cliente.teste@email.com",
  "phone": "(11) 99999-0000",
  "cep": "00000-000",
  "address": "Rua de Teste, 123",
  "district": "Centro"
}
```

Validar:

- Criou com status `201 Created`.
- Aparece em `GET /Customer`.
- Ao buscar `GET /Customer/{id}`, os dados batem.

Anotar `CustomerId`: ______

### 4.2 Funcionario

```http
POST /Employee
Authorization: Bearer TOKEN_ADM
Content-Type: application/json

{
  "name": "Funcionario Teste Loja",
  "email": "funcionario.teste@email.com",
  "phone": "(11) 98888-0000",
  "position": "Atendimento"
}
```

Validar:

- Criou com status `201 Created`.
- Aparece em `GET /Employee`.

Anotar `EmployeeId`: ______

### 4.3 Categoria

```http
POST /Category
Authorization: Bearer TOKEN_ADM
Content-Type: application/json

{
  "name": "Roupa Teste"
}
```

Validar:

- Criou com status `201 Created`.
- Aparece em `GET /Category`.

Anotar `CategoryId`: ______

### 4.4 Produto

```http
POST /Product
Authorization: Bearer TOKEN_ADM
Content-Type: application/json

{
  "name": "Produto Teste Loja",
  "description": "Produto usado apenas no teste da loja",
  "price": 49.90
}
```

Validar:

- Criou com status `201 Created`.
- Aparece em `GET /Product`.

Anotar `ProductId`: ______

### 4.5 Servico

```http
POST /ServiceItem
Authorization: Bearer TOKEN_ADM
Content-Type: application/json

{
  "name": "Ajuste Teste Loja",
  "serviceType": "Adjustment",
  "description": "Servico usado apenas no teste da loja",
  "price": 35.00
}
```

Validar:

- Criou com status `201 Created`.
- Aparece em `GET /ServiceItem`.

Anotar `ServiceItemId`: ______

## 5. Sugestao de servico

```http
GET /ServiceItem/suggestions?categoryId=CATEGORY_ID&audienceType=Adult&actionType=Adjustment
Authorization: Bearer TOKEN
```

Resultado esperado:

- Status `200 OK`.
- Retorna lista de sugestoes ou lista vazia, sem erro.
- Se houver servicos cadastrados para o contexto, conferir se o valor sugerido faz sentido para a loja.

## 6. Venda simples de produto

```http
POST /Sale
Authorization: Bearer TOKEN
Content-Type: application/json

{
  "customerId": CUSTOMER_ID,
  "sellerEmployeeId": EMPLOYEE_ID,
  "deliveryDate": "2026-08-22T18:00:00",
  "orderStatus": "Received",
  "paymentStatus": "Pending",
  "paymentMethod": "Pix",
  "problemDescription": null,
  "notes": "Venda teste feita na loja",
  "discountAmount": 0,
  "amountPaid": 49.90,
  "items": [
    {
      "productId": PRODUCT_ID,
      "serviceItemId": null,
      "executorEmployeeId": null,
      "quantity": 1,
      "unitPrice": 49.90,
      "itemStatus": "Delivered",
      "itemDescription": "Produto Teste Loja",
      "notes": "Item vendido no teste"
    }
  ],
  "entryItems": []
}
```

Validar:

- Status `201 Created`.
- Total da venda esta correto.
- Valor pago aparece corretamente.
- Venda aparece em `GET /Sale`.

Anotar `SaleId` da venda simples: ______

## 7. Ordem de servico com reparo

```http
POST /Sale
Authorization: Bearer TOKEN
Content-Type: application/json

{
  "customerId": CUSTOMER_ID,
  "sellerEmployeeId": EMPLOYEE_ID,
  "deliveryDate": "2026-08-25T18:00:00",
  "orderStatus": "Received",
  "paymentStatus": "Pending",
  "paymentMethod": "Pix",
  "problemDescription": "Cliente solicitou ajuste em peca de roupa",
  "notes": "Ordem teste feita na loja",
  "discountAmount": 0,
  "amountPaid": 10.00,
  "items": [],
  "entryItems": [
    {
      "categoryId": CATEGORY_ID,
      "description": "Calca jeans teste",
      "conditionNotes": "Peca recebida em bom estado",
      "audienceType": "Adult",
      "services": [
        {
          "actionType": "Adjustment",
          "serviceItemId": SERVICE_ITEM_ID,
          "executorEmployeeId": EMPLOYEE_ID,
          "quantity": 1,
          "unitPrice": 35.00,
          "repairDescription": "Ajuste de barra",
          "measurementUnit": "uni"
        }
      ]
    }
  ]
}
```

Validar:

- Status `201 Created`.
- Ordem possui cliente, funcionario, item recebido e servico.
- Total da ordem considera o servico.
- Pagamento inicial de `10.00` aparece no historico.

Anotar `SaleId` da ordem: ______

Anotar `EntryServiceId` do servico da ordem: ______

## 8. Pagamentos

### 8.1 Pagamento parcial

```http
POST /Sale/SALE_ID/payments
Authorization: Bearer TOKEN
Content-Type: application/json

{
  "amount": 15.00,
  "method": "Pix",
  "note": "Pagamento parcial no teste"
}
```

Validar:

- Status `200 OK` ou `201 Created`, conforme retorno da API.
- Valor pago acumulado aumentou.
- Status de pagamento continua pendente/parcial se ainda faltar valor.

### 8.2 Pagamento final

```http
POST /Sale/SALE_ID/payments
Authorization: Bearer TOKEN
Content-Type: application/json

{
  "amount": 10.00,
  "method": "Dinheiro",
  "note": "Quitacao no teste"
}
```

Validar:

- Valor pago nao ultrapassa o total.
- Ao quitar o valor total, status de pagamento fica como pago/quitado.
- Tentar pagar acima do total deve retornar erro.

## 9. Status do servico

Executar uma chamada por etapa e conferir se a consulta da ordem reflete a mudanca.

```http
PUT /Sale/entry-service/ENTRY_SERVICE_ID/status
Authorization: Bearer TOKEN
Content-Type: application/json

{
  "itemStatus": "InRepair",
  "deliveredToName": null,
  "deliveredByEmployeeId": null,
  "deliveryNote": "Servico iniciado no teste"
}
```

Depois testar:

- `Ready`
- `Delivered`
- `Canceled`, somente se fizer sentido para o teste.

Para entrega:

```http
PUT /Sale/entry-service/ENTRY_SERVICE_ID/status
Authorization: Bearer TOKEN
Content-Type: application/json

{
  "itemStatus": "Delivered",
  "deliveredToName": "Cliente Teste Loja",
  "deliveredByEmployeeId": EMPLOYEE_ID,
  "deliveryNote": "Entregue durante o teste"
}
```

Validar:

- Entrega sem `deliveredToName` deve falhar.
- Entrega sem `deliveredByEmployeeId` deve falhar.
- Entrega completa deve atualizar o status.

## 10. Recibo em PDF

```http
GET /Sale/SALE_ID/receipt-pdf
Authorization: Bearer TOKEN
```

Validar:

- Retorna PDF.
- O arquivo abre normalmente.
- Nome da loja, cliente, itens, servicos, valores, pagamento e observacoes estao corretos.
- Testar tanto venda simples quanto ordem de servico.

## 11. Testes de seguranca e permissao

### Sem token

Chamar qualquer endpoint protegido sem o header `Authorization`.

Exemplo:

```http
GET /Customer
```

Resultado esperado:

- Status `401 Unauthorized`.

### Funcionario tentando acao ADM

Com token de funcionario, tentar criar categoria, produto, servico ou funcionario.

Exemplo:

```http
POST /Product
Authorization: Bearer TOKEN_FUNCIONARIO
Content-Type: application/json

{
  "name": "Produto Bloqueado",
  "description": "Nao deve criar",
  "price": 10.00
}
```

Resultado esperado:

- Status `403 Forbidden`.
- Nenhum registro criado.

## 12. Testes de erro de regra de negocio

| Cenario | Como testar | Resultado esperado |
| --- | --- | --- |
| Venda vazia | Criar venda sem `items` e sem `entryItems` | API recusa |
| Desconto maior que subtotal | Enviar `discountAmount` maior que soma dos itens | API recusa |
| Pagamento maior que total | Enviar `amountPaid` maior que total | API recusa |
| Item de reparo sem servico | Enviar `entryItems` com `services: []` | API recusa |
| Publico invalido | Enviar `audienceType: "Outro"` | API recusa |
| Acao invalida | Enviar `actionType: "Outro"` | API recusa |
| Unidade invalida | Enviar `measurementUnit: "kg"` | API recusa |

## 13. Criterios para considerar aprovado

O teste pode ser considerado aprovado se:

- Login funciona para ADM e funcionario.
- Cadastros principais funcionam.
- Venda simples e ordem de servico sao criadas sem erro.
- Valores de subtotal, desconto, total e pagamento ficam corretos.
- Status do servico acompanha o fluxo real da loja.
- Recibo PDF abre e apresenta dados corretos.
- Acoes administrativas ficam bloqueadas para usuario sem perfil ADM.
- Erros importantes retornam mensagens compreensiveis.

## 14. Registro de problemas encontrados

| # | Fluxo | Problema | Gravidade | Responsavel | Status |
| --- | --- | --- | --- | --- | --- |
| 1 |  |  |  |  |  |
| 2 |  |  |  |  |  |
| 3 |  |  |  |  |  |

Gravidade sugerida:

- Alta: impede venda, ordem, pagamento ou entrega.
- Media: atrapalha o uso, mas existe contorno.
- Baixa: texto, layout, mensagem ou melhoria pequena.
