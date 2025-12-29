using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Policlinnic.Domain.Entities;

namespace Policlinnic.DAL.Repositories
{
    // Исправил наследование на BaseRepository, как ты просил
    public class DictionaryRepository : BaseRepository
    {
        // 1. ЛЕКАРСТВА
        public List<Medicine> GetAllMedicines()
        {
            var list = new List<Medicine>();
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = "SELECT Код, НаименованиеЛекарства, ЗависимостьОтЕды FROM Лекарство ORDER BY НаименованиеЛекарства";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Medicine
                        {
                            ID = (int)reader["Код"],
                            Name = reader["НаименованиеЛекарства"].ToString(),
                            FoodDependency = reader["ЗависимостьОтЕды"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        // 2. БОЛЕЗНИ
        public List<Illness> GetAllIllnesses()
        {
            var list = new List<Illness>();
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = "SELECT Код, Название, ДопПримечания FROM Болезнь ORDER BY Название";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Illness
                        {
                            ID = (int)reader["Код"],
                            Name = reader["Название"].ToString(),
                            Notes = reader["ДопПримечания"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        // 3. СПЕЦИАЛИЗАЦИИ
        public List<Specialization> GetAllSpecializations()
        {
            var list = new List<Specialization>();
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = "SELECT Код, Специализация FROM Специализация ORDER BY Специализация";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Specialization
                        {
                            ID = (int)reader["Код"],
                            Name = reader["Специализация"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        public void DeleteEntity(string tableName, int id)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                // Для защиты от SQL Injection в реальных проектах имена таблиц так не передают, 
                // но для курсового/лабы это допустимо.
                string sql = $"DELETE FROM {tableName} WHERE Код = @Id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }


        // --- ЛЕКАРСТВА (Добавление / Обновление) ---
        public void AddMedicine(string name, string foodDep)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = "INSERT INTO Лекарство (НаименованиеЛекарства, ЗависимостьОтЕды) VALUES (@Name, @Food)";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Food", foodDep);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateMedicine(int id, string name, string foodDep)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Лекарство SET НаименованиеЛекарства = @Name, ЗависимостьОтЕды = @Food WHERE Код = @Id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Food", foodDep);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // --- БОЛЕЗНИ (Добавление / Обновление) ---
        public void AddIllness(string name, string notes)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = "INSERT INTO Болезнь (Название, ДопПримечания) VALUES (@Name, @Notes)";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Notes", notes);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateIllness(int id, string name, string notes)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Болезнь SET Название = @Name, ДопПримечания = @Notes WHERE Код = @Id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Notes", notes);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // --- СПЕЦИАЛИЗАЦИИ (Добавление / Обновление) ---
        public void AddSpecialization(string name)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = "INSERT INTO Специализация (Специализация) VALUES (@Name)";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateSpecialization(int id, string name)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Специализация SET Специализация = @Name WHERE Код = @Id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}