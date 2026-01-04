using Data;
using Microsoft.Data.SqlClient;
using Models;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IDbConnectionFactory _factory;

        public RefreshTokenRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public int Create(RefreshToken token)
        {
            const string sql = @"
            INSERT INTO dbo.RefreshToken (UserId, Token, JwtId, ExpiresAt, CreatedAt, RevokedAt, IsRevoked, IsUsed)
            VALUES (@UserId, @Token, @JwtId, @ExpiresAt, @CreatedAt, @RevokedAt, @IsRevoked, @IsUsed);
            SELECT CAST(SCOPE_IDENTITY() AS int);";

            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            AddParam(cmd, "@UserId", token.UserId);
            AddParam(cmd, "@Token", token.Token);
            AddParam(cmd, "@JwtId", token.JwtId);
            AddParam(cmd, "@ExpiresAt", token.ExpiresAt);
            AddParam(cmd, "@CreatedAt", token.CreatedAt);
            AddParam(cmd, "@RevokedAt", token.RevokedAt);
            AddParam(cmd, "@IsRevoked", token.IsRevoked);
            AddParam(cmd, "@IsUsed", token.IsUsed);

            var newIdObj = cmd.ExecuteScalar();
            return newIdObj == null ? 0 : Convert.ToInt32(newIdObj);
        }

        public RefreshToken? GetByToken(string token)
        {
            const string sql = @"
            SELECT TOP 1 RefreshTokenId, UserId, Token, JwtId, ExpiresAt, CreatedAt, RevokedAt, IsRevoked, IsUsed
            FROM dbo.RefreshToken
            WHERE Token = @Token;";

            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            AddParam(cmd, "@Token", token);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return Map(reader);
        }

        public bool MarkUsed(int refreshTokenId)
        {
            const string sql = @"
            UPDATE dbo.RefreshToken
            SET IsUsed = 1
            WHERE RefreshTokenId = @Id AND IsUsed = 0;";

            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            AddParam(cmd, "@Id", refreshTokenId);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Revoke(int refreshTokenId)
        {
            const string sql = @"
            UPDATE dbo.RefreshToken
            SET IsRevoked = 1,
                RevokedAt = ISNULL(RevokedAt, @Now)
            WHERE RefreshTokenId = @Id AND IsRevoked = 0;";

            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            AddParam(cmd, "@Id", refreshTokenId);
            AddParam(cmd, "@Now", DateTime.UtcNow);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool RevokeAllByUser(int userId)
        {
            const string sql = @"
            UPDATE dbo.RefreshToken
            SET IsRevoked = 1,
                RevokedAt = ISNULL(RevokedAt, @Now)
            WHERE UserId = @UserId AND IsRevoked = 0;";

            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            AddParam(cmd, "@UserId", userId);
            AddParam(cmd, "@Now", DateTime.UtcNow);

            return cmd.ExecuteNonQuery() > 0;
        }

        private static void AddParam(IDbCommand cmd, string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private static RefreshToken Map(IDataRecord r)
        {
            return new RefreshToken
            {
                RefreshTokenId = Convert.ToInt32(r["RefreshTokenId"]),
                UserId = Convert.ToInt32(r["UserId"]),
                Token = r["Token"]?.ToString() ?? "",
                JwtId = r["JwtId"]?.ToString() ?? "",
                ExpiresAt = Convert.ToDateTime(r["ExpiresAt"]),
                CreatedAt = Convert.ToDateTime(r["CreatedAt"]),
                RevokedAt = r["RevokedAt"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(r["RevokedAt"]),
                IsRevoked = Convert.ToBoolean(r["IsRevoked"]),
                IsUsed = Convert.ToBoolean(r["IsUsed"]),
            };
        }
    }
}
