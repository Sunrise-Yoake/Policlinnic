using System;

namespace Policlinnic.Domain.Entities
{
    public class UserLogItem
    {
        public int Id { get; set; }
        public string Operation { get; set; }
        public DateTime Date { get; set; }
        public string SystemUser { get; set; }
        public string Login { get; set; }
        public bool IsArchived { get; set; }
    }
}