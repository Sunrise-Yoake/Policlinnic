using System;

namespace Policlinnic.Domain.Entities
{
    public class UserView
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string RoleName { get; set; }
        public string FIO { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }

        public string Address { get; set; }

        public int? Experience { get; set; } 

        public int? IDSpecialization { get; set; } 
    }
}