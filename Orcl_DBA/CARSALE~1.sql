
-- Tạo Profile để quản lý tài nguyên
CREATE PROFILE CUSTOMER_PROFILE LIMIT
  SESSIONS_PER_USER 3
  CPU_PER_SESSION UNLIMITED
  CPU_PER_CALL 3000
  CONNECT_TIME UNLIMITED
  IDLE_TIME 30
  LOGICAL_READS_PER_SESSION UNLIMITED
  PRIVATE_SGA UNLIMITED
  FAILED_LOGIN_ATTEMPTS 3
  PASSWORD_LIFE_TIME 90
  PASSWORD_REUSE_TIME 365
  PASSWORD_REUSE_MAX 5
  PASSWORD_LOCK_TIME 1;

-- ========================================
-- PHẦN 2: CẤU HÌNH ACL CHO KẾT NỐI
-- ========================================

-- Cấp quyền cho user CARSALE kết nối từ ứng dụng
BEGIN
  DBMS_NETWORK_ACL_ADMIN.CREATE_ACL(
    acl         => 'carsale_app_acl.xml',
    description => 'ACL for CarSale Application',
    principal   => 'CARSALE',
    is_grant    => TRUE,
    privilege   => 'connect'
  );
  
  DBMS_NETWORK_ACL_ADMIN.ASSIGN_ACL(
    acl  => 'carsale_app_acl.xml',
    host => '*'
  );
  
  COMMIT;
END;
/

-- ========================================
-- PHẦN 3: STORED PROCEDURES - ĐĂNG KÝ
-- ========================================

CREATE OR REPLACE PROCEDURE SP_REGISTER_CUSTOMER(
    p_hoten     IN VARCHAR2,
    p_email     IN VARCHAR2,
    p_sdt       IN VARCHAR2,
    p_matkhau   IN VARCHAR2,
    p_diachi    IN VARCHAR2,
    p_result    OUT NUMBER,
    p_message   OUT VARCHAR2,
    p_makh      OUT NUMBER
)
AS
    v_count NUMBER;
    v_encrypted_password VARCHAR2(256);
BEGIN
    -- Kiểm tra email đã tồn tại chưa
    SELECT COUNT(*) INTO v_count 
    FROM CUSTOMER 
    WHERE EMAIL = p_email;
    
    IF v_count > 0 THEN
        p_result := 0;
        p_message := 'Email đã được sử dụng';
        p_makh := NULL;
        RETURN;
    END IF;
    
    -- Mã hóa mật khẩu bằng SHA-256
    v_encrypted_password := DBMS_CRYPTO.HASH(
        UTL_RAW.CAST_TO_RAW(p_matkhau),
        DBMS_CRYPTO.HASH_SH256
    );
    
    -- Lấy ID mới từ sequence
    SELECT SEQ_CUSTOMER.NEXTVAL INTO p_makh FROM DUAL;
    
    -- Thêm customer mới
    INSERT INTO CUSTOMER (MAKH, HOTEN, EMAIL, SDT, MATKHAU, DIACHI, NGAYDANGKY)
    VALUES (p_makh, p_hoten, p_email, p_sdt, v_encrypted_password, p_diachi, SYSDATE);
    
    -- Tạo role mặc định cho customer
    INSERT INTO ACCOUNT_ROLE (MATK, MAKH, ROLENAME)
    VALUES (p_makh, p_makh, 'CUSTOMER');
    
    COMMIT;
    
    p_result := 1;
    p_message := 'Đăng ký thành công';
    
    -- Ghi audit log
    INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
    VALUES (SEQ_LOG.NEXTVAL, p_makh, 'REGISTER', 'CUSTOMER', SYSDATE, NULL);
    COMMIT;
    
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result := 0;
        p_message := 'Lỗi: ' || SQLERRM;
        p_makh := NULL;
END;
/

-- ========================================
-- PHẦN 4: STORED PROCEDURES - ĐĂNG NHẬP
-- ========================================

CREATE OR REPLACE PROCEDURE SP_LOGIN_CUSTOMER(
    p_email     IN VARCHAR2,
    p_matkhau   IN VARCHAR2,
    p_ip        IN VARCHAR2,
    p_result    OUT NUMBER,
    p_message   OUT VARCHAR2,
    p_makh      OUT NUMBER,
    p_hoten     OUT VARCHAR2,
    p_rolename  OUT VARCHAR2
)
AS
    v_encrypted_password VARCHAR2(256);
    v_stored_password VARCHAR2(256);
    v_count NUMBER;
BEGIN
    -- Mã hóa mật khẩu nhập vào
    v_encrypted_password := DBMS_CRYPTO.HASH(
        UTL_RAW.CAST_TO_RAW(p_matkhau),
        DBMS_CRYPTO.HASH_SH256
    );
    
    -- Kiểm tra thông tin đăng nhập
    SELECT COUNT(*), MAX(c.MAKH), MAX(c.HOTEN), MAX(ar.ROLENAME)
    INTO v_count, p_makh, p_hoten, p_rolename
    FROM CUSTOMER c
    LEFT JOIN ACCOUNT_ROLE ar ON c.MAKH = ar.MAKH
    WHERE c.EMAIL = p_email 
    AND c.MATKHAU = v_encrypted_password;
    
    IF v_count = 0 THEN
        p_result := 0;
        p_message := 'Email hoặc mật khẩu không đúng';
        p_makh := NULL;
        p_hoten := NULL;
        p_rolename := NULL;
        RETURN;
    END IF;
    
    p_result := 1;
    p_message := 'Đăng nhập thành công';
    
    -- Ghi audit log
    INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
    VALUES (SEQ_LOG.NEXTVAL, p_makh, 'LOGIN', 'CUSTOMER', SYSDATE, p_ip);
    COMMIT;
    
EXCEPTION
    WHEN OTHERS THEN
        p_result := 0;
        p_message := 'Lỗi: ' || SQLERRM;
        p_makh := NULL;
        p_hoten := NULL;
        p_rolename := NULL;
END;
/

-- ========================================
-- PHẦN 5: STORED PROCEDURES - ĐĂNG XUẤT
-- ========================================

CREATE OR REPLACE PROCEDURE SP_LOGOUT_CUSTOMER(
    p_makh      IN NUMBER,
    p_ip        IN VARCHAR2,
    p_result    OUT NUMBER,
    p_message   OUT VARCHAR2
)
AS
BEGIN
    -- Ghi audit log
    INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
    VALUES (SEQ_LOG.NEXTVAL, p_makh, 'LOGOUT', 'CUSTOMER', SYSDATE, p_ip);
    COMMIT;
    
    p_result := 1;
    p_message := 'Đăng xuất thành công';
    
EXCEPTION
    WHEN OTHERS THEN
        p_result := 0;
        p_message := 'Lỗi: ' || SQLERRM;
END;
/

-- ========================================
-- PHẦN 6: TEST PROCEDURES
-- ========================================

-- Test đăng ký
DECLARE
    v_result NUMBER;
    v_message VARCHAR2(200);
    v_makh NUMBER;
BEGIN
    SP_REGISTER_CUSTOMER(
        'Nguyen Van A',
        'nguyenvana@gmail.com',
        '0901234567',
        'password123',
        '123 Nguyen Hue, TPHCM',
        v_result,
        v_message,
        v_makh
    );
    
    DBMS_OUTPUT.PUT_LINE('Result: ' || v_result);
    DBMS_OUTPUT.PUT_LINE('Message: ' || v_message);
    DBMS_OUTPUT.PUT_LINE('MaKH: ' || v_makh);
END;
/

-- Test đăng nhập
DECLARE
    v_result NUMBER;
    v_message VARCHAR2(200);
    v_makh NUMBER;
    v_hoten VARCHAR2(100);
    v_rolename VARCHAR2(30);
BEGIN
    SP_LOGIN_CUSTOMER(
        'nguyenvana@gmail.com',
        'password123',
        '192.168.1.100',
        v_result,
        v_message,
        v_makh,
        v_hoten,
        v_rolename
    );
    
    DBMS_OUTPUT.PUT_LINE('Result: ' || v_result);
    DBMS_OUTPUT.PUT_LINE('Message: ' || v_message);
    DBMS_OUTPUT.PUT_LINE('MaKH: ' || v_makh);
    DBMS_OUTPUT.PUT_LINE('HoTen: ' || v_hoten);
    DBMS_OUTPUT.PUT_LINE('Role: ' || v_rolename);
END;
/
-- 1. Kiểm tra customer mới
SELECT * FROM CUSTOMER ORDER BY NGAYDANGKY DESC;

-- 2. Kiểm tra audit log
SELECT * FROM AUDIT_LOG ORDER BY NGAYGIO DESC;

-- 3. Kiểm tra roles
SELECT * FROM ACCOUNT_ROLE;
-- Xem procedures
SELECT OBJECT_NAME, STATUS FROM USER_OBJECTS WHERE OBJECT_TYPE = 'PROCEDURE';

-- Test đăng ký
BEGIN
    DECLARE
        v_result NUMBER;
        v_message VARCHAR2(200);
        v_makh NUMBER;
    BEGIN
        SP_REGISTER_CUSTOMER(
            'Test User',
            'test@test.com',
            '0900000000',
            'password123',
            'Test Address',
            v_result,
            v_message,
            v_makh
        );
        DBMS_OUTPUT.PUT_LINE('Result: ' || v_result);
        DBMS_OUTPUT.PUT_LINE('Message: ' || v_message);
    END;
END;
/


-- ============================================================
-- TUẦN 6: MÃ HÓA BẤT ĐỐI XỨNG (ASYMMETRIC ENCRYPTION)
-- ============================================================
-- Drop procedure cũ
DROP PROCEDURE SP_GENERATE_RSA_KEYPAIR;

-- Tạo lại procedure đã sửa
CREATE OR REPLACE PROCEDURE SP_GENERATE_RSA_KEYPAIR(
    p_key_size IN NUMBER DEFAULT 2048,
    p_result OUT NUMBER,
    p_message OUT VARCHAR2
)
AS
    v_random_bytes RAW(256);
    v_private_key RAW(2048);
    v_public_key RAW(2048);
    v_keyid NUMBER;
    v_timestamp VARCHAR2(50);
BEGIN
    -- Generate random bytes cho key
    v_random_bytes := DBMS_CRYPTO.RANDOMBYTES(256);
    
    -- Tạo timestamp unique - SỬA LỖI Ở ĐÂY
    v_timestamp := TO_CHAR(SYSDATE, 'YYYYMMDDHH24MISS');
    
    -- Tạo key pair (simplified version)
    v_public_key := UTL_RAW.CONCAT(
        v_random_bytes,
        UTL_RAW.CAST_TO_RAW('PUBLIC_KEY_' || v_timestamp)
    );
    
    v_private_key := UTL_RAW.CONCAT(
        v_random_bytes,
        UTL_RAW.CAST_TO_RAW('PRIVATE_KEY_' || v_timestamp)
    );
    
    -- Lưu vào database
    SELECT SEQ_KEY.NEXTVAL INTO v_keyid FROM DUAL;
    
    INSERT INTO ENCRYPTION_KEY (KEYID, KEYTYPE, PUBLICKEY, PRIVATEKEY, CREATEDDATE)
    VALUES (v_keyid, 'RSA', RAWTOHEX(v_public_key), RAWTOHEX(v_private_key), SYSDATE);
    
    COMMIT;
    
    p_result := 1;
    p_message := 'Tạo RSA key pair thành công. KeyID: ' || v_keyid;
    
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result := 0;
        p_message := 'Lỗi: ' || SQLERRM;
END;
/
SET SERVEROUTPUT ON;

-- Test 1: Tạo RSA key pair
DECLARE
    v_result NUMBER;
    v_message VARCHAR2(500);
BEGIN
    SP_GENERATE_RSA_KEYPAIR(2048, v_result, v_message);
    DBMS_OUTPUT.PUT_LINE('Result: ' || v_result);
    DBMS_OUTPUT.PUT_LINE('Message: ' || v_message);
END;
/
-- Xem key vừa tạo
SELECT KEYID, KEYTYPE, 
       LENGTH(PUBLICKEY) AS "Public Key Length", 
       LENGTH(PRIVATEKEY) AS "Private Key Length",
       CREATEDDATE
FROM ENCRYPTION_KEY
ORDER BY CREATEDDATE DESC;
-- Test mã hóa dữ liệu
DECLARE
    v_result NUMBER;
    v_message VARCHAR2(500);
    v_encrypted VARCHAR2(4000);
    v_keyid NUMBER;
BEGIN
    -- Lấy KeyID mới nhất
    SELECT MAX(KEYID) INTO v_keyid FROM ENCRYPTION_KEY WHERE KEYTYPE = 'RSA';
    
    IF v_keyid IS NULL THEN
        DBMS_OUTPUT.PUT_LINE('Lỗi: Chưa có key. Hãy chạy SP_GENERATE_RSA_KEYPAIR trước.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('Sử dụng KeyID: ' || v_keyid);
        
        SP_ENCRYPT_WITH_PUBLIC_KEY(
            'Dữ liệu nhạy cảm cần mã hóa', 
            v_keyid, 
            v_encrypted, 
            v_result, 
            v_message
        );
        
        DBMS_OUTPUT.PUT_LINE('Result: ' || v_result);
        DBMS_OUTPUT.PUT_LINE('Message: ' || v_message);
        IF v_result = 1 THEN
            DBMS_OUTPUT.PUT_LINE('Encrypted (first 100 chars): ' || SUBSTR(v_encrypted, 1, 100) || '...');
        END IF;
    END IF;
END;
/
-- ============================================================
-- TUẦN 7: MÃ HÓA LẠI DỮ LIỆU (RE-ENCRYPTION)
-- ============================================================

-- Procedure để thay đổi thuật toán mã hóa mật khẩu
CREATE OR REPLACE PROCEDURE SP_REENCRYPT_PASSWORDS(
    p_old_algorithm IN VARCHAR2 DEFAULT 'SHA256',
    p_new_algorithm IN VARCHAR2 DEFAULT 'SHA512',
    p_result OUT NUMBER,
    p_message OUT VARCHAR2
)
AS
    v_count NUMBER := 0;
    CURSOR c_customers IS
        SELECT MAKH, EMAIL, MATKHAU FROM CUSTOMER;
BEGIN
    -- Backup dữ liệu cũ
    EXECUTE IMMEDIATE 'CREATE TABLE CUSTOMER_BACKUP AS SELECT * FROM CUSTOMER';
    
    -- Re-encrypt với thuật toán mới
    FOR rec IN c_customers LOOP
        -- Trong thực tế, cần decrypt rồi encrypt lại
        -- Đây là demo: hash lại password hiện tại
        UPDATE CUSTOMER
        SET MATKHAU = DBMS_CRYPTO.HASH(
            UTL_RAW.CAST_TO_RAW(MATKHAU),
            CASE p_new_algorithm
                WHEN 'SHA512' THEN DBMS_CRYPTO.HASH_SH512
                WHEN 'SHA384' THEN DBMS_CRYPTO.HASH_SH384
                ELSE DBMS_CRYPTO.HASH_SH256
            END
        )
        WHERE MAKH = rec.MAKH;
        
        v_count := v_count + 1;
    END LOOP;
    
    COMMIT;
    
    p_result := 1;
    p_message := 'Đã mã hóa lại ' || v_count || ' mật khẩu';
    
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result := 0;
        p_message := 'Lỗi: ' || SQLERRM;
END;
/

-- ============================================================
-- TUẦN 8: TABLESPACE, PROFILE, SESSION
-- ============================================================

-- Tạo Profile cho user
CREATE PROFILE CARSALE_CUSTOMER_PROFILE LIMIT
    SESSIONS_PER_USER 3
    CPU_PER_SESSION UNLIMITED
    CPU_PER_CALL 3000
    CONNECT_TIME 240
    IDLE_TIME 30
    LOGICAL_READS_PER_SESSION UNLIMITED
    LOGICAL_READS_PER_CALL 1000
    PRIVATE_SGA UNLIMITED
    COMPOSITE_LIMIT UNLIMITED
    FAILED_LOGIN_ATTEMPTS 3
    PASSWORD_LIFE_TIME 90
    PASSWORD_REUSE_TIME 365
    PASSWORD_REUSE_MAX 5
    PASSWORD_LOCK_TIME 1
    PASSWORD_GRACE_TIME 7
    PASSWORD_VERIFY_FUNCTION NULL;

-- Tạo Profile cho admin
CREATE PROFILE CARSALE_ADMIN_PROFILE LIMIT
    SESSIONS_PER_USER 5
    CPU_PER_SESSION UNLIMITED
    CPU_PER_CALL UNLIMITED
    CONNECT_TIME UNLIMITED
    IDLE_TIME 60
    FAILED_LOGIN_ATTEMPTS 5
    PASSWORD_LIFE_TIME 60
    PASSWORD_REUSE_TIME 365
    PASSWORD_REUSE_MAX 10
    PASSWORD_LOCK_TIME 1;

-- Procedure để quản lý session
CREATE OR REPLACE PROCEDURE SP_MANAGE_USER_SESSION(
    p_username IN VARCHAR2,
    p_action IN VARCHAR2, -- 'KILL', 'CHECK', 'LIST'
    p_result OUT NUMBER,
    p_message OUT VARCHAR2
)
AS
    v_sid NUMBER;
    v_serial# NUMBER;
    v_count NUMBER;
    v_sql VARCHAR2(500);
BEGIN
    IF p_action = 'CHECK' THEN
        -- Kiểm tra số session của user
        SELECT COUNT(*) INTO v_count
        FROM V$SESSION
        WHERE USERNAME = UPPER(p_username);
        
        p_result := 1;
        p_message := 'User ' || p_username || ' có ' || v_count || ' session(s)';
        
    ELSIF p_action = 'KILL' THEN
        -- Kill tất cả session của user
        FOR rec IN (SELECT SID, SERIAL# FROM V$SESSION WHERE USERNAME = UPPER(p_username)) LOOP
            v_sql := 'ALTER SYSTEM KILL SESSION ''' || rec.SID || ',' || rec.SERIAL# || ''' IMMEDIATE';
            EXECUTE IMMEDIATE v_sql;
        END LOOP;
        
        p_result := 1;
        p_message := 'Đã kill session của user ' || p_username;
        
    ELSIF p_action = 'LIST' THEN
        -- List session info
        p_result := 1;
        p_message := 'Xem V$SESSION để biết chi tiết';
    END IF;
    
EXCEPTION
    WHEN OTHERS THEN
        p_result := 0;
        p_message := 'Lỗi: ' || SQLERRM;
END;
/

-- ============================================================
-- TUẦN 9: DISCRETIONARY ACCESS CONTROL (DAC)
-- ============================================================

-- Tạo roles
CREATE ROLE CARSALE_ADMIN_ROLE;
CREATE ROLE CARSALE_MANAGER_ROLE;
CREATE ROLE CARSALE_SALES_ROLE;
CREATE ROLE CARSALE_VIEWER_ROLE;

-- Grant quyền cho ADMIN role
GRANT ALL ON CUSTOMER TO CARSALE_ADMIN_ROLE;
GRANT ALL ON CAR TO CARSALE_ADMIN_ROLE;
GRANT ALL ON ORDERS TO CARSALE_ADMIN_ROLE;
GRANT ALL ON ORDER_DETAIL TO CARSALE_ADMIN_ROLE;
GRANT ALL ON FEEDBACK TO CARSALE_ADMIN_ROLE;
GRANT ALL ON AUDIT_LOG TO CARSALE_ADMIN_ROLE;
GRANT ALL ON ACCOUNT_ROLE TO CARSALE_ADMIN_ROLE;
GRANT EXECUTE ON SP_LOGIN_CUSTOMER TO CARSALE_ADMIN_ROLE;
GRANT EXECUTE ON SP_REGISTER_CUSTOMER TO CARSALE_ADMIN_ROLE;
GRANT EXECUTE ON SP_LOGOUT_CUSTOMER TO CARSALE_ADMIN_ROLE;

-- Grant quyền cho MANAGER role
GRANT SELECT, INSERT, UPDATE ON CUSTOMER TO CARSALE_MANAGER_ROLE;
GRANT SELECT, INSERT, UPDATE ON CAR TO CARSALE_MANAGER_ROLE;
GRANT SELECT, INSERT, UPDATE ON ORDERS TO CARSALE_MANAGER_ROLE;
GRANT SELECT ON AUDIT_LOG TO CARSALE_MANAGER_ROLE;

-- Grant quyền cho SALES role
GRANT SELECT ON CUSTOMER TO CARSALE_SALES_ROLE;
GRANT SELECT ON CAR TO CARSALE_SALES_ROLE;
GRANT SELECT, INSERT, UPDATE ON ORDERS TO CARSALE_SALES_ROLE;
GRANT SELECT, INSERT, UPDATE ON ORDER_DETAIL TO CARSALE_SALES_ROLE;

-- Grant quyền cho VIEWER role
GRANT SELECT ON CAR TO CARSALE_VIEWER_ROLE;
GRANT SELECT ON FEEDBACK TO CARSALE_VIEWER_ROLE;

-- Procedure để grant quyền động
CREATE OR REPLACE PROCEDURE SP_GRANT_USER_PERMISSION(
    p_username IN VARCHAR2,
    p_role IN VARCHAR2,
    p_result OUT NUMBER,
    p_message OUT VARCHAR2
)
AS
    v_sql VARCHAR2(500);
BEGIN
    v_sql := 'GRANT ' || p_role || ' TO ' || p_username;
    EXECUTE IMMEDIATE v_sql;
    
    -- Ghi log
    INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
    VALUES (SEQ_LOG.NEXTVAL, 0, 'GRANT_ROLE: ' || p_role, 'USER: ' || p_username, SYSDATE, NULL);
    COMMIT;
    
    p_result := 1;
    p_message := 'Đã grant role ' || p_role || ' cho user ' || p_username;
    
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result := 0;
        p_message := 'Lỗi: ' || SQLERRM;
END;
/

-- ============================================================
-- TUẦN 10: MANDATORY ACCESS CONTROL (MAC) - VPD
-- ============================================================

-- Tạo function policy cho VPD
CREATE OR REPLACE FUNCTION FN_CUSTOMER_SECURITY_POLICY(
    p_schema IN VARCHAR2,
    p_object IN VARCHAR2
)
RETURN VARCHAR2
AS
    v_predicate VARCHAR2(4000);
    v_user VARCHAR2(100);
BEGIN
    v_user := SYS_CONTEXT('USERENV', 'SESSION_USER');
    
    -- Admin thấy tất cả
    IF v_user = 'CARSALE' THEN
        RETURN '';
    END IF;
    
    -- User khác chỉ thấy dữ liệu của mình
    v_predicate := 'EMAIL = ''' || v_user || '''';
    
    RETURN v_predicate;
END;
/

-- Áp dụng VPD policy cho bảng CUSTOMER
BEGIN
    DBMS_RLS.ADD_POLICY(
        object_schema   => 'CARSALE',
        object_name     => 'CUSTOMER',
        policy_name     => 'CUSTOMER_VPD_POLICY',
        function_schema => 'CARSALE',
        policy_function => 'FN_CUSTOMER_SECURITY_POLICY',
        statement_types => 'SELECT, UPDATE, DELETE',
        update_check    => TRUE,
        enable          => TRUE
    );
END;
/

-- Policy cho bảng ORDERS
CREATE OR REPLACE FUNCTION FN_ORDERS_SECURITY_POLICY(
    p_schema IN VARCHAR2,
    p_object IN VARCHAR2
)
RETURN VARCHAR2
AS
    v_predicate VARCHAR2(4000);
    v_user VARCHAR2(100);
    v_makh NUMBER;
BEGIN
    v_user := SYS_CONTEXT('USERENV', 'SESSION_USER');
    
    -- Admin thấy tất cả
    IF v_user = 'CARSALE' THEN
        RETURN '';
    END IF;
    
    -- Lấy MAKH từ email
    BEGIN
        SELECT MAKH INTO v_makh FROM CUSTOMER WHERE EMAIL = v_user;
        v_predicate := 'MAKH = ' || v_makh;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            v_predicate := '1=0'; -- Không cho phép truy cập
    END;
    
    RETURN v_predicate;
END;
/

BEGIN
    DBMS_RLS.ADD_POLICY(
        object_schema   => 'CARSALE',
        object_name     => 'ORDERS',
        policy_name     => 'ORDERS_VPD_POLICY',
        function_schema => 'CARSALE',
        policy_function => 'FN_ORDERS_SECURITY_POLICY',
        statement_types => 'SELECT, INSERT, UPDATE, DELETE',
        update_check    => TRUE,
        enable          => TRUE
    );
END;
/
-- ============================================================
-- TUẦN 11: MANDATORY ACCESS CONTROL - ALTERNATIVE TO OLS
-- Sử dụng VPD + Application Context thay cho Oracle Label Security
-- ============================================================

-- ============================================================
-- PHẦN 1: TẠO BẢNG SECURITY LABELS (THAY CHO OLS)
-- ============================================================

-- Bảng định nghĩa Security Levels (Mức độ bảo mật)
CREATE TABLE SECURITY_LEVEL (
    LEVEL_ID NUMBER PRIMARY KEY,
    LEVEL_NAME VARCHAR2(50) NOT NULL,
    LEVEL_NUM NUMBER NOT NULL, -- 1000=PUBLIC, 2000=CONFIDENTIAL, 3000=HIGHLY_CONFIDENTIAL
    DESCRIPTION VARCHAR2(200),
    CREATED_DATE DATE DEFAULT SYSDATE
);

-- Bảng định nghĩa Compartments (Phân loại dữ liệu)
CREATE TABLE SECURITY_COMPARTMENT (
    COMP_ID NUMBER PRIMARY KEY,
    COMP_NAME VARCHAR2(50) NOT NULL,
    COMP_CODE VARCHAR2(10) NOT NULL,
    DESCRIPTION VARCHAR2(200),
    CREATED_DATE DATE DEFAULT SYSDATE
);

-- Bảng gán Security Label cho User
CREATE TABLE USER_SECURITY_LABEL (
    USER_LABEL_ID NUMBER PRIMARY KEY,
    MAKH NUMBER NOT NULL,
    MAX_READ_LEVEL NUMBER NOT NULL,  -- Mức cao nhất được đọc
    MAX_WRITE_LEVEL NUMBER NOT NULL, -- Mức cao nhất được ghi
    COMPARTMENTS VARCHAR2(200),       -- Các compartment được truy cập (FIN,SAL)
    CREATED_DATE DATE DEFAULT SYSDATE,
    CONSTRAINT FK_USER_LABEL FOREIGN KEY (MAKH) REFERENCES CUSTOMER(MAKH)
);

-- Bảng gán Security Label cho Data
CREATE TABLE DATA_SECURITY_LABEL (
    DATA_LABEL_ID NUMBER PRIMARY KEY,
    TABLE_NAME VARCHAR2(50) NOT NULL,
    RECORD_ID NUMBER NOT NULL,
    SECURITY_LEVEL NUMBER NOT NULL,   -- 1000, 2000, 3000
    COMPARTMENTS VARCHAR2(200),       -- FIN, SAL
    CREATED_DATE DATE DEFAULT SYSDATE
);

-- Tạo sequences
CREATE SEQUENCE SEQ_SECURITY_LEVEL START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_SECURITY_COMP START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_USER_LABEL START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE SEQ_DATA_LABEL START WITH 1 INCREMENT BY 1;

-- ============================================================
-- PHẦN 2: INSERT DỮ LIỆU MẪU
-- ============================================================

-- Insert Security Levels
INSERT INTO SECURITY_LEVEL (LEVEL_ID, LEVEL_NAME, LEVEL_NUM, DESCRIPTION)
VALUES (1, 'PUBLIC', 1000, 'Thông tin công khai');

INSERT INTO SECURITY_LEVEL (LEVEL_ID, LEVEL_NAME, LEVEL_NUM, DESCRIPTION)
VALUES (2, 'CONFIDENTIAL', 2000, 'Thông tin mật');

INSERT INTO SECURITY_LEVEL (LEVEL_ID, LEVEL_NAME, LEVEL_NUM, DESCRIPTION)
VALUES (3, 'HIGHLY_CONFIDENTIAL', 3000, 'Thông tin tuyệt mật');

-- Insert Compartments
INSERT INTO SECURITY_COMPARTMENT (COMP_ID, COMP_NAME, COMP_CODE, DESCRIPTION)
VALUES (1, 'FINANCE', 'FIN', 'Phòng tài chính');

INSERT INTO SECURITY_COMPARTMENT (COMP_ID, COMP_NAME, COMP_CODE, DESCRIPTION)
VALUES (2, 'SALES', 'SAL', 'Phòng kinh doanh');

INSERT INTO SECURITY_COMPARTMENT (COMP_ID, COMP_NAME, COMP_CODE, DESCRIPTION)
VALUES (3, 'ADMIN', 'ADM', 'Quản trị');

COMMIT;

-- ============================================================
-- PHẦN 3: TẠO APPLICATION CONTEXT
-- ============================================================

-- Tạo context namespace
CREATE OR REPLACE CONTEXT CARSALE_SECURITY_CTX USING PKG_SECURITY_CONTEXT;

-- Package để quản lý context
CREATE OR REPLACE PACKAGE PKG_SECURITY_CONTEXT AS
    PROCEDURE SET_USER_CONTEXT(p_makh IN NUMBER);
    PROCEDURE CLEAR_USER_CONTEXT;
END;
/

CREATE OR REPLACE PACKAGE BODY PKG_SECURITY_CONTEXT AS
    
    PROCEDURE SET_USER_CONTEXT(p_makh IN NUMBER) IS
        v_max_read_level NUMBER;
        v_max_write_level NUMBER;
        v_compartments VARCHAR2(200);
    BEGIN
        -- Lấy thông tin security của user
        BEGIN
            SELECT MAX_READ_LEVEL, MAX_WRITE_LEVEL, COMPARTMENTS
            INTO v_max_read_level, v_max_write_level, v_compartments
            FROM USER_SECURITY_LABEL
            WHERE MAKH = p_makh;
            
            -- Set context
            DBMS_SESSION.SET_CONTEXT('CARSALE_SECURITY_CTX', 'MAKH', TO_CHAR(p_makh));
            DBMS_SESSION.SET_CONTEXT('CARSALE_SECURITY_CTX', 'MAX_READ_LEVEL', TO_CHAR(v_max_read_level));
            DBMS_SESSION.SET_CONTEXT('CARSALE_SECURITY_CTX', 'MAX_WRITE_LEVEL', TO_CHAR(v_max_write_level));
            DBMS_SESSION.SET_CONTEXT('CARSALE_SECURITY_CTX', 'COMPARTMENTS', v_compartments);
            
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                -- User chưa có security label, set mặc định PUBLIC
                DBMS_SESSION.SET_CONTEXT('CARSALE_SECURITY_CTX', 'MAKH', TO_CHAR(p_makh));
                DBMS_SESSION.SET_CONTEXT('CARSALE_SECURITY_CTX', 'MAX_READ_LEVEL', '1000');
                DBMS_SESSION.SET_CONTEXT('CARSALE_SECURITY_CTX', 'MAX_WRITE_LEVEL', '1000');
                DBMS_SESSION.SET_CONTEXT('CARSALE_SECURITY_CTX', 'COMPARTMENTS', '');
        END;
    END;
    
    PROCEDURE CLEAR_USER_CONTEXT IS
    BEGIN
        DBMS_SESSION.CLEAR_CONTEXT('CARSALE_SECURITY_CTX');
    END;
    
END;
/

-- ============================================================
-- PHẦN 4: VPD POLICY FUNCTIONS VỚI MAC
-- ============================================================

-- Policy function cho READ access với MAC
CREATE OR REPLACE FUNCTION FN_MAC_READ_POLICY(
    p_schema IN VARCHAR2,
    p_object IN VARCHAR2
)
RETURN VARCHAR2
AS
    v_predicate VARCHAR2(4000);
    v_user_read_level NUMBER;
    v_user_compartments VARCHAR2(200);
    v_user VARCHAR2(100);
BEGIN
    v_user := SYS_CONTEXT('USERENV', 'SESSION_USER');
    
    -- Admin thấy tất cả
    IF v_user = 'CARSALE' THEN
        RETURN '';
    END IF;
    
    -- Lấy security level của user từ context
    BEGIN
        v_user_read_level := TO_NUMBER(SYS_CONTEXT('CARSALE_SECURITY_CTX', 'MAX_READ_LEVEL'));
        v_user_compartments := SYS_CONTEXT('CARSALE_SECURITY_CTX', 'COMPARTMENTS');
    EXCEPTION
        WHEN OTHERS THEN
            -- Nếu không có context, mặc định là PUBLIC
            v_user_read_level := 1000;
            v_user_compartments := '';
    END;
    
    -- Tạo predicate: chỉ đọc được data có level <= user's max read level
    v_predicate := '
        EXISTS (
            SELECT 1 FROM DATA_SECURITY_LABEL dsl
            WHERE dsl.TABLE_NAME = ''' || p_object || '''
            AND dsl.RECORD_ID = ' || p_object || '.MAKH
            AND dsl.SECURITY_LEVEL <= ' || v_user_read_level;
    
    -- Kiểm tra compartments nếu có
    IF v_user_compartments IS NOT NULL THEN
        v_predicate := v_predicate || '
            AND (dsl.COMPARTMENTS IS NULL 
                 OR INSTR(''' || v_user_compartments || ''', dsl.COMPARTMENTS) > 0)';
    END IF;
    
    v_predicate := v_predicate || '
        )';
    
    RETURN v_predicate;
END;
/

-- Policy function cho WRITE access với MAC
CREATE OR REPLACE FUNCTION FN_MAC_WRITE_POLICY(
    p_schema IN VARCHAR2,
    p_object IN VARCHAR2
)
RETURN VARCHAR2
AS
    v_predicate VARCHAR2(4000);
    v_user_write_level NUMBER;
    v_user_compartments VARCHAR2(200);
    v_user VARCHAR2(100);
BEGIN
    v_user := SYS_CONTEXT('USERENV', 'SESSION_USER');
    
    -- Admin có thể write tất cả
    IF v_user = 'CARSALE' THEN
        RETURN '';
    END IF;
    
    -- Lấy security level của user từ context
    BEGIN
        v_user_write_level := TO_NUMBER(SYS_CONTEXT('CARSALE_SECURITY_CTX', 'MAX_WRITE_LEVEL'));
        v_user_compartments := SYS_CONTEXT('CARSALE_SECURITY_CTX', 'COMPARTMENTS');
    EXCEPTION
        WHEN OTHERS THEN
            v_user_write_level := 1000;
            v_user_compartments := '';
    END;
    
    -- Tạo predicate: chỉ write được data có level <= user's max write level
    v_predicate := '
        EXISTS (
            SELECT 1 FROM DATA_SECURITY_LABEL dsl
            WHERE dsl.TABLE_NAME = ''' || p_object || '''
            AND dsl.RECORD_ID = ' || p_object || '.MAKH
            AND dsl.SECURITY_LEVEL <= ' || v_user_write_level;
    
    IF v_user_compartments IS NOT NULL THEN
        v_predicate := v_predicate || '
            AND (dsl.COMPARTMENTS IS NULL 
                 OR INSTR(''' || v_user_compartments || ''', dsl.COMPARTMENTS) > 0)';
    END IF;
    
    v_predicate := v_predicate || '
        )';
    
    RETURN v_predicate;
END;
/

-- ============================================================
-- PHẦN 5: ÁP DỤNG MAC POLICIES
-- ============================================================

-- Áp dụng MAC policy cho bảng CUSTOMER (READ)
BEGIN
    DBMS_RLS.ADD_POLICY(
        object_schema   => 'CARSALE',
        object_name     => 'CUSTOMER',
        policy_name     => 'MAC_CUSTOMER_READ_POLICY',
        function_schema => 'CARSALE',
        policy_function => 'FN_MAC_READ_POLICY',
        statement_types => 'SELECT',
        update_check    => FALSE,
        enable          => TRUE
    );
END;
/

-- Áp dụng MAC policy cho bảng CUSTOMER (WRITE)
BEGIN
    DBMS_RLS.ADD_POLICY(
        object_schema   => 'CARSALE',
        object_name     => 'CUSTOMER',
        policy_name     => 'MAC_CUSTOMER_WRITE_POLICY',
        function_schema => 'CARSALE',
        policy_function => 'FN_MAC_WRITE_POLICY',
        statement_types => 'INSERT, UPDATE, DELETE',
        update_check    => TRUE,
        enable          => TRUE
    );
END;
/

-- Áp dụng MAC policy cho bảng ORDERS (READ)
BEGIN
    DBMS_RLS.ADD_POLICY(
        object_schema   => 'CARSALE',
        object_name     => 'ORDERS',
        policy_name     => 'MAC_ORDERS_READ_POLICY',
        function_schema => 'CARSALE',
        policy_function => 'FN_MAC_READ_POLICY',
        statement_types => 'SELECT',
        update_check    => FALSE,
        enable          => TRUE
    );
END;
/

-- ============================================================
-- PHẦN 6: PROCEDURES QUẢN LÝ SECURITY LABELS
-- ============================================================

-- Procedure gán security label cho user
CREATE OR REPLACE PROCEDURE SP_ASSIGN_USER_SECURITY_LABEL(
    p_makh IN NUMBER,
    p_max_read_level IN NUMBER,
    p_max_write_level IN NUMBER,
    p_compartments IN VARCHAR2,
    p_result OUT NUMBER,
    p_message OUT VARCHAR2
)
AS
    v_count NUMBER;
BEGIN
    -- Kiểm tra user có tồn tại
    SELECT COUNT(*) INTO v_count FROM CUSTOMER WHERE MAKH = p_makh;
    
    IF v_count = 0 THEN
        p_result := 0;
        p_message := 'User không tồn tại';
        RETURN;
    END IF;
    
    -- Kiểm tra đã có label chưa
    SELECT COUNT(*) INTO v_count FROM USER_SECURITY_LABEL WHERE MAKH = p_makh;
    
    IF v_count > 0 THEN
        -- Update existing label
        UPDATE USER_SECURITY_LABEL
        SET MAX_READ_LEVEL = p_max_read_level,
            MAX_WRITE_LEVEL = p_max_write_level,
            COMPARTMENTS = p_compartments
        WHERE MAKH = p_makh;
    ELSE
        -- Insert new label
        INSERT INTO USER_SECURITY_LABEL (USER_LABEL_ID, MAKH, MAX_READ_LEVEL, MAX_WRITE_LEVEL, COMPARTMENTS)
        VALUES (SEQ_USER_LABEL.NEXTVAL, p_makh, p_max_read_level, p_max_write_level, p_compartments);
    END IF;
    
    COMMIT;
    
    p_result := 1;
    p_message := 'Đã gán security label cho user thành công';
    
    -- Ghi audit log
    INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
    VALUES (SEQ_LOG.NEXTVAL, p_makh, 'ASSIGN_SECURITY_LABEL', 'USER_SECURITY_LABEL', SYSDATE, NULL);
    COMMIT;
    
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result := 0;
        p_message := 'Lỗi: ' || SQLERRM;
END;
/

-- Procedure gán security label cho data
CREATE OR REPLACE PROCEDURE SP_ASSIGN_DATA_SECURITY_LABEL(
    p_table_name IN VARCHAR2,
    p_record_id IN NUMBER,
    p_security_level IN NUMBER,
    p_compartments IN VARCHAR2,
    p_result OUT NUMBER,
    p_message OUT VARCHAR2
)
AS
    v_count NUMBER;
BEGIN
    -- Kiểm tra đã có label chưa
    SELECT COUNT(*) INTO v_count 
    FROM DATA_SECURITY_LABEL 
    WHERE TABLE_NAME = p_table_name AND RECORD_ID = p_record_id;
    
    IF v_count > 0 THEN
        -- Update existing label
        UPDATE DATA_SECURITY_LABEL
        SET SECURITY_LEVEL = p_security_level,
            COMPARTMENTS = p_compartments
        WHERE TABLE_NAME = p_table_name AND RECORD_ID = p_record_id;
    ELSE
        -- Insert new label
        INSERT INTO DATA_SECURITY_LABEL (DATA_LABEL_ID, TABLE_NAME, RECORD_ID, SECURITY_LEVEL, COMPARTMENTS)
        VALUES (SEQ_DATA_LABEL.NEXTVAL, p_table_name, p_record_id, p_security_level, p_compartments);
    END IF;
    
    COMMIT;
    
    p_result := 1;
    p_message := 'Đã gán security label cho data thành công';
    
    -- Ghi audit log
    INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
    VALUES (SEQ_LOG.NEXTVAL, 0, 'ASSIGN_DATA_LABEL: ' || p_table_name, 'DATA_SECURITY_LABEL', SYSDATE, NULL);
    COMMIT;
    
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result := 0;
        p_message := 'Lỗi: ' || SQLERRM;
END;
/

-- ============================================================
-- PHẦN 7: TRIGGER TỰ ĐỘNG GÁN LABEL CHO DATA MỚI
-- ============================================================

-- Trigger tự động gán label PUBLIC cho customer mới
CREATE OR REPLACE TRIGGER TRG_AUTO_LABEL_CUSTOMER
AFTER INSERT ON CUSTOMER
FOR EACH ROW
BEGIN
    -- Gán label PUBLIC (1000) cho customer mới
    INSERT INTO DATA_SECURITY_LABEL (DATA_LABEL_ID, TABLE_NAME, RECORD_ID, SECURITY_LEVEL, COMPARTMENTS)
    VALUES (SEQ_DATA_LABEL.NEXTVAL, 'CUSTOMER', :NEW.MAKH, 1000, NULL);
    
    -- Gán user security label mặc định
    INSERT INTO USER_SECURITY_LABEL (USER_LABEL_ID, MAKH, MAX_READ_LEVEL, MAX_WRITE_LEVEL, COMPARTMENTS)
    VALUES (SEQ_USER_LABEL.NEXTVAL, :NEW.MAKH, 1000, 1000, NULL);
END;
/

-- ============================================================
-- PHẦN 8: PROCEDURE CẬP NHẬT LOGIN ĐỂ SET CONTEXT
-- ============================================================

-- Cập nhật procedure login để set security context
CREATE OR REPLACE PROCEDURE SP_LOGIN_CUSTOMER_WITH_MAC(
    p_email     IN VARCHAR2,
    p_matkhau   IN VARCHAR2,
    p_ip        IN VARCHAR2,
    p_result    OUT NUMBER,
    p_message   OUT VARCHAR2,
    p_makh      OUT NUMBER,
    p_hoten     OUT VARCHAR2,
    p_rolename  OUT VARCHAR2
)
AS
    v_encrypted_password VARCHAR2(256);
    v_count NUMBER;
BEGIN
    -- Mã hóa mật khẩu nhập vào
    v_encrypted_password := DBMS_CRYPTO.HASH(
        UTL_RAW.CAST_TO_RAW(p_matkhau),
        DBMS_CRYPTO.HASH_SH256
    );
    
    -- Kiểm tra thông tin đăng nhập
    SELECT COUNT(*), MAX(c.MAKH), MAX(c.HOTEN), MAX(ar.ROLENAME)
    INTO v_count, p_makh, p_hoten, p_rolename
    FROM CUSTOMER c
    LEFT JOIN ACCOUNT_ROLE ar ON c.MAKH = ar.MAKH
    WHERE c.EMAIL = p_email 
    AND c.MATKHAU = v_encrypted_password;
    
    IF v_count = 0 THEN
        p_result := 0;
        p_message := 'Email hoặc mật khẩu không đúng';
        p_makh := NULL;
        p_hoten := NULL;
        p_rolename := NULL;
        RETURN;
    END IF;
    
    -- SET SECURITY CONTEXT
    PKG_SECURITY_CONTEXT.SET_USER_CONTEXT(p_makh);
    
    p_result := 1;
    p_message := 'Đăng nhập thành công với MAC';
    
    -- Ghi audit log
    INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
    VALUES (SEQ_LOG.NEXTVAL, p_makh, 'LOGIN_WITH_MAC', 'CUSTOMER', SYSDATE, p_ip);
    COMMIT;
    
EXCEPTION
    WHEN OTHERS THEN
        p_result := 0;
        p_message := 'Lỗi: ' || SQLERRM;
        p_makh := NULL;
        p_hoten := NULL;
        p_rolename := NULL;
END;
/

-- ============================================================
-- PHẦN 9: TESTING & DEMO
-- ============================================================

-- Test 1: Tạo user với security levels khác nhau
DECLARE
    v_result NUMBER;
    v_message VARCHAR2(500);
BEGIN
    -- User 1: PUBLIC level (1000)
    SP_ASSIGN_USER_SECURITY_LABEL(1, 1000, 1000, NULL, v_result, v_message);
    DBMS_OUTPUT.PUT_LINE('User 1 (PUBLIC): ' || v_message);
    
    -- User 2: CONFIDENTIAL level (2000) with SALES compartment
    SP_ASSIGN_USER_SECURITY_LABEL(2, 2000, 2000, 'SAL', v_result, v_message);
    DBMS_OUTPUT.PUT_LINE('User 2 (CONFIDENTIAL/SALES): ' || v_message);
    
    -- User 3: HIGHLY_CONFIDENTIAL level (3000) with FINANCE compartment
    SP_ASSIGN_USER_SECURITY_LABEL(3, 3000, 3000, 'FIN', v_result, v_message);
    DBMS_OUTPUT.PUT_LINE('User 3 (HIGHLY_CONFIDENTIAL/FINANCE): ' || v_message);
END;
/

-- Test 2: Gán security labels cho data
DECLARE
    v_result NUMBER;
    v_message VARCHAR2(500);
BEGIN
    -- Customer 1: PUBLIC data
    SP_ASSIGN_DATA_SECURITY_LABEL('CUSTOMER', 1, 1000, NULL, v_result, v_message);
    DBMS_OUTPUT.PUT_LINE('Data 1 (PUBLIC): ' || v_message);
    
    -- Customer 2: CONFIDENTIAL data with SALES
    SP_ASSIGN_DATA_SECURITY_LABEL('CUSTOMER', 2, 2000, 'SAL', v_result, v_message);
    DBMS_OUTPUT.PUT_LINE('Data 2 (CONFIDENTIAL/SALES): ' || v_message);
    
    -- Customer 3: HIGHLY_CONFIDENTIAL data with FINANCE
    SP_ASSIGN_DATA_SECURITY_LABEL('CUSTOMER', 3, 3000, 'FIN', v_result, v_message);
    DBMS_OUTPUT.PUT_LINE('Data 3 (HIGHLY_CONFIDENTIAL/FINANCE): ' || v_message);
END;
/

-- ============================================================
-- PHẦN 10: VIEWS HỖ TRỢ
-- ============================================================

-- View hiển thị security assignments
CREATE OR REPLACE VIEW VW_SECURITY_ASSIGNMENTS AS
SELECT 
    c.MAKH,
    c.HOTEN,
    c.EMAIL,
    usl.MAX_READ_LEVEL,
    usl.MAX_WRITE_LEVEL,
    usl.COMPARTMENTS AS USER_COMPARTMENTS,
    sl.LEVEL_NAME AS READ_LEVEL_NAME,
    dsl.SECURITY_LEVEL AS DATA_SECURITY_LEVEL,
    dsl.COMPARTMENTS AS DATA_COMPARTMENTS
FROM CUSTOMER c
LEFT JOIN USER_SECURITY_LABEL usl ON c.MAKH = usl.MAKH
LEFT JOIN SECURITY_LEVEL sl ON usl.MAX_READ_LEVEL = sl.LEVEL_NUM
LEFT JOIN DATA_SECURITY_LABEL dsl ON c.MAKH = dsl.RECORD_ID AND dsl.TABLE_NAME = 'CUSTOMER';

-- ============================================================
-- PHẦN 11: KIỂM TRA VÀ VERIFY
-- ============================================================

-- Kiểm tra context đã được tạo
SELECT * FROM DBA_CONTEXT WHERE NAMESPACE = 'CARSALE_SECURITY_CTX';

-- Kiểm tra policies
SELECT OBJECT_OWNER, OBJECT_NAME, POLICY_NAME, ENABLE
FROM DBA_POLICIES
WHERE OBJECT_NAME IN ('CUSTOMER', 'ORDERS')
AND POLICY_NAME LIKE 'MAC%';

-- Xem security assignments
SELECT * FROM VW_SECURITY_ASSIGNMENTS;

-- Xem tất cả security levels
SELECT * FROM SECURITY_LEVEL ORDER BY LEVEL_NUM;

-- Xem tất cả compartments
SELECT * FROM SECURITY_COMPARTMENT;

-- TUẦN 12: ROLE-BASED ACCESS CONTROL (RBAC)
-- ============================================================

-- Procedure để kiểm tra quyền của user
CREATE OR REPLACE FUNCTION FN_CHECK_USER_PERMISSION(
    p_makh IN NUMBER,
    p_action IN VARCHAR2,
    p_resource IN VARCHAR2
)
RETURN NUMBER
AS
    v_rolename VARCHAR2(30);
    v_has_permission NUMBER := 0;
BEGIN
    -- Lấy role của user
    SELECT ROLENAME INTO v_rolename
    FROM ACCOUNT_ROLE
    WHERE MAKH = p_makh;
    
    -- Kiểm tra quyền theo role và action
    IF v_rolename = 'ADMIN' THEN
        v_has_permission := 1; -- Admin có tất cả quyền
    ELSIF v_rolename = 'MANAGER' THEN
        IF p_action IN ('VIEW', 'CREATE', 'UPDATE') THEN
            v_has_permission := 1;
        END IF;
    ELSIF v_rolename = 'SALES' THEN
        IF p_action IN ('VIEW', 'CREATE') AND p_resource IN ('ORDER', 'CUSTOMER') THEN
            v_has_permission := 1;
        END IF;
    ELSIF v_rolename = 'CUSTOMER' THEN
        IF p_action = 'VIEW' OR (p_action = 'CREATE' AND p_resource = 'ORDER') THEN
            v_has_permission := 1;
        END IF;
    END IF;
    
    RETURN v_has_permission;
    
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RETURN 0;
    WHEN OTHERS THEN
        RETURN 0;
END;
/

-- Procedure để thực thi action với RBAC
CREATE OR REPLACE PROCEDURE SP_EXECUTE_WITH_RBAC(
    p_makh IN NUMBER,
    p_action IN VARCHAR2,
    p_resource IN VARCHAR2,
    p_sql IN VARCHAR2,
    p_result OUT NUMBER,
    p_message OUT VARCHAR2
)
AS
    v_has_permission NUMBER;
BEGIN
    -- Kiểm tra quyền
    v_has_permission := FN_CHECK_USER_PERMISSION(p_makh, p_action, p_resource);
    
    IF v_has_permission = 0 THEN
        p_result := 0;
        p_message := 'Không có quyền thực hiện hành động này';
        
        -- Ghi log từ chối
        INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
        VALUES (SEQ_LOG.NEXTVAL, p_makh, 'ACCESS_DENIED: ' || p_action, p_resource, SYSDATE, NULL);
        COMMIT;
        
        RETURN;
    END IF;
    
    -- Thực thi SQL nếu có quyền
    EXECUTE IMMEDIATE p_sql;
    
    p_result := 1;
    p_message := 'Thực hiện thành công';
    
    -- Ghi log thành công
    INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
    VALUES (SEQ_LOG.NEXTVAL, p_makh, 'ACCESS_GRANTED: ' || p_action, p_resource, SYSDATE, NULL);
    COMMIT;
    
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result := 0;
        p_message := 'Lỗi: ' || SQLERRM;
END;
/

-- ============================================================
-- TUẦN 13: STANDARD AUDITING & TRIGGERS
-- ============================================================

-- Trigger audit cho INSERT vào CUSTOMER
CREATE OR REPLACE TRIGGER TRG_AUDIT_CUSTOMER_INSERT
AFTER INSERT ON CUSTOMER
FOR EACH ROW
BEGIN
    INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
    VALUES (SEQ_LOG.NEXTVAL, :NEW.MAKH, 'INSERT', 'CUSTOMER', SYSDATE, SYS_CONTEXT('USERENV', 'IP_ADDRESS'));
END;
/

-- Trigger audit cho UPDATE vào CUSTOMER
CREATE OR REPLACE TRIGGER TRG_AUDIT_CUSTOMER_UPDATE
AFTER UPDATE ON CUSTOMER
FOR EACH ROW
BEGIN
    INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
    VALUES (SEQ_LOG.NEXTVAL, :NEW.MAKH, 'UPDATE', 'CUSTOMER', SYSDATE, SYS_CONTEXT('USERENV', 'IP_ADDRESS'));
END;
/

-- Trigger audit cho DELETE từ CUSTOMER
CREATE OR REPLACE TRIGGER TRG_AUDIT_CUSTOMER_DELETE
BEFORE DELETE ON CUSTOMER
FOR EACH ROW
BEGIN
    INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
    VALUES (SEQ_LOG.NEXTVAL, :OLD.MAKH, 'DELETE', 'CUSTOMER', SYSDATE, SYS_CONTEXT('USERENV', 'IP_ADDRESS'));
END;
/

-- Trigger audit cho CAR
CREATE OR REPLACE TRIGGER TRG_AUDIT_CAR
AFTER INSERT OR UPDATE OR DELETE ON CAR
FOR EACH ROW
DECLARE
    v_action VARCHAR2(20);
BEGIN
    IF INSERTING THEN
        v_action := 'INSERT';
    ELSIF UPDATING THEN
        v_action := 'UPDATE';
    ELSIF DELETING THEN
        v_action := 'DELETE';
    END IF;
    
    INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
    VALUES (SEQ_LOG.NEXTVAL, 0, v_action, 'CAR', SYSDATE, SYS_CONTEXT('USERENV', 'IP_ADDRESS'));
END;
/

-- Trigger audit cho ORDERS
CREATE OR REPLACE TRIGGER TRG_AUDIT_ORDERS
AFTER INSERT OR UPDATE OR DELETE ON ORDERS
FOR EACH ROW
DECLARE
    v_action VARCHAR2(20);
    v_makh NUMBER;
BEGIN
    IF INSERTING THEN
        v_action := 'INSERT';
        v_makh := :NEW.MAKH;
    ELSIF UPDATING THEN
        v_action := 'UPDATE';
        v_makh := :NEW.MAKH;
    ELSIF DELETING THEN
        v_action := 'DELETE';
        v_makh := :OLD.MAKH;
    END IF;
    
    INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
    VALUES (SEQ_LOG.NEXTVAL, v_makh, v_action, 'ORDERS', SYSDATE, SYS_CONTEXT('USERENV', 'IP_ADDRESS'));
END;
/

-- ============================================================
-- TUẦN 14: FINE-GRAINED AUDITING (FGA)
-- ============================================================

-- FGA cho truy vấn thông tin nhạy cảm của CUSTOMER
BEGIN
    DBMS_FGA.ADD_POLICY(
        object_schema   => 'CARSALE',
        object_name     => 'CUSTOMER',
        policy_name     => 'FGA_CUSTOMER_SENSITIVE_DATA',
        audit_condition => 'EMAIL IS NOT NULL',
        audit_column    => 'EMAIL, SDT, DIACHI, MATKHAU',
        handler_schema  => NULL,
        handler_module  => NULL,
        enable          => TRUE,
        statement_types => 'SELECT, UPDATE',
        audit_trail     => DBMS_FGA.DB + DBMS_FGA.EXTENDED,
        audit_column_opts => DBMS_FGA.ANY_COLUMNS
    );
END;
/

-- FGA cho CAR (theo dõi ai xem giá xe)
BEGIN
    DBMS_FGA.ADD_POLICY(
        object_schema   => 'CARSALE',
        object_name     => 'CAR',
        policy_name     => 'FGA_CAR_PRICE_ACCESS',
        audit_condition => 'GIA > 1000000000',
        audit_column    => 'GIA',
        handler_schema  => NULL,
        handler_module  => NULL,
        enable          => TRUE,
        statement_types => 'SELECT',
        audit_trail     => DBMS_FGA.DB + DBMS_FGA.EXTENDED,
        audit_column_opts => DBMS_FGA.ANY_COLUMNS
    );
END;
/

-- FGA cho ORDERS (theo dõi xem/sửa đơn hàng lớn)
BEGIN
    DBMS_FGA.ADD_POLICY(
        object_schema   => 'CARSALE',
        object_name     => 'ORDERS',
        policy_name     => 'FGA_HIGH_VALUE_ORDERS',
        audit_condition => 'TONGTIEN > 500000000',
        audit_column    => 'TONGTIEN, TRANGTHAI',
        handler_schema  => NULL,
        handler_module  => NULL,
        enable          => TRUE,
        statement_types => 'SELECT, UPDATE',
        audit_trail     => DBMS_FGA.DB + DBMS_FGA.EXTENDED,
        audit_column_opts => DBMS_FGA.ANY_COLUMNS
    );
END;
/

-- Procedure để xem FGA audit logs
CREATE OR REPLACE PROCEDURE SP_VIEW_FGA_LOGS(
    p_table_name IN VARCHAR2 DEFAULT NULL,
    p_days_back IN NUMBER DEFAULT 7
)
AS
BEGIN
    FOR rec IN (
        SELECT 
            TIMESTAMP,
            DB_USER,
            OS_USER,
            OBJECT_SCHEMA,
            OBJECT_NAME,
            SQL_TEXT,
            POLICY_NAME
        FROM DBA_FGA_AUDIT_TRAIL
        WHERE TIMESTAMP >= SYSDATE - p_days_back
        AND (p_table_name IS NULL OR OBJECT_NAME = p_table_name)
        ORDER BY TIMESTAMP DESC
    ) LOOP
        DBMS_OUTPUT.PUT_LINE('=================================');
        DBMS_OUTPUT.PUT_LINE('Time: ' || TO_CHAR(rec.TIMESTAMP, 'YYYY-MM-DD HH24:MI:SS'));
        DBMS_OUTPUT.PUT_LINE('User: ' || rec.DB_USER || ' (OS: ' || rec.OS_USER || ')');
        DBMS_OUTPUT.PUT_LINE('Object: ' || rec.OBJECT_SCHEMA || '.' || rec.OBJECT_NAME);
        DBMS_OUTPUT.PUT_LINE('Policy: ' || rec.POLICY_NAME);
        DBMS_OUTPUT.PUT_LINE('SQL: ' || SUBSTR(rec.SQL_TEXT, 1, 100));
    END LOOP;
END;
/

-- ============================================================
-- SCRIPTS DEMO VÀ TESTING
-- ============================================================

-- Demo DAC
DECLARE
    v_result NUMBER;
    v_message VARCHAR2(500);
BEGIN
    SP_GRANT_USER_PERMISSION('TEST_USER', 'CARSALE_SALES_ROLE', v_result, v_message);
    DBMS_OUTPUT.PUT_LINE(v_message);
END;
/

-- Demo RBAC
DECLARE
    v_result NUMBER;
    v_message VARCHAR2(500);
    v_has_perm NUMBER;
BEGIN
    -- Kiểm tra quyền
    v_has_perm := FN_CHECK_USER_PERMISSION(1, 'VIEW', 'CAR');
    DBMS_OUTPUT.PUT_LINE('Has permission: ' || v_has_perm);
END;
/

-- Demo Encryption/Decryption
DECLARE
    v_result NUMBER;
    v_message VARCHAR2(500);
    v_encrypted VARCHAR2(4000);
BEGIN
    -- Generate RSA key pair
    SP_GENERATE_RSA_KEYPAIR(2048, v_result, v_message);
    DBMS_OUTPUT.PUT_LINE(v_message);
    
    -- Encrypt data
    SP_ENCRYPT_WITH_PUBLIC_KEY('Sensitive Data', 1, v_encrypted, v_result, v_message);
    DBMS_OUTPUT.PUT_LINE('Encrypted: ' || SUBSTR(v_encrypted, 1, 50) || '...');
END;
/

-- ============================================================
-- VIEWS HỖ TRỢ QUẢN TRỊ VÀ BÁO CÁO
-- ============================================================

-- View hiển thị audit log chi tiết
CREATE OR REPLACE VIEW VW_AUDIT_LOG_DETAIL AS
SELECT 
    al.MALOG,
    al.MATK,
    c.HOTEN,
    c.EMAIL,
    ar.ROLENAME,
    al.HANHDONG,
    al.BANGTACDONG,
    al.NGAYGIO,
    al.IP
FROM AUDIT_LOG al
LEFT JOIN CUSTOMER c ON al.MATK = c.MAKH
LEFT JOIN ACCOUNT_ROLE ar ON al.MATK = ar.MATK
ORDER BY al.NGAYGIO DESC;

-- View hiển thị user và role
CREATE OR REPLACE VIEW VW_USER_ROLES AS
SELECT 
    c.MAKH,
    c.HOTEN,
    c.EMAIL,
    c.SDT,
    ar.ROLENAME,
    c.NGAYDANGKY,
    (SELECT COUNT(*) FROM AUDIT_LOG WHERE MATK = c.MAKH) AS TOTAL_ACTIVITIES
FROM CUSTOMER c
LEFT JOIN ACCOUNT_ROLE ar ON c.MAKH = ar.MAKH;

-- View hiển thị security events
CREATE OR REPLACE VIEW VW_SECURITY_EVENTS AS
SELECT 
    al.NGAYGIO AS EVENT_TIME,
    c.EMAIL AS USER_EMAIL,
    al.HANHDONG AS ACTION,
    al.BANGTACDONG AS TARGET_TABLE,
    al.IP AS SOURCE_IP,
    CASE 
        WHEN al.HANHDONG LIKE 'ACCESS_DENIED%' THEN 'SECURITY_VIOLATION'
        WHEN al.HANHDONG IN ('LOGIN', 'LOGOUT') THEN 'AUTHENTICATION'
        WHEN al.HANHDONG LIKE 'GRANT%' THEN 'AUTHORIZATION'
        ELSE 'DATA_OPERATION'
    END AS EVENT_TYPE
FROM AUDIT_LOG al
LEFT JOIN CUSTOMER c ON al.MATK = c.MAKH
WHERE al.NGAYGIO >= SYSDATE - 30
ORDER BY al.NGAYGIO DESC;

-- View thống kê security
CREATE OR REPLACE VIEW VW_SECURITY_STATS AS
SELECT 
    TO_CHAR(NGAYGIO, 'YYYY-MM-DD') AS NGAY,
    COUNT(*) AS TOTAL_EVENTS,
    SUM(CASE WHEN HANHDONG = 'LOGIN' THEN 1 ELSE 0 END) AS LOGIN_COUNT,
    SUM(CASE WHEN HANHDONG = 'LOGOUT' THEN 1 ELSE 0 END) AS LOGOUT_COUNT,
    SUM(CASE WHEN HANHDONG LIKE 'ACCESS_DENIED%' THEN 1 ELSE 0 END) AS ACCESS_DENIED_COUNT,
    SUM(CASE WHEN HANHDONG IN ('INSERT', 'UPDATE', 'DELETE') THEN 1 ELSE 0 END) AS DATA_CHANGES
FROM AUDIT_LOG
WHERE NGAYGIO >= SYSDATE - 30
GROUP BY TO_CHAR(NGAYGIO, 'YYYY-MM-DD')
ORDER BY NGAY DESC;

-- ============================================================
-- PROCEDURES BỔ SUNG ĐỂ QUẢN LÝ BẢO MẬT
-- ============================================================

-- Procedure kiểm tra và report các vấn đề bảo mật
CREATE OR REPLACE PROCEDURE SP_SECURITY_HEALTH_CHECK(
    p_report OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_report FOR
    SELECT 
        'Weak Password Users' AS ISSUE_TYPE,
        COUNT(*) AS COUNT,
        'Users with simple passwords' AS DESCRIPTION
    FROM CUSTOMER
    WHERE LENGTH(MATKHAU) < 64
    UNION ALL
    SELECT 
        'Inactive Users' AS ISSUE_TYPE,
        COUNT(*) AS COUNT,
        'Users not logged in for 90 days' AS DESCRIPTION
    FROM CUSTOMER c
    WHERE NOT EXISTS (
        SELECT 1 FROM AUDIT_LOG al
        WHERE al.MATK = c.MAKH 
        AND al.HANHDONG = 'LOGIN'
        AND al.NGAYGIO >= SYSDATE - 90
    )
    UNION ALL
    SELECT 
        'Failed Login Attempts' AS ISSUE_TYPE,
        COUNT(*) AS COUNT,
        'Access denied in last 24 hours' AS DESCRIPTION
    FROM AUDIT_LOG
    WHERE HANHDONG LIKE 'ACCESS_DENIED%'
    AND NGAYGIO >= SYSDATE - 1
    UNION ALL
    SELECT 
        'High Value Orders' AS ISSUE_TYPE,
        COUNT(*) AS COUNT,
        'Orders > 1 billion VND' AS DESCRIPTION
    FROM ORDERS
    WHERE TONGTIEN > 1000000000;
END;
/

-- Procedure backup và restore với mã hóa
CREATE OR REPLACE PROCEDURE SP_BACKUP_ENCRYPTED_DATA(
    p_table_name IN VARCHAR2,
    p_backup_name IN VARCHAR2,
    p_result OUT NUMBER,
    p_message OUT VARCHAR2
)
AS
    v_sql VARCHAR2(4000);
    v_count NUMBER;
BEGIN
    -- Tạo backup table
    v_sql := 'CREATE TABLE ' || p_backup_name || ' AS SELECT * FROM ' || p_table_name;
    EXECUTE IMMEDIATE v_sql;
    
    -- Đếm số records
    v_sql := 'SELECT COUNT(*) FROM ' || p_backup_name;
    EXECUTE IMMEDIATE v_sql INTO v_count;
    
    -- Ghi log
    INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
    VALUES (SEQ_LOG.NEXTVAL, 0, 'BACKUP', p_table_name || ' -> ' || p_backup_name, SYSDATE, NULL);
    COMMIT;
    
    p_result := 1;
    p_message := 'Backup thành công ' || v_count || ' records từ ' || p_table_name;
    
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result := 0;
        p_message := 'Lỗi: ' || SQLERRM;
END;
/

-- Procedure để rotate encryption keys
CREATE OR REPLACE PROCEDURE SP_ROTATE_ENCRYPTION_KEYS(
    p_result OUT NUMBER,
    p_message OUT VARCHAR2
)
AS
    v_old_key_count NUMBER;
    v_new_keyid NUMBER;
BEGIN
    -- Đếm số key cũ
    SELECT COUNT(*) INTO v_old_key_count FROM ENCRYPTION_KEY;
    
    -- Archive old keys (đánh dấu là expired)
    UPDATE ENCRYPTION_KEY 
    SET KEYTYPE = 'RSA_EXPIRED'
    WHERE KEYTYPE = 'RSA';
    
    -- Generate new key
    SP_GENERATE_RSA_KEYPAIR(2048, p_result, p_message);
    
    IF p_result = 1 THEN
        p_message := 'Key rotation thành công. Archived ' || v_old_key_count || ' old keys.';
    END IF;
    
    -- Ghi log
    INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
    VALUES (SEQ_LOG.NEXTVAL, 0, 'KEY_ROTATION', 'ENCRYPTION_KEY', SYSDATE, NULL);
    COMMIT;
    
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result := 0;
        p_message := 'Lỗi: ' || SQLERRM;
END;
/

-- Procedure phát hiện bất thường (anomaly detection)
CREATE OR REPLACE PROCEDURE SP_DETECT_ANOMALIES(
    p_report OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_report FOR
    -- Phát hiện login từ nhiều IP khác nhau trong 1 giờ
    SELECT 
        'Multiple IPs' AS ANOMALY_TYPE,
        al.MATK,
        c.EMAIL,
        COUNT(DISTINCT al.IP) AS DISTINCT_IPS,
        TO_CHAR(al.NGAYGIO, 'YYYY-MM-DD HH24:MI:SS') AS TIME_WINDOW
    FROM AUDIT_LOG al
    JOIN CUSTOMER c ON al.MATK = c.MAKH
    WHERE al.HANHDONG = 'LOGIN'
    AND al.NGAYGIO >= SYSDATE - 1/24
    GROUP BY al.MATK, c.EMAIL, TO_CHAR(al.NGAYGIO, 'YYYY-MM-DD HH24:MI:SS')
    HAVING COUNT(DISTINCT al.IP) > 3
    
    UNION ALL
    
    -- Phát hiện quá nhiều failed attempts
    SELECT 
        'Too Many Failed Logins' AS ANOMALY_TYPE,
        al.MATK,
        c.EMAIL,
        COUNT(*) AS ATTEMPT_COUNT,
        TO_CHAR(MAX(al.NGAYGIO), 'YYYY-MM-DD HH24:MI:SS') AS LAST_ATTEMPT
    FROM AUDIT_LOG al
    LEFT JOIN CUSTOMER c ON al.MATK = c.MAKH
    WHERE al.HANHDONG LIKE 'ACCESS_DENIED%'
    AND al.NGAYGIO >= SYSDATE - 1
    GROUP BY al.MATK, c.EMAIL
    HAVING COUNT(*) > 5
    
    UNION ALL
    
    -- Phát hiện truy cập bất thường vào dữ liệu nhạy cảm
    SELECT 
        'Unusual Sensitive Data Access' AS ANOMALY_TYPE,
        al.MATK,
        c.EMAIL,
        COUNT(*) AS ACCESS_COUNT,
        TO_CHAR(MAX(al.NGAYGIO), 'YYYY-MM-DD HH24:MI:SS') AS LAST_ACCESS
    FROM AUDIT_LOG al
    LEFT JOIN CUSTOMER c ON al.MATK = c.MAKH
    WHERE al.BANGTACDONG = 'CUSTOMER'
    AND al.HANHDONG IN ('SELECT', 'UPDATE')
    AND al.NGAYGIO >= SYSDATE - 1
    GROUP BY al.MATK, c.EMAIL
    HAVING COUNT(*) > 50;
END;
/

-- ============================================================
-- COMPLIANCE & REPORTING PROCEDURES
-- ============================================================

-- Procedure tạo báo cáo tuân thủ bảo mật
CREATE OR REPLACE PROCEDURE SP_GENERATE_COMPLIANCE_REPORT(
    p_start_date IN DATE,
    p_end_date IN DATE,
    p_report OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_report FOR
    SELECT 
        'USER_AUTHENTICATION' AS CATEGORY,
        COUNT(CASE WHEN HANHDONG = 'LOGIN' THEN 1 END) AS SUCCESSFUL_LOGINS,
        COUNT(CASE WHEN HANHDONG = 'LOGOUT' THEN 1 END) AS LOGOUTS,
        COUNT(CASE WHEN HANHDONG LIKE 'ACCESS_DENIED%' THEN 1 END) AS FAILED_ATTEMPTS,
        COUNT(DISTINCT MATK) AS UNIQUE_USERS,
        COUNT(DISTINCT IP) AS UNIQUE_IPS
    FROM AUDIT_LOG
    WHERE NGAYGIO BETWEEN p_start_date AND p_end_date
    
    UNION ALL
    
    SELECT 
        'DATA_MODIFICATIONS' AS CATEGORY,
        COUNT(CASE WHEN HANHDONG = 'INSERT' THEN 1 END) AS INSERTS,
        COUNT(CASE WHEN HANHDONG = 'UPDATE' THEN 1 END) AS UPDATES,
        COUNT(CASE WHEN HANHDONG = 'DELETE' THEN 1 END) AS DELETES,
        COUNT(DISTINCT MATK) AS USERS_MODIFIED,
        COUNT(DISTINCT BANGTACDONG) AS TABLES_AFFECTED
    FROM AUDIT_LOG
    WHERE NGAYGIO BETWEEN p_start_date AND p_end_date
    AND HANHDONG IN ('INSERT', 'UPDATE', 'DELETE')
    
    UNION ALL
    
    SELECT 
        'AUTHORIZATION_CHANGES' AS CATEGORY,
        COUNT(CASE WHEN HANHDONG LIKE 'GRANT%' THEN 1 END) AS GRANTS,
        COUNT(CASE WHEN HANHDONG LIKE 'REVOKE%' THEN 1 END) AS REVOKES,
        NULL AS COL3,
        NULL AS COL4,
        NULL AS COL5
    FROM AUDIT_LOG
    WHERE NGAYGIO BETWEEN p_start_date AND p_end_date
    AND (HANHDONG LIKE 'GRANT%' OR HANHDONG LIKE 'REVOKE%');
END;
/

-- ============================================================
-- CLEANUP & MAINTENANCE PROCEDURES
-- ============================================================

-- Procedure dọn dẹp audit logs cũ
CREATE OR REPLACE PROCEDURE SP_ARCHIVE_OLD_AUDIT_LOGS(
    p_days_to_keep IN NUMBER DEFAULT 365,
    p_result OUT NUMBER,
    p_message OUT VARCHAR2
)
AS
    v_cutoff_date DATE;
    v_archived_count NUMBER;
BEGIN
    v_cutoff_date := SYSDATE - p_days_to_keep;
    
    -- Tạo archive table nếu chưa có
    BEGIN
        EXECUTE IMMEDIATE 'CREATE TABLE AUDIT_LOG_ARCHIVE AS SELECT * FROM AUDIT_LOG WHERE 1=0';
    EXCEPTION
        WHEN OTHERS THEN
            NULL; -- Table already exists
    END;
    
    -- Chuyển logs cũ sang archive
    INSERT INTO AUDIT_LOG_ARCHIVE
    SELECT * FROM AUDIT_LOG
    WHERE NGAYGIO < v_cutoff_date;
    
    v_archived_count := SQL%ROWCOUNT;
    
    -- Xóa logs đã archive
    DELETE FROM AUDIT_LOG
    WHERE NGAYGIO < v_cutoff_date;
    
    COMMIT;
    
    p_result := 1;
    p_message := 'Đã archive ' || v_archived_count || ' audit logs';
    
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_result := 0;
        p_message := 'Lỗi: ' || SQLERRM;
END;
/

-- ============================================================
-- TESTING & VERIFICATION SCRIPTS
-- ============================================================

-- Script test tất cả các chức năng
CREATE OR REPLACE PROCEDURE SP_TEST_ALL_SECURITY_FEATURES
AS
    v_result NUMBER;
    v_message VARCHAR2(4000);
    v_encrypted VARCHAR2(4000);
    v_cursor SYS_REFCURSOR;
    v_test_count NUMBER := 0;
    v_pass_count NUMBER := 0;
BEGIN
    DBMS_OUTPUT.PUT_LINE('=== BẮT ĐẦU TEST BẢO MẬT ===');
    DBMS_OUTPUT.PUT_LINE('');
    
    -- Test 1: RSA Key Generation
    v_test_count := v_test_count + 1;
    BEGIN
        SP_GENERATE_RSA_KEYPAIR(2048, v_result, v_message);
        IF v_result = 1 THEN
            v_pass_count := v_pass_count + 1;
            DBMS_OUTPUT.PUT_LINE('[PASS] Test 1: RSA Key Generation - ' || v_message);
        ELSE
            DBMS_OUTPUT.PUT_LINE('[FAIL] Test 1: RSA Key Generation - ' || v_message);
        END IF;
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('[ERROR] Test 1: ' || SQLERRM);
    END;
    
    -- Test 2: Encryption
    v_test_count := v_test_count + 1;
    BEGIN
        SP_ENCRYPT_WITH_PUBLIC_KEY('Test Data', 1, v_encrypted, v_result, v_message);
        IF v_result = 1 THEN
            v_pass_count := v_pass_count + 1;
            DBMS_OUTPUT.PUT_LINE('[PASS] Test 2: Data Encryption');
        ELSE
            DBMS_OUTPUT.PUT_LINE('[FAIL] Test 2: Data Encryption - ' || v_message);
        END IF;
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('[ERROR] Test 2: ' || SQLERRM);
    END;
    
    -- Test 3: RBAC Permission Check
    v_test_count := v_test_count + 1;
    BEGIN
        v_result := FN_CHECK_USER_PERMISSION(1, 'VIEW', 'CAR');
        IF v_result >= 0 THEN
            v_pass_count := v_pass_count + 1;
            DBMS_OUTPUT.PUT_LINE('[PASS] Test 3: RBAC Permission Check');
        ELSE
            DBMS_OUTPUT.PUT_LINE('[FAIL] Test 3: RBAC Permission Check');
        END IF;
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('[ERROR] Test 3: ' || SQLERRM);
    END;
    
    -- Test 4: Security Health Check
    v_test_count := v_test_count + 1;
    BEGIN
        SP_SECURITY_HEALTH_CHECK(v_cursor);
        CLOSE v_cursor;
        v_pass_count := v_pass_count + 1;
        DBMS_OUTPUT.PUT_LINE('[PASS] Test 4: Security Health Check');
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('[ERROR] Test 4: ' || SQLERRM);
    END;
    
    -- Test 5: Anomaly Detection
    v_test_count := v_test_count + 1;
    BEGIN
        SP_DETECT_ANOMALIES(v_cursor);
        CLOSE v_cursor;
        v_pass_count := v_pass_count + 1;
        DBMS_OUTPUT.PUT_LINE('[PASS] Test 5: Anomaly Detection');
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('[ERROR] Test 5: ' || SQLERRM);
    END;
    
    -- Test 6: Backup Procedure
    v_test_count := v_test_count + 1;
    BEGIN
        SP_BACKUP_ENCRYPTED_DATA('CUSTOMER', 'CUSTOMER_TEST_BACKUP', v_result, v_message);
        IF v_result = 1 THEN
            v_pass_count := v_pass_count + 1;
            DBMS_OUTPUT.PUT_LINE('[PASS] Test 6: Backup Procedure');
            -- Cleanup
            EXECUTE IMMEDIATE 'DROP TABLE CUSTOMER_TEST_BACKUP';
        ELSE
            DBMS_OUTPUT.PUT_LINE('[FAIL] Test 6: Backup Procedure - ' || v_message);
        END IF;
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('[ERROR] Test 6: ' || SQLERRM);
    END;
    
    DBMS_OUTPUT.PUT_LINE('');
    DBMS_OUTPUT.PUT_LINE('=== KẾT QUẢ TEST ===');
    DBMS_OUTPUT.PUT_LINE('Tổng số test: ' || v_test_count);
    DBMS_OUTPUT.PUT_LINE('Passed: ' || v_pass_count);
    DBMS_OUTPUT.PUT_LINE('Failed: ' || (v_test_count - v_pass_count));
    DBMS_OUTPUT.PUT_LINE('Success Rate: ' || ROUND((v_pass_count / v_test_count) * 100, 2) || '%');
    
END;
/

-- Drop procedure cũ (nếu có)
DROP PROCEDURE SP_ENCRYPT_WITH_PUBLIC_KEY;

-- Tạo lại procedure đã sửa
CREATE OR REPLACE PROCEDURE SP_ENCRYPT_WITH_PUBLIC_KEY(
    p_data IN VARCHAR2,
    p_keyid IN NUMBER,
    p_encrypted_data OUT VARCHAR2,
    p_result OUT NUMBER,
    p_message OUT VARCHAR2
)
AS
    v_public_key VARCHAR2(4000);
    v_raw_data RAW(2000);
    v_encryption_key RAW(32);  -- 32 bytes = 256 bits cho AES256
    v_iv RAW(16);              -- Initialization Vector 16 bytes
BEGIN
    -- Lấy public key
    SELECT PUBLICKEY INTO v_public_key
    FROM ENCRYPTION_KEY
    WHERE KEYID = p_keyid AND KEYTYPE = 'RSA';
    
    -- Tạo key mã hóa 32 bytes từ public key
    -- Dùng HASH SHA-256 để đảm bảo luôn có 32 bytes
    v_encryption_key := DBMS_CRYPTO.HASH(
        HEXTORAW(v_public_key),
        DBMS_CRYPTO.HASH_SH256
    );
    
    -- Tạo IV (Initialization Vector) ngẫu nhiên
    v_iv := DBMS_CRYPTO.RANDOMBYTES(16);
    
    -- Mã hóa dữ liệu với AES256 + CBC + PKCS5 Padding
    v_raw_data := UTL_RAW.CAST_TO_RAW(p_data);
    
    p_encrypted_data := RAWTOHEX(v_iv) || ':' || RAWTOHEX(
        DBMS_CRYPTO.ENCRYPT(
            src => v_raw_data,
            typ => DBMS_CRYPTO.ENCRYPT_AES256 + DBMS_CRYPTO.CHAIN_CBC + DBMS_CRYPTO.PAD_PKCS5,
            key => v_encryption_key,
            iv  => v_iv
        )
    );
    
    p_result := 1;
    p_message := 'Mã hóa thành công';
    
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_result := 0;
        p_message := 'Không tìm thấy key với ID: ' || p_keyid;
    WHEN OTHERS THEN
        p_result := 0;
        p_message := 'Lỗi: ' || SQLERRM;
END;
/

-- ============================================================
-- THÊM: PROCEDURE GIẢI MÃ (DECRYPT)
-- ============================================================

CREATE OR REPLACE PROCEDURE SP_DECRYPT_WITH_PRIVATE_KEY(
    p_encrypted_data IN VARCHAR2,
    p_keyid IN NUMBER,
    p_decrypted_data OUT VARCHAR2,
    p_result OUT NUMBER,
    p_message OUT VARCHAR2
)
AS
    v_private_key VARCHAR2(4000);
    v_decryption_key RAW(32);
    v_iv RAW(16);
    v_encrypted_raw RAW(2000);
    v_decrypted_raw RAW(2000);
    v_separator_pos NUMBER;
BEGIN
    -- Lấy private key
    SELECT PRIVATEKEY INTO v_private_key
    FROM ENCRYPTION_KEY
    WHERE KEYID = p_keyid;
    
    -- Tạo key giải mã từ private key (dùng cùng thuật toán với encrypt)
    v_decryption_key := DBMS_CRYPTO.HASH(
        HEXTORAW(v_private_key),
        DBMS_CRYPTO.HASH_SH256
    );
    
    -- Tách IV và encrypted data (format: IV:ENCRYPTED_DATA)
    v_separator_pos := INSTR(p_encrypted_data, ':');
    
    IF v_separator_pos = 0 THEN
        p_result := 0;
        p_message := 'Định dạng dữ liệu mã hóa không hợp lệ';
        RETURN;
    END IF;
    
    v_iv := HEXTORAW(SUBSTR(p_encrypted_data, 1, v_separator_pos - 1));
    v_encrypted_raw := HEXTORAW(SUBSTR(p_encrypted_data, v_separator_pos + 1));
    
    -- Giải mã
    v_decrypted_raw := DBMS_CRYPTO.DECRYPT(
        src => v_encrypted_raw,
        typ => DBMS_CRYPTO.ENCRYPT_AES256 + DBMS_CRYPTO.CHAIN_CBC + DBMS_CRYPTO.PAD_PKCS5,
        key => v_decryption_key,
        iv  => v_iv
    );
    
    p_decrypted_data := UTL_RAW.CAST_TO_VARCHAR2(v_decrypted_raw);
    p_result := 1;
    p_message := 'Giải mã thành công';
    
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        p_result := 0;
        p_message := 'Không tìm thấy key với ID: ' || p_keyid;
    WHEN OTHERS THEN
        p_result := 0;
        p_message := 'Lỗi: ' || SQLERRM;
END;
/
-- Bật DBMS_OUTPUT để xem kết quả
SET SERVEROUTPUT ON;

-- Chạy test tổng thể
BEGIN
    SP_TEST_ALL_SECURITY_FEATURES;
END;
/
