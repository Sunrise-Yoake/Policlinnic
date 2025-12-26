using Microsoft.Data.SqlClient;

namespace Policlinnic.DAL.Repositories
{
    public abstract class BaseRepository
    {
        protected readonly string _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Policlinnic;Integrated Security=True;";

        protected SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}