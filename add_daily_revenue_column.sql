USE QLPhongNet_ASPMVC2;

-- Thêm cột DailyRevenueID vào bảng RechargeRequest
ALTER TABLE RechargeRequest ADD COLUMN DailyRevenueID int;

-- Thêm khóa ngoại
ALTER TABLE RechargeRequest 
ADD CONSTRAINT FK_RechargeRequest_DailyRevenue 
FOREIGN KEY (DailyRevenueID) REFERENCES DailyRevenue(ID); 