-- =============================================================================
-- Cleanup script: xoa du lieu de admin nhap lai
-- Giu lai: 8 parent categories (parent_id IS NULL), Users, Doctors, Symptoms...
-- Xoa: Tat ca sub-categories, Medications, Cart, Orders, va cac bang phu thuoc
-- =============================================================================
-- Cach chay:
--   1. Stop API (PharmaIntel.API) truoc khi chay
--   2. Mo SSMS / Azure Data Studio, connect (localdb)\MSSQLLocalDB, database PharmaIntelDB
--   3. Chay file nay
-- HOAC qua dotnet:
--   sqlcmd -S "(localdb)\MSSQLLocalDB" -d PharmaIntelDB -i cleanup_data.sql
-- =============================================================================

USE PharmaIntelDB;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- === Order chain (delete children first, then parents) ===
DELETE FROM order_items;
DELETE FROM payment_transactions;
DELETE FROM orders;

-- === Cart ===
DELETE FROM cart_items;

-- === Medication-dependent tables (FK Restrict tu medications -> phai xoa truoc) ===
DELETE FROM diagnostic_result_medications;
DELETE FROM medication_reminder_logs;
DELETE FROM medication_reminders;

-- prescription_items.medication_id la SetNull, nhung ta van xoa luon vi
-- prescription mat y nghia khi medication bi xoa.
DELETE FROM prescription_items;

-- === Medications ===
DELETE FROM medications;

-- === Categories: chi xoa con (parent_id IS NOT NULL), giu 8 root ===
DELETE FROM categories WHERE parent_id IS NOT NULL;

-- === Reset identity counters (tuy chon: id moi se bat dau tu 1) ===
DBCC CHECKIDENT ('order_items',                    RESEED, 0);
DBCC CHECKIDENT ('payment_transactions',           RESEED, 0);
DBCC CHECKIDENT ('orders',                         RESEED, 0);
DBCC CHECKIDENT ('cart_items',                     RESEED, 0);
DBCC CHECKIDENT ('diagnostic_result_medications',  RESEED, 0);
DBCC CHECKIDENT ('medication_reminder_logs',       RESEED, 0);
DBCC CHECKIDENT ('medication_reminders',           RESEED, 0);
DBCC CHECKIDENT ('prescription_items',             RESEED, 0);
DBCC CHECKIDENT ('medications',                    RESEED, 0);
-- Khong reset categories vi 8 parent dang giu id 1..8 (hoac id da dung)

COMMIT TRANSACTION;
GO

-- === Verify ===
SELECT 'categories' AS [table], COUNT(*) AS [count] FROM categories
UNION ALL SELECT 'medications',   COUNT(*) FROM medications
UNION ALL SELECT 'cart_items',    COUNT(*) FROM cart_items
UNION ALL SELECT 'orders',        COUNT(*) FROM orders
UNION ALL SELECT 'order_items',   COUNT(*) FROM order_items;
GO

SELECT id, name, slug, parent_id, display_order FROM categories ORDER BY display_order;
GO
