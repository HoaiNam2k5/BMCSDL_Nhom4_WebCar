# 🚗 WebCar - Hệ Thống Quản Lý Bán Xe Ô Tô

## 📋 Giới Thiệu

WebCar là một ứng dụng web quản lý bán xe ô tô được phát triển bằng ASP.NET MVC và Oracle Database. Hệ thống cung cấp các tính năng quản lý xe, khách hàng, đơn hàng, và nhiều chức năng khác với bảo mật cao cấp bao gồm mã hóa dữ liệu và kiểm toán (audit logging).

## ✨ Tính Năng Chính

- 🔐 **Quản lý tài khoản và phân quyền**: Hệ thống role-based authentication
- 🚙 **Quản lý xe**: Thêm, sửa, xóa thông tin xe
- 👥 **Quản lý khách hàng**: Quản lý thông tin khách hàng
- 📦 **Quản lý đơn hàng**: Tạo và theo dõi đơn hàng
- 💬 **Feedback**: Hệ thống đánh giá và phản hồi từ khách hàng
- 🔒 **Bảo mật nâng cao**: 
  - Mã hóa dữ liệu nhạy cảm
  - Oracle Label Security
  - Virtual Private Database (VPD)
- 📊 **Audit Logging**: Ghi nhận tất cả hành động trên hệ thống
- 👨‍💼 **Trang quản trị**: Dashboard cho quản trị viên

## 🛠️ Công Nghệ Sử Dụng

### Backend
- **Framework**: ASP.NET MVC 5.2.9
- **.NET Framework**: 4.8
- **ORM**: Entity Framework 6.5.1
- **Database**: Oracle Database (Oracle.ManagedDataAccess 23.26.0)

### Frontend
- **Bootstrap**: 5.2.3
- **jQuery**: 3.7.0
- **jQuery Validation**: 1.19.5
- **Modernizr**: 2.8.3

### Các Package Quan Trọng
- Oracle.ManagedDataAccess.EntityFramework
- Newtonsoft.Json 13.0.3
- Microsoft.AspNet.Web.Optimization

## 📋 Yêu Cầu Hệ Thống

- **Visual Studio**: 2017 trở lên (khuyến nghị Visual Studio 2022)
- **.NET Framework**: 4.8
- **Oracle Database**: 11g trở lên (khuyến nghị Oracle 19c hoặc 21c)
- **IIS Express**: Được cài đặt cùng Visual Studio
- **Oracle Client**: Oracle Data Access Components (ODAC)

## 🚀 Hướng Dẫn Cài Đặt

### 1. Clone Repository

```bash
git clone https://github.com/HoaiNam2k5/BMCSDL_Nhom4_WebCar.git
cd BMCSDL_Nhom4_WebCar
```

### 2. Cài Đặt Oracle Database

#### Tạo Tablespace và User

```sql
-- Tạo tablespace
CREATE TABLESPACE CARSALE_TBS
DATAFILE 'carsale_tbs.dbf' SIZE 100M
AUTOEXTEND ON NEXT 10M MAXSIZE UNLIMITED;

-- Tạo user CARSALE
-- Lưu ý: Thay thế 'your_password' bằng mật khẩu mạnh (tối thiểu 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt)
CREATE USER CARSALE IDENTIFIED BY your_password
DEFAULT TABLESPACE CARSALE_TBS
QUOTA UNLIMITED ON CARSALE_TBS;

-- Cấp quyền
GRANT CONNECT, RESOURCE, DBA TO CARSALE;
```

#### Import Database

Sử dụng file backup trong thư mục `Orcl_DBA`:

```bash
# Thay thế 'your_password' bằng mật khẩu của user CARSALE
# Thay thế 'your_database' bằng tên Oracle instance của bạn
sqlplus CARSALE/your_password@your_database @Orcl_DBA/CARSALE_FULL_BACKUP_20250105.sql
```

### 3. Cấu Hình Connection String

Mở file `WebCar/Web.config` và cập nhật connection string:

```xml
<connectionStrings>
  <!-- Cập nhật Password và Data Source theo cấu hình Oracle của bạn -->
  <add name="Model1" 
       connectionString="User Id=CARSALE;Password=your_password;Data Source=your_oracle_instance"
       providerName="Oracle.ManagedDataAccess.Client" />
</connectionStrings>
```

### 4. Restore NuGet Packages

Trong Visual Studio:
- Click chuột phải vào Solution
- Chọn "Restore NuGet Packages"

Hoặc sử dụng Package Manager Console:

```powershell
Update-Package -reinstall
```

### 5. Build Solution

```bash
# Trong Visual Studio
Build > Build Solution (Ctrl + Shift + B)
```

### 6. Chạy Ứng Dụng

- Nhấn **F5** hoặc click **IIS Express** trong Visual Studio
- Ứng dụng sẽ mở tại: `https://localhost:44312`

## 📁 Cấu Trúc Thư Mục

```
BMCSDL_Nhom4_WebCar/
├── WebCar/                         # Ứng dụng web chính
│   ├── Controllers/                # Các controller
│   │   ├── AccountController.cs    # Xử lý đăng nhập/đăng ký
│   │   ├── AdminController.cs      # Quản trị hệ thống
│   │   ├── AuditController.cs      # Quản lý audit logs
│   │   ├── HomeController.cs       # Trang chủ
│   │   ├── OrderController.cs      # Quản lý đơn hàng
│   │   └── ProductController.cs    # Quản lý sản phẩm/xe
│   ├── Models/                     # Các model/entity
│   │   ├── ACCOUNT_ROLE.cs        # Model tài khoản
│   │   ├── AUDIT_LOG.cs           # Model audit log
│   │   ├── CAR.cs                 # Model xe
│   │   ├── CUSTOMER.cs            # Model khách hàng
│   │   ├── ORDER.cs               # Model đơn hàng
│   │   ├── ORDER_DETAIL.cs        # Chi tiết đơn hàng
│   │   ├── FEEDBACK.cs            # Model feedback
│   │   └── ENCRYPTION_KEY.cs      # Model mã hóa
│   ├── Views/                      # Các view
│   │   ├── Home/                  # Views trang chủ
│   │   ├── Account/               # Views tài khoản
│   │   ├── Admin/                 # Views quản trị
│   │   └── Shared/                # Views dùng chung
│   ├── Content/                    # CSS, images
│   ├── Scripts/                    # JavaScript files
│   ├── App_Start/                  # Cấu hình ứng dụng
│   └── Web.config                  # File cấu hình chính
├── Orcl_DBA/                       # Database scripts
│   ├── CARSALE_FULL_BACKUP_20250105.sql  # Full database backup
│   └── Diagram.dmd                 # Database diagram
├── packages/                       # NuGet packages
└── WebCar.sln                      # Solution file
```

## 🔑 Tài Khoản Mặc Định

Sau khi import database, bạn có thể kiểm tra tài khoản admin bằng cách truy vấn:

```sql
-- Xem danh sách tài khoản trong hệ thống
SELECT * FROM CARSALE.CUSTOMER WHERE ROLENAME = 'Admin';

-- Hoặc kiểm tra bảng ACCOUNT_ROLE
SELECT * FROM CARSALE.ACCOUNT_ROLE WHERE ROLENAME = 'Admin';
```

> ⚠️ **Lưu ý Bảo Mật**: 
> - Đổi mật khẩu mặc định ngay sau khi đăng nhập lần đầu
> - Sử dụng mật khẩu mạnh (tối thiểu 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt)

## 🔐 Các Tính Năng Bảo Mật

### 1. Mã Hóa Dữ Liệu (Encryption)
- Sử dụng Oracle Transparent Data Encryption (TDE)
- Bảng `ENCRYPTION_KEY` quản lý các khóa mã hóa

### 2. Oracle Label Security (OLS)
- Phân loại dữ liệu theo các mức độ bảo mật
- Kiểm soát truy cập dựa trên nhãn (label)

### 3. Virtual Private Database (VPD)
- Row-level security
- Tự động lọc dữ liệu dựa trên user context

### 4. Audit Logging
- Ghi nhận tất cả các thao tác:
  - Đăng nhập/đăng xuất
  - Thêm/sửa/xóa dữ liệu
  - Truy vấn dữ liệu nhạy cảm
- Lưu trữ: IP address, timestamp, user, action

## 📊 Database Schema

### Các Bảng Chính

- **ACCOUNT_ROLE**: Quản lý tài khoản và phân quyền
- **CUSTOMER**: Thông tin khách hàng
- **CAR**: Thông tin xe
- **ORDER**: Đơn hàng
- **ORDER_DETAIL**: Chi tiết đơn hàng
- **FEEDBACK**: Đánh giá từ khách hàng
- **AUDIT_LOG**: Log kiểm toán
- **ENCRYPTION_KEY**: Quản lý khóa mã hóa

### Sequences

Tất cả các sequence sử dụng tiền tố `SEQ_` để tự động tạo ID.

## 🧪 Testing

### Chạy Tests

```bash
# Sử dụng Test Explorer trong Visual Studio
Test > Test Explorer
```

## 🤝 Đóng Góp

Nếu bạn muốn đóng góp cho dự án:

1. Fork repository
2. Tạo branch mới (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

## 👥 Nhóm Phát Triển

**Nhóm 4 - Bảo Mật Cơ Sở Dữ Liệu**

- **Repository**: [HoaiNam2k5/BMCSDL_Nhom4_WebCar](https://github.com/HoaiNam2k5/BMCSDL_Nhom4_WebCar)

## 📝 Giấy Phép

Dự án này được phát triển cho mục đích học tập.

## 📞 Liên Hệ

Nếu có bất kỳ câu hỏi nào, vui lòng tạo issue trên GitHub repository.

## 📚 Tài Liệu Tham Khảo

- [ASP.NET MVC Documentation](https://docs.microsoft.com/en-us/aspnet/mvc/)
- [Oracle Database Documentation](https://docs.oracle.com/en/database/)
- [Entity Framework Documentation](https://docs.microsoft.com/en-us/ef/)
- [Oracle Label Security Documentation](https://docs.oracle.com/en/database/oracle/oracle-database/19/olsag/)

---

**Last Updated**: December 2025
**Version**: 1.0
