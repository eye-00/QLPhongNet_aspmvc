CREATE Database QLPhongNet_ASPMVC2
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;
Use QLPhongNet_ASPMVC2;
Create Table ComputerCategories(
	ID int auto_increment primary key,
    Name varchar(50),
    PricePerHour decimal(15, 2) not null);
Create Table Computers(
	ID int auto_increment primary key,
	Name varchar(30),
	Status enum('Available', 'In Use', 'Maintenance') default 'Available',
    CatID int not null,
    Foreign key (CatID) References ComputerCategories(ID));
	

Create Table User(
	ID int auto_increment primary key,
	Username varchar(30) UNIQUE NOT NULL,
	Password varchar(30) not null,
	FullName varchar(50),
	Phone varchar(15),
	Balance decimal(15, 2) default 0.0,
    Role ENUM('Admin', 'User') DEFAULT 'User');
    
Create Table UsageSession(
	ID int auto_increment primary key,
    UserID int not null,
    ComputerID int not null,
    StartTime datetime not null,
    EndTime datetime,
    TotalCost decimal(15, 2),
    foreign key (UserID) references User(ID),
    foreign key (ComputerID) references Computers(ID));
    
Create Table Services (
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
Insert into ComputerCategories(Name, PricePerHour) values
('Phổ thông', 10000),
('Cao cấp', 20000);

Insert into Computers(Name, CatID) values
('Máy 1', 1),
('Máy 2', 1),
('Máy VIP 1', 2),
('Máy VIP 2', 2);

Insert into User(Username, Password, FullName, Phone, Balance, Role) values
('user1', '123456', 'Nguyen Van A', '0912345678', 50000, 'User'),
('user2', '123456', 'Nguyen Thi B', '0987654321', 200000, 'User'),
('admin1', 'Admin1234456', 'Tran Thi C', '0123123123', 0, 'Admin');
