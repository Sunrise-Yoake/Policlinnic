namespace Policlinnic.Domain.Entities
{
    public class Treatment
    {
        public int ID { get; set; } // Код
        public int IDMedicine { get; set; } // КодЛекарства
        public int IDDiagnosis { get; set; } // КодДиагностирования
        public string Regimen { get; set; } = string.Empty; // Режим
        public int Dosage { get; set; } // Дозировка
    }
}