using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Implementations
{
    public class TaskRepository : ItaskInterface
    {
        private readonly NpgsqlConnection _conn;

        public TaskRepository(NpgsqlConnection npgsql)
        {
            _conn = npgsql;
        }

        //  Fetch all task accuracy
        public async Task<List<t_taskaccurarcy>> GetTaskAccuracyAsync(int empid) //int empid add
        {
            await _conn.OpenAsync();
            try
            {
                var qry = @"
                      SELECT 
                                t.c_taskid,
                                p.c_projectname,
                                t.c_title,
                                e.c_empid,
                                e.c_ename,
                                CASE 
                                    WHEN t.c_status = 'Completed' 
                                        AND t.c_enddate IS NOT NULL 
                                        AND t.c_startdate <= t.c_enddate
                                    THEN ROUND(LEAST((t.c_estimateddays::decimal / GREATEST(t.c_enddate - t.c_startdate, 1)) * 100, 100))
                                    ELSE 0
                                END AS accuracy_percentage
                            FROM 
                                public.t_tasks t
                            JOIN 
                                public.t_employees e ON e.c_empid = t.c_empid
                            JOIN
                                public.t_projects p ON t.c_projectid = p.c_projectid
                            WHERE 
                            e.c_empid=@id;
                ";

                using var cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@id", empid);
                using var reader = await cmd.ExecuteReaderAsync();

                var taskList = new List<t_taskaccurarcy>();

                while (await reader.ReadAsync())
                {
                    taskList.Add(new t_taskaccurarcy
                    {
                        TaskId = (int)reader["c_taskid"],
                        Title = reader["c_title"].ToString(),
                        EmployeeName = reader["c_ename"].ToString(),
                        ProjectName = reader["c_projectname"].ToString(),
                        AccuracyPercentage = (decimal)reader["accuracy_percentage"]
                    });
                }

                return taskList;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving task accuracy", ex);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<List<t_taskaccurarcy>> OverallAccuracyAsync()
        {
            await _conn.OpenAsync();
            try
            {
                var qry = @"
                WITH task_stats AS (
                    SELECT 

                        t.c_empid,
                        t.c_estimateddays,
                        GREATEST((t.c_enddate - t.c_startdate), 1) AS actual_days,
                        LEAST(
                            (t.c_estimateddays::decimal / GREATEST((t.c_enddate - t.c_startdate), 1)) * 100, 
                            100
                        ) AS accuracy_percentage
                    FROM 
                        t_tasks t
                    WHERE 
                        t.c_status = 'Completed'
                        AND t.c_enddate IS NOT NULL
                        AND t.c_startdate <= t.c_enddate
                ),
                aggregated AS (
                    SELECT 
                    e.c_role,
                    e.c_profileimage,
                        e.c_empid,
                        e.c_ename,
                        COUNT(ts.c_empid) AS total_tasks,
                        ROUND(
                            CASE 
                                WHEN SUM(ts.actual_days) > 0 THEN 
                                    LEAST((SUM(ts.c_estimateddays)::DECIMAL / SUM(ts.actual_days)) * 100,100)
                                ELSE NULL
                            END, 
                            2
                        ) AS efficiency_percentage,
                        ROUND(AVG(ts.accuracy_percentage), 2) AS average_task_accuracy
                    FROM 
                        t_employees e
                    LEFT JOIN task_stats ts ON e.c_empid = ts.c_empid
                    GROUP BY 
                        e.c_empid, e.c_ename
                )
                SELECT 
                c_profileimage,
                    c_empid,
                    c_ename AS employee_name,
                    total_tasks,
                    efficiency_percentage,
                    average_task_accuracy
                FROM aggregated
                Where c_role='Employee'
                ORDER BY efficiency_percentage;

          ";

                using var cmd = new NpgsqlCommand(qry, _conn);
                using var reader = await cmd.ExecuteReaderAsync();

                var taskList = new List<t_taskaccurarcy>();

                while (await reader.ReadAsync())
                {
                    taskList.Add(new t_taskaccurarcy
                    {
                        EmpId = Convert.ToInt32(reader["c_empid"]),
                        EmployeeName = reader["employee_name"].ToString(),
                        EfficiencyPercentage = reader["efficiency_percentage"] is DBNull ? (decimal?)null : Convert.ToDecimal(reader["efficiency_percentage"]),
                        AccuracyPercentage = reader["average_task_accuracy"] is DBNull ? (decimal?)null : Convert.ToDecimal(reader["average_task_accuracy"]),
                        totaltasks = Convert.ToInt32(reader["total_tasks"]),
                        Profileimage=reader["c_profileimage"].ToString()
                    });
                }

                return taskList;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving task accuracy", ex);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        //  Fetch all tasks for employee (including project name)
        public async Task<List<t_tasks>> GetAllTasksAsync(int empId)
        {
            var list = new List<t_tasks>();

            const string sql = @"
                SELECT 
                    t.c_taskid,
                    t.c_projectid,
                    t.c_empid,
                    t.c_title,
                    t.c_description,
                    t.c_estimateddays,
                    t.c_startdate,
                    t.c_enddate,
                    t.c_status,
                    p.c_projectname
                FROM 
                    t_tasks t
                LEFT JOIN 
                    t_projects p ON p.c_projectid = t.c_projectid
                WHERE 
                    t.c_empid = @c_empid
                ORDER BY t.c_taskid DESC;
            ";

            await _conn.OpenAsync();
            try
            {
                using var cmd = new NpgsqlCommand(sql, _conn);
                cmd.Parameters.AddWithValue("@c_empid", empId);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var t = new t_tasks
                    {
                        c_taskid = (int)reader["c_taskid"],
                        c_projectid = reader["c_projectid"] == DBNull.Value ? null : (int?)reader["c_projectid"],
                        c_empid = (int)reader["c_empid"],
                        c_title = reader["c_title"].ToString(),
                        c_description = reader["c_description"].ToString(),
                        c_estimateddays = (int)reader["c_estimateddays"],
                        c_startdate = reader["c_startdate"] == DBNull.Value ? default : (DateTime)reader["c_startdate"],
                        c_enddate = reader["c_enddate"] == DBNull.Value ? null : (DateTime?)reader["c_enddate"],
                        c_status = reader["c_status"].ToString(),
                        c_projectname = reader["c_projectname"] == DBNull.Value ? string.Empty : reader["c_projectname"].ToString()
                    };
                    list.Add(t);
                }
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return list;
        }

        //  Fetch single task with project name
        public async Task<t_tasks?> GetTaskByIdAsync(int taskId, int empId)
        {
            const string sql = @"
                SELECT 
                    t.c_taskid,
                    t.c_projectid,
                    t.c_empid,
                    t.c_title,
                    t.c_description,
                    t.c_estimateddays,
                    t.c_startdate,
                    t.c_enddate,
                    t.c_status,
                    p.c_projectname
                FROM 
                    t_tasks t
                LEFT JOIN 
                    t_projects p ON p.c_projectid = t.c_projectid
                WHERE 
                    t.c_taskid=@c_taskid AND t.c_empid=@c_empid;
            ";

            await _conn.OpenAsync();
            try
            {
                using var cmd = new NpgsqlCommand(sql, _conn);
                cmd.Parameters.AddWithValue("@c_taskid", taskId);
                cmd.Parameters.AddWithValue("@c_empid", empId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new t_tasks
                    {
                        c_taskid = (int)reader["c_taskid"],
                        c_projectid = reader["c_projectid"] == DBNull.Value ? null : (int?)reader["c_projectid"],
                        c_empid = (int)reader["c_empid"],
                        c_title = reader["c_title"].ToString(),
                        c_description = reader["c_description"].ToString(),
                        c_estimateddays = (int)reader["c_estimateddays"],
                        c_startdate = reader["c_startdate"] == DBNull.Value ? default : (DateTime)reader["c_startdate"],
                        c_enddate = reader["c_enddate"] == DBNull.Value ? null : (DateTime?)reader["c_enddate"],
                        c_status = reader["c_status"].ToString(),
                        c_projectname = reader["c_projectname"] == DBNull.Value ? string.Empty : reader["c_projectname"].ToString()
                    };
                }
            }
            finally
            {
                await _conn.CloseAsync();
            }

            return null;
        }

        //  Add new task
        public async Task<int> AddTaskAsync(t_tasks task)
        {
            const string sql = @"
                INSERT INTO t_tasks
                (c_projectid, c_empid, c_title, c_description, c_estimateddays, c_startdate, c_enddate, c_status)
                VALUES (@c_projectid, @c_empid, @c_title, @c_description, @c_estimateddays, @c_startdate, NULL, @c_status)
                RETURNING c_taskid;
            ";

            await _conn.OpenAsync();
            try
            {
                using var cmd = new NpgsqlCommand(sql, _conn);
                
                // Handle nullable projectid
                if (task.c_projectid.HasValue && task.c_projectid.Value > 0)
                    cmd.Parameters.AddWithValue("@c_projectid", task.c_projectid.Value);
                else
                    cmd.Parameters.AddWithValue("@c_projectid", DBNull.Value);
                    
                cmd.Parameters.AddWithValue("@c_empid", task.c_empid);
                cmd.Parameters.AddWithValue("@c_title", task.c_title ?? string.Empty);
                cmd.Parameters.AddWithValue("@c_description", task.c_description ?? string.Empty);
                cmd.Parameters.AddWithValue("@c_estimateddays", task.c_estimateddays);
                cmd.Parameters.AddWithValue("@c_startdate", 
                    task.c_startdate == default ? (object)DBNull.Value : task.c_startdate);
                cmd.Parameters.AddWithValue("@c_status", task.c_status ?? "Pending");

                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        //  Update task with CASE statement for end date
        public async Task<int> UpdateTaskAsync(t_tasks task, int empId)
        {
            const string sql = @"
                UPDATE t_tasks
                SET
                    c_projectid = @c_projectid,
                    c_title = @c_title,
                    c_description = @c_description,
                    c_status = @c_status,
                    c_estimateddays = @c_estimateddays,
                    c_startdate = @c_startdate,
                    c_enddate = CASE
                        WHEN c_status <> @c_status AND @c_status = 'Completed' THEN CURRENT_DATE
                        WHEN c_status <> @c_status AND @c_status = 'Pending' THEN NULL
                        ELSE c_enddate
                    END
                WHERE c_taskid = @c_taskid AND c_empid = @c_empid;";

            await _conn.OpenAsync();
            try
            {
                using var cmd = new NpgsqlCommand(sql, _conn);
                
                cmd.Parameters.AddWithValue("@c_taskid", task.c_taskid);
                cmd.Parameters.AddWithValue("@c_empid", empId);
                
                // Handle nullable projectid
                if (task.c_projectid.HasValue && task.c_projectid.Value > 0)
                    cmd.Parameters.AddWithValue("@c_projectid", task.c_projectid.Value);
                else
                    cmd.Parameters.AddWithValue("@c_projectid", DBNull.Value);
                    
                cmd.Parameters.AddWithValue("@c_title", task.c_title ?? string.Empty);
                cmd.Parameters.AddWithValue("@c_description", task.c_description ?? string.Empty);
                cmd.Parameters.AddWithValue("@c_estimateddays", task.c_estimateddays);
                cmd.Parameters.AddWithValue("@c_startdate", 
                    task.c_startdate == default ? (object)DBNull.Value : task.c_startdate);
                cmd.Parameters.AddWithValue("@c_status", task.c_status ?? "Pending");

                return await cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        //  Delete task
        public async Task<int> DeleteTaskAsync(int taskId, int empId)
        {
            const string sql = @"DELETE FROM t_tasks WHERE c_taskid=@c_taskid AND c_empid=@c_empid";

            await _conn.OpenAsync();
            try
            {
                using var cmd = new NpgsqlCommand(sql, _conn);
                cmd.Parameters.AddWithValue("@c_taskid", taskId);
                cmd.Parameters.AddWithValue("@c_empid", empId);
                return await cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }
    }
}