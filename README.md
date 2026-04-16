<img width="1137" height="1161" alt="home-page" src="https://github.com/user-attachments/assets/261dde8b-09e9-4be4-93ec-18ff22f10a6a" />

# 🚗 AutoHub: Enterprise-Grade Car Sales System

## 📖 The Story: Why AutoHub?
In the fast-paced automotive e-commerce industry, **User Experience** and **Data Security** must go hand in hand. AutoHub was born to solve a dual challenge: providing a seamless, intuitive car purchasing journey for customers while enforcing strict, enterprise-level data protection under the hood. 

By leveraging **ASP.NET MVC** for a robust backend and **Oracle Database** for uncompromised security, AutoHub demonstrates how a standard e-commerce flow can be fortified against insider threats, data breaches, and unauthorized access using state-of-the-art Database Security mechanisms.

---

## 🔄 Core Business Flow
The system is designed around a clear, real-world e-commerce lifecycle:

1. **🛍️ Discovery (Guest/Customer):** Users browse the vehicle catalog, view detailed car specifications, pricing, and read feedback from previous buyers.
2. **🔐 Authentication & Role Assignment:** Users register/login. The system authenticates them and dynamically assigns database-level roles (RBAC) and Security Labels (VPD/MAC).
3. **🛒 Purchasing (Customer):** Customers place orders. Sensitive checkout data (like passwords or personal info) is encrypted symmetrically/asymmetrically at the database layer before storage.
4. **💬 Post-Purchase (Customer):** After a successful transaction, customers can leave ratings and feedback for the cars they purchased.
5. **⚙️ Administration & Monitoring (Admin):** Admins access a dedicated dashboard to manage inventory (Cars), track Orders, and monitor system health. Every critical action triggers a Fine-Grained Audit (FGA) log, ensuring complete traceability.

---

## ✨ Highlighted Features

### 🏢 Business Functionalities
- **Vehicle Catalog Management:** Full CRUD operations for car listings with detailed specs and images.
- **Order Processing:** End-to-end order tracking, from cart to final delivery status.
- **Customer Profiles:** Secure storage of customer history and order tracking.
- **Interactive Feedback System:** Allow users to rate and review vehicles, building community trust.

### 🛡️ Enterprise Security Functionalities (Oracle DB)
- **Virtual Private Database (VPD):** Row-level security ensures that customers can only query and view their own orders, while admins have global access.
- **Mandatory Access Control (MAC):** Custom implementation of Security Labels (Levels & Compartments) assigning read/write boundaries based on user clearance.
- **Asymmetric Encryption:** Sensitive data is actively encrypted using `DBMS_CRYPTO` to prevent data leaks even if the physical database is compromised.
- **Comprehensive Audit Logging:** Triggers and Oracle FGA (Fine-Grained Auditing) automatically log unauthorized access attempts, login failures, and critical data modifications.

---

## 🛠️ Tech Stack
- **Backend:** ASP.NET MVC 5.2.9, .NET Framework 4.8, Entity Framework 6.5.1, Oracle.ManagedDataAccess (23.26.0).
- **Frontend:** Bootstrap 5.2.3, jQuery 3.7.0.
- **Database:** Oracle Database 19c/21c.

---

## 📊 Entity-Relationship Diagram (ERD)
Below is the comprehensive database schema based on the Oracle tables. This structure is essential for setting up Entity Framework ORM on the backend.

*(Note: Historical backup tables prefixed with `BKP_` such as `BKP_CAR_1`, `BKP_CUSTOMER_1`, etc., are omitted from the diagram for clarity but are managed by the backup metadata tables).*  

```mermaid
erDiagram
    %% Core Business Entities
    CUSTOMER ||--o{ ORDERS : "places"
    CUSTOMER ||--o{ FEEDBACK : "writes"
    CUSTOMER ||--o{ ACCOUNT_ROLE : "has"
    
    ORDERS ||--|{ ORDER_DETAIL : "contains"
    CAR ||--o{ ORDER_DETAIL : "included in"
    CAR ||--o{ FEEDBACK : "receives"
    
    %% Security Entities
    CUSTOMER ||--o{ USER_SECURITY_LABEL : "assigned to"

    %% Backup Entities
    BACKUP_METADATA ||--|{ BACKUP_TABLE_DETAILS : "tracks"

    CUSTOMER {
        NUMBER MAKH PK
        VARCHAR2 HOTEN "Full Name"
        VARCHAR2 EMAIL
        VARCHAR2 SDT "Phone"
        VARCHAR2 MATKHAU "Encrypted Password"
        VARCHAR2 DIACHI "Address"
        DATE NGAYDANGKY
    }
    
    CAR {
        NUMBER MAXE PK
        VARCHAR2 TENXE
        NUMBER GIA
        VARCHAR2 MOTA
    }

    ORDERS {
        NUMBER MADON PK
        NUMBER MAKH FK
        NUMBER TONGTIEN
        VARCHAR2 TRANGTHAI
        DATE NGAYTAO
    }
    
    ORDER_DETAIL {
        NUMBER MADON FK
        NUMBER MAXE FK
        NUMBER SOLUONG
        NUMBER DONGIA
    }

    FEEDBACK {
        NUMBER MAFB PK
        NUMBER MAKH FK
        NUMBER MAXE FK
        VARCHAR2 NOIDUNG
        NUMBER DIEMDANHGIA
        DATE NGAYDANHGIA
    }

    ACCOUNT_ROLE {
        NUMBER ROLE_ID PK
        NUMBER MAKH FK
        VARCHAR2 ROLE_NAME
    }

    %% Security & Encryption
    SECURITY_LEVEL {
        NUMBER LEVEL_ID PK
        VARCHAR2 LEVEL_NAME
        NUMBER LEVEL_NUM
    }
    
    SECURITY_COMPARTMENT {
        NUMBER COMP_ID PK
        VARCHAR2 COMP_NAME
        VARCHAR2 COMP_CODE
    }

    USER_SECURITY_LABEL {
        NUMBER USER_LABEL_ID PK
        NUMBER MAKH FK
        NUMBER MAX_READ_LEVEL
        NUMBER MAX_WRITE_LEVEL
        VARCHAR2 COMPARTMENTS
    }

    DATA_SECURITY_LABEL {
        NUMBER DATA_LABEL_ID PK
        VARCHAR2 TABLE_NAME
        NUMBER RECORD_ID
        NUMBER SECURITY_LEVEL
        VARCHAR2 COMPARTMENTS
    }

    ENCRYPTION_KEY {
        NUMBER KEYID PK
        VARCHAR2 KEYTYPE
        VARCHAR2 PUBLICKEY
        VARCHAR2 PRIVATEKEY
    }

    %% Auditing & Backups
    AUDIT_LOG {
        NUMBER LOG_ID PK
        VARCHAR2 ACTION_NAME
        VARCHAR2 TABLE_NAME
        DATE LOG_DATE
    }
    
    BACKUP_METADATA {
        NUMBER BACKUP_ID PK
        DATE BACKUP_DATE
        VARCHAR2 STATUS
    }
    
    BACKUP_TABLE_DETAILS {
        NUMBER DETAIL_ID PK
        NUMBER BACKUP_ID FK
        VARCHAR2 TABLE_NAME
        NUMBER ROW_COUNT
    }
```

---

## ⚙️ Database Setup & SQL Scripts

To get the backend up and running, you need to configure the Oracle Database by executing the provided scripts in the following order:

### 1. User Creation & Privilege Granting (`orcl~1.sql`)
Execute this script using a DBA account (e.g., `SYSTEM/SYSDBA`). It handles the creation of the `CARSALE` user and grants necessary privileges, including VPD, MAC, and Auditing rights.
```sql
-- Run via SQL*Plus or Oracle SQL Developer
@Orcl_DBA/orcl~1.sql
```

### 2. Import Schema & Seed Data (`CARSALE_FULL_BACKUP_20250105.sql`)
This is the full database backup containing the DDL (tables, views, sequences) and initial seed data. 
- **Location**: `Orcl_DBA/CARSALE_FULL_BACKUP_20250105.sql`
- **Action**: Run this script under the newly created `CARSALE` schema. It will construct all core tables (`CUSTOMER`, `CAR`, `ORDERS`, `ORDER_DETAIL`), security tables, backup tracking tables, and set up roles like `CARSALE_ADMIN_ROLE`.

---

## 🔌 Backend Integration Guide

**1. Database Connection String**
Open the `Web.config` file in the `WebCar` project. Update the `<connectionStrings>` block to match your local Oracle instance:
```xml
<connectionStrings>
    <add name="OracleDbContext" 
         connectionString="Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ORCL)));User Id=CARSALE;Password=YourPassword;" 
         providerName="Oracle.ManagedDataAccess.Client" />
</connectionStrings>
```

**2. Restore NuGet Packages**
Open `WebCar.sln` in Visual Studio. Right-click the Solution and select **Restore NuGet Packages** to ensure `EntityFramework 6.5.1` and `Oracle.ManagedDataAccess` are properly installed.

**3. Application Context & Security Labels**
The backend must set the Oracle Application Context immediately after opening a connection if you want VPD (Virtual Private Database) rules to apply correctly based on the logged-in user. This ensures rows in tables like `ORDERS` or `CUSTOMER` are filtered based on the `USER_SECURITY_LABEL`.

---

## 📸 Screenshots & Demo (Backend Notes)

### 1. Discovery & Browsing (Home Page)
<img width="1137" height="1161" alt="home-page" src="https://github.com/user-attachments/assets/99fa9bbd-54d0-4173-9e29-e890be8a9c2e" />

> **Backend Integration Note:** The `Index` action fetches the list of available cars via Entity Framework from the `CAR` table. If VPD is active, the database automatically filters out records the current DB session doesn't have the `SECURITY_LEVEL` to view.

### 2. Administration & Monitoring (Admin Dashboard)
<img width="1197" height="1026" alt="admin-dashboard" src="https://github.com/user-attachments/assets/7b6695d3-e7f4-4e13-b172-db43d1495687" />

> **Backend Integration Note:** Accessing this dashboard requires the `CARSALE_ADMIN_ROLE`. The backend aggregates data from `CUSTOMER`, `CAR`, and `ORDERS` tables. It also provides quick access to manage Data Security Labels, Users, and initiate Backup & Restore processes.

### 3. Traceability & Protection (Security & Audit Logs)
<img width="2549" height="3023" alt="audit-logs" src="https://github.com/user-attachments/assets/a0749591-b40f-4480-a1b4-21087a2e95be" />

> **Backend Integration Note:** 
> - **Auditing:** Fine-Grained Auditing (FGA) and triggers capture every critical event (`LOGIN`, `UPDATE`, `ACCESS_DENIED`, etc.). These events are directly logged into the `AUDIT_LOG` table and surfaced on this admin view.
> - **Encryption:** Custom logs also indicate events like `CREATE_ORDER_ENCRYPTED`, verifying that sensitive customer data or order details are securely encrypted at the database level using `SYS.DBMS_CRYPTO` before being persisted.

---

## 👨‍💻 Author

- **Hoài Nam**
- GitHub: [HoaiNam2k5](https://github.com/HoaiNam2k5)

**Last Updated**: 2026  
**Version**: 2.0
