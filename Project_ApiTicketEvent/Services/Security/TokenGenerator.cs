using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Services.Security
{
    public static class TokenGenerator
    {
        public static string GenerateRefreshToken(int size = 32)
        {
            var bytes = RandomNumberGenerator.GetBytes(size);
            return Convert.ToBase64String(bytes); 
        }
    }
}
