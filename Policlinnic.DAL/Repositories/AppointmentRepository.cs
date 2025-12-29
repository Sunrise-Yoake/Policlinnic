using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Policlinnic.Domain.Entities;

namespace Policlinnic.DAL.Repositories
{
    public class AppointmentRepository : BaseRepository
    {
        // 1. Метод для ПАЦИЕНТА (Мои записи)
        public List<AppointmentItem> GetByPatient(int patientId)
        {
            return ExecuteReadProc("GetPatientAppointments", new SqlParameter("@PatientID", patientId));
        }

        // 2. Метод для АДМИНА / ВРАЧА (Все записи или записи врача)
        public List<AppointmentItem> GetByDoctor(int? doctorId = null)
        {
            // Если doctorId == null, процедура вернет всё (для Админа)
            var param = new SqlParameter("@DoctorID", (object)doctorId ?? DBNull.Value);
            return ExecuteReadProc("GetDoctorAppointments", param);
        }

        // 3. Метод для ПОИСКА (Свободные слоты)
        public List<AppointmentItem> GetFreeSlots(int? specId = null, int? doctorId = null)
        {
            var p1 = new SqlParameter("@SpecID", (object)specId ?? DBNull.Value);
            var p2 = new SqlParameter("@DoctorID", (object)doctorId ?? DBNull.Value);
            return ExecuteReadProc("GetFreeSlots", p1, p2);
        }

        // --- Вспомогательный метод чтения (чтобы не дублировать код чтения Reader) ---
        private List<AppointmentItem> ExecuteReadProc(string procName, params SqlParameter[] parameters)
        {
            var list = new List<AppointmentItem>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(procName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddRange(parameters);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            list.Add(new AppointmentItem
                            {
                                Id = (int)r["Id"],
                                DateVisit = (DateTime)r["DateVisit"],
                                Cabinet = r["Cabinet"].ToString(),      // Название из View
                                DoctorName = r["DoctorName"].ToString(),
                                SpecName = r["SpecName"].ToString(),
                                PatientName = r["PatientName"].ToString(),
                                PatientId = r["PatientID"] as int?
                            });
                        }
                    }
                }
            }
            return list;
        }

        // --- ОСТАЛЬНЫЕ МЕТОДЫ (Без изменений, только вызовы) ---

        public void BookSlot(int appointmentId, int patientId)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("BookAppointment", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
                    cmd.Parameters.AddWithValue("@PatientId", patientId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Add(int doctorId, DateTime date, string cabinet)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("AddAppointmentSlot", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                    cmd.Parameters.AddWithValue("@Date", date);
                    cmd.Parameters.AddWithValue("@Cabinet", cabinet);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("DeleteAppointmentSlot", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int CancelDoctorDay(int doctorId, DateTime day)
        {
            int cancelled = 0;
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("Admin_CancelDoctorDay", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                    cmd.Parameters.AddWithValue("@Day", day);
                    var outParam = new SqlParameter("@CancelledCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(outParam);
                    cmd.ExecuteNonQuery();
                    if (outParam.Value != DBNull.Value) cancelled = (int)outParam.Value;
                }
            }
            return cancelled;
        }

        // --- СПРАВОЧНИКИ (Для ComboBox) ---
        public List<Specialization> GetSpecs()
        {
            var list = new List<Specialization>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT Код, Специализация FROM Специализация", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(new Specialization { ID = (int)r["Код"], Name = (string)r["Специализация"] });
            }
            return list;
        }

        public List<Doctor> GetAllDoctors()
        {
            var list = new List<Doctor>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT КодВрача, ФИО FROM Врач", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(new Doctor { ID = (int)r["КодВрача"], FullName = (string)r["ФИО"] });
            }
            return list;
        }

        public List<Doctor> GetDoctorsBySpec(int specId)
        {
            var list = new List<Doctor>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT КодВрача, ФИО FROM Врач WHERE КодСпециализации = @SpecId", conn))
                {
                    cmd.Parameters.AddWithValue("@SpecId", specId);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(new Doctor { ID = (int)r["КодВрача"], FullName = (string)r["ФИО"] });
                }
            }
            return list;
        }

        public List<Patient> GetAllPatients()
        {
            var list = new List<Patient>();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT КодПациента, ФИО FROM Пациент", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(new Patient { ID = (int)r["КодПациента"], FullName = (string)r["ФИО"] });
            }
            return list;
        }
    }
}