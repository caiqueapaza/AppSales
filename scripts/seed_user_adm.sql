DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM "Users" WHERE lower("UserName") = 'adm') THEN
    UPDATE "Users"
    SET "Password" = '$2a$11$DPmZEhqfN.ekfRKejajHme0Amq.uHrtes8RNjYBY4FmPOYo98lmZ2',
        "IsAdmin" = TRUE,
        "IsActive" = TRUE,
        "UpdatedAt" = NOW()
    WHERE lower("UserName") = 'adm';
  ELSE
    INSERT INTO "Users" ("Id","Name","LastName","Email","Password","UserName","IsAdmin","IsActive","CreatedAt","UpdatedAt","EmployeeId")
    VALUES ('00000000-0000-0000-0000-000000000001','Adm','Sistema',NULL,'$2a$11$DPmZEhqfN.ekfRKejajHme0Amq.uHrtes8RNjYBY4FmPOYo98lmZ2','adm',TRUE,TRUE,NOW(),NOW(),NULL);
  END IF;
END $$;
