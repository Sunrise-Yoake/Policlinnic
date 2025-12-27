using System.Data;
using Microsoft.Data.SqlClient;
using Policlinnic.Domain.Entities;

namespace Policlinnic.DAL.Repositories
{
    public class AdminRepository : BaseRepository
    {
        // Метод для получения данных админа по его ID (который совпадает с ID пользователя)
        public Admin GetAdminById(int id)
        {
            Admin admin = null;
            string sql = "SELECT * FROM Администратор WHERE КодАдмина = @Id";

            using (SqlConnection conn = GetConnection()) // Используем метод из BaseRepository
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            admin = new Admin
                            {
                                IdAdmin = (int)reader["КодАдмина"],
                                FullName = (string)reader["ФИО"],
                                BirthDate = (DateTime)reader["ДатаРождения"],
                                Gender = (string)reader["Пол"],
                                Experience = (int)reader["Стаж"]
                            };
                        }
                    }
                }
            }
            return admin;
        }
    }
}