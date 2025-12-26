using Microsoft.Data.SqlClient;
using Policlinnic.Domain.Entities;
using System.Collections.Generic;
using System.Data;

namespace Policlinnic.DAL.Repositories
{
    public class PatientRepository : BaseRepository
    {
        // Метод для получения всех пациентов 
        public IEnumerable<Patient> GetAll()
        {
            var patients = new List<Patient>();

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "SELECT ID, ФИО, ДатаРождения, Адрес, Пол FROM Пациент"; // SQL-запрос

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        patients.Add(new Patient
                        {
                            ID = (int)reader["ID"],
                            FullName = reader["ФИО"].ToString() ?? string.Empty,
                            BirthDate = (DateTime)reader["ДатаРождения"],
                            Address = reader["Адрес"].ToString() ?? string.Empty,
                            Gender = reader["Пол"].ToString() ?? string.Empty
                        });
                    }
                }
            }
            return patients;
        }
    }
}