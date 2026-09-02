-- Opcional: apaga fisicamente serviços inativos que não estão ligados a ordens.
--
-- Use depois de rodar 20260820_replace_service_catalog_neondb.sql.
-- Serviços já usados em SaleEntryItemServices ou SaleItens serão preservados
-- para não quebrar histórico.

BEGIN;

DELETE FROM "ServiceItens" s
WHERE s."IsActive" = FALSE
  AND NOT EXISTS (
    SELECT 1
    FROM "SaleEntryItemServices" es
    WHERE es."ServiceItemId" = s."Id"
  )
  AND NOT EXISTS (
    SELECT 1
    FROM "SaleItens" si
    WHERE si."ServiceItemId" = s."Id"
  );

SELECT "Id", "Name", "ServiceType", "Price", "IsActive"
FROM "ServiceItens"
ORDER BY "IsActive" DESC, "ServiceType", "Name";

COMMIT;
