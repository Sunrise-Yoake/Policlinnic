namespace Policlinnic.Domain.Entities
{
    public class Patient
    {
        public int ID { get; set; } // Код
        public string FullName { get; set; } = string.Empty; // ФИО
        public DateTime BirthDate { get; set; } // ДатаРождения
        public string Address { get; set; } = string.Empty; // Адрес
        public string Gender { get; set; } = string.Empty; // Пол
    }
}