using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
﻿using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
namespace Data
{
   public interface ISqlConnectionFactory
    {
        SqlConnection Create();
    }
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _cs;
        public SqlConnectionFactory(IConfiguration configuration)
        {
            _cs = configuration.GetConnectionString("Default")
             ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        }
        public SqlConnection Create()
        {
            return new SqlConnection(_cs);
        }
    }
}
