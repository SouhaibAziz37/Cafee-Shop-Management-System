CREATE DATABASE CafeManagementDB;
GO
USE CafeManagementDB;

-- 1. Users Table (for Login)
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username VARCHAR(50) UNIQUE NOT NULL,
    Password VARCHAR(50) NOT NULL,
    Role VARCHAR(20) -- 'Owner', 'Staff', 'Customer'
);

-- 2. Products/Menu Table
CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    ItemName VARCHAR(100),
    Category VARCHAR(50),
    Price DECIMAL(10, 2),
    StockQuantity INT
);

-- 3. Orders Table
CREATE TABLE Orders (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(10, 2),
    StaffName VARCHAR(50)
);

-- 4. OrderDetails Table (Linking Orders and Products)
CREATE TABLE OrderDetails (
    DetailID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT FOREIGN KEY REFERENCES Orders(OrderID),
    ProductID INT FOREIGN KEY REFERENCES Products(ProductID),
    Quantity INT,
    SubTotal DECIMAL(10, 2)
);

-- 1. Create the Trigger
CREATE TRIGGER trg_UpdateStock
ON OrderDetails
AFTER INSERT
AS
BEGIN
    UPDATE Products
    SET StockQuantity = StockQuantity - (SELECT Quantity FROM inserted)
    WHERE ProductID = (SELECT ProductID FROM inserted);
END;
GO  -- This 'GO' is the fix! It separates the batches.

-- 2. Create the Procedure
CREATE PROCEDURE sp_PlaceOrder
    @StaffName VARCHAR(50),
    @Total DECIMAL(10,2)
AS
BEGIN
    INSERT INTO Orders (OrderDate, TotalAmount, StaffName)
    VALUES (GETDATE(), @Total, @StaffName);
    
    SELECT SCOPE_IDENTITY(); 
END;
GO

INSERT INTO Users (Username, Password, Role) VALUES ('admin', 'admin123', 'Owner');
INSERT INTO Products (ItemName, Category, Price, StockQuantity) VALUES ('Espresso', 'Beverages', 3.50, 100);
INSERT INTO Products (ItemName, Category, Price, StockQuantity) VALUES ('Croissant', 'Pastries', 4.00, 20);