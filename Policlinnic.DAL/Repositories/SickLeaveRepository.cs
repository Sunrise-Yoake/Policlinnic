using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using Policlinnic.Domain.Entities;

namespace Policlinnic.DAL.Repositories
{
    public class SickLeaveRepository : BaseRepository
    {
        public List<SickLeaveView> GetAll()
        {
            // Сортируем: Сначала открытые, потом самые новые по дате начала
            string sql = "SELECT * FROM ViewSickLeaves ORDER BY CASE WHEN DateEnd IS NULL THEN 0 ELSE 1 END, DateStart DESC";
            return ExecuteQuery(sql);
        }

        public List<SickLeaveView> GetByPatient(int patientId)
        {
            // Используем твою процедуру
            var list = new List<SickLeaveView>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("PatientSickLeaves", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDPatient", patientId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) list.Add(Map(reader));
                    }
                }
            }
            return list;
        }

        public List<string> GetSpecs()
        {
            var list = new List<string>();
            try
            {
                string sql = "SELECT DISTINCT Специализация FROM Специализация ORDER BY Специализация";
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read()) list.Add(reader[0].ToString());
                }
            }
            catch { /* Игнорируем ошибки загрузки фильтров */ }
            return list;
        }

        // --- ИЗМЕНЕНИЕ ДАННЫХ ---

        public void Add(SickLeave item)
        {
            string sql = @"INSERT INTO Больничный (КодПациента, КодВрача, ДатаНачала, ДатаОкончания) 
                           VALUES (@p, @d, @start, @end)";
            ExecuteNonQ(sql, item);
        }

        public void Update(SickLeave item)
        {
            string sql = @"UPDATE Больничный 
                           SET КодПациента=@p, КодВрача=@d, ДатаНачала=@start, ДатаОкончания=@end 
                           WHERE Код=@id";
            ExecuteNonQ(sql, item);
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM Больничный WHERE Код = @id";
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // --- ВСПОМОГАТЕЛЬНЫЕ ---

        private void ExecuteNonQ(string sql, SickLeave item)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@p", item.IDPatient);
                    cmd.Parameters.AddWithValue("@d", item.IDDoctor);
                    cmd.Parameters.AddWithValue("@start", item.DateStart);
                    cmd.Parameters.AddWithValue("@end", (object)item.DateEnd ?? DBNull.Value);
                    if (item.Id > 0) cmd.Parameters.AddWithValue("@id", item.Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private List<SickLeaveView> ExecuteQuery(string sql)
        {
            var list = new List<SickLeaveView>();
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

        private SickLeaveView Map(SqlDataReader reader)
        {
            var item = new SickLeaveView
            {
                Id = (int)reader["Id"],
                IDPatient = (int)reader["IDPatient"],
                IDDoctor = (int)reader["IDDoctor"],
                PatientFIO = reader["PatientFIO"].ToString(),
                DoctorFIO = reader["DoctorFIO"].ToString(),
                RawDateStart = (DateTime)reader["DateStart"],
                DateStart = ((DateTime)reader["DateStart"]).ToString("dd.MM.yyyy")
            };

            if (reader["DateEnd"] == DBNull.Value)
            {
                item.DateEnd = "—";
                item.IsOpen = true;
            }
            else
            {
                item.DateEnd = ((DateTime)reader["DateEnd"]).ToString("dd.MM.yyyy");
                item.IsOpen = false;
            }

            // Проверка, есть ли колонка Spec в выборке (в процедуре пациента её может не быть)
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i) == "Spec")
                {
                    item.SpecName = reader["Spec"].ToString();
                    break;
                }
            }

            return item;
        }

        // Метод для получения списка пациентов для ComboBox
        public List<LookupItem> GetPatientsLookup()
        {
            var list = new List<LookupItem>();
            string sql = "SELECT КодПациента, ФИО FROM Пациент ORDER BY ФИО";
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new LookupItem
                        {
                            Id = (int)reader["КодПациента"],
                            Name = reader["ФИО"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        // Метод для получения списка врачей для ComboBox
        public List<LookupItem> GetDoctorsLookup()
        {
            var list = new List<LookupItem>();
            string sql = "SELECT КодВрача, ФИО FROM Врач ORDER BY ФИО";
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new LookupItem
                        {
                            Id = (int)reader["КодВрача"],
                            Name = reader["ФИО"].ToString()
                        });
                    }
                }
            }
            return list;
        }
    }
}