using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace WebCar.Helpers
{
    public static class SecurityContextHelper
    {
        public static void SetUserSecurityContext(int customerId, string connectionString)
        {
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    var cmd = new OracleCommand("PKG_SECURITY_CONTEXT.SET_USER_CONTEXT", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_makh", OracleDbType.Int32).Value = customerId;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Log error (optional)
                System.Diagnostics.Debug.WriteLine($"Error setting security context: {ex.Message}");
            }
        }

        public static void ClearUserSecurityContext(string connectionString)
        {
            try
            {
                using (var conn = new OracleConnection(connectionString))
                {
                    var cmd = new OracleCommand("PKG_SECURITY_CONTEXT.CLEAR_USER_CONTEXT", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing security context: {ex.Message}");
            }
        }
    }
}