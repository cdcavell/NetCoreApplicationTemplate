CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "AuditRecords" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AuditRecords" PRIMARY KEY,
    "ModifiedBy" TEXT NOT NULL,
    "ModifiedOnUtc" TEXT NOT NULL,
    "Application" TEXT NOT NULL,
    "Entity" TEXT NOT NULL,
    "State" TEXT NOT NULL,
    "KeyValues" TEXT NOT NULL,
    "OriginalValues" TEXT NOT NULL,
    "CurrentValues" TEXT NOT NULL
);

CREATE TABLE "ExternalLoginAccounts" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_ExternalLoginAccounts" PRIMARY KEY,
    "LocalUserId" TEXT NOT NULL,
    "ProviderName" TEXT NOT NULL,
    "ProviderUserId" TEXT NOT NULL,
    "DisplayName" TEXT NULL,
    "Email" TEXT NULL,
    "CreatedOnUtc" TEXT NOT NULL,
    "UpdatedOnUtc" TEXT NULL,
    "LastLoginOnUtc" TEXT NULL
);

CREATE INDEX "IX_ExternalLoginAccounts_Email" ON "ExternalLoginAccounts" ("Email");

CREATE INDEX "IX_ExternalLoginAccounts_LocalUserId" ON "ExternalLoginAccounts" ("LocalUserId");

CREATE UNIQUE INDEX "IX_ExternalLoginAccounts_ProviderName_ProviderUserId" ON "ExternalLoginAccounts" ("ProviderName", "ProviderUserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260521230436_InitialCreate', '10.0.8');

COMMIT;

BEGIN TRANSACTION;
ALTER TABLE "ExternalLoginAccounts" ADD "ConcurrencyStamp" TEXT NOT NULL DEFAULT '';

ALTER TABLE "AuditRecords" ADD "ConcurrencyStamp" TEXT NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260528165924_AddDataEntityConcurrencyStamp', '10.0.8');

COMMIT;

