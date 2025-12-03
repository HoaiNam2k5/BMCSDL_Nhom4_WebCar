using System;
using System.Collections.Generic;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using WebCar.Models;

namespace WebCar.Data
{
    public class CarRepository
    {
        private readonly string _connectionString;

        public CarRepository()
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["OracleDb"];

            if (connectionStringSettings == null)
            {
                throw new InvalidOperationException(
                    "Connection string 'OracleDb' not found in Web.config."
                );
            }

            _connectionString = connectionStringSettings.ConnectionString;
        }

        // ✅ FIX: GetAllCars với tìm kiếm CHÍNH XÁC
        public List<CAR> GetAllCars(string searchTerm = null, string brand = null,
            decimal? minPrice = null, decimal? maxPrice = null, short? year = null)
        {
            var cars = new List<CAR>();

            using (var conn = new OracleConnection(_connectionString))
            {
                var sql = @"
                    SELECT MAXE, TENXE, HANGXE, GIA, NAMSX, MOTA, HINHANH, TRANGTHAI
                    FROM CAR
                    WHERE TRANGTHAI != 'Da xoa'";

                // ✅ Tìm kiếm: loại bỏ khoảng trắng + không phân biệt hoa thường
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    sql += @" AND (
                        UPPER(REPLACE(TENXE, ' ', '')) LIKE '%' || UPPER(REPLACE(:searchTerm, ' ', '')) || '%'
                        OR UPPER(REPLACE(HANGXE, ' ', '')) LIKE '%' || UPPER(REPLACE(:searchTerm, ' ', '')) || '%'
                        OR UPPER(MOTA) LIKE '%' || UPPER(:searchTerm) || '%'
                    )";
                }

                if (!string.IsNullOrWhiteSpace(brand))
                {
                    sql += " AND UPPER(HANGXE) = UPPER(:brand)";
                }

                if (minPrice.HasValue)
                {
                    sql += " AND GIA >= :minPrice";
                }

                if (maxPrice.HasValue)
                {
                    sql += " AND GIA <= :maxPrice";
                }

                if (year.HasValue)
                {
                    sql += " AND NAMSX = :year";
                }

                sql += " ORDER BY MAXE DESC";

                var cmd = new OracleCommand(sql, conn);

                // Bind parameters
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    cmd.Parameters.Add(new OracleParameter("searchTerm", OracleDbType.Varchar2)
                    {
                        Value = searchTerm.Trim()
                    });
                }

                if (!string.IsNullOrWhiteSpace(brand))
                {
                    cmd.Parameters.Add(new OracleParameter("brand", OracleDbType.Varchar2)
                    {
                        Value = brand.Trim()
                    });
                }

                if (minPrice.HasValue)
                {
                    cmd.Parameters.Add(new OracleParameter("minPrice", OracleDbType.Decimal)
                    {
                        Value = minPrice.Value
                    });
                }

                if (maxPrice.HasValue)
                {
                    cmd.Parameters.Add(new OracleParameter("maxPrice", OracleDbType.Decimal)
                    {
                        Value = maxPrice.Value
                    });
                }

                if (year.HasValue)
                {
                    cmd.Parameters.Add(new OracleParameter("year", OracleDbType.Int16)
                    {
                        Value = year.Value
                    });
                }

                conn.Open();

                // ✅ DEBUG LOG
                System.Diagnostics.Debug.WriteLine("=== SQL QUERY ===");
                System.Diagnostics.Debug.WriteLine(sql);
                System.Diagnostics.Debug.WriteLine("=== PARAMETERS ===");
                foreach (OracleParameter param in cmd.Parameters)
                {
                    System.Diagnostics.Debug.WriteLine($"{param.ParameterName} = {param.Value}");
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cars.Add(new CAR
                        {
                            MAXE = reader["MAXE"] != DBNull.Value ? Convert.ToDecimal(reader["MAXE"]) : 0,
                            TENXE = reader["TENXE"]?.ToString() ?? "",
                            HANGXE = reader["HANGXE"]?.ToString() ?? "",
                            GIA = reader["GIA"] != DBNull.Value ? Convert.ToDecimal(reader["GIA"]) : (decimal?)null,
                            NAMSX = reader["NAMSX"] != DBNull.Value ? Convert.ToInt16(reader["NAMSX"]) : (short?)null,
                            MOTA = reader["MOTA"]?.ToString() ?? "",
                            HINHANH = reader["HINHANH"]?.ToString() ?? "",
                            TRANGTHAI = reader["TRANGTHAI"]?.ToString() ?? "Con hang"
                        });
                    }
                }

                System.Diagnostics.Debug.WriteLine($"=== TOTAL CARS FOUND: {cars.Count} ===");
            }

            return cars;
        }

        public CAR GetCarById(decimal carId)
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                var cmd = new OracleCommand(@"
                    SELECT MAXE, TENXE, HANGXE, GIA, NAMSX, MOTA, HINHANH, TRANGTHAI
                    FROM CAR
                    WHERE MAXE = :carId", conn);

                cmd.Parameters.Add(new OracleParameter("carId", OracleDbType.Decimal) { Value = carId });

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new CAR
                        {
                            MAXE = reader["MAXE"] != DBNull.Value ? Convert.ToDecimal(reader["MAXE"]) : 0,
                            TENXE = reader["TENXE"]?.ToString() ?? "",
                            HANGXE = reader["HANGXE"]?.ToString() ?? "",
                            GIA = reader["GIA"] != DBNull.Value ? Convert.ToDecimal(reader["GIA"]) : (decimal?)null,
                            NAMSX = reader["NAMSX"] != DBNull.Value ? Convert.ToInt16(reader["NAMSX"]) : (short?)null,
                            MOTA = reader["MOTA"]?.ToString() ?? "",
                            HINHANH = reader["HINHANH"]?.ToString() ?? "",
                            TRANGTHAI = reader["TRANGTHAI"]?.ToString() ?? "Con hang"
                        };
                    }
                }
            }

            return null;
        }

        public List<string> GetBrands()
        {
            var brands = new List<string>();

            using (var conn = new OracleConnection(_connectionString))
            {
                var cmd = new OracleCommand(@"
                    SELECT DISTINCT HANGXE 
                    FROM CAR 
                    WHERE HANGXE IS NOT NULL 
                    AND TRANGTHAI != 'Da xoa'
                    ORDER BY HANGXE", conn);

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var brand = reader["HANGXE"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(brand))
                        {
                            brands.Add(brand);
                        }
                    }
                }
            }

            return brands;
        }

        public List<CAR> GetRelatedCars(decimal carId, string brand, int limit = 4)
        {
            var cars = new List<CAR>();

            using (var conn = new OracleConnection(_connectionString))
            {
                var cmd = new OracleCommand(@"
                    SELECT * FROM (
                        SELECT MAXE, TENXE, HANGXE, GIA, NAMSX, MOTA, HINHANH, TRANGTHAI
                        FROM CAR
                        WHERE UPPER(HANGXE) = UPPER(:brand)
                        AND MAXE != :carId 
                        AND TRANGTHAI = 'Con hang'
                        ORDER BY MAXE DESC
                    ) WHERE ROWNUM <= :limit", conn);

                cmd.Parameters.Add(new OracleParameter("brand", OracleDbType.Varchar2) { Value = brand });
                cmd.Parameters.Add(new OracleParameter("carId", OracleDbType.Decimal) { Value = carId });
                cmd.Parameters.Add(new OracleParameter("limit", OracleDbType.Int32) { Value = limit });

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cars.Add(new CAR
                        {
                            MAXE = reader["MAXE"] != DBNull.Value ? Convert.ToDecimal(reader["MAXE"]) : 0,
                            TENXE = reader["TENXE"]?.ToString() ?? "",
                            HANGXE = reader["HANGXE"]?.ToString() ?? "",
                            GIA = reader["GIA"] != DBNull.Value ? Convert.ToDecimal(reader["GIA"]) : (decimal?)null,
                            NAMSX = reader["NAMSX"] != DBNull.Value ? Convert.ToInt16(reader["NAMSX"]) : (short?)null,
                            HINHANH = reader["HINHANH"]?.ToString() ?? "",
                            TRANGTHAI = reader["TRANGTHAI"]?.ToString() ?? "Con hang"
                        });
                    }
                }
            }

            return cars;
        }

        // Các methods khác giữ nguyên...
        public (bool Success, string Message, decimal CarId) CreateCar(CAR car)
        {
            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    var cmd = new OracleCommand(@"
                        INSERT INTO CAR (MAXE, TENXE, HANGXE, GIA, NAMSX, MOTA, HINHANH, TRANGTHAI)
                        VALUES (SEQ_CAR.NEXTVAL, :tenxe, :hangxe, :gia, :namsx, :mota, :hinhanh, :trangthai)
                        RETURNING MAXE INTO :newId", conn);

                    cmd.Parameters.Add(new OracleParameter("tenxe", OracleDbType.Varchar2) { Value = car.TENXE });
                    cmd.Parameters.Add(new OracleParameter("hangxe", OracleDbType.Varchar2) { Value = car.HANGXE });
                    cmd.Parameters.Add(new OracleParameter("gia", OracleDbType.Decimal) { Value = car.GIA ?? (object)DBNull.Value });
                    cmd.Parameters.Add(new OracleParameter("namsx", OracleDbType.Int16) { Value = car.NAMSX ?? (object)DBNull.Value });
                    cmd.Parameters.Add(new OracleParameter("mota", OracleDbType.Varchar2) { Value = car.MOTA ?? (object)DBNull.Value });
                    cmd.Parameters.Add(new OracleParameter("hinhanh", OracleDbType.Varchar2) { Value = car.HINHANH ?? (object)DBNull.Value });
                    cmd.Parameters.Add(new OracleParameter("trangthai", OracleDbType.Varchar2) { Value = car.TRANGTHAI ?? "Con hang" });

                    var newIdParam = new OracleParameter("newId", OracleDbType.Decimal, System.Data.ParameterDirection.Output);
                    cmd.Parameters.Add(newIdParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    decimal newId = Convert.ToDecimal(newIdParam.Value.ToString());
                    return (true, "Thêm xe thành công!", newId);
                }
            }
            catch (Exception ex)
            {
                return (false, "Lỗi: " + ex.Message, 0);
            }
        }

        public (bool Success, string Message) UpdateCar(CAR car)
        {
            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    var cmd = new OracleCommand(@"
                        UPDATE CAR 
                        SET TENXE = :tenxe, 
                            HANGXE = :hangxe, 
                            GIA = :gia, 
                            NAMSX = :namsx, 
                            MOTA = :mota, 
                            HINHANH = :hinhanh, 
                            TRANGTHAI = :trangthai
                        WHERE MAXE = :maxe", conn);

                    cmd.Parameters.Add(new OracleParameter("tenxe", OracleDbType.Varchar2) { Value = car.TENXE });
                    cmd.Parameters.Add(new OracleParameter("hangxe", OracleDbType.Varchar2) { Value = car.HANGXE });
                    cmd.Parameters.Add(new OracleParameter("gia", OracleDbType.Decimal) { Value = car.GIA ?? (object)DBNull.Value });
                    cmd.Parameters.Add(new OracleParameter("namsx", OracleDbType.Int16) { Value = car.NAMSX ?? (object)DBNull.Value });
                    cmd.Parameters.Add(new OracleParameter("mota", OracleDbType.Varchar2) { Value = car.MOTA ?? (object)DBNull.Value });
                    cmd.Parameters.Add(new OracleParameter("hinhanh", OracleDbType.Varchar2) { Value = car.HINHANH ?? (object)DBNull.Value });
                    cmd.Parameters.Add(new OracleParameter("trangthai", OracleDbType.Varchar2) { Value = car.TRANGTHAI ?? "Con hang" });
                    cmd.Parameters.Add(new OracleParameter("maxe", OracleDbType.Decimal) { Value = car.MAXE });

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                        return (true, "Cập nhật xe thành công!");
                    else
                        return (false, "Không tìm thấy xe để cập nhật!");
                }
            }
            catch (Exception ex)
            {
                return (false, "Lỗi: " + ex.Message);
            }
        }

        public (bool Success, string Message) DeleteCar(decimal carId)
        {
            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    var cmd = new OracleCommand(@"
                        UPDATE CAR 
                        SET TRANGTHAI = 'Da xoa'
                        WHERE MAXE = :maxe", conn);

                    cmd.Parameters.Add(new OracleParameter("maxe", OracleDbType.Decimal) { Value = carId });

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                        return (true, "Xóa xe thành công!");
                    else
                        return (false, "Không tìm thấy xe để xóa!");
                }
            }
            catch (Exception ex)
            {
                return (false, "Lỗi: " + ex.Message);
            }
        }
    }
}