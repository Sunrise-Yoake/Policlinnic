using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using Policlinnic.Domain.Entities;

namespace Policlinnic.DAL.Repositories
{
    // Убедись, что BaseRepository содержит метод GetConnection()
    public class UserRepository : BaseRepository
    {
        public User GetUserByLogin(string login)
        {
            User user = null;
            string sql = "SELECT Код, Логин, Пароль, КодРоли, Телефон FROM Пользователь WHERE Логин = @Login";

            using (SqlConnection conn = GetConnection()) // Теперь GetConnection будет виден
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
                            list.Add(new UserView
                            {
                                Id = (int)reader["Id"],
                                Login = reader["Login"].ToString(),
                                // Пароль обычно не тянем во вьюху для безопасности, 
                                // но если нужно для редактирования:
                                Password = reader["Password"]?.ToString(),
                                Phone = reader["Phone"]?.ToString(),
                                RoleName = reader["RoleName"].ToString(),
                                FIO = reader["FIO"].ToString(),
                                Gender = reader["Gender"]?.ToString(),
                                Address = reader["Address"]?.ToString(),

                                // ИСПРАВЛЕНИЕ ДАТЫ: Конвертируем из БД в строку нужного формата
                                DateOfBirth = reader["DateOfBirth"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["DateOfBirth"]).ToString("yyyy-MM-dd")
                                    : string.Empty,

                                // ИСПРАВЛЕНИЕ СТАЖА: Проверяем на null, так как тип int?
                                Experience = reader["Experience"] != DBNull.Value
                                    ? Convert.ToInt32(reader["Experience"])
                                    : (int?)null,

                                // ИСПРАВЛЕНИЕ СПЕЦИАЛИЗАЦИИ:
                                IDSpecialization = reader["IDSpecialization"] != DBNull.Value
                                    ? Convert.ToInt32(reader["IDSpecialization"])
                                    : (int?)null
                            });
                        }
                    }
                }
            }
            return list;
        }

        public List<LookupItem> GetSpecializations()
        {
            var list = new List<LookupItem>();
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                // Исправлено: используем имя колонки 'Специализация' вместо 'Название'
                string sql = "SELECT Код, Специализация FROM Специализация ORDER BY Специализация";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new LookupItem
                        {
                            Id = Convert.ToInt32(r["Код"]),
                            Name = r["Специализация"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        // Метод заглушка для компиляции (реализуем позже если нужно)
        public void AddUserWithProfile(UserView user)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Получаем ID роли (считаем, что в RoleName пришло "Врач", "Пациент" и т.д.)
                        string roleSql = "SELECT Код FROM Роль WHERE Название = @RoleName";
                        SqlCommand roleCmd = new SqlCommand(roleSql, conn, trans);
                        roleCmd.Parameters.AddWithValue("@RoleName", user.RoleName);
                        int roleId = Convert.ToInt32(roleCmd.ExecuteScalar());

                        // 2. Вставка в таблицу Пользователь
                        string userSql = @"INSERT INTO Пользователь (КодРоли, Телефон, Логин, Пароль) 
                                   VALUES (@Rid, @Tel, @Log, @Pas); 
                                   SELECT SCOPE_IDENTITY();";
                        SqlCommand userCmd = new SqlCommand(userSql, conn, trans);
                        userCmd.Parameters.AddWithValue("@Rid", roleId);
                        userCmd.Parameters.AddWithValue("@Tel", user.Phone);
                        userCmd.Parameters.AddWithValue("@Log", user.Login);
                        userCmd.Parameters.AddWithValue("@Pas", user.Password); // В идеале хешировать

                        int newId = Convert.ToInt32(userCmd.ExecuteScalar());
                        user.Id = newId; // Запоминаем новый ID

                        // 3. Вставка в таблицу профиля
                        InsertProfile(user, conn, trans);

                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public void UpdateUserWithProfile(UserView user)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Обновляем базовую таблицу
                        string userSql = "UPDATE Пользователь SET Телефон = @Tel, Логин = @Log WHERE Код = @Id";
                        SqlCommand userCmd = new SqlCommand(userSql, conn, trans);
                        userCmd.Parameters.AddWithValue("@Tel", user.Phone);
                        userCmd.Parameters.AddWithValue("@Log", user.Login);
                        userCmd.Parameters.AddWithValue("@Id", user.Id);
                        userCmd.ExecuteNonQuery();

                        // 2. Обновляем таблицу профиля (сначала удаляем старый профиль или просто UPDATE)
                        // Безопаснее сделать UPDATE по конкретной таблице
                        UpdateProfile(user, conn, trans);

                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        // Приватные методы-помощники, чтобы не дублировать код
        private void InsertProfile(UserView user, SqlConnection conn, SqlTransaction trans)
        {
            string sql = user.RoleName switch
            {
                "Врач" => "INSERT INTO Врач (КодВрача, КодСпециализации, ФИО, ДатаРождения, Пол, Стаж) VALUES (@Id, @Spec, @Fio, @Date, @Gen, @Exp)",
                "Пациент" => "INSERT INTO Пациент (КодПациента, ФИО, ДатаРождения, Адрес, Пол) VALUES (@Id, @Fio, @Date, @Adr, @Gen)",
                "Администратор" => "INSERT INTO Администратор (КодАдмина, ФИО, ДатаРождения, Пол, Стаж) VALUES (@Id, @Fio, @Date, @Gen, @Exp)",
                _ => throw new Exception("Неизвестная роль")
            };

            SqlCommand cmd = new SqlCommand(sql, conn, trans);
            AddProfileParameters(cmd, user);
            cmd.ExecuteNonQuery();
        }

        private void UpdateProfile(UserView user, SqlConnection conn, SqlTransaction trans)
        {
            string sql = user.RoleName switch
            {
                "Врач" => "UPDATE Врач SET КодСпециализации=@Spec, ФИО=@Fio, ДатаРождения=@Date, Пол=@Gen, Стаж=@Exp WHERE КодВрача=@Id",
                "Пациент" => "UPDATE Пациент SET ФИО=@Fio, ДатаРождения=@Date, Адрес=@Adr, Пол=@Gen WHERE КодПациента=@Id",
                "Администратор" => "UPDATE Администратор SET ФИО=@Fio, ДатаРождения=@Date, Пол=@Gen, Стаж=@Exp WHERE КодАдмина=@Id",
                _ => throw new Exception("Неизвестная роль")
            };

            SqlCommand cmd = new SqlCommand(sql, conn, trans);
            AddProfileParameters(cmd, user);
            cmd.ExecuteNonQuery();
        }

        private void AddProfileParameters(SqlCommand cmd, UserView user)
        {
            cmd.Parameters.AddWithValue("@Id", user.Id);
            cmd.Parameters.AddWithValue("@Fio", user.FIO);
            cmd.Parameters.AddWithValue("@Date", DateTime.Parse(user.DateOfBirth));
            cmd.Parameters.AddWithValue("@Gen", user.Gender);

            if (user.RoleName == "Врач")
            {
                cmd.Parameters.AddWithValue("@Spec", user.IDSpecialization);
                cmd.Parameters.AddWithValue("@Exp", user.Experience);
            }
            else if (user.RoleName == "Пациент")
            {
                cmd.Parameters.AddWithValue("@Adr", user.Address);
            }
            else if (user.RoleName == "Администратор")
            {
                cmd.Parameters.AddWithValue("@Exp", user.Experience);
            }
        }
    }
}