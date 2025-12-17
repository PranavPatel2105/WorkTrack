using System;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Implementations
{
    public class EmployeeRepository : IEmployeeInterface
    {
        private readonly NpgsqlConnection _conn;

        // DI-friendly ctor so you can use:
        // builder.Services.AddSingleton<IEmployeeInterface, EmployeeRepository>();
        public EmployeeRepository(NpgsqlConnection conn)
        {
            _conn = conn;
        }
        public async Task<int> Register(t_employees data)
        {
            try
            {
                var cmd = new NpgsqlCommand("SELECT c_email FROM t_employees WHERE c_email = @c_email;", _conn);
                cmd.Parameters.AddWithValue("c_email", data.c_email);

                await _conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();

                if (reader.Read())
                {
                    await _conn.CloseAsync();
                    return 0;
                }

                await _conn.CloseAsync();

                var cmdd = new NpgsqlCommand(@"INSERT INTO t_employees(c_ename, c_email, c_password, c_role, c_profileimage)
                    VALUES (@c_ename, @c_email, @c_password, @c_role, @c_profileimage);", _conn);
                cmdd.Parameters.AddWithValue("c_ename", data.c_ename);
                cmdd.Parameters.AddWithValue("c_email", data.c_email);
                cmdd.Parameters.AddWithValue("c_password", data.c_password);
                cmdd.Parameters.AddWithValue("c_role", data.c_role ?? "Employee");
                cmdd.Parameters.AddWithValue("c_profileimage", (object?)data.c_profileimage ?? DBNull.Value);

                await _conn.OpenAsync();
                await cmdd.ExecuteNonQueryAsync();
                await _conn.CloseAsync();

                return 1;
            }
            catch (Exception ex)
            {
                await _conn.CloseAsync();
                Console.WriteLine("Register Error: " + ex.Message);
                return -1;
            }
        }

        public async Task<t_employees> Login(vm_Login employee)
        {
            t_employees empData = null;

            string query = @"SELECT c_empid,c_ename, c_email, c_role, c_profileimage FROM t_employees WHERE c_email = @c_email AND c_password = @c_password";

            try
            {
                using var cmd = new NpgsqlCommand(query, _conn);

                cmd.Parameters.AddWithValue("@c_email", employee.c_email);
                cmd.Parameters.AddWithValue("@c_password", employee.c_password);

                await _conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    empData = new t_employees
                    {
                        c_empid = Convert.ToInt32(reader["c_empid"]),
                        c_ename = reader["c_ename"].ToString(),
                        c_email = reader["c_email"].ToString(),
                        c_role = reader["c_role"].ToString(),
                        c_profileimage = reader["c_profileimage"] == DBNull.Value ? null : reader["c_profileimage"].ToString()
                    };
                    await _conn.CloseAsync();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Login Error: " + ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return empData;
        }

        //ep
         public async Task<t_employees> GetUser(int empId)
        {
            t_employees emp = null;

            if (_conn.State != ConnectionState.Open)
                await _conn.OpenAsync();

            const string sql = @"
                SELECT c_empid, c_ename, c_email, c_profileimage
                FROM t_employees
                WHERE c_empid = @id
                LIMIT 1;";

            await using var cmd = new NpgsqlCommand(sql, _conn);
            cmd.Parameters.AddWithValue("@id", empId);

            await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (await reader.ReadAsync())
            {
                emp = new t_employees
                {
                    c_empid = reader.GetInt32(reader.GetOrdinal("c_empid")),
                    c_ename = reader.GetString(reader.GetOrdinal("c_ename")),
                    c_email = reader.GetString(reader.GetOrdinal("c_email")),
                    c_profileimage = reader.IsDBNull(reader.GetOrdinal("c_profileimage"))
                                        ? string.Empty
                                        : reader.GetString(reader.GetOrdinal("c_profileimage"))
                };
            }
            await reader.CloseAsync();
            return emp;
        }
        
         public async Task<bool> UpdateProfile(int empId, t_employees model)
{
    if (_conn.State != ConnectionState.Open)
        await _conn.OpenAsync();

    // If no new image provided, keep the existing one
    string sql;
    if (!string.IsNullOrEmpty(model.c_profileimage))
    {
        // Update both name and image
        sql = @"
            UPDATE t_employees
            SET c_ename = @name,
                c_profileimage = @img
            WHERE c_empid = @id;";
        
        await using var cmd = new NpgsqlCommand(sql, _conn);
        cmd.Parameters.AddWithValue("@name", model.c_ename?.Trim() ?? string.Empty);
        cmd.Parameters.AddWithValue("@img", model.c_profileimage.Trim());
        cmd.Parameters.AddWithValue("@id", empId);
        
        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }
    else
    {
        // Update only name, keep existing image
        sql = @"
            UPDATE t_employees
            SET c_ename = @name
            WHERE c_empid = @id;";
        
        await using var cmd = new NpgsqlCommand(sql, _conn);
        cmd.Parameters.AddWithValue("@name", model.c_ename?.Trim() ?? string.Empty);
        cmd.Parameters.AddWithValue("@id", empId);
        
        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }
}

       public async Task<bool> ChangePassword(int empId, vm_ChangePassword req)
        {
            if (string.IsNullOrWhiteSpace(req.OldPassword) ||
                string.IsNullOrWhiteSpace(req.NewPassword) ||
                string.IsNullOrWhiteSpace(req.ConfirmPassword) ||
                !string.Equals(req.NewPassword, req.ConfirmPassword, StringComparison.Ordinal))
                return false;

            // Open only if needed
            if (_conn.State != ConnectionState.Open)
                await _conn.OpenAsync();

            const string getSql = "SELECT c_password FROM t_employees WHERE c_empid=@id;";
            string? currentPwd = null;


            await using (var getCmd = new NpgsqlCommand(getSql, _conn))
            {
                getCmd.Parameters.AddWithValue("@id", empId);
                await using var rdr = await getCmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
                if (await rdr.ReadAsync())
                    currentPwd = rdr["c_password"] as string;
            }

            if (currentPwd == null)
                return false;

            if (!string.Equals(currentPwd, req.OldPassword.Trim(), StringComparison.Ordinal))
                return false;

            const string updSql = @"
        UPDATE t_employees
        SET c_password = @pwd
        WHERE c_empid = @id;";

            await using var upd = new NpgsqlCommand(updSql, _conn);
            upd.Parameters.AddWithValue("@pwd", req.NewPassword.Trim());
            upd.Parameters.AddWithValue("@id", empId);

            var rows = await upd.ExecuteNonQueryAsync();
            return rows > 0;
        }

    }
}
