using Microsoft.Data.SqlClient;
using Policlinnic.Domain.Entities;
using System.Collections.Generic;
using System.Data;

namespace Policlinnic.DAL.Repositories
{
    public class UserRepository : BaseRepository
    {
        public User GetUserByLogin(string login)
        {
            User user = null;
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

        public List<UserView> GetAllUsers()
        {
            var list = new List<UserView>();

            // Выбираем всё из представления
            string sql = "SELECT * FROM ViewFirstPage ORDER BY RoleName, FIO";

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                           
                            var userView = new UserView
                            {
                                Id = (int)reader["Id"],
                                Login = reader["Login"].ToString(),
                                Password = reader["Password"].ToString(),
                                Phone = reader["Phone"] == System.DBNull.Value ? "" : reader["Phone"].ToString(),
                                RoleName = reader["RoleName"].ToString(),
                                FIO = reader["FIO"].ToString()
                            };

                            list.Add(userView);
                        }
                    }
                }
            }
            return list;
        }

        // Метод добавления пользователя вместе с профилем
        public void AddUserWithProfile(UserView user)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                // Начинаем транзакцию: всё выполнится или всё отменится
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Вставка в таблицу ПОЛЬЗОВАТЕЛЬ
                        // SCOPE_IDENTITY() возвращает ID, который только что создался
                        string sqlUser = @"INSERT INTO Пользователь (Логин, Пароль, Телефон, КодРоли) 
                                   VALUES (@Login, @Password, @Phone, 
                                          (SELECT Код FROM Роль WHERE КодНазвания = @RoleName));
                                   SELECT SCOPE_IDENTITY();";

                        int newUserId;
                        using (var cmd = new SqlCommand(sqlUser, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Login", user.Login);
                            cmd.Parameters.AddWithValue("@Password", user.Password); // Не забудь хэшировать перед вызовом!
                            cmd.Parameters.AddWithValue("@Phone", user.Phone);
                            cmd.Parameters.AddWithValue("@RoleName", user.RoleName); // "Врач", "Пациент"

                            // Получаем ID нового пользователя
                            newUserId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // 2. Вставка в таблицу ПРОФИЛЯ (В зависимости от роли)
                        string sqlProfile = "";

                        // ...
                        if (!string.IsNullOrEmpty(sqlProfile))
                        {
                            using (var cmd = new SqlCommand(sqlProfile, conn, transaction))
                            {
                                // Общие параметры для всех профилей
                                cmd.Parameters.AddWithValue("@Id", newUserId);
                                cmd.Parameters.AddWithValue("@FIO", user.FIO);
                                cmd.Parameters.AddWithValue("@Dob", DateTime.Parse(user.DateOfBirth));
                                cmd.Parameters.AddWithValue("@Gender", user.Gender);

                                // ПАЦИЕНТ: Только адрес
                                if (user.RoleName == "Пациент")
                                {
                                    cmd.Parameters.AddWithValue("@Address", user.Address);
                                }

                                // АДМИН и ВРАЧ: Стаж есть у обоих
                                if (user.RoleName == "Врач" || user.RoleName == "Админ")
                                {
                                    cmd.Parameters.AddWithValue("@Exp", user.Experience ?? 0); // Защита от null
                                }

                                // ВРАЧ: Только у него есть Специализация
                                if (user.RoleName == "Врач")
                                {
                                    // Если SpecializationId не выбран (0 или null), ставим 1 (Терапевт) или кидаем ошибку
                                    int specId = user.IDSpecialization > 0 ? user.IDSpecialization.Value : 1;
                                    cmd.Parameters.AddWithValue("@SpecId", specId);
                                }

                                cmd.ExecuteNonQuery();
                            }
                        }
                        // Если всё прошло успешно — подтверждаем
                        transaction.Commit();
                    }
                    catch
                    {
                        // Если ошибка — отменяем всё
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        // Простой класс для специализации
        public class Specialization
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        // Метод в UserRepository
        public List<Specialization> GetSpecializations()
        {
            var list = new List<Specialization>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT Код, Специализация FROM Специализация", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Specialization
                        {
                            Id = (int)reader["Код"],
                            Name = reader["Специализация"].ToString()
                        });
                    }
                }
            }
            return list;
        }

    }
}