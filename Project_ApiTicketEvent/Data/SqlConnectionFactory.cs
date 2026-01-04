using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
﻿using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
namespace Data
{
      public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("TicketDb")
                ?? throw new InvalidOperationException(
                    "Missing connection string 'TicketDb' in appsettings.json");
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
