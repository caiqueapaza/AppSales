BEGIN;

TRUNCATE TABLE
    "SalePayments",
    "SaleEntryItemServices",
    "SaleEntryItems",
    "SaleItens",
    "Sales"
RESTART IDENTITY CASCADE;

COMMIT;
