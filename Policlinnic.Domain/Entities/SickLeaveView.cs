using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Policlinnic.Domain.Entities
{
    public class SickLeaveView
    {
        public int Id { get; set; }
        public int IDPatient { get; set; }
        public int IDDoctor { get; set; }
        public string PatientFIO { get; set; } = string.Empty;
        public string DoctorFIO { get; set; } = string.Empty;
        public DateTime RawDateStart { get; set; }
        public string? DateStart { get; set; }
        public string? DateEnd { get; set; } 
        //Для фильтрации
        public bool IsOpen { get; set; }
        public int SpecId { get; set; }
        public string SpecName { get; set; } = string.Empty;
    }
}
