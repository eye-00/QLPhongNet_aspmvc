-- Xóa cơ sở dữ liệu cũ
DROP DATABASE IF EXISTS QLPhongNet_ASPMVC2;

-- Tạo lại cơ sở dữ liệu
CREATE DATABASE QLPhongNet_ASPMVC2
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE QLPhongNet_ASPMVC2;

-- Tạo các bảng
CREATE TABLE ComputerCategories(
    ID int auto_increment primary key,
    Name varchar(50),
    PricePerHour decimal(15, 2) not null
);

CREATE TABLE Computers(
    ID int auto_increment primary key,
    Name varchar(30),
    Status enum('Available', 'In Use', 'Maintenance') default 'Available',
    CatID int NOT NULL,
    CONSTRAINT FK_Computers_ComputerCategories FOREIGN KEY (CatID) REFERENCES ComputerCategories(ID)
);

CREATE TABLE User(
    ID int auto_increment primary key,
    Username varchar(30) UNIQUE NOT NULL,
    Password varchar(30) not null,
    FullName varchar(50),
    Phone varchar(15),
    Balance decimal(15, 2) default 0.0,
    Role ENUM('Admin', 'User') DEFAULT 'User'
);

CREATE TABLE UsageSession(
    ID int auto_increment primary key,
    UserID int not null,
    ComputerID int not null,
    StartTime datetime not null,
    EndTime datetime,
    TotalCost decimal(15, 2),
    foreign key (UserID) references User(ID),
    foreign key (ComputerID) references Computers(ID)
);

CREATE TABLE Services (
    ID int auto_increment primary key,
    Name varchar(50) not null,
    Price decimal(15, 2) not null
);

CREATE TABLE ServiceUsage (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    UserID INT NOT NULL,
    ServiceID INT NOT NULL,
    Quantity INT DEFAULT 1,
    UsageTime DATETIME DEFAULT CURRENT_TIMESTAMP,
    TotalPrice DECIMAL(15, 2),
    FOREIGN KEY (UserID) REFERENCES User(ID),
    FOREIGN KEY (ServiceID) REFERENCES Services(ID)
);

CREATE TABLE DailyRevenue (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    ReportDate DATE,
    TotalUsageRevenue DECIMAL(15, 2),
    TotalRecharge DECIMAL(15, 2),
    TotalServiceRevenue DECIMAL(15, 2)
);

-- Thêm dữ liệu mẫu
INSERT INTO ComputerCategories(Name, PricePerHour) VALUES
('Phổ thông', 10000),
('Cao cấp', 20000);

INSERT INTO Computers(Name, CatID) VALUES
('Máy 1', 1),
('Máy 2', 1),
('Máy VIP 1', 2),
('Máy VIP 2', 2);

INSERT INTO User(Username, Password, FullName, Phone, Balance, Role) VALUES
('user1', '123456', 'Nguyen Van A', '0912345678', 50000, 0),
('user2', '123456', 'Nguyen Thi B', '0987654321', 200000, 0),
('admin1', 'Admin1234456', 'Tran Thi C', '0123123123', 0, 1);