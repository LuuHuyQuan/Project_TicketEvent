using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Security
{
    public static class PasswordHasher
    {
        private static readonly PasswordHasher<string> _hasher
            = new();

        public static string Hash(string password)
        {
            return _hasher.HashPassword("user", password);
        }

        public static bool Verify(string password, string hashedPasswordFromDb)
        {
            var result = _hasher.VerifyHashedPassword("user", hashedPasswordFromDb, password);

            return result == PasswordVerificationResult.Success
                   || result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
