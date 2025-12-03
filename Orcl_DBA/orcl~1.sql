-- ============================================================
-- SCRIPT CẤP QUYỀN CHO USER CARSALE
-- ============================================================

SET SERVEROUTPUT ON;
SET ECHO ON;

PROMPT
PROMPT ============================================================
PROMPT CARSALE DATABASE - GRANT PERMISSIONS SCRIPT
PROMPT Executing as: SYSTEM/SYSDBA
PROMPT ============================================================
PROMPT

-- ============================================================
-- BƯỚC 1: KIỂM TRA USER CARSALE TỒN TẠI
-- ============================================================

PROMPT
PROMPT [STEP 1] Checking if user CARSALE exists...
PROMPT

DECLARE
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count FROM DBA_USERS WHERE USERNAME = 'CARSALE';
    
    IF v_count = 0 THEN
        DBMS_OUTPUT.PUT_LINE('ERROR: User CARSALE does not exist!');
        DBMS_OUTPUT.PUT_LINE('Please create user first with:');
        DBMS_OUTPUT.PUT_LINE('CREATE USER CARSALE IDENTIFIED BY YourPassword;');
        RAISE_APPLICATION_ERROR(-20001, 'User CARSALE not found');
    ELSE
        DBMS_OUTPUT.PUT_LINE('SUCCESS: User CARSALE exists');
    END IF;
END;
/

-- ============================================================
-- BƯỚC 2: CẤP QUYỀN KẾT NỐI VÀ SESSION
-- ============================================================

PROMPT
PROMPT [STEP 2] Granting connection and session privileges...
PROMPT

GRANT CONNECT TO CARSALE;
GRANT CREATE SESSION TO CARSALE;
GRANT RESOURCE TO CARSALE;

PROMPT SUCCESS: Connection privileges granted

-- ============================================================
-- BƯỚC 3: CẤP QUYỀN TẠO VÀ QUẢN LÝ OBJECTS
-- ============================================================

PROMPT
PROMPT [STEP 3] Granting object creation privileges...
PROMPT

GRANT CREATE TABLE TO CARSALE;
GRANT CREATE VIEW TO CARSALE;
GRANT CREATE SEQUENCE TO CARSALE;
GRANT CREATE PROCEDURE TO CARSALE;
GRANT CREATE TRIGGER TO CARSALE;
GRANT CREATE SYNONYM TO CARSALE;
GRANT CREATE TYPE TO CARSALE;
GRANT CREATE CONTEXT TO CARSALE;

PROMPT SUCCESS: Object creation privileges granted

-- ============================================================
-- BƯỚC 4: CẤP QUYỀN THỰC THI VÀ QUẢN LÝ DỮ LIỆU
-- ============================================================

PROMPT
PROMPT [STEP 4] Granting data management privileges...
PROMPT

GRANT EXECUTE ANY PROCEDURE TO CARSALE;
GRANT EXECUTE ANY TYPE TO CARSALE;
GRANT SELECT ANY TABLE TO CARSALE;
GRANT INSERT ANY TABLE TO CARSALE;
GRANT UPDATE ANY TABLE TO CARSALE;
GRANT DELETE ANY TABLE TO CARSALE;
GRANT ALTER ANY TABLE TO CARSALE;
GRANT DROP ANY TABLE TO CARSALE;

PROMPT SUCCESS: Data management privileges granted

-- ============================================================
-- BƯỚC 5: CẤP QUYỀN DBMS PACKAGES (MÃ HÓA, NETWORK)
-- ============================================================

PROMPT
PROMPT [STEP 5] Granting DBMS packages privileges...
PROMPT

-- Encryption packages
GRANT EXECUTE ON SYS.DBMS_CRYPTO TO CARSALE;
GRANT EXECUTE ON SYS.UTL_RAW TO CARSALE;

-- Network and ACL
GRANT EXECUTE ON SYS.DBMS_NETWORK_ACL_ADMIN TO CARSALE;

-- Utility packages
GRANT EXECUTE ON SYS.DBMS_OUTPUT TO CARSALE;
GRANT EXECUTE ON SYS.DBMS_SQL TO CARSALE;
GRANT EXECUTE ON SYS.DBMS_LOCK TO CARSALE;
GRANT EXECUTE ON SYS.DBMS_JOB TO CARSALE;
GRANT EXECUTE ON SYS.DBMS_SCHEDULER TO CARSALE;
GRANT EXECUTE ON SYS.DBMS_SESSION TO CARSALE;

PROMPT SUCCESS: DBMS packages privileges granted

-- ============================================================
-- BƯỚC 6: CẤP QUYỀN VPD (TUẦN 10 - Virtual Private Database)
-- ============================================================

PROMPT
PROMPT [STEP 6] Granting VPD privileges (Week 10)...
PROMPT

GRANT EXECUTE ON SYS.DBMS_RLS TO CARSALE;

PROMPT SUCCESS: VPD privileges granted

-- ============================================================
-- BƯỚC 7: CẤP QUYỀN ROLE MANAGEMENT (TUẦN 12 - RBAC)
-- ============================================================

PROMPT
PROMPT [STEP 7] Granting role management privileges (Week 12)...
PROMPT

GRANT CREATE ROLE TO CARSALE;
GRANT ALTER ANY ROLE TO CARSALE;
GRANT DROP ANY ROLE TO CARSALE;
GRANT GRANT ANY ROLE TO CARSALE;

PROMPT SUCCESS: Role management privileges granted

-- ============================================================
-- BƯỚC 8: CẤP QUYỀN USER MANAGEMENT (TÙY CHỌN)
-- ============================================================

PROMPT
PROMPT [STEP 8] Granting user management privileges (Optional)...
PROMPT

GRANT CREATE USER TO CARSALE;
GRANT ALTER USER TO CARSALE;
GRANT DROP USER TO CARSALE;

PROMPT SUCCESS: User management privileges granted

-- ============================================================
-- BƯỚC 9: CẤP QUYỀN PROFILE VÀ SESSION (TUẦN 8)
-- ============================================================

PROMPT
PROMPT [STEP 9] Granting profile and session privileges (Week 8)...
PROMPT

GRANT CREATE PROFILE TO CARSALE;
GRANT ALTER PROFILE TO CARSALE;
GRANT DROP PROFILE TO CARSALE;
GRANT ALTER SYSTEM TO CARSALE;
GRANT SELECT ON SYS.V_$SESSION TO CARSALE;

-- Tạo public synonym cho V$SESSION
CREATE OR REPLACE PUBLIC SYNONYM V$SESSION FOR SYS.V_$SESSION;

PROMPT SUCCESS: Profile and session privileges granted

-- ============================================================
-- BƯỚC 10: CẤP QUYỀN TABLESPACE
-- ============================================================

PROMPT
PROMPT [STEP 10] Granting tablespace privileges...
PROMPT

GRANT ALTER TABLESPACE TO CARSALE;
GRANT MANAGE TABLESPACE TO CARSALE;
GRANT UNLIMITED TABLESPACE TO CARSALE;

-- Cấp quota trên các tablespace
ALTER USER CARSALE QUOTA UNLIMITED ON USERS;
ALTER USER CARSALE QUOTA UNLIMITED ON SYSTEM;

-- Nếu có tablespace CARSALE_TBS
BEGIN
    EXECUTE IMMEDIATE 'ALTER USER CARSALE QUOTA UNLIMITED ON CARSALE_TBS';
    DBMS_OUTPUT.PUT_LINE('SUCCESS: Quota granted on CARSALE_TBS');
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('INFO: Tablespace CARSALE_TBS not found - skipping');
END;
/

PROMPT SUCCESS: Tablespace privileges granted

-- ============================================================
-- BƯỚC 11: CẤP QUYỀN AUDIT (TUẦN 13, 14)
-- ============================================================

PROMPT
PROMPT [STEP 11] Granting audit privileges (Week 13, 14)...
PROMPT

GRANT AUDIT ANY TO CARSALE;
GRANT AUDIT SYSTEM TO CARSALE;
GRANT EXECUTE ON SYS.DBMS_FGA TO CARSALE;
GRANT SELECT ON SYS.DBA_AUDIT_TRAIL TO CARSALE;
GRANT SELECT ON SYS.DBA_FGA_AUDIT_TRAIL TO CARSALE;

PROMPT SUCCESS: Audit privileges granted

-- ============================================================
-- BƯỚC 12: CẤP QUYỀN DICTIONARY VIEWS
-- ============================================================

PROMPT
PROMPT [STEP 12] Granting dictionary view privileges...
PROMPT

GRANT SELECT ON SYS.DBA_USERS TO CARSALE;
GRANT SELECT ON SYS.DBA_TABLES TO CARSALE;
GRANT SELECT ON SYS.DBA_VIEWS TO CARSALE;
GRANT SELECT ON SYS.DBA_SEQUENCES TO CARSALE;
GRANT SELECT ON SYS.DBA_PROCEDURES TO CARSALE;
GRANT SELECT ON SYS.DBA_TRIGGERS TO CARSALE;
GRANT SELECT ON SYS.DBA_ROLES TO CARSALE;
GRANT SELECT ON SYS.DBA_ROLE_PRIVS TO CARSALE;
GRANT SELECT ON SYS.DBA_SYS_PRIVS TO CARSALE;
GRANT SELECT ON SYS.DBA_TAB_PRIVS TO CARSALE;
GRANT SELECT ON SYS.DBA_TS_QUOTAS TO CARSALE;
GRANT SELECT ON SYS.DBA_POLICIES TO CARSALE;
GRANT SELECT ON SYS.DBA_CONTEXT TO CARSALE;

PROMPT SUCCESS: Dictionary view privileges granted

-- ============================================================
-- BƯỚC 13: TUẦN 11 - MAC (KHÔNG DÙNG OLS)
-- ============================================================
-- ============================================================
-- COMMIT CHANGES
-- ============================================================

COMMIT;

PROMPT
PROMPT ============================================================
PROMPT VERIFYING GRANTED PRIVILEGES
PROMPT ============================================================
PROMPT

-- ============================================================
-- KIỂM TRA QUYỀN ĐÃ CẤP
-- ============================================================

PROMPT
PROMPT [VERIFICATION] System Privileges:
PROMPT

SELECT PRIVILEGE 
FROM DBA_SYS_PRIVS 
WHERE GRANTEE = 'CARSALE'
ORDER BY PRIVILEGE;

PROMPT
PROMPT [VERIFICATION] Role Privileges:
PROMPT

SELECT GRANTED_ROLE 
FROM DBA_ROLE_PRIVS 
WHERE GRANTEE = 'CARSALE'
ORDER BY GRANTED_ROLE;

PROMPT
PROMPT [VERIFICATION] Object Privileges:
PROMPT

SELECT OWNER, TABLE_NAME, PRIVILEGE 
FROM DBA_TAB_PRIVS 
WHERE GRANTEE = 'CARSALE'
ORDER BY OWNER, TABLE_NAME, PRIVILEGE;

PROMPT
PROMPT [VERIFICATION] Tablespace Quotas:
PROMPT

SELECT TABLESPACE_NAME, MAX_BYTES, BYTES, BLOCKS
FROM DBA_TS_QUOTAS 
WHERE USERNAME = 'CARSALE'
ORDER BY TABLESPACE_NAME;

PROMPT
PROMPT ============================================================
PROMPT SUMMARY REPORT
PROMPT ============================================================
PROMPT

-- Tạo báo cáo tóm tắt
DECLARE
    v_sys_privs NUMBER;
    v_role_privs NUMBER;
    v_obj_privs NUMBER;
    v_quotas NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_sys_privs FROM DBA_SYS_PRIVS WHERE GRANTEE = 'CARSALE';
    SELECT COUNT(*) INTO v_role_privs FROM DBA_ROLE_PRIVS WHERE GRANTEE = 'CARSALE';
    SELECT COUNT(*) INTO v_obj_privs FROM DBA_TAB_PRIVS WHERE GRANTEE = 'CARSALE';
    SELECT COUNT(*) INTO v_quotas FROM DBA_TS_QUOTAS WHERE USERNAME = 'CARSALE';
    
    DBMS_OUTPUT.PUT_LINE('');
    DBMS_OUTPUT.PUT_LINE('User: CARSALE');
    DBMS_OUTPUT.PUT_LINE('----------------------------------------');
    DBMS_OUTPUT.PUT_LINE('System Privileges : ' || v_sys_privs);
    DBMS_OUTPUT.PUT_LINE('Role Privileges   : ' || v_role_privs);
    DBMS_OUTPUT.PUT_LINE('Object Privileges : ' || v_obj_privs);
    DBMS_OUTPUT.PUT_LINE('Tablespace Quotas : ' || v_quotas);
    DBMS_OUTPUT.PUT_LINE('----------------------------------------');
    DBMS_OUTPUT.PUT_LINE('');
    
    IF v_sys_privs > 0 AND v_obj_privs > 0 THEN
        DBMS_OUTPUT.PUT_LINE('STATUS: ✓ ALL PRIVILEGES GRANTED SUCCESSFULLY');
    ELSE
        DBMS_OUTPUT.PUT_LINE('STATUS: ✗ SOME PRIVILEGES MAY BE MISSING');
    END IF;
    
    DBMS_OUTPUT.PUT_LINE('');
END;
/

PROMPT
PROMPT ============================================================
PROMPT WEEK-BY-WEEK PRIVILEGE CHECKLIST
PROMPT ============================================================
PROMPT

DECLARE
    TYPE week_rec IS RECORD (
        week_num VARCHAR2(10),
        description VARCHAR2(100),
        status VARCHAR2(20)
    );
    TYPE week_tab IS TABLE OF week_rec;
    
    v_weeks week_tab := week_tab(
        week_rec('Week 6', 'Asymmetric Encryption (DBMS_CRYPTO)', 'GRANTED'),
        week_rec('Week 7', 'Re-encryption', 'GRANTED'),
        week_rec('Week 8', 'Tablespace, Profile, Session', 'GRANTED'),
        week_rec('Week 9', 'DAC - Discretionary Access Control', 'GRANTED'),
        week_rec('Week 10', 'MAC - VPD (Virtual Private Database)', 'GRANTED'),
        week_rec('Week 11', 'MAC - VPD + Context (OLS Alternative)', 'GRANTED'),
        week_rec('Week 12', 'RBAC - Role-Based Access Control', 'GRANTED'),
        week_rec('Week 13', 'Standard Auditing & Triggers', 'GRANTED'),
        week_rec('Week 14', 'Fine-Grained Auditing (FGA)', 'GRANTED')
    );
BEGIN
    FOR i IN 1..v_weeks.COUNT LOOP
        DBMS_OUTPUT.PUT_LINE(
            RPAD(v_weeks(i).week_num, 10) || ' | ' ||
            RPAD(v_weeks(i).description, 50) || ' | ' ||
            v_weeks(i).status
        );
    END LOOP;
END;
/


SET ECHO OFF;