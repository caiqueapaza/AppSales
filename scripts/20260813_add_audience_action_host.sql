BEGIN;

ALTER TABLE "SaleEntryItems"
ADD COLUMN IF NOT EXISTS "AudienceType" character varying(20) NOT NULL DEFAULT 'Adult';

ALTER TABLE "SaleEntryItemServices"
ADD COLUMN IF NOT EXISTS "ActionType" character varying(20) NOT NULL DEFAULT 'Adjustment';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260813120000_AddAudienceAndActionToRepairs', '8.0.22'
WHERE NOT EXISTS (
    SELECT 1
    FROM "__EFMigrationsHistory"
    WHERE "MigrationId" = '20260813120000_AddAudienceAndActionToRepairs'
);

COMMIT;
