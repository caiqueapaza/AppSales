-- Substitui o catálogo ativo de serviços de reparo/conserto.
-- Recomendado para NeonDB: preserva histórico desativando serviços antigos
-- e inserindo/reativando os novos serviços.
--
-- Os preços abaixo são valores-base para cadastro inicial. Ajuste conforme a loja.

BEGIN;

UPDATE "ServiceItens"
SET "IsActive" = FALSE,
    "UpdatedAt" = NOW();

WITH new_services("Name", "Description", "ServiceType", "Price") AS (
  VALUES
    ('Barra simples', 'Fazer barra comum em peça recebida.', 'Barra', 25.00),
    ('Barra italiana', 'Fazer barra italiana/dobrada.', 'Barra', 35.00),
    ('Barra original', 'Refazer barra preservando acabamento original da peça.', 'Barra', 40.00),
    ('Barra em tecido fino', 'Fazer barra em tecido delicado, leve ou escorregadio.', 'Barra', 40.00),
    ('Barra em tecido grosso', 'Fazer barra em tecido grosso ou pesado.', 'Barra', 30.00),
    ('Barra com acabamento especial', 'Fazer barra com acabamento diferenciado ou mais elaborado.', 'Barra', 45.00),

    ('Zíper simples', 'Trocar ou colocar zíper comum em peça simples.', 'Zíper', 35.00),
    ('Zíper invisível', 'Trocar ou colocar zíper invisível.', 'Zíper', 45.00),
    ('Zíper destacável', 'Trocar ou colocar zíper destacável/separável.', 'Zíper', 60.00),
    ('Zíper reforçado', 'Trocar ou colocar zíper reforçado.', 'Zíper', 55.00),
    ('Cursor de zíper', 'Trocar cursor/puxador do zíper sem trocar o zíper completo.', 'Zíper', 20.00),
    ('Zíper travando/solto', 'Reparar zíper quando não exige troca completa.', 'Zíper', 25.00),

    ('Cintura', 'Apertar, soltar ou ajustar cintura.', 'Ajuste', 35.00),
    ('Lateral', 'Apertar ou ajustar laterais da peça.', 'Ajuste', 35.00),
    ('Cós', 'Ajustar cós.', 'Ajuste', 40.00),
    ('Quadril', 'Ajustar região do quadril da peça.', 'Ajuste', 40.00),
    ('Gancho/cavalo', 'Ajustar gancho/cavalo da peça.', 'Ajuste', 35.00),
    ('Busto', 'Ajustar busto.', 'Ajuste', 45.00),
    ('Ombro', 'Ajustar ombro.', 'Ajuste', 45.00),
    ('Manga', 'Ajustar largura ou comprimento de manga.', 'Ajuste', 35.00),
    ('Alça', 'Regular, encurtar ou trocar alça.', 'Ajuste', 25.00),
    ('Gola/decote', 'Ajustar gola ou decote.', 'Ajuste', 35.00),
    ('Punho', 'Ajustar punho.', 'Ajuste', 30.00),
    ('Entrepernas', 'Ajustar largura ou costura interna da perna.', 'Ajuste', 35.00),
    ('Modelagem', 'Ajuste geral de caimento/modelagem da peça.', 'Ajuste', 60.00),

    ('Costura simples', 'Refazer costura aberta ou solta em trecho simples.', 'Costura', 20.00),
    ('Costura reforçada', 'Reforçar costura em área de maior tensão.', 'Costura', 30.00),
    ('Rasgo', 'Rasgo pequeno ou médio.', 'Costura', 30.00),
    ('Remendo simples', 'Aplicar remendo simples em tecido.', 'Remendo', 30.00),
    ('Remendo interno', 'Aplicar remendo interno/discreto.', 'Remendo', 35.00),
    ('Remendo em tecido grosso', 'Aplicar remendo em tecido grosso ou pesado.', 'Remendo', 40.00),
    ('Reforço entrepernas', 'Reforçar região entrepernas.', 'Remendo', 45.00),

    ('Botão comum', 'Botão comum solto, ausente ou danificado.', 'Botão', 10.00),
    ('Botão solto', 'Botão solto ou com costura fraca.', 'Botão', 8.00),
    ('Casa de botão', 'Fazer ou refazer casa de botão.', 'Botão', 15.00),
    ('Botão de pressão', 'Botão de pressão ausente ou danificado.', 'Botão', 20.00),
    ('Colchete/gancho', 'Colchete, gancho ou fecho pequeno.', 'Fecho', 15.00),

    ('Elástico', 'Elástico ausente, frouxo, apertado ou danificado.', 'Elástico', 30.00),
    ('Elástico embutido', 'Elástico embutido em canaleta ou acabamento interno.', 'Elástico', 35.00),
    ('Elástico regulável', 'Elástico com regulagem ou ajuste específico.', 'Elástico', 25.00),

    ('Forro completo', 'Forro completo da peça.', 'Forro', 70.00),
    ('Forro solto', 'Forro solto, descosturado ou parcialmente aberto.', 'Forro', 35.00),
    ('Forro novo', 'Forro novo em peça sem forro.', 'Forro', 80.00),

    ('Renda/detalhe', 'Renda, faixa, viés, acabamento ou detalhe decorativo.', 'Aplicação', 45.00),
    ('Etiqueta', 'Etiqueta, tag ou identificação.', 'Aplicação', 10.00),
    ('Velcro', 'Velcro ausente, gasto ou danificado.', 'Aplicação', 25.00),
    ('Patch/aplique', 'Patch, aplique ou remendo aparente.', 'Aplicação', 30.00),

    ('Bolso', 'Bolso rasgado, danificado, solto ou novo.', 'Bolso', 45.00),
    ('Bolso solto', 'Bolso solto ou aberto na costura.', 'Bolso', 25.00),
    ('Bolso novo', 'Bolso novo em peça sem bolso.', 'Bolso', 55.00),

    ('Ilhós', 'Ilhós ausente ou danificado.', 'Ilhós', 60.00),
    ('Costura reta longa', 'Costura reta em trecho longo.', 'Costura', 35.00),
    ('Acabamento de canto', 'Fazer acabamento ou reforço de canto.', 'Acabamento', 30.00),

    ('Alça reforçada', 'Trocar, costurar ou reforçar alça.', 'Alça', 50.00),
    ('Acessório têxtil', 'Costura simples em acessório têxtil.', 'Acessório', 45.00),

    ('Overlock', 'Acabamento em overlock para evitar desfiar.', 'Acabamento', 25.00),
    ('Acabamento simples', 'Acabamento simples de costura, borda ou bainha.', 'Acabamento', 20.00),
    ('Reforma geral', 'Reforma ampla combinando mais de um tipo de ajuste.', 'Reforma', 100.00),
    ('Outro serviço', 'Serviço avulso não listado no catálogo.', 'Outro', 30.00)
),
updated AS (
  UPDATE "ServiceItens" s
  SET "Description" = n."Description",
      "ServiceType" = n."ServiceType",
      "Price" = n."Price",
      "IsActive" = TRUE,
      "UpdatedAt" = NOW()
  FROM new_services n
  WHERE LOWER(s."Name") = LOWER(n."Name")
  RETURNING s."Name"
)
INSERT INTO "ServiceItens" ("Name", "Description", "ServiceType", "Price", "IsActive", "CreatedAt", "UpdatedAt")
SELECT n."Name", n."Description", n."ServiceType", n."Price", TRUE, NOW(), NOW()
FROM new_services n
WHERE NOT EXISTS (
  SELECT 1
  FROM "ServiceItens" s
  WHERE LOWER(s."Name") = LOWER(n."Name")
);

SELECT "Id", "Name", "ServiceType", "Price", "IsActive"
FROM "ServiceItens"
ORDER BY "IsActive" DESC, "ServiceType", "Name";

COMMIT;
