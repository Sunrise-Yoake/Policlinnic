using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using Policlinnic.Domain.Entities;

namespace Policlinnic.DAL.Repositories
{
    /// <summary>
    /// Репозиторий для функций быстрого поиска и автодополнения во всей системе
    /// </summary>
    public class LookupRepository : BaseRepository
    {
        // ПОИСК ПАЦИЕНТОВ (Новая копия для использования в формах)
        public List<PatientLookupItem> SearchPatientsLookup(string name, string phone)
        {
            var list = new List<PatientLookupItem>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SearchPatients", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NamePart", string.IsNullOrWhiteSpace(name) ? (object)DBNull.Value : name);
                    cmd.Parameters.AddWithValue("@PhonePart", string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new PatientLookupItem
                            {
                                Id = (int)reader["Id"],
                                DisplayText = $"{reader["FIO"]} | {reader["Phone"]}"
                            });
                        }
                    }
                }
            }
            return list;
        }

        // ПОИСК ВРАЧЕЙ (Новая функция для использования в формах)
        public List<DoctorLookupItem> SearchDoctorsLookup(string name, string spec)
        {
            var list = new List<DoctorLookupItem>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SearchDoctors", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NamePart", string.IsNullOrWhiteSpace(name) ? (object)DBNull.Value : name);
                    cmd.Parameters.AddWithValue("@SpecPart", string.IsNullOrWhiteSpace(spec) ? (object)DBNull.Value : spec);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new DoctorLookupItem
                            {
                                Id = (int)reader["Id"],
                                DisplayText = $"{reader["FIO"]} | {reader["SpecName"]}"
                            });
                        }
                    }
                }
            }
            return list;
        }
    }
}