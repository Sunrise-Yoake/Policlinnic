using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using Policlinnic.Domain.Entities;

namespace Policlinnic.DAL.Repositories
{
    public class SickLeaveRepository : BaseRepository
    {
        // 1. Все записи (для Врача/Админа)
        public List<SickLeaveView> GetAllSickLeaves()
        {
            var list = new List<SickLeaveView>();
            // Сортировка в SQL: Сначала открытые (NULL), потом по дате
            string sql = "SELECT * FROM ViewSickLeaves ORDER BY CASE WHEN DateEnd IS NULL THEN 0 ELSE 1 END, DateStart DESC";

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) list.Add(Map(reader));
                }
            }
            return list;
        }

        // 2. Только свои (для Пациента)
        public List<SickLeaveView> GetByIDPatient(int idPatient)
        {
            var list = new List<SickLeaveView>();
            string sql = "PatientSickLeaves";

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDPatient", idPatient);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) list.Add(Map(reader));
                    }
                }
            }
            return list;
        }

        // 3. Получить список специализаций (Для фильтра)
        public List<string> GetSpecializationNames()
        {
            var list = new List<string>();
            // Если таблица называется по-другому, поправь тут. Обычно "Специализация".
            string sql = "SELECT DISTINCT Специализация FROM Специализация ORDER BY Специализация";

            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) list.Add(reader[0].ToString());
                    }
                }
            }
            catch { /* Если ошибка с таблицей, вернет пустой список, не уронив программу */ }
            return list;
        }

        // --- БЕЗОПАСНЫЙ МАППИНГ ---
        private SickLeaveView Map(SqlDataReader reader)
        {
            var item = new SickLeaveView
            {
                Id = (int)reader["Id"],
                IDPatient = (int)reader["IDPatient"],
                IDDoctor = (int)reader["IDDoctor"],
                PatientFIO = reader["PatientFIO"].ToString(),
                DoctorFIO = reader["DoctorFIO"].ToString(),

                // Сырая дата для сортировки
                RawDateStart = (DateTime)reader["DateStart"],
                // Красивая дата для отображения
                DateStart = ((DateTime)reader["DateStart"]).ToString("dd.MM.yyyy")
            };

            // !!! ИСПРАВЛЕНИЕ ОШИБКИ ВЫЛЕТА !!!
            if (reader["DateEnd"] == DBNull.Value)
            {
                item.DateEnd = "-";     // Если NULL, ставим прочерк
                item.IsOpen = true;     // И помечаем как "Открыт"
            }
            else
            {
                item.DateEnd = ((DateTime)reader["DateEnd"]).ToString("dd.MM.yyyy");
                item.IsOpen = false;
            }

            // Безопасное чтение специализации (если её нет в процедуре пациента - не упадем)
            if (ColumnExists(reader, "Spec"))
                item.SpecName = reader["Spec"].ToString();

            return item;
        }

        private bool ColumnExists(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase)) return true;
            }
            return false;
        }
    }
}