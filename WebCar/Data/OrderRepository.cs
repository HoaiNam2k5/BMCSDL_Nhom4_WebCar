using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using WebCar.Models;
using WebCar.Models.ViewModels;
using WebCar.Helpers; // ✅ Add RSA Helper

namespace WebCar.Data
{
    public class OrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["Model1"].ConnectionString;
        }

        // ====================== TẠO ĐƠN HÀNG (WITH RSA ENCRYPTION) ======================
        public dynamic CreateOrder(int maKH, int maXe, int soLuong, string diaChiGiaoHang = "", string soDienThoai = "", string ghiChu = "")
        {
            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    // ✅ Encrypt
                    string encryptedAddress = string.IsNullOrEmpty(diaChiGiaoHang)
                        ? ""
                        : RSAEncryptionHelper.Encrypt(diaChiGiaoHang);

                    string encryptedPhone = string.IsNullOrEmpty(soDienThoai)
                        ? ""
                        : RSAEncryptionHelper.Encrypt(soDienThoai);

                    System.Diagnostics.Debug.WriteLine("========================================");
                    System.Diagnostics.Debug.WriteLine("CALLING SP_CREATE_ORDER_V2");
                    System.Diagnostics.Debug.WriteLine($"Customer: {maKH}");
                    System.Diagnostics.Debug.WriteLine($"Car: {maXe}");
                    System.Diagnostics.Debug.WriteLine($"Quantity: {soLuong}");
                    System.Diagnostics.Debug.WriteLine($"Address (encrypted): {encryptedAddress?.Length ?? 0} chars");
                    System.Diagnostics.Debug.WriteLine($"Phone (encrypted): {encryptedPhone?.Length ?? 0} chars");
                    System.Diagnostics.Debug.WriteLine("========================================");

                    using (var cmd = new OracleCommand("SP_CREATE_ORDER_V2", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Input parameters
                        cmd.Parameters.Add("p_makh", OracleDbType.Int32).Value = maKH;
                        cmd.Parameters.Add("p_maxe", OracleDbType.Int32).Value = maXe;
                        cmd.Parameters.Add("p_soluong", OracleDbType.Int32).Value = soLuong;
                        cmd.Parameters.Add("p_diachi_enc", OracleDbType.Varchar2).Value = encryptedAddress ?? (object)DBNull.Value;
                        cmd.Parameters.Add("p_sdt_enc", OracleDbType.Varchar2).Value = encryptedPhone ?? (object)DBNull.Value;
                        cmd.Parameters.Add("p_ghichu", OracleDbType.Varchar2).Value = ghiChu ?? (object)DBNull.Value;

                        // Output parameters
                        var resultParam = cmd.Parameters.Add("p_result", OracleDbType.Int32);
                        resultParam.Direction = ParameterDirection.Output;

                        var messageParam = cmd.Parameters.Add("p_message", OracleDbType.Varchar2, 4000);
                        messageParam.Direction = ParameterDirection.Output;

                        var madonParam = cmd.Parameters.Add("p_madon", OracleDbType.Int32);
                        madonParam.Direction = ParameterDirection.Output;

                        // Execute
                        System.Diagnostics.Debug.WriteLine("→ Executing procedure...");
                        cmd.ExecuteNonQuery();

                        // Get results
                        int result = resultParam.Value != DBNull.Value
                            ? ((OracleDecimal)resultParam.Value).ToInt32()
                            : 0;

                        string message = messageParam.Value != DBNull.Value
                            ? messageParam.Value.ToString()
                            : "No message";

                        int maDon = madonParam.Value != DBNull.Value
                            ? ((OracleDecimal)madonParam.Value).ToInt32()
                            : 0;

                        System.Diagnostics.Debug.WriteLine($"← Result: {result}");
                        System.Diagnostics.Debug.WriteLine($"← Message: {message}");
                        System.Diagnostics.Debug.WriteLine($"← Order ID: {maDon}");
                        System.Diagnostics.Debug.WriteLine("========================================");

                        return new
                        {
                            Success = result == 1,
                            Message = message,
                            OrderId = maDon
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CreateOrder Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack:  {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine("========================================");

                return new
                {
                    Success = false,
                    Message = "Lỗi: " + ex.Message,
                    OrderId = 0
                };
            }
        }

        // ====================== LẤY ĐƠN HÀNG (WITH VPD) ======================
        public List<OrderViewModel> GetOrdersWithVPD(int maKH, string userRole)
        {
            var orders = new List<OrderViewModel>();

            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    //   SET VPD CONTEXT
                    SetVPDContext(conn, maKH, userRole);

                    // Query với VPD tự động filter
                    using (var cmd = new OracleCommand(@"
                        SELECT 
                            o.MADON,
                            o.MAKH,
                            o.NGAYDAT,
                            o. TONGTIEN,
                            o.TRANGTHAI,
                            od.MAXE,
                            c.TENXE,
                            c.HANGXE,
                            c.HINHANH,
                            od.SOLUONG,
                            od.DONGIA
                        FROM ORDERS o
                        JOIN ORDER_DETAIL od ON o.MADON = od.MADON
                        JOIN CAR c ON od.MAXE = c. MAXE
                        ORDER BY o.NGAYDAT DESC", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                orders.Add(new OrderViewModel
                                {
                                    MaDon = Convert.ToInt32(reader["MADON"]),
                                    MaKH = Convert.ToInt32(reader["MAKH"]),
                                    NgayDat = Convert.ToDateTime(reader["NGAYDAT"]),
                                    TongTien = Convert.ToDecimal(reader["TONGTIEN"]),
                                    TrangThai = reader["TRANGTHAI"].ToString(),
                                    MaXe = Convert.ToInt32(reader["MAXE"]),
                                    TenXe = reader["TENXE"].ToString(),
                                    HangXe = reader["HANGXE"].ToString(),
                                    HinhAnh = reader["HINHANH"].ToString(),
                                    SoLuong = Convert.ToInt32(reader["SOLUONG"]),
                                    DonGia = Convert.ToDecimal(reader["DONGIA"])
                                });
                            }
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ VPD Query: User={maKH}, Role={userRole}, Orders={orders.Count}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetOrdersWithVPD Error:  {ex.Message}");
            }

            return orders;
        }

        // ====================== LẤY ĐƠN HÀNG CỦA TÔI (WITH ROLE-BASED FILTERING) ======================
        public List<OrderViewModel> GetMyOrders(int customerId, string roleName)
        {
            var orders = new List<OrderViewModel>();

            try
            {
                System.Diagnostics.Debug.WriteLine($"GetMyOrders - Customer:  {customerId}, Role: {roleName}");

                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    //  BUILD QUERY BASED ON ROLE
                    string query = @"
                SELECT 
                    o.MADON,
                    o.MAKH,
                    o.NGAYDAT,
                    o. TONGTIEN,
                    o.TRANGTHAI,
                    od.MAXE,
                    od.SOLUONG,
                    od. DONGIA,
                    c.TENXE,
                    c. HANGXE,
                    c.HINHANH
                FROM ORDERS o
                JOIN ORDER_DETAIL od ON o. MADON = od.MADON
                JOIN CAR c ON od.MAXE = c.MAXE
                WHERE 1=1";

                    //  FILTER BY ROLE
                    if (roleName == "ADMIN")
                    {
                        // Admin sees all orders
                        query += " ORDER BY o.NGAYDAT DESC";
                        System.Diagnostics.Debug.WriteLine("Filter:  ADMIN - All orders");
                    }
                    else if (roleName == "MANAGER")
                    {
                        // Manager sees non-cancelled orders
                        query += " AND o.TRANGTHAI != 'Da huy' ORDER BY o.NGAYDAT DESC";
                        System.Diagnostics.Debug.WriteLine("Filter: MANAGER - Non-cancelled orders");
                    }
                    else
                    {
                        // Customer sees only own orders
                        query += " AND o.MAKH = :customerId ORDER BY o.NGAYDAT DESC";
                        System.Diagnostics.Debug.WriteLine($"Filter: CUSTOMER - Only user {customerId}");
                    }

                    using (var cmd = new OracleCommand(query, conn))
                    {
                        // Add parameter for customer filter
                        if (roleName != "ADMIN" && roleName != "MANAGER")
                        {
                            cmd.Parameters.Add("customerId", OracleDbType.Int32).Value = customerId;
                        }

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var order = new OrderViewModel
                                {
                                    MaDon = Convert.ToInt32(reader["MADON"]),
                                    MaKH = Convert.ToInt32(reader["MAKH"]),
                                    NgayDat = Convert.ToDateTime(reader["NGAYDAT"]),
                                    TongTien = Convert.ToDecimal(reader["TONGTIEN"]),
                                    TrangThai = reader["TRANGTHAI"]?.ToString(),
                                    MaXe = Convert.ToInt32(reader["MAXE"]),
                                    TenXe = reader["TENXE"]?.ToString(),
                                    HangXe = reader["HANGXE"]?.ToString(),
                                    HinhAnh = reader["HINHANH"]?.ToString(),
                                    SoLuong = Convert.ToInt32(reader["SOLUONG"]),
                                    DonGia = Convert.ToDecimal(reader["DONGIA"])
                                };

                                orders.Add(order);
                            }
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ Retrieved {orders.Count} orders");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetMyOrders Error:  {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                throw;
            }

            return orders;
        }

        // ====================== LẤY CHI TIẾT ĐƠN HÀNG (FOR PERMISSION CHECK) ======================
        public OrderDetailViewModel GetOrderDetails(int orderId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetOrderDetails - Order:  {orderId}");

                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    //  GET ORDER WITHOUT FILTERING (will check permission in controller)
                    using (var cmd = new OracleCommand(@"
                SELECT 
                    o. MADON,
                    o.MAKH,
                    cust.HOTEN,
                    cust.EMAIL,
                    cust.SDT,
                    cust.DIACHI,
                    o.NGAYDAT,
                    o. TONGTIEN,
                    o.TRANGTHAI,
                    o. DIACHI_ENC,
                    o.SDT_ENC,
                    o. GHICHU,
                    od.MAXE,
                    od.SOLUONG,
                    od.DONGIA,
                    c.TENXE,
                    c. HANGXE,
                    c.HINHANH,
                    c.MOTA
                FROM ORDERS o
                JOIN CUSTOMER cust ON o. MAKH = cust.MAKH
                LEFT JOIN ORDER_DETAIL od ON o.MADON = od.MADON
                LEFT JOIN CAR c ON od. MAXE = c.MAXE
                WHERE o.MADON = :orderId", conn))
                    {
                        cmd.Parameters.Add("orderId", OracleDbType.Int32).Value = orderId;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                //  Decrypt encrypted fields
                                string encryptedAddress = reader["DIACHI_ENC"]?.ToString();
                                string encryptedPhone = reader["SDT_ENC"]?.ToString();

                                string decryptedAddress = "";
                                string decryptedPhone = "";

                                if (!string.IsNullOrEmpty(encryptedAddress))
                                {
                                    try
                                    {
                                        decryptedAddress = RSAEncryptionHelper.Decrypt(encryptedAddress);
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"⚠️ Decrypt Address Error: {ex.Message}");
                                        decryptedAddress = "[Encrypted]";
                                    }
                                }

                                if (!string.IsNullOrEmpty(encryptedPhone))
                                {
                                    try
                                    {
                                        decryptedPhone = RSAEncryptionHelper.Decrypt(encryptedPhone);
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"⚠️ Decrypt Phone Error: {ex.Message}");
                                        decryptedPhone = "[Encrypted]";
                                    }
                                }

                                var orderDetail = new OrderDetailViewModel
                                {
                                    MaDon = Convert.ToInt32(reader["MADON"]),
                                    MaKH = Convert.ToInt32(reader["MAKH"]),
                                    HoTen = reader["HOTEN"]?.ToString(),
                                    Email = reader["EMAIL"]?.ToString(),
                                    SDT = reader["SDT"]?.ToString(),
                                    DiaChi = reader["DIACHI"]?.ToString(),
                                    NgayDat = Convert.ToDateTime(reader["NGAYDAT"]),
                                    TongTien = Convert.ToDecimal(reader["TONGTIEN"]),
                                    TrangThai = reader["TRANGTHAI"]?.ToString(),
                                    DiaChiGiaoHang = decryptedAddress,
                                    SoDienThoai = decryptedPhone,
                                    GhiChu = reader["GHICHU"]?.ToString(),
                                    MaXe = reader["MAXE"] != DBNull.Value ? Convert.ToInt32(reader["MAXE"]) : 0,
                                    TenXe = reader["TENXE"]?.ToString() ?? "N/A",
                                    HangXe = reader["HANGXE"]?.ToString() ?? "N/A",
                                    HinhAnh = reader["HINHANH"]?.ToString() ?? "default-car.jpg",
                                    MoTa = reader["MOTA"]?.ToString(),
                                    SoLuong = reader["SOLUONG"] != DBNull.Value ? Convert.ToInt32(reader["SOLUONG"]) : 0,
                                    DonGia = reader["DONGIA"] != DBNull.Value ? Convert.ToDecimal(reader["DONGIA"]) : 0
                                };

                                System.Diagnostics.Debug.WriteLine($"✅ Order found - Owner: {orderDetail.MaKH}");

                                return orderDetail;
                            }
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"❌ Order #{orderId} not found");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetOrderDetails Error: {ex.Message}");
                throw;
            }
        }

        // ====================== HỦY ĐƠN HÀNG ======================
        public dynamic CancelOrder(int orderId, int userId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"CancelOrder - Order: {orderId}, User: {userId}");

                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    // Check if order can be cancelled
                    using (var checkCmd = new OracleCommand(@"
                SELECT TRANGTHAI 
                FROM ORDERS 
                WHERE MADON = :orderId", conn))
                    {
                        checkCmd.Parameters.Add("orderId", OracleDbType.Int32).Value = orderId;

                        var status = checkCmd.ExecuteScalar()?.ToString();

                        if (string.IsNullOrEmpty(status))
                        {
                            return new
                            {
                                Success = false,
                                Message = "Không tìm thấy đơn hàng!"
                            };
                        }

                        if (status == "Da huy")
                        {
                            return new
                            {
                                Success = false,
                                Message = "Đơn hàng đã bị hủy trước đó!"
                            };
                        }

                        if (status == "Hoan thanh")
                        {
                            return new
                            {
                                Success = false,
                                Message = "Không thể hủy đơn hàng đã hoàn thành!"
                            };
                        }
                    }

                    // Update status to cancelled
                    using (var updateCmd = new OracleCommand(@"
                UPDATE ORDERS 
                SET TRANGTHAI = 'Da huy' 
                WHERE MADON = :orderId", conn))
                    {
                        updateCmd.Parameters.Add("orderId", OracleDbType.Int32).Value = orderId;

                        int rowsAffected = updateCmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Log cancellation
                            using (var logCmd = new OracleCommand(@"
                        INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
                        VALUES (SEQ_LOG.NEXTVAL, :userId, :action, 'ORDER', SYSDATE, :ip)", conn))
                            {
                                logCmd.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
                                logCmd.Parameters.Add("action", OracleDbType.Varchar2).Value = $"CANCEL_ORDER:  #{orderId}";
                                logCmd.Parameters.Add("ip", OracleDbType.Varchar2).Value = "127.0.0.1";

                                logCmd.ExecuteNonQuery();
                            }

                            System.Diagnostics.Debug.WriteLine($"✅ Order #{orderId} cancelled by User #{userId}");

                            return new
                            {
                                Success = true,
                                Message = "Đã hủy đơn hàng thành công!"
                            };
                        }

                        return new
                        {
                            Success = false,
                            Message = "Không thể hủy đơn hàng!"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ CancelOrder Error: {ex.Message}");

                return new
                {
                    Success = false,
                    Message = "Lỗi:  " + ex.Message
                };
            }
        }
        // ====================== CHI TIẾT ĐƠN HÀNG (WITH VPD & RSA DECRYPTION) ======================
        public OrderDetailViewModel GetOrderDetailWithVPD(int maDon, int currentUserId, string userRole)
        {
            OrderDetailViewModel orderDetail = null;

            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    // ✅ SET VPD CONTEXT
                    SetVPDContext(conn, currentUserId, userRole);

                    using (var cmd = new OracleCommand(@"
                        SELECT 
                            o.MADON,
                            o.MAKH,
                            cust.HOTEN,
                            cust.EMAIL,
                            cust.SDT,
                            cust.DIACHI,
                            o.NGAYDAT,
                            o.TONGTIEN,
                            o. TRANGTHAI,
                            o.DIACHI_ENC,
                            o.SDT_ENC,
                            o.GHICHU,
                            od.MAXE,
                            c.TENXE,
                            c.HANGXE,
                            c.HINHANH,
                            c.MOTA,
                            od. SOLUONG,
                            od.DONGIA
                        FROM ORDERS o
                        JOIN CUSTOMER cust ON o.MAKH = cust.MAKH
                        JOIN ORDER_DETAIL od ON o. MADON = od.MADON
                        JOIN CAR c ON od.MAXE = c.MAXE
                        WHERE o.MADON = :madon", conn))
                    {
                        cmd.Parameters.Add("madon", OracleDbType.Int32).Value = maDon;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string encryptedAddress = reader["DIACHI_ENC"]?.ToString();
                                string encryptedPhone = reader["SDT_ENC"]?.ToString();

                                // ✅ TUẦN 6:  DECRYPT RSA DATA
                                string decryptedAddress = "";
                                string decryptedPhone = "";

                                if (!string.IsNullOrEmpty(encryptedAddress))
                                {
                                    try
                                    {
                                        decryptedAddress = RSAEncryptionHelper.Decrypt(encryptedAddress);
                                        System.Diagnostics.Debug.WriteLine($"✅ Decrypted Address: {decryptedAddress}");
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"❌ Decrypt Address Error: {ex.Message}");
                                        decryptedAddress = "[Encrypted]";
                                    }
                                }

                                if (!string.IsNullOrEmpty(encryptedPhone))
                                {
                                    try
                                    {
                                        decryptedPhone = RSAEncryptionHelper.Decrypt(encryptedPhone);
                                        System.Diagnostics.Debug.WriteLine($"✅ Decrypted Phone: {decryptedPhone}");
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"❌ Decrypt Phone Error: {ex.Message}");
                                        decryptedPhone = "[Encrypted]";
                                    }
                                }

                                orderDetail = new OrderDetailViewModel
                                {
                                    MaDon = Convert.ToInt32(reader["MADON"]),
                                    MaKH = Convert.ToInt32(reader["MAKH"]),
                                    HoTen = reader["HOTEN"].ToString(),
                                    Email = reader["EMAIL"].ToString(),
                                    SDT = reader["SDT"].ToString(),
                                    DiaChi = reader["DIACHI"].ToString(),
                                    NgayDat = Convert.ToDateTime(reader["NGAYDAT"]),
                                    TongTien = Convert.ToDecimal(reader["TONGTIEN"]),
                                    TrangThai = reader["TRANGTHAI"].ToString(),
                                    // ✅ Decrypted shipping info
                                    DiaChiGiaoHang = decryptedAddress,
                                    SoDienThoai = decryptedPhone,
                                    GhiChu = reader["GHICHU"]?.ToString(),
                                    MaXe = Convert.ToInt32(reader["MAXE"]),
                                    TenXe = reader["TENXE"].ToString(),
                                    HangXe = reader["HANGXE"].ToString(),
                                    HinhAnh = reader["HINHANH"].ToString(),
                                    MoTa = reader["MOTA"].ToString(),
                                    SoLuong = Convert.ToInt32(reader["SOLUONG"]),
                                    DonGia = Convert.ToDecimal(reader["DONGIA"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetOrderDetailWithVPD Error:  {ex.Message}");
            }

            return orderDetail;
        }

        // ====================== CHI TIẾT ĐƠN HÀNG (LEGACY) ======================
        public OrderDetailViewModel GetOrderDetail(int maDon)
        {
            // For backward compatibility - assume CUSTOMER role
            return GetOrderDetailWithVPD(maDon, 0, "CUSTOMER");
        }

        // ====================== CẬP NHẬT TRẠNG THÁI ======================
        public dynamic UpdateOrderStatus(int maDon, string trangThai)
        {
            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    using (var cmd = new OracleCommand("SP_UPDATE_ORDER_STATUS", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_madon", OracleDbType.Int32).Value = maDon;
                        cmd.Parameters.Add("p_trangthai", OracleDbType.Varchar2).Value = trangThai;
                        cmd.Parameters.Add("p_result", OracleDbType.Int32).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("p_message", OracleDbType.Varchar2, 200).Direction = ParameterDirection.Output;

                        cmd.ExecuteNonQuery();

                        int result = ((OracleDecimal)cmd.Parameters["p_result"].Value).ToInt32();
                        string message = cmd.Parameters["p_message"].Value.ToString();

                        return new
                        {
                            Success = result == 1,
                            Message = message
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
        public OrderDetailViewModel GetOrderDetailById(int orderId)
        {
            System.Diagnostics.Debug.WriteLine($"========================================");
            System.Diagnostics.Debug.WriteLine($"GetOrderDetailById:  {orderId}");

            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    // ✅ USE LEFT JOIN - Don't fail if ORDER_DETAIL missing
                    using (var cmd = new OracleCommand(@"
                SELECT 
                    o. MADON,
                    o. MAKH,
                    c.HOTEN,
                    c.EMAIL,
                    c.SDT,
                    c.DIACHI,
                    o.NGAYDAT,
                    o.TONGTIEN,
                    o.TRANGTHAI,
                    o. DIACHI_ENC,
                    o.SDT_ENC,
                    o.GHICHU,
                    od.MAXE,
                    car.TENXE,
                    car.HANGXE,
                    car. HINHANH,
                    car.MOTA,
                    od.SOLUONG,
                    od. DONGIA
                FROM ORDERS o
                INNER JOIN CUSTOMER c ON o. MAKH = c.MAKH
                LEFT JOIN ORDER_DETAIL od ON o.MADON = od. MADON
                LEFT JOIN CAR car ON od.MAXE = car.MAXE
                WHERE o.MADON = :orderId", conn))
                    {
                        cmd.Parameters.Add("orderId", OracleDbType.Int32).Value = orderId;

                        System.Diagnostics.Debug.WriteLine($"Executing query with LEFT JOINs.. .");

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                System.Diagnostics.Debug.WriteLine($"✅ Order found!");

                                var result = new OrderDetailViewModel
                                {
                                    MaDon = Convert.ToInt32(reader["MADON"]),
                                    MaKH = Convert.ToInt32(reader["MAKH"]),
                                    HoTen = reader["HOTEN"]?.ToString() ?? "",
                                    Email = reader["EMAIL"]?.ToString() ?? "",
                                    SDT = reader["SDT"]?.ToString() ?? "",
                                    DiaChi = reader["DIACHI"]?.ToString() ?? "",
                                    NgayDat = Convert.ToDateTime(reader["NGAYDAT"]),
                                    TongTien = Convert.ToDecimal(reader["TONGTIEN"]),
                                    TrangThai = reader["TRANGTHAI"]?.ToString() ?? "",
                                    DiaChiGiaoHang = reader["DIACHI_ENC"]?.ToString() ?? "",
                                    SoDienThoai = reader["SDT_ENC"]?.ToString() ?? "",
                                    GhiChu = reader["GHICHU"]?.ToString() ?? "",

                                    // ✅ Handle NULL values
                                    MaXe = reader["MAXE"] != DBNull.Value ? Convert.ToInt32(reader["MAXE"]) : 0,
                                    TenXe = reader["TENXE"]?.ToString() ?? "Xe không xác định",
                                    HangXe = reader["HANGXE"]?.ToString() ?? "N/A",
                                    HinhAnh = reader["HINHANH"]?.ToString() ?? "default-car.jpg",
                                    MoTa = reader["MOTA"]?.ToString() ?? "",
                                    SoLuong = reader["SOLUONG"] != DBNull.Value ? Convert.ToInt32(reader["SOLUONG"]) : 0,
                                    DonGia = reader["DONGIA"] != DBNull.Value ? Convert.ToDecimal(reader["DONGIA"]) : 0
                                };

                                System.Diagnostics.Debug.WriteLine($"  Order #{result.MaDon}");
                                System.Diagnostics.Debug.WriteLine($"  Customer: {result.HoTen} (ID: {result.MaKH})");
                                System.Diagnostics.Debug.WriteLine($"  Car: {result.TenXe} (ID: {result.MaXe})");
                                System.Diagnostics.Debug.WriteLine($"  Status: {result.TrangThai}");
                                System.Diagnostics.Debug.WriteLine($"========================================");

                                return result;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"❌ No rows returned");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error:  {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
            }

            System.Diagnostics.Debug.WriteLine($"========================================");
            return null;
        }
        // ====================== HELPER:  SET VPD CONTEXT ======================
        // ====================== HELPER:  SET VPD CONTEXT (FIXED) ======================
        private void SetVPDContext(OracleConnection conn, int userId, string userRole)
        {
            try
            {
                using (var cmd = new OracleCommand("PKG_CARSALE_SECURITY.SET_USER_CONTEXT", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
                    cmd.Parameters.Add("p_role", OracleDbType.Varchar2).Value = userRole.ToUpper(); // ✅ Ensure uppercase

                    cmd.ExecuteNonQuery();

                    System.Diagnostics.Debug.WriteLine($"✅ VPD Context Set Successfully:  UserID={userId}, Role={userRole}");

                    // ✅ VERIFY context was actually set
                    using (var verifyCmd = new OracleCommand(@"
                SELECT 
                    SYS_CONTEXT('CARSALE_CTX', 'USER_ID') AS USER_ID,
                    SYS_CONTEXT('CARSALE_CTX', 'USER_ROLE') AS USER_ROLE
                FROM DUAL", conn))
                    {
                        using (var reader = verifyCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string ctxUserId = reader["USER_ID"]?.ToString();
                                string ctxUserRole = reader["USER_ROLE"]?.ToString();

                                System.Diagnostics.Debug.WriteLine($"🔍 Context Verification: USER_ID={ctxUserId}, USER_ROLE={ctxUserRole}");

                                if (string.IsNullOrEmpty(ctxUserId) || string.IsNullOrEmpty(ctxUserRole))
                                {
                                    throw new Exception("VPD Context not set properly!");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ VPD Context Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                // ✅ MUST THROW to prevent query without VPD! 
                throw new Exception($"Failed to set VPD context: {ex.Message}", ex);
            }
        }
    }
}