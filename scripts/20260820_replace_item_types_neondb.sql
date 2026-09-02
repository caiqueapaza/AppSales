-- Substitui o catálogo ativo de tipos de itens para reparo/conserto.
-- Recomendado para NeonDB: preserva historico desativando tipos antigos
-- e inserindo/reativando os novos tipos.
--
-- Rode em uma transação. Se a lista final não ficar como esperado, use ROLLBACK.

BEGIN;

UPDATE "Categories"
SET "IsActive" = FALSE,
    "Updated" = NOW();

WITH new_item_types("Name", "Description") AS (
  VALUES
    ('Calça', 'Calças em geral: jeans, social, alfaiataria, legging ou sarja.'),
    ('Bermuda/Shorts', 'Bermudas e shorts adultos ou infantis.'),
    ('Saia', 'Saias curtas, midi, longas, sociais ou casuais.'),
    ('Vestido', 'Vestidos curtos, longos, festa, casual ou infantil.'),
    ('Macacão', 'Macacões e macaquinhos.'),
    ('Camisa', 'Camisas sociais, polo, manga longa ou manga curta.'),
    ('Camiseta', 'Camisetas, t-shirts e camisetas básicas.'),
    ('Blusa', 'Blusas femininas, batas, cropped e peças leves superiores.'),
    ('Regata/Top', 'Regatas, tops e peças sem manga.'),
    ('Moletom', 'Blusa, calça ou conjunto de moletom.'),
    ('Casaco', 'Casacos, sobretudos e peças pesadas de frio.'),
    ('Jaqueta', 'Jaquetas jeans, couro, nylon, bomber ou similares.'),
    ('Blazer/Paletó', 'Blazers, paletós e peças de alfaiataria superiores.'),
    ('Terno/Conjunto', 'Ternos, conjuntos sociais e conjuntos coordenados.'),
    ('Uniforme', 'Uniformes escolares, profissionais, jalecos e aventais.'),
    ('Roupa infantil', 'Peças infantis quando não compensar classificar pelo tipo principal.'),
    ('Roupa fitness', 'Legging, top, bermuda ciclista, camiseta ou conjunto fitness.'),
    ('Roupa íntima/Praia', 'Moda íntima, biquíni, maiô, sunga e saída de praia.'),
    ('Cortina', 'Cortinas, voil, blackout e barrados.'),
    ('Lençol', 'Lençóis de solteiro, casal, queen, king ou infantil.'),
    ('Fronha', 'Fronhas e capas de travesseiro.'),
    ('Toalha', 'Toalhas de banho, rosto, lavabo, mesa ou praia.'),
    ('Colcha/Edredom', 'Colchas, edredons, cobre-leitos e mantas.'),
    ('Capa/Almofada', 'Capas de almofada, sofá, cadeira, banco ou similares.'),
    ('Bolsa/Acessório', 'Bolsas, necessaires, mochilas simples e acessórios têxteis.'),
    ('Outro item', 'Use quando a peça não se encaixar nos demais tipos.')
),
updated AS (
  UPDATE "Categories" c
  SET "Description" = n."Description",
      "IsActive" = TRUE,
      "Updated" = NOW()
  FROM new_item_types n
  WHERE LOWER(c."Name") = LOWER(n."Name")
  RETURNING c."Name"
)
INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
SELECT n."Name", n."Description", TRUE, NOW(), NOW()
FROM new_item_types n
WHERE NOT EXISTS (
  SELECT 1
  FROM "Categories" c
  WHERE LOWER(c."Name") = LOWER(n."Name")
);

SELECT "Id", "Name", "Description", "IsActive"
FROM "Categories"
ORDER BY "IsActive" DESC, "Name";

COMMIT;
