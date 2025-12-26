namespace Policlinnic.Domain.Entities
{
    public class Illness
    {
        public int ID { get; set; } // Код
        public string Name { get; set; } = string.Empty; // Название
        public string Notes { get; set; } = string.Empty; // ДопПримечания
    }
}