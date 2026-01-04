using System;
using System.Collections.Generic;
using Policlinnic.DAL.Repositories;
using Policlinnic.Domain.Entities;

namespace Policlinnic.BLL.Services
{
    public class UserService
    {
        private readonly UserRepository _repository = new UserRepository();

        // Получение всех пользователей (из ViewFirstPage)
        public List<UserView> GetUsers()
        {
            return _repository.GetAllUsers();
        }

        // Получение специализаций для врачей
        public List<LookupItem> GetSpecializations()
        {
            return _repository.GetSpecializations();
        }

        // Основной метод сохранения
        public void SaveUser(UserView user, string plainPassword = null)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(user.Login)) throw new Exception("Логин не может быть пустым");
            if (string.IsNullOrWhiteSpace(user.FIO)) throw new Exception("ФИО обязательно для заполнения");

            if (user.Id == 0) // Создание нового
            {
                if (string.IsNullOrWhiteSpace(plainPassword))
                    throw new Exception("Для нового пользователя нужен пароль");

                // Здесь можно добавить хеширование: user.Password = Hash(plainPassword);
                user.Password = plainPassword;

                _repository.AddUserWithProfile(user);
            }
            else // Редактирование существующего
            {
                _repository.UpdateUserWithProfile(user);
            }
        }
    }
}