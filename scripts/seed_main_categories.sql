DO $$
BEGIN
  UPDATE "Categories"
  SET "IsActive" = FALSE,
      "Updated" = NOW();

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Calça') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Calça', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Calça';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Shorts') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Shorts', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Shorts';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Camisa') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Camisa', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Camisa';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Blusa') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Blusa', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Blusa';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Casaco') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Casaco', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Casaco';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Jaqueta') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Jaqueta', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Jaqueta';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Vestido') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Vestido', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Vestido';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Saia') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Saia', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Saia';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Terno') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Terno', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Terno';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Uniforme') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Uniforme', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Uniforme';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Roupa infantil') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Roupa infantil', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Roupa infantil';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Toalha de banho') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Toalha de banho', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Toalha de banho';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Toalha de rosto') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Toalha de rosto', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Toalha de rosto';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Lençol') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Lençol', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Lençol';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Fronha') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Fronha', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Fronha';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Edredom') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Edredom', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Edredom';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Colcha') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Colcha', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Colcha';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Cortina') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Cortina', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Cortina';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM "Categories" WHERE "Name" = 'Persiana') THEN
    INSERT INTO "Categories" ("Name", "Description", "IsActive", "Created", "Updated")
    VALUES ('Persiana', NULL, TRUE, NOW(), NOW());
  ELSE
    UPDATE "Categories" SET "IsActive" = TRUE, "Updated" = NOW() WHERE "Name" = 'Persiana';
  END IF;
END $$;
