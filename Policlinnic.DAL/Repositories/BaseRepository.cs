using Microsoft.Data.SqlClient;

namespace Policlinnic.DAL.Repositories
{
    public abstract class BaseRepository
    {
        protected readonly string _connectionString = @"Data Source=YOAKE\SQLEXPRESS;Initial Catalog=Policlinnic;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";
        protected SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}