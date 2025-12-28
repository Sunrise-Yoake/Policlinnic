using Microsoft.Data.SqlClient;
using Policlinnic.Domain.Entities;
using System.Data;

namespace Policlinnic.DAL.Repositories
{
    public class UserRepository : BaseRepository
    {
        public User GetUserByLogin(string login)
        {
            User user = null;

            // Добавляем Телефон в SELECT
            string sql = "SELECT Код, Логин, Пароль, КодРоли, Телефон FROM Пользователь WHERE Логин = @Login";

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
                                IDRole = (int)reader["КодРоли"],
                                Phone = reader["Телефон"] as string
                            };
                        }
                    }
                }
            }
            return user;
        }
    }
}