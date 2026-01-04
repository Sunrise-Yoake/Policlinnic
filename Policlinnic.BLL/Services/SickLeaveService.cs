using System;
using System.Collections.Generic;
using Policlinnic.DAL.Repositories;
using Policlinnic.Domain.Entities;

namespace Policlinnic.BLL.Services
{
    public class SickLeaveService
    {
        private readonly SickLeaveRepository _repo = new SickLeaveRepository();

        public List<SickLeaveView> GetList(User user)
        {
            // 3 - это ID роли Пациента
            if (user.IDRole == 3)
                return _repo.GetByPatient(user.Id);

            return _repo.GetAll();
        }

        public List<string> GetSpecializations()
        {
            return _repo.GetSpecs();
        }

        public void Delete(int id, User user)
        {
            if (user.IDRole == 3)
                throw new Exception("Пациенты не могут удалять больничные листы.");

            try
            {
                _repo.Delete(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при удалении. Возможно, запись уже удалена или используется.", ex);
            }
        }

        // Заготовка на будущее для Add/Edit
        public void Save(SickLeave item)
        {
            if (item.DateEnd.HasValue && item.DateEnd < item.DateStart)
                throw new Exception("Дата окончания не может быть раньше начала!");

            if (item.Id == 0) _repo.Add(item);
            else _repo.Update(item);
        }
        public List<LookupItem> GetPatients() => _repo.GetPatientsLookup();
        public List<LookupItem> GetDoctors() => _repo.GetDoctorsLookup();
    }
}