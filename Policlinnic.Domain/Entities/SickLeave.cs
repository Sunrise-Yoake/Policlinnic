namespace Policlinnic.Domain.Entities
{
    public class SickLeave
    {
        public int ID { get; set; } // Код
        public int IDPatient { get; set; } // КодПациента
        public int IDDoctor { get; set; } // КодВрача
        public DateTime StartDate { get; set; } // ДатаНачала
        public DateTime EndDate { get; set; } // ДатаОкончания
    }
}