CREATE Database DATA1
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;
Use DATA1;

create table Table1(
	id int auto_increment primary key, 
    name varchar(50),
    address varchar(200),
    total int);

insert into Table1(name, address, total) values
('Nguyễn Văn A', 'Đà Nẵng', 10000000),
('Nguyễn Lê Tấn Đạt', 'Huế', 20000000),
('Hoàng Văn Lê Dung', 'Đà Nẵng', 20000000),
('Nguyến Thái Đỗ Thắng', 'Huế', 15000000),
('Nguyễn Đỗ Ngọc Thảo', 'Đà Nẵng', 10000000),
('Trần Thị Phương Thảo', 'Quảng Trị', 30000000);


    