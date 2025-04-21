USE QLPhongNet_ASPMVC2;

-- Thêm cột Description vào bảng Services
ALTER TABLE Services
ADD COLUMN Description varchar(500);

-- Tạo bảng RechargeRequest nếu chưa tồn tại
CREATE TABLE IF NOT EXISTS RechargeRequest (
    ID int auto_increment primary key,
    UserID int not null,
    Amount decimal(15, 2) not null,
    RequestTime datetime not null default CURRENT_TIMESTAMP,
    ProcessedTime datetime,
    Status enum('Pending', 'Approved', 'Rejected') default 'Pending',
    Note varchar(500),
    DailyRevenueID int,
    foreign key (UserID) references User(ID),
    foreign key (DailyRevenueID) references DailyRevenue(ID)
); 