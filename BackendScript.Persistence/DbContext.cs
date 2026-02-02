using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BackendScript.Persistence;

public sealed class DbContext(IConfiguration configuration)
{
    private IConfiguration Configuration => configuration;
    public string ConnectionString => Configuration.GetConnectionString("DbConnectionString")!;

    public IDbConnection Create() => new SqlConnection(ConnectionString);
}