namespace Policlinnic.Domain.Entities
{
    public class Medicine
    {
        public int ID { get; set; } // Код
        public string Name { get; set; } = string.Empty; // НаименованиеЛекарства
        public string FoodDependency { get; set; } = string.Empty; // ЗависимостьОтЕды
    }
}