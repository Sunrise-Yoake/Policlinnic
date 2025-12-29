using Policlinnic.DAL.Repositories;
using Policlinnic.Domain.Entities;
using System.Collections.Generic;

namespace Policlinnic.BLL.Services
{
    public class PatientService
    {
        private readonly PatientRepository _patientRepository;

        public PatientService()
        {
            _patientRepository = new PatientRepository();
        }

        // Метод для получения всех пациентов
        public IEnumerable<Patient> GetAllPatients()
        {
            
            return _patientRepository.GetAll();
        }
    }
}