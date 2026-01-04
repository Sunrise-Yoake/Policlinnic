namespace Policlinnic.Domain.Entities
{
    // Класс для результатов поиска врачей (аналог PatientLookupItem)
    public class DoctorLookupItem
    {
        public int Id { get; set; }
        public string DisplayText { get; set; } // Формат: "ФИО | Специализация"
    }
}
