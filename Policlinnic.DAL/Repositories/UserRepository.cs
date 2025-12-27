using Microsoft.Data.SqlClient;
using Policlinnic.Domain.Entities;
using System.Data;

namespace Policlinnic.DAL.Repositories
{
    public class UserRepository : BaseRepository // Наследуемся
    {
        public User GetUserByLogin(string login)
        {
            User user = null;

            string sql = "SELECT Код, Логин, Пароль, КодРоли FROM Пользователь WHERE Логин = @Login";

            // ИСПОЛЬЗУЕМ ТВОЙ МЕТОД GetConnection() из BaseRepository
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@Login", SqlDbType.VarChar).Value = login;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Id = (int)reader["Код"],
                                Login = (string)reader["Логин"],
                                Password = (string)reader["Пароль"],
                                IDRole = (int)reader["КодРоли"]
                            };
                        }
                    }
                }
            }
            return user;
        }
    }
}