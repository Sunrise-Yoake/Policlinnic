namespace Policlinnic.Domain.Entities
{
    public class ExaminationPlan
    {
        public int ID { get; set; } // Код
        public int IDDiagnosis { get; set; } // КодДиагноза
        public string MedicalService { get; set; } = string.Empty; // МедУслуга
        public string Type { get; set; } = string.Empty; // Платная/Бесплатная
        public decimal? Cost { get; set; } // Стоимость
    }
}