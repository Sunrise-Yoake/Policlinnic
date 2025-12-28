using Policlinnic.DAL.Repositories;
using Policlinnic.Domain.Entities;

namespace Policlinnic.BLL.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;

        public AuthService()
        {
            _userRepository = new UserRepository();
        }

        public User Login(string login, string password)
        {
            var user = _userRepository.GetUserByLogin(login);

            if (user == null) return null;

            string inputHash = PasswordHasher.Hash(password);

            if (user.Password == inputHash)
            {
                return user;
            }

            return null;
        }
    }
}