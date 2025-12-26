namespace Policlinnic.Domain.Entities
{
    public class Doctor
    {
        public int ID { get; set; } // Код
        public int IDSpecialization { get; set; } // КодСпециализации
        public string FullName { get; set; } = string.Empty; // ФИО
        public DateTime BirthDate { get; set; } // ДатаРождения
        public string Gender { get; set; } = string.Empty; // Пол
        public int Experience { get; set; } // Стаж
    }
}