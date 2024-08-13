CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "CompressedEvents" (
    "LocationIdentifier" character varying(10) NOT NULL,
    "ArchiveDate" Date NOT NULL,
    "DeviceId" integer NOT NULL,
    "Data" bytea NULL,
    "DataType" character varying(32) NOT NULL,
    CONSTRAINT "PK_CompressedEvents" PRIMARY KEY ("LocationIdentifier", "DeviceId", "ArchiveDate")
);
COMMENT ON TABLE "CompressedEvents" IS 'Compressed device data log events';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20240207213242_V5_Initial', '7.0.14');

COMMIT;

