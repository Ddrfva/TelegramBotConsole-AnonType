-- ====================================================
-- SELECT scripts for Flower database
-- ====================================================

-- 1. Get all flowers for a user
SELECT 
    "Id", "Name", "Species", "UserId", "CollectionId",
    "WateringFrequencyDays", "LastWateredAt", "LightRequirement", "Notes",
    "State", "CreatedAtUtc", "StateChangedAtUtc"
FROM "Flower"
WHERE "UserId" = '11111111-1111-1111-1111-111111111111';

-- 2. Get active flowers for a user (State = 0)
SELECT 
    "Id", "Name", "Species", "UserId", "CollectionId",
    "WateringFrequencyDays", "LastWateredAt", "LightRequirement", "Notes",
    "State", "CreatedAtUtc", "StateChangedAtUtc"
FROM "Flower"
WHERE "UserId" = '11111111-1111-1111-1111-111111111111'
  AND "State" = 0;

-- 3. Get flowers by collection
SELECT 
    "Id", "Name", "Species", "UserId", "CollectionId",
    "WateringFrequencyDays", "LastWateredAt", "LightRequirement", "Notes",
    "State", "CreatedAtUtc", "StateChangedAtUtc"
FROM "Flower"
WHERE "UserId" = '11111111-1111-1111-1111-111111111111'
  AND "CollectionId" = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';

-- 4. Collection statistics
SELECT 
    c."Name" AS "CollectionName",
    COUNT(f."Id") AS "TotalFlowers",
    SUM(CASE WHEN f."State" = 0 THEN 1 ELSE 0 END) AS "ActiveFlowers",
    SUM(CASE WHEN f."State" = 1 THEN 1 ELSE 0 END) AS "CompletedFlowers"
FROM "Collection" c
LEFT JOIN "Flower" f ON c."Id" = f."CollectionId"
WHERE c."UserId" = '11111111-1111-1111-1111-111111111111'
GROUP BY c."Id", c."Name";