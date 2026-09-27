using KTXManagement.Demo.Data;
using KTXManagement.Demo.Models;
using KTXManagement.Demo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KTXManagement.Demo.Controllers
{
    // UC003: Quản lý hồ sơ sinh viên nội trú - Quản lý KTX thao tác; Sinh viên chỉ xem hồ sơ của mình
    [Authorize]
    public class SinhVienController : Controller
    {
        private readonly ApplicationDbContext _db;
        public SinhVienController(ApplicationDbContext db) => _db = db;

        [Authorize(Roles = "QuanLyKTX")]
        public async Task<IActionResult> Index()
        {
            var list = await _db.SinhViens.Include(s => s.TaiKhoan).Include(s => s.CongNo).ToListAsync();
            return View(list);
        }

        [Authorize(Roles = "QuanLyKTX")]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var sv = await _db.SinhViens
                .Include(s => s.TaiKhoan)
                .Include(s => s.CongNo)
                .Include(s => s.DanhSachDangKyO).ThenInclude(d => d.Phong)
                .Include(s => s.DanhSachPhieuPhi)
                .Include(s => s.DanhSachPhanAnh)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (sv == null) return NotFound();
            return View(sv);
        }

        [Authorize(Roles = "QuanLyKTX")]
        [HttpGet]
        public IActionResult TaoMoi() => View(new TaoSinhVienInput());

        [Authorize(Roles = "QuanLyKTX")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoMoi(TaoSinhVienInput input)
        {
            if (!ModelState.IsValid) return View(input);

            if (await _db.TaiKhoans.AnyAsync(t => t.TenDangNhap == input.TenDangNhap))
            {
                ModelState.AddModelError(nameof(input.TenDangNhap), "Tên đăng nhập đã tồn tại.");
                return View(input);
            }

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = input.TenDangNhap,
                MatKhauHash = PasswordHasher.Hash(input.MatKhau),
                HoTen = input.HoTen,
                Email = input.Email,
                VaiTro = VaiTro.SinhVien
            };
            _db.TaiKhoans.Add(taiKhoan);
            await _db.SaveChangesAsync();

            var sv = new SinhVien
            {
                TaiKhoanId = taiKhoan.Id,
                MSSV = input.MSSV,
                Lop = input.Lop,
                Khoa = input.Khoa,
                GioiTinh = input.GioiTinh,
                NgaySinh = input.NgaySinh,
                QueQuan = input.QueQuan,
                TrangThaiO = TrangThaiOKTX.ChuaDangKy
            };
            _db.SinhViens.Add(sv);
            await _db.SaveChangesAsync();

            _db.CongNos.Add(new CongNo { SinhVienId = sv.Id, TongNoHienTai = 0 });
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = $"Đã tạo hồ sơ sinh viên {sv.MSSV} - tài khoản đăng nhập: {taiKhoan.TenDangNhap}.";
            return RedirectToAction(nameof(Index));
        }

        // Sinh viên xem hồ sơ của chính mình
        [Authorize(Roles = "SinhVien")]
        public async Task<IActionResult> HoSoCuaToi()
        {
            var taiKhoanId = User.GetTaiKhoanId();
            var sv = await _db.SinhViens
                .Include(s => s.TaiKhoan)
                .Include(s => s.CongNo)
                .Include(s => s.DanhSachDangKyO).ThenInclude(d => d.Phong)
                .FirstOrDefaultAsync(s => s.TaiKhoanId == taiKhoanId);
            if (sv == null) return NotFound();
            return View("ChiTiet", sv);
        }
    }

    // Input model đơn giản để tạo tài khoản + hồ sơ sinh viên cùng lúc
    public class TaoSinhVienInput
    {
        public string TenDangNhap { get; set; } = string.Empty;
        public string MatKhau { get; set; } = "123456";
        public string HoTen { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string MSSV { get; set; } = string.Empty;
        public string? Lop { get; set; }
        public string? Khoa { get; set; }
        public GioiTinh GioiTinh { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? QueQuan { get; set; }
    }
}
