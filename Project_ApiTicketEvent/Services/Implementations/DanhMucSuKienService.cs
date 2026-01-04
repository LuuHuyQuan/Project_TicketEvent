using Repositories.Implementations;
using Services.Interfaces;
using Repositories.Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class DanhMucSuKienService:IDanhMucSuKienService
    {
        private readonly IDanhMucSuKien _repo;

        public DanhMucSuKienService(IDanhMucSuKien repo)
        {
            _repo = repo;
        }

        // Attendee: thường chỉ lấy danh mục đang hoạt động
        public Task<List<DanhMucSuKien>> GetAllAsync()
            => _repo.GetAllAsync(trangThai: true);

        public Task<DanhMucSuKien?> GetByNameAsync(string tenDanhMuc)
            => _repo.GetByNameAsync(tenDanhMuc, trangThai: true);
    }
}
