using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using WebCar.Models;
using WebCar.Models.ViewModels;

namespace WebCar.Data
{
    public class CustomerRepository
    {
        private readonly string connStr;

        public CustomerRepository()
        {
            connStr = System.Configuration.ConfigurationManager
                     .ConnectionStrings["Model1"].ConnectionString;
        }

        // ====================== ĐĂNG KÝ ======================
        public dynamic Register(RegisterViewModel model)
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();

                using (var cmd = new OracleCommand("SP_REGISTER_CUSTOMER", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // INPUT PARAMS
                    cmd.Parameters.Add("p_hoten", OracleDbType.Varchar2).Value = model.HOTEN;
                    cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = model.EMAIL;
                    cmd.Parameters.Add("p_sdt", OracleDbType.Varchar2).Value = model.SDT;
                    cmd.Parameters.Add("p_matkhau", OracleDbType.Varchar2).Value = model.MATKHAU;
                    cmd.Parameters.Add("p_diachi", OracleDbType.Varchar2).Value = model.DIACHI;

                    // OUTPUT PARAMS
                    cmd.Parameters.Add("p_result", OracleDbType.Int32).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("p_message", OracleDbType.Varchar2, 200).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("p_makh", OracleDbType.Int32).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();

                    // READ OUT PARAMETERS
                    int result = ((OracleDecimal)cmd.Parameters["p_result"].Value).ToInt32();
                    string message = cmd.Parameters["p_message"].Value.ToString();

                    int makh = 0;
                    if (cmd.Parameters["p_makh"].Value != DBNull.Value)
                        makh = ((OracleDecimal)cmd.Parameters["p_makh"].Value).ToInt32();

                    return new
                    {
                        Success = result == 1,
                        Message = message,
                        CustomerId = makh
                    };
                }
            }
        }

        // ====================== ĐĂNG NHẬP ======================
        public dynamic Login(LoginViewModel model)
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();

                using (var cmd = new OracleCommand("SP_LOGIN_CUSTOMER", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // INPUT PARAMS
                    cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = model.EMAIL;
                    cmd.Parameters.Add("p_matkhau", OracleDbType.Varchar2).Value = model.MATKHAU;
                    cmd.Parameters.Add("p_ip", OracleDbType.Varchar2).Value = "127.0.0.1";

                    // OUTPUT PARAMS
                    cmd.Parameters.Add("p_result", OracleDbType.Int32).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("p_message", OracleDbType.Varchar2, 200).Direction = ParameterDirection.Output;

                    cmd.Parameters.Add("p_makh", OracleDbType.Int32).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("p_hoten", OracleDbType.Varchar2, 100).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("p_rolename", OracleDbType.Varchar2, 30).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();

                    int result = ((OracleDecimal)cmd.Parameters["p_result"].Value).ToInt32();
                    string message = cmd.Parameters["p_message"].Value.ToString();

                    if (result == 0)
                    {
                        return new
                        {
                            Success = false,
                            Message = message
                        };
                    }

                    int makh = ((OracleDecimal)cmd.Parameters["p_makh"].Value).ToInt32();
                    string hoten = cmd.Parameters["p_hoten"].Value.ToString();
                    string role = cmd.Parameters["p_rolename"].Value.ToString();

                    return new
                    {
                        Success = true,
                        Message = message,
                        Customer = new
                        {
                            MaKH = makh,
                            HoTen = hoten,
                            Email = model.EMAIL,
                            RoleName = role
                        }
                    };
                }
            }
        }

        // ====================== ĐĂNG XUẤT ======================
        public void Logout(int makh)
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();

                using (var cmd = new OracleCommand("SP_LOGOUT_CUSTOMER", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_makh", OracleDbType.Int32).Value = makh;
                    cmd.Parameters.Add("p_ip", OracleDbType.Varchar2).Value = "127.0.0.1";
                    cmd.Parameters.Add("p_result", OracleDbType.Int32).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("p_message", OracleDbType.Varchar2, 200).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ====================== LẤY THÔNG TIN CUSTOMER THEO ID ======================
        public CUSTOMER GetCustomerById(int customerId)
        {
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();

                using (var cmd = new OracleCommand(@"
                    SELECT MAKH, HOTEN, EMAIL, SDT, DIACHI, NGAYDANGKY
                    FROM CUSTOMER
                    WHERE MAKH = :customerId", conn))
                {
                    cmd.Parameters.Add("customerId", OracleDbType.Int32).Value = customerId;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new CUSTOMER
                            {
                                MAKH = Convert.ToInt32(reader["MAKH"]),
                                HOTEN = reader["HOTEN"]?.ToString(),
                                EMAIL = reader["EMAIL"]?.ToString(),
                                SDT = reader["SDT"]?.ToString(),
                                DIACHI = reader["DIACHI"]?.ToString(),
                                NGAYDANGKY = reader["NGAYDANGKY"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["NGAYDANGKY"])
                                    : (DateTime?)null
                            };
                        }
                    }
                }
            }

            return null;
        }

        // ====================== CẬP NHẬT THÔNG TIN CUSTOMER ======================
        public dynamic UpdateCustomer(CUSTOMER customer)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    using (var cmd = new OracleCommand(@"
                        UPDATE CUSTOMER 
                        SET HOTEN = :hoten,
                            SDT = :sdt,
                            DIACHI = :diachi
                        WHERE MAKH = :makh", conn))
                    {
                        cmd.Parameters.Add("hoten", OracleDbType.Varchar2).Value = customer.HOTEN;
                        cmd.Parameters.Add("sdt", OracleDbType.Varchar2).Value = customer.SDT;
                        cmd.Parameters.Add("diachi", OracleDbType.Varchar2).Value = customer.DIACHI ?? (object)DBNull.Value;
                        cmd.Parameters.Add("makh", OracleDbType.Int32).Value = customer.MAKH;

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return new
                            {
                                Success = true,
                                Message = "Cập nhật thông tin thành công!"
                            };
                        }

                        return new
                        {
                            Success = false,
                            Message = "Không tìm thấy tài khoản để cập nhật!"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new
                {
                    Success = false,
                    Message = "Lỗi: " + ex.Message
                };
            }
        }

        // ====================== ĐỔI MẬT KHẨU ======================
        public dynamic ChangePassword(int customerId, string oldPassword, string newPassword)
        {
            try
            {
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();

                    // Hash mật khẩu cũ để kiểm tra
                    string oldPasswordHash = HashPassword(oldPassword);

                    // Kiểm tra mật khẩu cũ có đúng không
                    using (var checkCmd = new OracleCommand(@"
                        SELECT COUNT(*) FROM CUSTOMER 
                        WHERE MAKH = :makh AND MATKHAU = :oldPassword", conn))
                    {
                        checkCmd.Parameters.Add("makh", OracleDbType.Int32).Value = customerId;
                        checkCmd.Parameters.Add("oldPassword", OracleDbType.Varchar2).Value = oldPasswordHash;

                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count == 0)
                        {
                            return new
                            {
                                Success = false,
                                Message = "Mật khẩu cũ không đúng!"
                            };
                        }
                    }

                    // Hash mật khẩu mới
                    string newPasswordHash = HashPassword(newPassword);

                    // Cập nhật mật khẩu mới
                    using (var updateCmd = new OracleCommand(@"
                        UPDATE CUSTOMER 
                        SET MATKHAU = :newPassword
                        WHERE MAKH = :makh", conn))
                    {
                        updateCmd.Parameters.Add("newPassword", OracleDbType.Varchar2).Value = newPasswordHash;
                        updateCmd.Parameters.Add("makh", OracleDbType.Int32).Value = customerId;

                        int rowsAffected = updateCmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Ghi audit log
                            using (var logCmd = new OracleCommand(@"
                                INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
                                VALUES (SEQ_LOG.NEXTVAL, :matk, 'CHANGE_PASSWORD', 'CUSTOMER', SYSDATE, '127.0.0.1')", conn))
                            {
                                logCmd.Parameters.Add("matk", OracleDbType.Int32).Value = customerId;
                                logCmd.ExecuteNonQuery();
                            }

                            return new
                            {
                                Success = true,
                                Message = "Đổi mật khẩu thành công!"
                            };
                        }

                        return new
                        {
                            Success = false,
                            Message = "Không thể đổi mật khẩu!"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new
                {
                    Success = false,
                    Message = "Lỗi: " + ex.Message
                };
            }
        }

        // ====================== HASH PASSWORD (SHA256) ======================
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
    }
}