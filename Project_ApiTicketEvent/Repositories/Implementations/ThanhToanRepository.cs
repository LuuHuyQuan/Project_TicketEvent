using Data;
using Microsoft.Data.SqlClient;
using Models;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class ThanhToanRepository:IThanhToanRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ThanhToanRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public int Insert(ThanhToan tt, SqlConnection conn, SqlTransaction tran)
        {
            using var cmd = new SqlCommand(@"INSERT INTO dbo.ThanhToan(DonHangID, MaGiaoDich, PhuongThuc, SoTien, TrangThai, ThoiGianThanhToan, RawResponse)VALUES(@DonHangID, @MaGiaoDich, @PhuongThuc, @SoTien, @TrangThai, @ThoiGianThanhToan, @RawResponse);SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tran);
            cmd.Parameters.AddWithValue("@DonHangID", tt.DonHangID);
            cmd.Parameters.AddWithValue("@MaGiaoDich", (object?)tt.MaGiaoDich ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PhuongThuc", tt.PhuongThuc);
            cmd.Parameters.AddWithValue("@SoTien", tt.SoTien);
            cmd.Parameters.AddWithValue("@TrangThai", tt.TrangThai);
            cmd.Parameters.AddWithValue("@ThoiGianThanhToan", (object?)tt.ThoiGianThanhToan ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RawResponse", (object?)tt.RawResponse ?? DBNull.Value);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        public async Task<List<ThanhToan>> GetHistoryAsync(int nguoiMuaId)
        {
            const string sql = @"
SELECT tt.ThanhToanID, tt.DonHangID, tt.MaGiaoDich, tt.PhuongThuc, tt.SoTien,
       tt.TrangThai, tt.ThoiGianThanhToan, tt.RawResponse
FROM dbo.ThanhToan tt
JOIN dbo.DonHang dh ON dh.DonHangID = tt.DonHangID
WHERE dh.NguoiMuaID = @NguoiMuaID
ORDER BY ISNULL(tt.ThoiGianThanhToan, '1900-01-01') DESC, tt.ThanhToanID DESC;";

            var list = new List<ThanhToan>();

            using var conn = (SqlConnection)_connectionFactory.CreateConnection();
            await conn.OpenAsync();

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@NguoiMuaID", nguoiMuaId);

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new ThanhToan
                {
                    ThanhToanID = r.GetInt32(r.GetOrdinal("ThanhToanID")),
                    DonHangID = r.GetInt32(r.GetOrdinal("DonHangID")),
                    MaGiaoDich = r.IsDBNull(r.GetOrdinal("MaGiaoDich")) ? null : r.GetString(r.GetOrdinal("MaGiaoDich")),
                    PhuongThuc = r.GetString(r.GetOrdinal("PhuongThuc")),
                    SoTien = r.GetDecimal(r.GetOrdinal("SoTien")),
                    TrangThai = Convert.ToByte(r["TrangThai"]), // TINYINT => byte
                    ThoiGianThanhToan = r.IsDBNull(r.GetOrdinal("ThoiGianThanhToan")) ? null : r.GetDateTime(r.GetOrdinal("ThoiGianThanhToan")),
                    RawResponse = r.IsDBNull(r.GetOrdinal("RawResponse")) ? null : r.GetString(r.GetOrdinal("RawResponse"))
                });
            }

            return list;
        }

        public async Task<List<ThanhToan>> GetHistoryByDonHangAsync(int nguoiMuaId, int donHangId)
        {
            const string sql = @"
SELECT tt.ThanhToanID, tt.DonHangID, tt.MaGiaoDich, tt.PhuongThuc, tt.SoTien,
       tt.TrangThai, tt.ThoiGianThanhToan, tt.RawResponse
FROM dbo.ThanhToan tt
JOIN dbo.DonHang dh ON dh.DonHangID = tt.DonHangID
WHERE dh.NguoiMuaID = @NguoiMuaID
  AND tt.DonHangID = @DonHangID
ORDER BY ISNULL(tt.ThoiGianThanhToan, '1900-01-01') DESC, tt.ThanhToanID DESC;";

            var list = new List<ThanhToan>();

            using var conn = (SqlConnection)_connectionFactory.CreateConnection();
            await conn.OpenAsync();

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@NguoiMuaID", nguoiMuaId);
            cmd.Parameters.AddWithValue("@DonHangID", donHangId);

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new ThanhToan
                {
                    ThanhToanID = r.GetInt32(r.GetOrdinal("ThanhToanID")),
                    DonHangID = r.GetInt32(r.GetOrdinal("DonHangID")),
                    MaGiaoDich = r.IsDBNull(r.GetOrdinal("MaGiaoDich")) ? null : r.GetString(r.GetOrdinal("MaGiaoDich")),
                    PhuongThuc = r.GetString(r.GetOrdinal("PhuongThuc")),
                    SoTien = r.GetDecimal(r.GetOrdinal("SoTien")),
                    TrangThai = Convert.ToByte(r["TrangThai"]),
                    ThoiGianThanhToan = r.IsDBNull(r.GetOrdinal("ThoiGianThanhToan")) ? null : r.GetDateTime(r.GetOrdinal("ThoiGianThanhToan")),
                    RawResponse = r.IsDBNull(r.GetOrdinal("RawResponse")) ? null : r.GetString(r.GetOrdinal("RawResponse"))
                });
            }

            return list;
        }
    }
}
