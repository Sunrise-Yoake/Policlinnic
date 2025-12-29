using System;

namespace Policlinnic.Domain.Entities
{
    public class StatModel
    {
        public DateTime Month { get; set; }
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }
        public int TotalCount { get; set; }

        public string MonthName => Month.ToString("MMMM yyyy");
    }
}