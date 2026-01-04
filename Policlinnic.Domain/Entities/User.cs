namespace Policlinnic.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int IDRole { get; set; }
        public string RoleName { get; set; }
        public string Phone { get; set; }
    }
}