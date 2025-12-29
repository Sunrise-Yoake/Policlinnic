using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Policlinnic.Domain.Entities;

namespace Policlinnic.DAL.Repositories
{
    public class ReportRepository : BaseRepository
    {
        // ПОИСК ПАЦИЕНТОВ (для автодополнения)
        public List<PatientLookupItem> SearchPatients(string name, string phone)
        {
            var list = new List<PatientLookupItem>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SearchPatients", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    // Передаем NULL, если строка пустая
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

        // 2. Отчет: Платные услуги (View)
        public List<PaidServiceReportItem> GetPaidServicesReport()
        {
            var list = new List<PaidServiceReportItem>();
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM ViewPaidServicesReport ORDER BY TotalIncome DESC";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new PaidServiceReportItem
                        {
                            DoctorName = reader["DoctorName"].ToString(),
                            Spec = reader["Spec"].ToString(),
                            ServiceName = reader["ServiceName"].ToString(),
                            CountServices = (int)reader["CountServices"],
                            TotalIncome = (decimal)reader["TotalIncome"]
                        });
                    }
                }
            }
            return list;
        }
        // Получить общую сумму дохода (скалярная функция)
        public decimal GetTotalIncome()
        {
            decimal total = 0;
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT dbo.GetTotalIncome()", conn))
                {
                    var result = cmd.ExecuteScalar(); // Выполняет запрос и возвращает 1-ю ячейку
                    if (result != null && result != DBNull.Value)
                    {
                        total = (decimal)result;
                    }
                }
            }
            return total;
        }

        // 3. Отчет: История пациента (Function)
        public List<PatientHistoryItem> GetPatientHistory(int patientId)
        {
            var list = new List<PatientHistoryItem>();
            using (var conn = GetConnection())
            {
                conn.Open();
                // Вызов функции через SELECT * FROM
                string sql = "SELECT * FROM GetPatientHistory(@Id) ORDER BY DateVisit DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", patientId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new PatientHistoryItem
                            {
                                DateVisit = (DateTime)reader["DateVisit"],
                                DoctorName = reader["DoctorName"].ToString(),
                                Spec = reader["Spec"].ToString(),
                                IllnessName = reader["IllnessName"].ToString(),
                                Diagnosis = reader["Diagnosis"].ToString(),
                                Medicine = reader["Medicine"] == DBNull.Value ? "-" : reader["Medicine"].ToString(),
                                TreatmentInfo = reader["TreatmentInfo"] == DBNull.Value ? "-" : reader["TreatmentInfo"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }
    }
}