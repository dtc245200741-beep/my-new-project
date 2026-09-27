using KTXManagement.Demo.Data;
using KTXManagement.Demo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KTXManagement.Demo.Controllers
{
    // UC002: Quản lý hạ tầng ký túc xá - chỉ Quản lý KTX được thao tác
    [Authorize(Roles = "QuanLyKTX")]
    public class HaTangController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HaTangController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var toaNhas = await _db.ToaNhas
                .Include(t => t.DanhSachTang).ThenInclude(t => t.DanhSachPhong).ThenInclude(p => p.DanhSachGiuong).ThenInclude(g => g.SinhVien)
                .ToListAsync();
            return View(toaNhas);
        }

        [HttpGet]
        public IActionResult TaoToaNha() => View(new ToaNha());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoToaNha(ToaNha model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.ToaNhas.Add(model);
            await _db.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã tạo tòa nhà \"{model.TenToaNha}\".";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> TaoTang(int toaNhaId)
        {
            var toaNha = await _db.ToaNhas.FindAsync(toaNhaId);
            if (toaNha == null) return NotFound();
            ViewBag.ToaNha = toaNha;
            return View(new Tang { ToaNhaId = toaNhaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoTang(Tang model)
        {
            _db.Tangs.Add(model);
            await _db.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã thêm tầng {model.SoThuTu}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> TaoPhong(int tangId)
        {
            var tang = await _db.Tangs.Include(t => t.ToaNha).FirstOrDefaultAsync(t => t.Id == tangId);
            if (tang == null) return NotFound();
            ViewBag.Tang = tang;
            return View(new Phong { TangId = tangId, SucChua = 4, DonGiaThang = 700_000 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoPhong(Phong model, int soGiuongTao)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Phongs.Add(model);
            await _db.SaveChangesAsync();

            // Tự động sinh danh sách giường theo sức chứa nhập vào (tối đa = sức chứa phòng)
            var soLuong = Math.Min(soGiuongTao <= 0 ? model.SucChua : soGiuongTao, model.SucChua);
            for (int i = 1; i <= soLuong; i++)
            {
                _db.Giuongs.Add(new Giuong { PhongId = model.Id, SoGiuong = $"{model.SoPhong}-{i:00}", TrangThai = TrangThaiGiuong.Trong });
            }
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = $"Đã tạo phòng {model.SoPhong} với {soLuong} giường.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThaiPhong(int phongId, TrangThaiPhong trangThai)
        {
            var phong = await _db.Phongs.FindAsync(phongId);
            if (phong == null) return NotFound();
            phong.TrangThai = trangThai;
            await _db.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã cập nhật trạng thái phòng {phong.SoPhong}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
