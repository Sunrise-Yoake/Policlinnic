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

        // Получение только свободных приёмов 
        public IEnumerable<Appointment> GetAvailableSlots()
        {
            return _appointmentRepository.GetFreeAppointments();
        }

        // Логика записи
        public bool MakeAppointment(int appointmentId, int patientId)
        {
            // Бизнес-проверка: ID пациента должен быть положительным 
            if (patientId <= 0) return false;

            return _appointmentRepository.BookAppointment(appointmentId, patientId);
        }
    }
}