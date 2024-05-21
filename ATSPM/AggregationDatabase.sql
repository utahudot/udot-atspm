CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "CompressedAggregations" (
    "LocationIdentifier" character varying(10) NOT NULL,
    "DataType" character varying(32) NOT NULL,
    "ArchiveDate" Date NOT NULL,
    "Data" bytea NULL,
    CONSTRAINT "PK_CompressedAggregations" PRIMARY KEY ("LocationIdentifier", "ArchiveDate", "DataType")
);
COMMENT ON TABLE "CompressedAggregations" IS 'Compressed aggregations';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20240207213050_V5_Initial', '7.0.14');

COMMIT;

