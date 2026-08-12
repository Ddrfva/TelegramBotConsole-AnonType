CREATE TABLE IF NOT EXISTS "User" (
    "Id" UUID PRIMARY KEY,
    "TelegramUserId" BIGINT NOT NULL,
    "TelegramUserName" VARCHAR(255) NOT NULL,
    "RegisteredAtUtc" TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS "Collection" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "UserId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    CONSTRAINT "FK_Collection_User" FOREIGN KEY ("UserId") REFERENCES "User"("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "Flower" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Species" VARCHAR(200) NULL,
    "UserId" UUID NOT NULL,
    "CollectionId" UUID NULL,
    "WateringFrequencyDays" INT NULL CHECK ("WateringFrequencyDays" > 0),
    "LastWateredAt" TIMESTAMP NULL,
    "LightRequirement" VARCHAR(50) NULL CHECK ("LightRequirement" IN ('Bright', 'Indirect', 'Shade', 'PartialShade')),
    "Notes" TEXT NULL,
    "State" INT NOT NULL CHECK ("State" IN (0, 1)),
    "CreatedAtUtc" TIMESTAMP NOT NULL,
    "StateChangedAtUtc" TIMESTAMP NULL,
    CONSTRAINT "FK_Flower_User" FOREIGN KEY ("UserId") REFERENCES "User"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Flower_Collection" FOREIGN KEY ("CollectionId") REFERENCES "Collection"("Id") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_Collection_UserId" ON "Collection"("UserId");
CREATE INDEX IF NOT EXISTS "IX_Flower_UserId" ON "Flower"("UserId");
CREATE INDEX IF NOT EXISTS "IX_Flower_CollectionId" ON "Flower"("CollectionId");

CREATE UNIQUE INDEX IF NOT EXISTS "UX_User_TelegramUserId" ON "User"("TelegramUserId");

CREATE INDEX IF NOT EXISTS "IX_Flower_LastWateredAt" ON "Flower"("LastWateredAt");
CREATE INDEX IF NOT EXISTS "IX_Flower_LightRequirement" ON "Flower"("LightRequirement");
