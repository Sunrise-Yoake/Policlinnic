using System;

namespace Policlinnic.Domain.Entities
{
    public class SickLeave
    {
        public int Id { get; set; }           // Было ID
        public int IDPatient { get; set; }
        public int IDDoctor { get; set; }
        public DateTime DateStart { get; set; } // Было StartDate
        public DateTime? DateEnd { get; set; }  // Было EndDate (сделали nullable)
    }

    // Вспомогательный класс для выпадающих списков (Пациенты/Врачи)
    public class LookupItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}