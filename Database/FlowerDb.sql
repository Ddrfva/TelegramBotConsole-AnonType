DROP TABLE IF EXISTS "Flower";
DROP TABLE IF EXISTS "Collection";
DROP TABLE IF EXISTS "User";

CREATE TABLE "User" (
    "Id" UUID PRIMARY KEY,
    "TelegramUserId" BIGINT NOT NULL UNIQUE,
    "TelegramUserName" VARCHAR(255) NOT NULL,
    "RegisteredAtUtc" TIMESTAMP NOT NULL
);

CREATE TABLE "Collection" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "UserId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    CONSTRAINT "FK_Collection_User" FOREIGN KEY ("UserId") REFERENCES "User"("Id") ON DELETE CASCADE
);

CREATE TABLE "Flower" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Species" VARCHAR(200) NULL,
    "UserId" UUID NOT NULL,
    "CollectionId" UUID NULL,
    "WateringFrequencyDays" INT NULL,
    "LastWateredAt" TIMESTAMP NULL,
    "LightRequirement" VARCHAR(50) NULL,
    "Notes" TEXT NULL,
    "State" INT NOT NULL DEFAULT 0,
    "CreatedAtUtc" TIMESTAMP NOT NULL,
    "StateChangedAtUtc" TIMESTAMP NULL,
    CONSTRAINT "FK_Flower_User" FOREIGN KEY ("UserId") REFERENCES "User"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Flower_Collection" FOREIGN KEY ("CollectionId") REFERENCES "Collection"("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_Collection_UserId" ON "Collection"("UserId");
CREATE INDEX "IX_Flower_UserId" ON "Flower"("UserId");
CREATE INDEX "IX_Flower_CollectionId" ON "Flower"("CollectionId");
CREATE INDEX "IX_Flower_LastWateredAt" ON "Flower"("LastWateredAt");
CREATE INDEX "IX_Flower_LightRequirement" ON "Flower"("LightRequirement");