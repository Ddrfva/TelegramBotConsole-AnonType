-- =============================================
-- Таблица уведомлений (Notifications)
-- Для PostgreSQL (без CASCADE)
-- =============================================

-- Удаляем таблицу, если существует
DROP TABLE IF EXISTS "Notifications";

-- Создаём таблицу
CREATE TABLE "Notifications" (
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL,
    "Type" TEXT NOT NULL,
    "Text" TEXT NOT NULL,
    "ScheduledAt" TIMESTAMP NOT NULL,
    "IsNotified" BOOLEAN NOT NULL DEFAULT FALSE,
    "NotifiedAt" TIMESTAMP NULL
);

-- Добавляем внешний ключ отдельно (без CASCADE)
ALTER TABLE "Notifications" 
ADD CONSTRAINT "FK_Notifications_User" 
FOREIGN KEY ("UserId") REFERENCES "User"("Id");

-- Индексы
CREATE INDEX "IX_Notifications_UserId" ON "Notifications"("UserId");
CREATE INDEX "IX_Notifications_ScheduledAt" ON "Notifications"("ScheduledAt");
CREATE INDEX "IX_Notifications_IsNotified" ON "Notifications"("IsNotified");
CREATE INDEX "IX_Notifications_IsNotified_ScheduledAt" ON "Notifications"("IsNotified", "ScheduledAt");