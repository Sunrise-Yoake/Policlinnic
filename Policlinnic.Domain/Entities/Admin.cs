using System;

namespace Policlinnic.Domain.Entities
{
    public class Admin
    {
        public int IdAdmin { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; } = string.Empty;
        public int Experience { get; set; }
    }
}
