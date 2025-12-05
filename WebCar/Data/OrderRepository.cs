using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using WebCar.Models;
using WebCar.Models.ViewModels;

namespace WebCar.Data
{
    public class OrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["Model1"].ConnectionString;
        }

        // ====================== TẠO ĐƠN HÀNG ======================
        public dynamic CreateOrder(int maKH, int maXe, int soLuong)
        {
            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    using (var cmd = new OracleCommand("SP_CREATE_ORDER", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Input parameters
                        cmd.Parameters.Add("p_makh", OracleDbType.Int32).Value = maKH;
                        cmd.Parameters.Add("p_maxe", OracleDbType.Int32).Value = maXe;
                        cmd.Parameters.Add("p_soluong", OracleDbType.Int32).Value = soLuong;

                        // Output parameters
                        cmd.Parameters.Add("p_result", OracleDbType.Int32).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("p_message", OracleDbType.Varchar2, 200).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("p_madon", OracleDbType.Int32).Direction = ParameterDirection.Output;

                        cmd.ExecuteNonQuery();

                        int result = ((OracleDecimal)cmd.Parameters["p_result"].Value).ToInt32();
                        string message = cmd.Parameters["p_message"].Value.ToString();

                        int maDon = 0;
                        if (cmd.Parameters["p_madon"].Value != DBNull.Value)
                            maDon = ((OracleDecimal)cmd.Parameters["p_madon"].Value).ToInt32();

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
                return new
                {
                    Success = false,
                    Message = "Lỗi: " + ex.Message,
                    OrderId = 0
                };
            }
        }

        // ====================== LẤY ĐƠN HÀNG CỦA TÔI ======================
        public List<OrderViewModel> GetMyOrders(int maKH)
        {
            var orders = new List<OrderViewModel>();

            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    using (var cmd = new OracleCommand("SP_GET_MY_ORDERS", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_makh", OracleDbType.Int32).Value = maKH;
                        cmd.Parameters.Add("p_orders", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                orders.Add(new OrderViewModel
                                {
                                    MaDon = Convert.ToInt32(reader["MADON"]),
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
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetMyOrders: {ex.Message}");
            }

            return orders;
        }

        // ====================== CHI TIẾT ĐƠN HÀNG ======================
        public OrderDetailViewModel GetOrderDetail(int maDon)
        {
            OrderDetailViewModel orderDetail = null;

            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    using (var cmd = new OracleCommand("SP_GET_ORDER_DETAIL", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_madon", OracleDbType.Int32).Value = maDon;
                        cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
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
                System.Diagnostics.Debug.WriteLine($"Error GetOrderDetail: {ex.Message}");
            }

            return orderDetail;
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
    }
}