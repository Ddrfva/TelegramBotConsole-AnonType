-- ====================================================
-- Заполнение таблиц тестовыми данными (Flowers)
-- ====================================================

-- 1. Добавление пользователей
INSERT INTO "User" ("UserId", "TelegramUserId", "TelegramUserName", "RegisteredAtUtc")
VALUES 
    ('11111111-1111-1111-1111-111111111111', 123456789, 'anna_flower', '2026-01-01 10:00:00'),
    ('22222222-2222-2222-2222-222222222222', 987654321, 'dmitry_garden', '2026-01-02 11:00:00');

-- 2. Добавление коллекций
INSERT INTO "Collection" ("Id", "Name", "UserId", "CreatedAt")
VALUES 
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Indoor Plants', '11111111-1111-1111-1111-111111111111', '2026-01-01 10:30:00'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Garden Flowers', '11111111-1111-1111-1111-111111111111', '2026-01-01 11:00:00'),
    ('cccccccc-cccc-cccc-cccc-cccccccccccc', 'Cacti & Succulents', '22222222-2222-2222-2222-222222222222', '2026-01-02 11:30:00');

-- 3. Добавление цветов
INSERT INTO "Flower" (
    "Id", "Name", "Species", "UserId", "CollectionId", 
    "WateringFrequencyDays", "LastWateredAt", "LightRequirement", "Notes", 
    "State", "CreatedAtUtc", "StateChangedAtUtc"
)
VALUES 
    -- Indoor Plants
    ('11111111-aaaa-1111-aaaa-111111111111', 'Monstera', 'Monstera deliciosa', 
     '11111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
     7, '2026-01-28 10:00:00', 'Indirect', 'Loves humidity, mist leaves regularly',
     0, '2026-01-01 10:35:00', NULL),
     
    ('22222222-aaaa-2222-aaaa-222222222222', 'African Violet', 'Saintpaulia ionantha',
     '11111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
     3, '2026-01-30 09:00:00', 'Bright', 'Does not like drafts',
     0, '2026-01-01 10:40:00', NULL),
     
    -- Garden Flowers
    ('33333333-aaaa-3333-aaaa-333333333333', 'Rose', 'Rosa hybrida',
     '11111111-1111-1111-1111-111111111111', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
     2, '2026-01-29 18:00:00', 'Bright', 'Needs regular pruning',
     0, '2026-01-01 11:05:00', NULL),
     
    ('44444444-aaaa-4444-aaaa-444444444444', 'Lavender', 'Lavandula angustifolia',
     '11111111-1111-1111-1111-111111111111', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
     5, '2026-01-25 08:00:00', 'Bright', 'Drought tolerant',
     1, '2026-01-01 11:10:00', '2026-01-28 12:00:00'),
     
    -- Cacti & Succulents
    ('55555555-aaaa-5555-aaaa-555555555555', 'Aloe Vera', 'Aloe vera',
     '22222222-2222-2222-2222-222222222222', 'cccccccc-cccc-cccc-cccc-cccccccccccc',
     14, '2026-01-20 10:00:00', 'Bright', 'Medicinal plant, gel used in cosmetics',
     0, '2026-01-02 11:35:00', NULL),
     
    ('66666666-aaaa-6666-aaaa-666666666666', 'Cactus', 'Cactaceae',
     '22222222-2222-2222-2222-222222222222', 'cccccccc-cccc-cccc-cccc-cccccccccccc',
     21, '2026-01-15 14:00:00', 'Bright', 'Water rarely, almost none in winter',
     0, '2026-01-02 11:40:00', NULL);