DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Calça jeans') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Calça jeans', NULL, TRUE, NOW(), NOW());
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Shorts jeans') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Shorts jeans', NULL, TRUE, NOW(), NOW());
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Camisa Social') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Camisa Social', NULL, TRUE, NOW(), NOW());
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Camisa Regata') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Camisa Regata', NULL, TRUE, NOW(), NOW());
  END IF;
END $$;
