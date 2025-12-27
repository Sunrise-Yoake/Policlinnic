using Policlinnic.DAL.Repositories;
using Policlinnic.Domain.Entities;
using System.Collections.Generic;

namespace Policlinnic.BLL.Services
{
    public class DoctorService
    {
        private readonly DoctorRepository _doctorRepository;

        public DoctorService()
        {
            _doctorRepository = new DoctorRepository();
        }

        public IEnumerable<Doctor> GetAllDoctors()
        {
            // Здесь можно добавить сортировку по ФИО 
            return _doctorRepository.GetAll();
        }
    }
}