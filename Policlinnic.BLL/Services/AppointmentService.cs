using Policlinnic.DAL.Repositories;
using Policlinnic.Domain.Entities;
using System.Collections.Generic;

namespace Policlinnic.BLL.Services
{
    public class AppointmentService
    {
        private readonly AppointmentRepository _appointmentRepository;

        public AppointmentService()
        {
            _appointmentRepository = new AppointmentRepository();
        }

        // Получение только свободных приёмов (Критерий 7.6)
        public IEnumerable<Appointment> GetAvailableSlots()
        {
            return _appointmentRepository.GetFreeAppointments();
        }

        // Логика записи (Критерий 9.8)
        public bool MakeAppointment(int appointmentId, int patientId)
        {
            // Бизнес-проверка: ID пациента должен быть положительным (Критерий 6.3)
            if (patientId <= 0) return false;

            return _appointmentRepository.BookAppointment(appointmentId, patientId);
        }
    }
}