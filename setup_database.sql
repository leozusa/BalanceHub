-- BalanceHub Database Setup Script
-- Execute this in SQLite to create test users for login testing

-- Create Users table
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" TEXT NOT NULL PRIMARY KEY,
    "Email" TEXT NOT NULL UNIQUE,
    "PasswordHash" TEXT,
    "Role" TEXT NOT NULL DEFAULT 'Employee',
    "EntraId" TEXT,
    "FirstName" TEXT,
    "LastName" TEXT,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "LastLoginAt" TEXT,
    "FailedLoginAttempts" INTEGER DEFAULT 0,
    "LockoutEnd" TEXT
);

-- Create test users with BCrypt hashed passwords for "test123"
-- Hash generated with: BCrypt.HashPassword("test123", workFactor: 12)

INSERT OR REPLACE INTO "Users" ("Id", "Email", "FirstName", "LastName", "Role", "IsActive", "PasswordHash") VALUES
('user-1', 'john.doe@example.com', 'John', 'Doe', 'Employee', 1, '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LeKZEFFzGaOw3gBbO'),
('user-2', 'sarah.smith@example.com', 'Sarah', 'Smith', 'Manager', 1, '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LeKZEFFzGaOw3gBbO'),
('user-3', 'alex.jones@example.com', 'Alex', 'Jones', 'Employee', 1, '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LeKZEFFzGaOw3gBbO');

-- Create Tasks table (for Eisenhower Matrix)
CREATE TABLE IF NOT EXISTS "Tasks" (
    "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "Title" TEXT NOT NULL,
    "Description" TEXT,
    "IsCompleted" INTEGER NOT NULL DEFAULT 0,
    "Quadrant" TEXT NOT NULL, -- Urgent&Important, NotUrgent&Important, Urgent&NotImportant, NotUrgent&NotImportant
    "Priority" INTEGER,
    "DueDate" TEXT,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "UserId" TEXT,
    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

-- Verify setup
SELECT COUNT(*) as TotalUsers FROM Users;
SELECT Email, Role, IsActive FROM Users;
