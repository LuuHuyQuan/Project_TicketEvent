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
SELECT TongTien, TrangThai
FROM dbo.DonHang
WHERE DonHangID = @DonHangID AND NguoiMuaID = @NguoiMuaID;";

                decimal tongTien;
                byte trangThaiDon;

                using (var cmd = new SqlCommand(sqlGetDon, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@DonHangID", donHangId);
                    cmd.Parameters.AddWithValue("@NguoiMuaID", nguoiMuaId);

                    using var r = cmd.ExecuteReader();
                    if (!r.Read())
                        return NotFound(new { message = "Không tìm thấy đơn hàng hoặc đơn không thuộc về bạn." });

                    tongTien = r.GetDecimal(r.GetOrdinal("TongTien"));
                    trangThaiDon = Convert.ToByte(r["TrangThai"]);
                }

                // chỉ cho thanh toán khi đơn đang chờ (0)
                if (trangThaiDon != 0)
                    return BadRequest(new { message = $"Đơn hàng không ở trạng thái chờ thanh toán (TrangThai={trangThaiDon})." });

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
