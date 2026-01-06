using Data;
using Microsoft.Data.SqlClient;
using Models.DTOs.Requests;
using System;
using Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.Interfaces;

namespace Repositories.Implementations
{
    public class DonHangRepository:IDonHangRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DonHangRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<DonHang>> GetByNguoiMuaAsync(int nguoiMuaId)
        {
            const string sql = @"
SELECT DonHangID, NguoiMuaID, SuKienID, NgayDat, TongTien, TrangThai
FROM dbo.DonHang
WHERE NguoiMuaID = @NguoiMuaID
ORDER BY NgayDat DESC;";

            var result = new List<DonHang>();

            using var conn = _connectionFactory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            var p = cmd.CreateParameter();
            p.ParameterName = "@NguoiMuaID";
            p.Value = nguoiMuaId;
            cmd.Parameters.Add(p);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(MapDonHang(reader));
            }

            return await Task.FromResult(result);
        }

        public async Task<DonHangDetail?> GetDetailAsync(int donHangId, int nguoiMuaId)
        {
            const string sqlDonHang = @"
SELECT DonHangID, NguoiMuaID, SuKienID, NgayDat, TongTien, TrangThai
FROM dbo.DonHang
WHERE DonHangID = @DonHangID AND NguoiMuaID = @NguoiMuaID;";

            const string sqlItems = @"
SELECT ChiTietID, DonHangID, LoaiVeID, SoLuong, DonGia, (SoLuong * DonGia) AS ThanhTien
FROM dbo.ChiTietDonHang
WHERE DonHangID = @DonHangID
ORDER BY ChiTietID;";

            using var conn = _connectionFactory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();

            DonHangDetail? dh = null;

            // 1) header
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sqlDonHang;

                var p1 = cmd.CreateParameter();
                p1.ParameterName = "@DonHangID";
                p1.Value = donHangId;
                cmd.Parameters.Add(p1);

                var p2 = cmd.CreateParameter();
                p2.ParameterName = "@NguoiMuaID";
                p2.Value = nguoiMuaId;
                cmd.Parameters.Add(p2);

                using var r = cmd.ExecuteReader();
                if (!r.Read()) return null;

                var baseDh = MapDonHang(r);
                dh = new DonHangDetail
                {
                    DonHangID = baseDh.DonHangID,
                    NguoiMuaID = baseDh.NguoiMuaID,
                    SuKienID = baseDh.SuKienID,
                    NgayDat = baseDh.NgayDat,
                    TongTien = baseDh.TongTien,
                    TrangThai = baseDh.TrangThai
                };
            }

            // 2) items
            using (var cmd2 = conn.CreateCommand())
            {
                cmd2.CommandText = sqlItems;

                var p = cmd2.CreateParameter();
                p.ParameterName = "@DonHangID";
                p.Value = donHangId;
                cmd2.Parameters.Add(p);

                using var r2 = cmd2.ExecuteReader();
                while (r2.Read())
                {
                    dh!.Items.Add(new DonHangChiTiet
                    {
                        ChiTietID = r2.GetInt32(r2.GetOrdinal("ChiTietID")),
                        DonHangID = r2.GetInt32(r2.GetOrdinal("DonHangID")),
                        LoaiVeID = r2.GetInt32(r2.GetOrdinal("LoaiVeID")),
                        SoLuong = r2.GetInt32(r2.GetOrdinal("SoLuong")),
                        DonGia = r2.GetDecimal(r2.GetOrdinal("DonGia")),
                        ThanhTien = r2.GetDecimal(r2.GetOrdinal("ThanhTien"))
                    });
                }
            }

            return await Task.FromResult(dh);
        }

        public async Task<int> CreateAsync(TaoDonHangRequest req)
        {
            if (req.Items == null || req.Items.Count == 0)
                throw new ArgumentException("Items is required");

            // DonHang.TrangThai: 0=chờ thanh toán (gợi ý)
            const string sqlInsertDonHang = @"
INSERT INTO dbo.DonHang (NguoiMuaID, SuKienID, TongTien, TrangThai)
VALUES (@NguoiMuaID, @SuKienID, 0, 0);
SELECT CAST(SCOPE_IDENTITY() AS int);";

            const string sqlInsertItem = @"
INSERT INTO dbo.ChiTietDonHang (DonHangID, LoaiVeID, SoLuong, DonGia)
VALUES (@DonHangID, @LoaiVeID, @SoLuong, @DonGia);";

            const string sqlUpdateTongTien = @"
UPDATE dbo.DonHang
SET TongTien = (
    SELECT ISNULL(SUM(SoLuong * DonGia), 0)
    FROM dbo.ChiTietDonHang
    WHERE DonHangID = @DonHangID
)
WHERE DonHangID = @DonHangID;";

            // (MỚI) Lấy SuKienID + DonGia chuẩn từ DB theo LoaiVeID
            const string sqlGetLoaiVeInfo = @"
SELECT SuKienID, DonGia
FROM dbo.LoaiVe
WHERE LoaiVeID = @LoaiVeID;";

            using var raw = _connectionFactory.CreateConnection();
            var conn = (SqlConnection)raw;
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            using var tx = conn.BeginTransaction();

            try
            {
                // 0) VALIDATE + CHUẨN HÓA ITEMS (fix lệch sự kiện ngay từ gốc)
                int suKienIdDb = 0;
                var normalizedItems = new List<(int LoaiVeID, int SoLuong, decimal DonGia)>();

                foreach (var it in req.Items)
                {
                    if (it.LoaiVeID <= 0) throw new ArgumentException("LoaiVeID invalid");
                    if (it.SoLuong <= 0) throw new ArgumentException("SoLuong must be > 0");

                    using (var cmdLv = new SqlCommand(sqlGetLoaiVeInfo, conn, tx))
                    {
                        cmdLv.Parameters.AddWithValue("@LoaiVeID", it.LoaiVeID);

                        using var r = await cmdLv.ExecuteReaderAsync();
                        if (!await r.ReadAsync())
                            throw new ArgumentException($"LoaiVeID={it.LoaiVeID} không tồn tại.");

                        var lvSuKienId = r.GetInt32(r.GetOrdinal("SuKienID"));
                        var lvDonGia = r.GetDecimal(r.GetOrdinal("DonGia"));

                        // đảm bảo tất cả LoaiVe thuộc cùng 1 sự kiện
                        if (suKienIdDb == 0) suKienIdDb = lvSuKienId;
                        else if (suKienIdDb != lvSuKienId)
                            throw new ArgumentException(
                                $"Các LoaiVe không cùng 1 sự kiện. " +
                                $"Đang có SuKienID={suKienIdDb} nhưng LoaiVeID={it.LoaiVeID} thuộc SuKienID={lvSuKienId}.");

                        normalizedItems.Add((it.LoaiVeID, it.SoLuong, lvDonGia));
                    }
                }

                // (Tuỳ chọn) Nếu bạn muốn bắt buộc client gửi đúng SuKienID thì check:
                if (req.SuKienID > 0 && req.SuKienID != suKienIdDb)
                    throw new ArgumentException($"SuKienID gửi lên ({req.SuKienID}) không khớp DB ({suKienIdDb}).");

                // 1) tạo DonHang (DÙNG suKienIdDb, không tin req.SuKienID nữa)
                int donHangId;
                using (var cmd = new SqlCommand(sqlInsertDonHang, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@NguoiMuaID", req.NguoiMuaID);
                    cmd.Parameters.AddWithValue("@SuKienID", suKienIdDb);

                    donHangId = (int)await cmd.ExecuteScalarAsync();
                }

                // 2) thêm ChiTietDonHang (DÙNG DonGia từ DB, không tin request)
                foreach (var it in normalizedItems)
                {
                    using var cmd = new SqlCommand(sqlInsertItem, conn, tx);
                    cmd.Parameters.AddWithValue("@DonHangID", donHangId);
                    cmd.Parameters.AddWithValue("@LoaiVeID", it.LoaiVeID);
                    cmd.Parameters.AddWithValue("@SoLuong", it.SoLuong);
                    cmd.Parameters.AddWithValue("@DonGia", it.DonGia);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 3) cập nhật tổng tiền
                using (var cmd = new SqlCommand(sqlUpdateTongTien, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@DonHangID", donHangId);
                    await cmd.ExecuteNonQueryAsync();
                }

                tx.Commit();
                return donHangId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }


        public async Task<bool> CancelAsync(int donHangId, int nguoiMuaId)
        {
            // Gợi ý trạng thái: 2=Hủy (tuỳ bạn quy ước)
            const string sql = @"
UPDATE dbo.DonHang
SET TrangThai = 2
WHERE DonHangID = @DonHangID
  AND NguoiMuaID = @NguoiMuaID
  AND TrangThai IN (0,1);"; // chỉ cho hủy khi chưa hoàn tất

            using var conn = _connectionFactory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            var p1 = cmd.CreateParameter();
            p1.ParameterName = "@DonHangID";
            p1.Value = donHangId;
            cmd.Parameters.Add(p1);

            var p2 = cmd.CreateParameter();
            p2.ParameterName = "@NguoiMuaID";
            p2.Value = nguoiMuaId;
            cmd.Parameters.Add(p2);

            var affected = cmd.ExecuteNonQuery();
            return await Task.FromResult(affected > 0);
        }

        private static DonHang MapDonHang(IDataRecord r)
        {
            return new DonHang
            {
                DonHangID = r.GetInt32(r.GetOrdinal("DonHangID")),
                NguoiMuaID = r.GetInt32(r.GetOrdinal("NguoiMuaID")),
                SuKienID = r.GetInt32(r.GetOrdinal("SuKienID")),
                NgayDat = r.GetDateTime(r.GetOrdinal("NgayDat")),
                TongTien = r.GetDecimal(r.GetOrdinal("TongTien")),
                TrangThai = Convert.ToByte(r["TrangThai"]) // tinyint -> byte
            };
        }
    }
}
