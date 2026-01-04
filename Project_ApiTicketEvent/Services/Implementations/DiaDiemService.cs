using Models;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class DiaDiemService : IDiaDiemService
    {
        private readonly IDiaDiemReponsitory _repo;

        public DiaDiemService(IDiaDiemReponsitory repo)
        {
            _repo = repo;
        }

        public int Create(DiaDiem entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.TenDiaDiem = (entity.TenDiaDiem ?? "").Trim();
            entity.DiaChi = (entity.DiaChi ?? "").Trim();

            if (string.IsNullOrWhiteSpace(entity.TenDiaDiem))
                throw new InvalidOperationException("Tên địa điểm không được để trống.");

            if (entity.SucChua.HasValue && entity.SucChua.Value < 0)
                throw new InvalidOperationException("Sức chứa không hợp lệ.");

            entity.TrangThai = true;

            return _repo.Create(entity);
        }

        public bool Update(DiaDiem entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (entity.DiaDiemID <= 0)
                throw new InvalidOperationException("DiaDiemID không hợp lệ.");

            entity.TenDiaDiem = (entity.TenDiaDiem ?? "").Trim();
            entity.DiaChi = (entity.DiaChi ?? "").Trim();

            if (string.IsNullOrWhiteSpace(entity.TenDiaDiem))
                throw new InvalidOperationException("Tên địa điểm không được để trống.");

            if (entity.SucChua.HasValue && entity.SucChua.Value < 0)
                throw new InvalidOperationException("Sức chứa không hợp lệ.");

            entity.TrangThai = true;

            return _repo.Update(entity);
        }

        public bool Delete(int id)
        {
            if (id <= 0) throw new InvalidOperationException("Id không hợp lệ.");
            return _repo.Delete(id);
        }
    }
}
