CREATE TABLE IF NOT EXISTS PriceItems (
    Id SERIAL PRIMARY KEY,
    Vendor VARCHAR(64),
    Number VARCHAR(64),
    SearchVendor VARCHAR(64),
    SearchNumber VARCHAR(64),
    Description VARCHAR(512),
    Price DECIMAL(18,2),
    Count INT,
    SupplierName VARCHAR(128),
    FileName VARCHAR(256),
    ProcessedAt TIMESTAMP
);

CREATE TABLE IF NOT EXISTS ProcessingLogs (
    Id SERIAL PRIMARY KEY,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    SupplierName VARCHAR(128),
    FileName VARCHAR(256),
    Message TEXT,
    IsError BOOLEAN
);
