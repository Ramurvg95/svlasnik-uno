using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Security.Cryptography.Xml;
using TeamManagement.Models;

namespace TeamManagement.Services
{
    public class DataAccessService
    {
        private readonly string _dbConnectionString;

        public DataAccessService()
        {
            _dbConnectionString = "server=teammanagement.c1msi4aw0okr.us-east-2.rds.amazonaws.com;uid=ramu_rvg;pwd=K2ge20232023;database=team_management;Allow Zero Datetime=True";
        }
        ///<summary>
        ///Gets the team members.
        ///</summary>
        ///<param name="searchText"></param>
        ///<param name="userIds"></param>
        ///<returns></returns>

        public async Task<List<User>> FindTeamMembersAsync(string searchtext, long[] userIds = null)
        {
            searchtext = searchtext + "%";
            string sql = $"select * from users u where u.status_id=1";

            if (!string.IsNullOrWhiteSpace(searchtext))
            {
                sql = sql + $" and (u.username like ?searchText or first_name like ?searchText or last_name like ?searchText or email like ?searchText)";
            }
            if (userIds != null && userIds.Length > 0)
            {
                sql = sql + $" and u.user_id in ({string.Join(",", userIds)})";
            }
            List<User> _users = new List<User>();

            using (var connection = new MySqlConnection(_dbConnectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    command.Parameters.Add(new MySqlParameter("searchText", searchtext));
                    using var reader = (MySqlDataReader)await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        _users.Add(ParseUser(reader));
                    }
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
            return _users;

        }


        ///<summary>
        /// Searches the members.
        ///</summary>
        /// <param name="userName"></param>
        /// <returns></returns>

        public async Task<User> FindUserByUserNameAsync(string userName)
        {
            string sql = $"select * from users u where username like ?username and u.status_id=1";
            User _user = new User();

            using (var connection = new MySqlConnection(_dbConnectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    command.Parameters.Add( new MySqlParameter( "?username", userName));
                    using var reader = (MySqlDataReader)await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        _user = ParseUser(reader);
                    }
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
            return _user;
        }

        ///<summary>
        /// Gets the teams.
        ///</summary>
        ///<param name="teamId"></param>
        ///<returns></returns>

        public async Task<Team> GetTeamAsync(long teamId, string name = null)
        {
            string sql = string.Empty;
            if (teamId > 0)
            {
                sql = $"select * from teams where status_id=1 and team_id = {teamId}";
            }
            else
            {
                sql = $"select * from teams where status_id=1 and name = '{name}'";
            }
            Team _team = new();

            using (var connection = new MySqlConnection(_dbConnectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    using var reader = (MySqlDataReader)await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        _team = ParseTeam(reader);
                    }
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
            return _team;
        }

        ///<summary>
        /// Gets the team members.
        ///</summary>
        ///<param name="teamId"></param>
        ///<returns></returns>

        public async Task<List<TeamMember>> GetTeamMembersAsync(long teamId)
        {
            string sql = $"select * from team_members tm inner join users u on tm.user_id = u.user_id where team_id ={teamId} and tm.status_id=1 and u.status_id =1";
            List<TeamMember> _teamMembers = new List<TeamMember>();

            using (var connection = new MySqlConnection(_dbConnectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    command.Parameters.Add( new MySqlParameter( "?teamId", teamId));
                    using var reader = (MySqlDataReader)await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        _teamMembers.Add(ParseTeamMember(reader));
                    }
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
            return _teamMembers;
        }
        /// <summary>
        /// Gets the teams
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>

        public async Task<List<Team>> GetTeamsAsync(long? userId = null)
        {
            string sql = $"select * from teams t";
            if (userId.HasValue)
            {
                // if there is used id then load only the team that the user has access to.
                sql = sql + $" inner join team_members tm on tm.team_id = t.team_id and tm.user_id = {userId.Value} and tm.status_id = 1";
            }
            sql = sql + " where t.status_id=1 limit 10";


            List<Team> _teams = new List<Team>();

            using (var connection = new MySqlConnection(_dbConnectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    using var reader = (MySqlDataReader)await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        _teams.Add(ParseTeam(reader));
                    }
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
            return _teams;

        }
        ///<summary>
        /// Gets the teams.
        ///</summary>
        ///<param name="userId"></param>
        ///<returns></returns>

        public async Task<List<Role>> GetUserRolesAsync(long userId)
        {
            string sql = $"select r.name, r.role_id, ur.user_id from user_roles ur inner join roles r on r.role_id = ur.role_id where r.status_id=1 and user_id = {userId}";
            List<Role> _roles = new List<Role>();

            using (var connection = new MySqlConnection(_dbConnectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    using var reader = (MySqlDataReader)await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        _roles.Add(ParseRole(reader));
                    }
                }
                catch (Exception exception)
                {
                    throw exception;
                }

            }
            return _roles;
        }

        /// <summary>
        /// Saves the team members to db.
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>

        public async Task RemoveTeamAsync(long teamId, string lastModBy)
        {
            string sql = $"update teams set status_id =2, last_mod_by='{lastModBy}', last_mod_at= 'RemoveTeamAsync' where team_id = {teamId}";

            using (var connection = new MySqlConnection(_dbConnectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
        }

        /// <summary>
        /// Saves the team members to db
        /// </summary>
        /// <param name="teamId">The team.id.</param>
        /// <param name="userId">The user.id.</param>
        /// <returns></returns>

        public async Task RemoveTeamMemberAsync(long teamId, long userId, string lastModBy)
        {
            string sql = $" update team_members set status_id = 2, last_mod_by='{lastModBy}', last_mod_at='RemoveTeamAsync' where team_id = {teamId} and user_id = {userId}";

            using (var connection = new MySqlConnection(_dbConnectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
        }

        ///<summary>
        /// Saves the team members to db.
        ///</summary>
        ///<param name="teamMember"></param>
        ///<returns></returns>

        public async Task SaveTeamMemberToDbAsync(TeamMember teamMember)
        {
            string sql = $" Insert into team_members (team_id, user_id, team_member_type, created_by, created_at, status_id) values({teamMember.TeamId}, {teamMember.UserId}, '{teamMember.TeamMemberType}', '{teamMember.CreatedBy}','Toad', 1)";

            using (var connection = new MySqlConnection(_dbConnectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
        }

        ///<summary>
        /// Saves the team members to db.
        ///</summary>
        ///<param name="team"></param>
        ///<returns></returns>

        public async Task SaveTeamToDbAsync(Team team)
        {
            string sql = $" Insert into teams (name, created_by, created_at, status_id) values('{team.Name}', '{team.CreatedBy}','Toad', 1)";

            using (var connection = new MySqlConnection(_dbConnectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
        }

        /// <summary>
        /// Saves the team to db.
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>

        public async Task UpdateTeamToDbAsync(TeamDetailsViewModel team, string lastModBy)
        {
            string sql = $"update teams set name= '{team.TeamName}', last_mod_by= '{lastModBy}', last_mod_at='UpdateTeamToDbAsync' where team_id={team.TeamId}";

            using (var connection = new MySqlConnection(_dbConnectionString))
            using (var command = new MySqlCommand(sql, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
        }
        ///<summary>
        /// Parses the team object
        ///</summary>
        /// <param name="reader"></param>
        /// <returns></returns>

        private Role ParseRole(MySqlDataReader reader)
        {
            Role _role = new Role
            {
                RoleName = reader.GetString( "name"),
                UserId = reader.GetInt64( "user_id"),
                RoleId = reader.GetInt64("role_id"),
            };
            return _role;
        }

        ///<summary>
        ///Parses the team object.
        ///</summary>
        ///<param name="reader"></param>
        /// <returns></returns>

        private Team ParseTeam(MySqlDataReader reader)
        {
            Team _team = new Team
            {
                Name = reader.GetString( "name"),
                TeamId = reader.GetInt64( "team_id")
            };
            return _team;
        }

        /// <summary>
        /// Parses the team object.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>

        private TeamMember ParseTeamMember(MySqlDataReader reader)
        {
            TeamMember _teamMember = new TeamMember
            {
                TeamId = reader.GetInt64( "team_id"),
                UserId = reader.GetInt64( "user_id"),
                Name =  $"{reader.GetString( "first_name")} {reader.GetString("last_name")}",
                TeamMemberType = reader.GetString("team_member_type"),
            };
            return _teamMember;
        }

        ///<summary>
        /// Parses the User object
        ///</summary>
        /// <param name="reader"></param>
        /// <returns></returns>

        private User ParseUser(MySqlDataReader reader)
        {
            User _user = new User
            {
                FirstName = reader.GetString( "first_name"),
                LastName = reader.GetString( "last_name"),
                Email = reader.GetString ("email"),
                Password = reader.GetString( "password"),
                UserId = reader.GetInt64( "user_id"),
                UserName = reader.GetString( "username"),
                MemberType = reader.GetString( "member_type")
            };
            return _user;

        }

    }
}