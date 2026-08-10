-- ====================================================
-- База данных для дипломной работы: Flower Care
-- ====================================================

-- 1. Создание таблицы пользователей
CREATE TABLE IF NOT EXISTS "User" (
    "UserId" UUID PRIMARY KEY,
    "TelegramUserId" BIGINT NOT NULL,
    "TelegramUserName" VARCHAR(255) NOT NULL,
    "RegisteredAtUtc" TIMESTAMP NOT NULL
);

-- 2. Создание таблицы коллекций цветов
CREATE TABLE IF NOT EXISTS "Collection" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "UserId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    CONSTRAINT "FK_Collection_User" FOREIGN KEY ("UserId") REFERENCES "User"("UserId") ON DELETE CASCADE
);

-- 3. Создание таблицы цветов
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
    CONSTRAINT "FK_Flower_User" FOREIGN KEY ("UserId") REFERENCES "User"("UserId") ON DELETE CASCADE,
    CONSTRAINT "FK_Flower_Collection" FOREIGN KEY ("CollectionId") REFERENCES "Collection"("Id") ON DELETE SET NULL
);

-- 4. Индексы для внешних ключей
CREATE INDEX IF NOT EXISTS "IX_Collection_UserId" ON "Collection"("UserId");
CREATE INDEX IF NOT EXISTS "IX_Flower_UserId" ON "Flower"("UserId");
CREATE INDEX IF NOT EXISTS "IX_Flower_CollectionId" ON "Flower"("CollectionId");

-- 5. Уникальный индекс для TelegramUserId
CREATE UNIQUE INDEX IF NOT EXISTS "IX_User_TelegramUserId" ON "User"("TelegramUserId");

-- 6. Индекс для поиска по LastWateredAt (полив)
CREATE INDEX IF NOT EXISTS "IX_Flower_LastWateredAt" ON "Flower"("LastWateredAt");

-- 7. Индекс для поиска по LightRequirement
CREATE INDEX IF NOT EXISTS "IX_Flower_LightRequirement" ON "Flower"("LightRequirement");