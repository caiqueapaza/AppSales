-- Opcional: apaga fisicamente categorias/tipos de itens que estão inativos
-- e não possuem vínculo com produtos ou ordens.
--
-- Use depois de rodar 20260820_replace_item_types_neondb.sql.
-- Categorias já usadas em Products ou SaleEntryItems serão preservadas
-- para não quebrar histórico.

BEGIN;

DELETE FROM "Categories" c
WHERE c."IsActive" = FALSE
  AND NOT EXISTS (
    SELECT 1
    FROM "Products" p
    WHERE p."CategoryId" = c."Id"
  )
  AND NOT EXISTS (
    SELECT 1
    FROM "SaleEntryItems" sei
    WHERE sei."CategoryId" = c."Id"
  );

SELECT "Id", "Name", "IsActive"
FROM "Categories"
ORDER BY "IsActive" DESC, "Name";

COMMIT;
