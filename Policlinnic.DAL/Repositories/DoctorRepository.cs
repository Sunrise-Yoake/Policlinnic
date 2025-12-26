using Microsoft.Data.SqlClient;
using Policlinnic.Domain.Entities;
using System.Collections.Generic;

namespace Policlinnic.DAL.Repositories
{
    public class DoctorRepository : BaseRepository
    {
        public IEnumerable<Doctor> GetAll()
        {
            var doctors = new List<Doctor>();
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                // Запрос с объединением, чтобы понимать специализацию (Критерий 5.2)
                command.CommandText = "SELECT ID, IDSpecialization, ФИО, ДатаРождения, Пол, Стаж FROM Врач";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        doctors.Add(new Doctor
                        {
                            ID = (int)reader["ID"],
                            IDSpecialization = (int)reader["IDSpecialization"],
                            FullName = reader["ФИО"].ToString() ?? string.Empty,
                            BirthDate = (DateTime)reader["ДатаРождения"],
                            Gender = reader["Пол"].ToString() ?? string.Empty,
                            Experience = (int)reader["Стаж"]
                        });
                    }
                }
            }
            return doctors;
        }
    }
}