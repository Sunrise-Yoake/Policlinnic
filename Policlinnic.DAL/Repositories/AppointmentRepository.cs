using Microsoft.Data.SqlClient;
using Policlinnic.Domain.Entities;
using System.Collections.Generic;
using System.Data;

namespace Policlinnic.DAL.Repositories
{
    public class AppointmentRepository : BaseRepository
    {
        // 1. Метод для получения свободных слотов (где пациента еще нет)
        // Это нужно, чтобы пациент мог увидеть, куда можно записаться
        public IEnumerable<Appointment> GetFreeAppointments()
        {
            var appointments = new List<Appointment>();

            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                // Ищем записи, где КодПациента (IDPatient) равен NULL
                command.CommandText = "SELECT ID, IDDoctor, ДатаиВремя, Кабинет FROM Приём WHERE КодПациента IS NULL";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        appointments.Add(new Appointment
                        {
                            ID = (int)reader["ID"],
                            IDDoctor = (int)reader["IDDoctor"],
                            DateAndTime = (DateTime)reader["ДатаиВремя"],
                            Office = reader["Кабинет"].ToString() ?? string.Empty
                        });
                    }
                }
            }
            return appointments;
        }

        // 2. Метод для записи пациента на приём (UPDATE существующей строки)
        // Выполняет требование критерия 9.8 (модификация данных)
        public bool BookAppointment(int idAppointment, int idPatient)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand())
            {
                connection.Open();
                command.Connection = connection;
                command.CommandText = "UPDATE Приём SET КодПациента = @patientId WHERE ID = @appointmentId";

                command.Parameters.Add("@patientId", SqlDbType.Int).Value = idPatient;
                command.Parameters.Add("@appointmentId", SqlDbType.Int).Value = idAppointment;

                return command.ExecuteNonQuery() > 0;
            }
        }
    }
}