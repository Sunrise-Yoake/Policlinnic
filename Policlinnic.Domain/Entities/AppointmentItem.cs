using System;

namespace Policlinnic.Domain.Entities
{
    public class AppointmentItem
    {
        public int Id { get; set; }
        public DateTime DateVisit { get; set; }
        public string Cabinet { get; set; }

        public int DoctorId { get; set; }
        public string DoctorName { get; set; }

        public string SpecName { get; set; }
        public string PatientName { get; set; }
        public int? PatientId { get; set; }

        public string DisplayDate => DateVisit.ToString("dd.MM.yyyy HH:mm");
    }
}