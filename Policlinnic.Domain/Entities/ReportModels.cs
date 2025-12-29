namespace Policlinnic.Domain.Entities
{
    // Для отчета по платным услугам
    public class PaidServiceReportItem
    {
        public string? DoctorName { get; set; }
        public string? Spec { get; set; }
        public string? ServiceName { get; set; }
        public int CountServices { get; set; }
        public decimal TotalIncome { get; set; }
    }

    // Для истории болезни
    public class PatientHistoryItem
    {
        public System.DateTime DateVisit { get; set; }
        public string? DoctorName { get; set; }
        public string? Spec { get; set; }
        public string? IllnessName { get; set; }
        public string? Diagnosis { get; set; }
        public string? Medicine { get; set; }
        public string? TreatmentInfo { get; set; }
    }

    // Вспомогательный класс для выпадающего списка выбора пациента
    public class PatientLookupItem
    {
        public int Id { get; set; }
        public string? DisplayText { get; set; } 
    }
}