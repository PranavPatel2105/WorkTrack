using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Implementations
{
    public class ProjectRepository : IProjectInterface
    {
        private readonly NpgsqlConnection _conn;
        public ProjectRepository(NpgsqlConnection npgsqlConnection)
        {
            _conn = npgsqlConnection;
        }
        public async Task<int> Add(t_projects projectData)
        {
            await _conn.OpenAsync();
            try
            {
                string qry = @"INSERT INTO t_projects(
                    c_projectname, c_description)
                        VALUES ( @c_projectname, @c_description);";
                using (NpgsqlCommand cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@c_projectname", projectData.c_projectname);
                    cmd.Parameters.AddWithValue("@c_description", projectData.c_description);
                    await cmd.ExecuteNonQueryAsync();
                    return 1;

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
            finally
            {
                await _conn.CloseAsync();

            }
        }

        public async Task<int> Delete(string projectid)
        {
            await _conn.OpenAsync();
            string checkQuery = "SELECT COUNT(*) FROM t_tasks WHERE c_projectid = @project_id";
            using (var checkCmd = new NpgsqlCommand(checkQuery, _conn))
            {
                checkCmd.Parameters.AddWithValue("@project_id", int.Parse(projectid));
                var taskCount = (long)await checkCmd.ExecuteScalarAsync();

                if (taskCount > 0)
                {
                    return -1;
                }
            }
            try
            {
                string qry = "DELETE FROM t_projects where c_projectid=@c_projectid";
                using (NpgsqlCommand cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@c_projectid", int.Parse(projectid));
                    await cmd.ExecuteNonQueryAsync();
                    return 1;
                }
            }
            catch (System.Exception ex)
            {

                Console.WriteLine(ex.Message);
                return 0;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<List<t_projects>> GetAll(string? filter = null)
        {
            var projects = new List<t_projects>();
            await _conn.OpenAsync();
            try
            {
                string qry = @"SELECT c_projectid, c_projectname, c_description
	            FROM t_projects ";
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    qry += "WHERE " + filter;
                }
                Console.WriteLine(qry);
                using (NpgsqlCommand cmd = new NpgsqlCommand(qry, _conn))
                {
                    var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        projects.Add(new t_projects
                        {
                            c_projectid = reader.GetInt32(0),
                            c_projectname = reader.GetString(1),
                            c_description = reader.GetString(2),

                        });
                    }
                }
            }
            catch (System.Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }
            return projects;
        }

        public async Task<int> Update(t_projects projectData)
        {
            await _conn.OpenAsync();
            try
            {
                string qry = @"UPDATE public.t_projects
		        SET c_projectname=@c_projectname, c_description=@c_description
	            WHERE  c_projectid=@c_projectid;";
                using (NpgsqlCommand cmd = new NpgsqlCommand(qry, _conn))
                {
                    cmd.Parameters.AddWithValue("@c_description", projectData.c_description);
                    cmd.Parameters.AddWithValue("@c_projectname", projectData.c_projectname);
                    cmd.Parameters.AddWithValue("@c_projectid", projectData.c_projectid);
                    await cmd.ExecuteNonQueryAsync();
                    return 1;
                }
            }
            catch (System.Exception ex)
            {

                Console.WriteLine(ex.Message);
                return 0;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        
        }
}
