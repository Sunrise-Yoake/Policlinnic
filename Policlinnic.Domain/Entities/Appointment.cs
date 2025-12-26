namespace Policlinnic.Domain.Entities
{
    public class Appointment
    {
        public int ID { get; set; } // Код
        public int? IDPatient { get; set; } // КодПациента (может быть NULL)
        public int IDDoctor { get; set; } // КодВрача
        public DateTime DateAndTime { get; set; } // ДатаиВремя
        public string Office { get; set; } = string.Empty; // Кабинет
    }
}