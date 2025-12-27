namespace Policlinnic.Domain.Entities
{
    public class User
    {
        public int Id { get; set; } // Код
        public string Login { get; set; } // Логин
        public string Password { get; set; } // Пароль (в БД здесь будет хеш)
        public int IDRole { get; set; } // КодРоли (1 - Админ, 2 - Врач, 3 - Пациент)
    }
}
