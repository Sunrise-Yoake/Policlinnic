namespace Policlinnic.Domain.Entities
{
    public class Diagnosis
    {
        public int ID { get; set; } // Код
        public int IDAppointment { get; set; } // КодПриёма
        public int IDIllness { get; set; } // КодБолезни
        public string Name { get; set; } = string.Empty; // НазваниеДиагноза
        public string Description { get; set; } = string.Empty; // Описание
    }
}