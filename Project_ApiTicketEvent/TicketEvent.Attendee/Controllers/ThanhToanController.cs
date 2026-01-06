using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;
using Models.DTOs.Requests;
using Repositories.Interfaces;
using static Models.DTOs.Requests.ThanhToanRequest;

namespace TicketEvent.Attendee.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThanhToanController : ControllerBase
    {
        private readonly IThanhToanRepository _thanhToanRepo;
        private readonly IConfiguration _config;

        public ThanhToanController(IThanhToanRepository thanhToanRepo, IConfiguration config)
        {
            _thanhToanRepo = thanhToanRepo;
            _config = config;
        }
        // GET: /api/ThanhToan/history?nguoiMuaId=1
        [HttpGet("history")]
        public async Task<IActionResult> History([FromQuery] int nguoiMuaId)
        {
            if (nguoiMuaId <= 0) return BadRequest(new { message = "nguoiMuaId invalid" });

            var data = await _thanhToanRepo.GetHistoryAsync(nguoiMuaId);
            return Ok(data);
        }

        // GET: /api/ThanhToan/history/by-donhang?nguoiMuaId=1&donHangId=10
        [HttpGet("history/by-donhang")]
        public async Task<IActionResult> HistoryByDonHang([FromQuery] int nguoiMuaId, [FromQuery] int donHangId)
        {
            if (nguoiMuaId <= 0 || donHangId <= 0)
                return BadRequest(new { message = "nguoiMuaId/donHangId invalid" });

            var data = await _thanhToanRepo.GetHistoryByDonHangAsync(nguoiMuaId, donHangId);
            return Ok(data);
        }

        /// <summary>
        /// MOCK thanh toán cho Attendee: thành công => tạo record ThanhToan + set DonHang.TrangThai = 1
        /// </summary>
        /// <param name="donHangId">ID đơn hàng</param>
        /// <param name="nguoiMuaId">ID người mua (tạm truyền query; sau này lấy từ JWT)</param>
        /// <param name="req">MockThanhToanRequest</param>
        [HttpPost("mock/{donHangId:int}")]
        public IActionResult MockThanhToan(
            [FromRoute] int donHangId,
            [FromQuery] int nguoiMuaId,
            [FromBody] ThanhToanRequest req)
        {
            if (donHangId <= 0) return BadRequest(new { message = "donHangId invalid" });
            if (nguoiMuaId <= 0) return BadRequest(new { message = "nguoiMuaId invalid" });

            var cs = _config.GetConnectionString("TicketDb");
            if (string.IsNullOrWhiteSpace(cs))
                return StatusCode(500, new { message = "Missing connection string: TicketDb" });

            using var conn = new SqlConnection(cs);
            conn.Open();

            using var tran = conn.BeginTransaction();

            try
            {
                // 1) kiểm tra đơn hàng thuộc về attendee + lấy tổng tiền + trạng thái
                const string sqlGetDon = @"
SELECT TongTien, TrangThai, SuKienID
FROM dbo.DonHang
WHERE DonHangID = @DonHangID AND NguoiMuaID = @NguoiMuaID;";

                decimal tongTien;
                byte trangThaiDon;
                int suKienIdDon;

                using (var cmd = new SqlCommand(sqlGetDon, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@DonHangID", donHangId);
                    cmd.Parameters.AddWithValue("@NguoiMuaID", nguoiMuaId);

                    using var r = cmd.ExecuteReader();
                    if (!r.Read())
                        return NotFound(new { message = "Không tìm thấy đơn hàng hoặc đơn không thuộc về bạn." });

                    tongTien = r.GetDecimal(r.GetOrdinal("TongTien"));
                    trangThaiDon = Convert.ToByte(r["TrangThai"]);
                    suKienIdDon = r.GetInt32(r.GetOrdinal("SuKienID"));
                }

                // chỉ cho thanh toán khi đơn đang chờ (0)
                if (trangThaiDon != 0)
                    return BadRequest(new { message = $"Đơn hàng không ở trạng thái chờ thanh toán (TrangThai={trangThaiDon})." });

                // (MỚI) 1.1) Validate: LoaiVe trong đơn phải thuộc đúng SuKienID của đơn
                const string sqlMismatch = @"
SELECT TOP 1 lv.LoaiVeID, lv.SuKienID
FROM dbo.ChiTietDonHang ct
JOIN dbo.LoaiVe lv ON lv.LoaiVeID = ct.LoaiVeID
WHERE ct.DonHangID = @DonHangID
  AND lv.SuKienID <> @SuKienID;";

                using (var cmdMis = new SqlCommand(sqlMismatch, conn, tran))
                {
                    cmdMis.Parameters.AddWithValue("@DonHangID", donHangId);
                    cmdMis.Parameters.AddWithValue("@SuKienID", suKienIdDon);

                    using var rMis = cmdMis.ExecuteReader();
                    if (rMis.Read())
                    {
                        var loaiVeId = rMis.GetInt32(rMis.GetOrdinal("LoaiVeID"));
                        var suKienIdLoaiVe = rMis.GetInt32(rMis.GetOrdinal("SuKienID"));

                        // Không commit => transaction sẽ rollback khi dispose (hoặc bạn có thể tran.Rollback() rồi return)
                        return BadRequest(new
                        {
                            message = "Dữ liệu đơn hàng bị lệch: Loại vé không thuộc sự kiện của đơn hàng.",
                            DonHang_SuKienID = suKienIdDon,
                            LoaiVeID_BiLech = loaiVeId,
                            LoaiVe_SuKienID = suKienIdLoaiVe
                        });
                    }
                }

                // (MỚI) 1.2) Chặn gọi thanh toán lại gây sinh vé trùng (an toàn thêm)
                const string sqlVeExists = @"SELECT COUNT(1) FROM dbo.Ve WHERE DonHangID = @DonHangID;";
                using (var cmdVe = new SqlCommand(sqlVeExists, conn, tran))
                {
                    cmdVe.Parameters.AddWithValue("@DonHangID", donHangId);
                    var countVe = Convert.ToInt32(cmdVe.ExecuteScalar());
                    if (countVe > 0)
                        return BadRequest(new { message = "Đơn hàng này đã được sinh vé trước đó." });
                }

                // 2) insert ThanhToan
                var thanhToan = new ThanhToan
                {
                    DonHangID = donHangId,
                    MaGiaoDich = $"MOCK_{Guid.NewGuid():N}",
                    PhuongThuc = string.IsNullOrWhiteSpace(req?.PhuongThuc) ? "MOCK" : req!.PhuongThuc!,
                    SoTien = tongTien,
                    TrangThai = 1, // 1=ThanhCong
                    ThoiGianThanhToan = DateTime.Now,
                    RawResponse = req?.RawResponse
                };

                var thanhToanId = _thanhToanRepo.Insert(thanhToan, conn, tran);

                // 3) update DonHang => paid
                const string sqlUpdateDon = @"
UPDATE dbo.DonHang
SET TrangThai = 1
WHERE DonHangID = @DonHangID AND NguoiMuaID = @NguoiMuaID AND TrangThai = 0;";

                using (var cmdUp = new SqlCommand(sqlUpdateDon, conn, tran))
                {
                    cmdUp.Parameters.AddWithValue("@DonHangID", donHangId);
                    cmdUp.Parameters.AddWithValue("@NguoiMuaID", nguoiMuaId);

                    var affected = cmdUp.ExecuteNonQuery();
                    if (affected == 0)
                        return BadRequest(new { message = "Không thể cập nhật trạng thái đơn hàng (có thể đã thay đổi trạng thái)." });
                }
                // 4) SINH VÉ
                const string sqlGetItems = @"
SELECT LoaiVeID, SoLuong
FROM dbo.ChiTietDonHang
WHERE DonHangID = @DonHangID;";

                var items = new List<(int loaiVeId, int soLuong)>();

                using (var cmdItems = new SqlCommand(sqlGetItems, conn, tran))
                {
                    cmdItems.Parameters.AddWithValue("@DonHangID", donHangId);

                    using var rItems = cmdItems.ExecuteReader();
                    while (rItems.Read())
                    {
                        var loaiVeId = rItems.GetInt32(rItems.GetOrdinal("LoaiVeID"));
                        var soLuong = rItems.GetInt32(rItems.GetOrdinal("SoLuong"));
                        items.Add((loaiVeId, soLuong));
                    }
                }

                // Nếu đơn không có chi tiết thì không sinh vé
                if (items.Count == 0)
                    return BadRequest(new { message = "Đơn hàng không có chi tiết, không thể sinh vé." });

                const string sqlInsertVe = @"
INSERT INTO dbo.Ve (DonHangID, LoaiVeID, NguoiSoHuuID, MaVe, QrToken, TrangThai)
VALUES (@DonHangID, @LoaiVeID, @NguoiSoHuuID, @MaVe, @QrToken, @TrangThai);";

                foreach (var (loaiVeId, soLuong) in items)
                {
                    if (soLuong <= 0) continue;

                    for (int i = 1; i <= soLuong; i++)
                    {
                        // MaVe: nên unique + dễ đọc (tùy bạn)
                        var maVe = $"DH{donHangId}-LV{loaiVeId}-{Guid.NewGuid():N}".ToUpper();

                        // QrToken: unique, dùng để encode QR (tùy bạn)
                        var qrToken = Guid.NewGuid().ToString("N");

                        using var cmdIns = new SqlCommand(sqlInsertVe, conn, tran);
                        cmdIns.Parameters.AddWithValue("@DonHangID", donHangId);
                        cmdIns.Parameters.AddWithValue("@LoaiVeID", loaiVeId);
                        cmdIns.Parameters.AddWithValue("@NguoiSoHuuID", nguoiMuaId);
                        cmdIns.Parameters.AddWithValue("@MaVe", maVe);
                        cmdIns.Parameters.AddWithValue("@QrToken", qrToken);
                        cmdIns.Parameters.AddWithValue("@TrangThai", (byte)0); // 0=ChuaSuDung

                        cmdIns.ExecuteNonQuery();
                    }
                }
                tran.Commit();

                return Ok(new
                {
                    message = "Thanh toán (MOCK) thành công.",
                    ThanhToanID = thanhToanId,
                    DonHangID = donHangId,
                    MaGiaoDich = thanhToan.MaGiaoDich,
                    SoTien = tongTien,
                    PhuongThuc = thanhToan.PhuongThuc
                });
            }
            catch (SqlException ex)
            {
                tran.Rollback();
                return StatusCode(500, new { message = "SQL Error", detail = ex.Message });
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return StatusCode(500, new { message = "Server Error", detail = ex.Message });
            }

        }
    }
}
