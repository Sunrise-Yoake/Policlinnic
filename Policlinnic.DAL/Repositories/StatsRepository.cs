using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Policlinnic.Domain.Entities;

namespace Policlinnic.DAL.Repositories
{
    public class StatsRepository : BaseRepository
    {
        // Получение данных для графика
        public List<StatModel> GetStats(DateTime start, DateTime end)
        {
            var list = new List<StatModel>();
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM DiseaseStats(@Start, @End) ORDER BY MonthStart";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Start", start);
                    cmd.Parameters.AddWithValue("@End", end);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new StatModel
                            {
                                Month = (DateTime)reader["MonthStart"],
                                MaleCount = (int)reader["MaleCount"],
                                FemaleCount = (int)reader["FemaleCount"],
                                TotalCount = (int)reader["TotalCount"]
                            });
                        }
                    }
                }
            }
            return list;
        }

        // Проверка тренда (вызов процедуры)
        public bool IsTrendDecreasing(DateTime start, DateTime end)
        {
            bool result = false;
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("CheckStrictly", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@StartDate", start);
                    cmd.Parameters.AddWithValue("@EndDate", end);

                    // Выходной параметр
                    var outParam = new SqlParameter("@IsDecreasing", System.Data.SqlDbType.Bit)
                    {
                        Direction = System.Data.ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outParam);

                    cmd.ExecuteNonQuery();

                    result = (bool)outParam.Value;
                }
            }
            return result;
        }
    }
}