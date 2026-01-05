using Data;
using Microsoft.Data.SqlClient;
using Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Models.DTOs.Requests.ThanhToanRequest;

namespace Repositories.Implementations
{
    public class ThanhToanRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ThanhToanRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // GET lịch sử thanh toán theo đơn (Attendee chỉ xem đơn của mình)
        public Task<List<ThanhToan>> GetByDonHangAsync(int nguoiMuaId, int donHangId)
        {
            const string sql = @"
SELECT tt.ThanhToanID, tt.DonHangID, tt.MaGiaoDich, tt.PhuongThuc, tt.SoTien,
       tt.TrangThai, tt.ThoiGianThanhToan, tt.RawResponse
FROM dbo.ThanhToan tt
JOIN dbo.DonHang dh ON dh.DonHangID = tt.DonHangID
WHERE tt.DonHangID = @DonHangID AND dh.NguoiMuaID = @NguoiMuaID
ORDER BY tt.ThanhToanID DESC;";

            var result = new List<ThanhToan>();

            using var conn = _connectionFactory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            AddParam(cmd, "@DonHangID", donHangId);
            AddParam(cmd, "@NguoiMuaID", nguoiMuaId);

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                result.Add(new ThanhToan
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

            return Task.FromResult(result);
        }

        // Tạo bản ghi thanh toán trạng thái Pending (0)
        public async Task<int> TaoThanhToanAsync(TaoThanhToanRequest req)
        {
            using var raw = _connectionFactory.CreateConnection();
            var conn = (SqlConnection)raw;
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            using var tx = conn.BeginTransaction();

            try
            {
                // verify đơn thuộc user + lấy tổng tiền + trạng thái
                var don = await LoadDonHangAsync(conn, tx, req.NguoiMuaID, req.DonHangID);

                // chỉ tạo thanh toán khi đơn đang chờ (0)
                if (don.TrangThai != 0)
                    throw new ArgumentException($"Đơn hàng không ở trạng thái chờ thanh toán (TrangThai={don.TrangThai}).");

                const string sqlInsert = @"
INSERT INTO dbo.ThanhToan (DonHangID, MaGiaoDich, PhuongThuc, SoTien, TrangThai, RawResponse)
VALUES (@DonHangID, @MaGiaoDich, @PhuongThuc, @SoTien, 0, @RawResponse);
SELECT CAST(SCOPE_IDENTITY() AS int);";

                using var cmd = new SqlCommand(sqlInsert, conn, tx);
                cmd.Parameters.AddWithValue("@DonHangID", req.DonHangID);
                cmd.Parameters.AddWithValue("@MaGiaoDich", (object?)req.MaGiaoDich ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PhuongThuc", req.PhuongThuc);
                cmd.Parameters.AddWithValue("@SoTien", don.TongTien);
                cmd.Parameters.AddWithValue("@RawResponse", (object?)req.RawResponse ?? DBNull.Value);

                var id = (int)await cmd.ExecuteScalarAsync();

                tx.Commit();
                return id;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // Xác nhận thanh toán (success/failed)
        // - success: update ThanhToan.TrangThai=1, DonHang.TrangThai=1, tạo vé
        // - failed : update ThanhToan.TrangThai=2, DonHang.TrangThai=3 (tuỳ bạn, ở đây set 3=thất bại)
        public async Task<object> XacNhanThanhToanAsync(XacNhanThanhToanRequest req)
        {
            using var raw = _connectionFactory.CreateConnection();
            var conn = (SqlConnection)raw;
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            using var tx = conn.BeginTransaction();

            try
            {
                var don = await LoadDonHangAsync(conn, tx, req.NguoiMuaID, req.DonHangID);

                // Nếu đã thanh toán rồi thì không cho xác nhận lại (tránh sinh vé trùng)
                if (don.TrangThai == 1)
                {
                    return new
                    {
                        message = "Đơn hàng đã thanh toán trước đó.",
                        DonHangID = req.DonHangID,
                        DonTrangThai = don.TrangThai
                    };
                }

                if (don.TrangThai != 0)
                    throw new ArgumentException($"Không thể xác nhận thanh toán khi DonHang.TrangThai={don.TrangThai}");

                // Tạo hoặc cập nhật bản ghi ThanhToan
                // Nếu có MaGiaoDich unique: insert mới (tránh trùng MaGiaoDich)
                // Ở đây: luôn insert 1 record để lưu trace
                byte ttTrangThai = req.ThanhCong ? (byte)1 : (byte)2; // 1=success, 2=failed
                byte donTrangThai = req.ThanhCong ? (byte)1 : (byte)3; // 1=paid, 3=fail

                const string sqlInsertTT = @"
INSERT INTO dbo.ThanhToan (DonHangID, MaGiaoDich, PhuongThuc, SoTien, TrangThai, ThoiGianThanhToan, RawResponse)
VALUES (@DonHangID, @MaGiaoDich, @PhuongThuc, @SoTien, @TrangThai, SYSDATETIME(), @RawResponse);
SELECT CAST(SCOPE_IDENTITY() AS int);";

                int thanhToanId;
                using (var cmdTT = new SqlCommand(sqlInsertTT, conn, tx))
                {
                    cmdTT.Parameters.AddWithValue("@DonHangID", req.DonHangID);
                    cmdTT.Parameters.AddWithValue("@MaGiaoDich", (object?)req.MaGiaoDich ?? DBNull.Value);
                    cmdTT.Parameters.AddWithValue("@PhuongThuc", req.PhuongThuc);
                    cmdTT.Parameters.AddWithValue("@SoTien", don.TongTien);
                    cmdTT.Parameters.AddWithValue("@TrangThai", ttTrangThai);
                    cmdTT.Parameters.AddWithValue("@RawResponse", (object?)req.RawResponse ?? DBNull.Value);

                    thanhToanId = (int)await cmdTT.ExecuteScalarAsync();
                }

                // Update DonHang trạng thái
                const string sqlUpdateDH = @"
UPDATE dbo.DonHang
SET TrangThai = @TrangThai
WHERE DonHangID = @DonHangID AND NguoiMuaID = @NguoiMuaID;";

                using (var cmdDH = new SqlCommand(sqlUpdateDH, conn, tx))
                {
                    cmdDH.Parameters.AddWithValue("@TrangThai", donTrangThai);
                    cmdDH.Parameters.AddWithValue("@DonHangID", req.DonHangID);
                    cmdDH.Parameters.AddWithValue("@NguoiMuaID", req.NguoiMuaID);
                    await cmdDH.ExecuteNonQueryAsync();
                }

                // Nếu thất bại thì dừng ở đây
                if (!req.ThanhCong)
                {
                    tx.Commit();
                    return new
                    {
                        message = "Thanh toán thất bại.",
                        ThanhToanID = thanhToanId,
                        DonHangID = req.DonHangID,
                        DonTrangThai = donTrangThai,
                        ThanhToanTrangThai = ttTrangThai
                    };
                }

                // SUCCESS -> sinh vé theo ChiTietDonHang
                // Tránh sinh trùng: nếu đã có vé cho DonHang thì không sinh nữa
                if (await HasAnyTicketAsync(conn, tx, req.DonHangID))
                {
                    tx.Commit();
                    return new
                    {
                        message = "Đã thanh toán. Vé đã tồn tại (không sinh trùng).",
                        ThanhToanID = thanhToanId,
                        DonHangID = req.DonHangID,
                        DonTrangThai = donTrangThai
                    };
                }

                var items = await LoadChiTietAsync(conn, tx, req.DonHangID);

                const string sqlInsertVe = @"
INSERT INTO dbo.Ve (DonHangID, LoaiVeID, NguoiSoHuuID, MaVe, QrToken, TrangThai)
VALUES (@DonHangID, @LoaiVeID, @NguoiSoHuuID, @MaVe, @QrToken, 0);";

                var maVeList = new List<string>();

                foreach (var it in items)
                {
                    for (int i = 0; i < it.SoLuong; i++)
                    {
                        var maVe = GenerateMaVe(req.DonHangID, it.LoaiVeID);
                        var qr = GenerateQrToken(req.DonHangID, it.LoaiVeID, req.NguoiMuaID);

                        using var cmdVe = new SqlCommand(sqlInsertVe, conn, tx);
                        cmdVe.Parameters.AddWithValue("@DonHangID", req.DonHangID);
                        cmdVe.Parameters.AddWithValue("@LoaiVeID", it.LoaiVeID);
                        cmdVe.Parameters.AddWithValue("@NguoiSoHuuID", req.NguoiMuaID);
                        cmdVe.Parameters.AddWithValue("@MaVe", maVe);
                        cmdVe.Parameters.AddWithValue("@QrToken", qr);
                        await cmdVe.ExecuteNonQueryAsync();

                        maVeList.Add(maVe);
                    }
                }

                tx.Commit();

                return new
                {
                    message = "Thanh toán thành công.",
                    ThanhToanID = thanhToanId,
                    DonHangID = req.DonHangID,
                    DonTrangThai = donTrangThai,
                    SoVeSinhRa = maVeList.Count,
                    MaVe = maVeList
                };
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // ===== Helpers =====

        private static void AddParam(IDbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }

        private async Task<(decimal TongTien, byte TrangThai)> LoadDonHangAsync(SqlConnection conn, SqlTransaction tx, int nguoiMuaId, int donHangId)
        {
            const string sql = @"
SELECT TongTien, TrangThai
FROM dbo.DonHang
WHERE DonHangID = @DonHangID AND NguoiMuaID = @NguoiMuaID;";

            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("@DonHangID", donHangId);
            cmd.Parameters.AddWithValue("@NguoiMuaID", nguoiMuaId);

            using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync())
                throw new ArgumentException("Đơn hàng không tồn tại hoặc không thuộc về bạn.");

            return (r.GetDecimal(r.GetOrdinal("TongTien")), Convert.ToByte(r["TrangThai"]));
        }

        private async Task<bool> HasAnyTicketAsync(SqlConnection conn, SqlTransaction tx, int donHangId)
        {
            const string sql = "SELECT TOP 1 1 FROM dbo.Ve WHERE DonHangID = @DonHangID;";
            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("@DonHangID", donHangId);
            var obj = await cmd.ExecuteScalarAsync();
            return obj != null;
        }

        private async Task<List<(int LoaiVeID, int SoLuong)>> LoadChiTietAsync(SqlConnection conn, SqlTransaction tx, int donHangId)
        {
            const string sql = @"
SELECT LoaiVeID, SoLuong
FROM dbo.ChiTietDonHang
WHERE DonHangID = @DonHangID;";

            var list = new List<(int LoaiVeID, int SoLuong)>();
            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("@DonHangID", donHangId);

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add((r.GetInt32(r.GetOrdinal("LoaiVeID")), r.GetInt32(r.GetOrdinal("SoLuong"))));
            }

            if (list.Count == 0) throw new ArgumentException("Đơn hàng không có chi tiết vé.");
            return list;
        }

        private static string GenerateMaVe(int donHangId, int loaiVeId)
        {
            var random = RandomNumberGenerator.GetBytes(4);
            return $"VE{donHangId}-{loaiVeId}-{Convert.ToHexString(random)}";
        }

        private static string GenerateQrToken(int donHangId, int loaiVeId, int nguoiMuaId)
        {
            var raw = $"{donHangId}|{loaiVeId}|{nguoiMuaId}|{DateTime.UtcNow.Ticks}";
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(raw)));
        }
    }
}
