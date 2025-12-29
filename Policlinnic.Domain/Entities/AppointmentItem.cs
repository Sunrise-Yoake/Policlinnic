using System;

namespace Policlinnic.Domain.Entities
{
    public class AppointmentItem
    {
        public int Id { get; set; }
        public DateTime DateVisit { get; set; }
        public string Cabinet { get; set; }
        public string DoctorName { get; set; } // ФИО (из таблицы Врачей)
        public string SpecName { get; set; }   // Специализация (из таблицы Спец)
        public string PatientName { get; set; }
        public int? PatientId { get; set; }

        public string DisplayDate => DateVisit.ToString("dd.MM.yyyy HH:mm");
    }
}