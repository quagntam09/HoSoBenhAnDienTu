using HoSoBenhAnDienTu.Models;
using System;
using System.Data.SqlClient;
using System.Linq;              
using System.Web.Mvc;

namespace HoSoBenhAnDienTu.Controllers
{
    public class AccountController : Controller
    {
        HoSoSucKhoeCaNhan_FullEntities db = new HoSoSucKhoeCaNhan_FullEntities();

        

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {

            var user = db.TaiKhoan.FirstOrDefault(u => u.TenDangNhap == username && u.MatKhauHash == password);
            if (user != null && user.TrangThai == true)
            {
                Session["UserID"] = user.MaTaiKhoan;
                Session["RoleID"] = user.MaVaiTro;

                var hoSo = db.HoSoNguoiDung.FirstOrDefault(h => h.MaTaiKhoan == user.MaTaiKhoan);
                if (hoSo != null)
                {
                    Session["MaHoSo"] = hoSo.MaHoSo;
                    Session["TenHienThi"] = hoSo.HoTen;
                }

                if (user.MaVaiTro == 1) return RedirectToAction("DanhSachBenhNhan", "BacSi");
                else return RedirectToAction("HoSoCaNhan", "BenhNhan");
            }
            ViewBag.Error = "Đăng nhập thất bại hoặc tài khoản bị khóa";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                   

                    var result = db.Database.SqlQuery<RegisterResult>(
                        "Proc_DangKyTaiKhoan @TenDangNhap, @MatKhauHash, @Email, @HoTen, @NgaySinh, @GioiTinh, @SoDienThoai, @NhomMau, @ChieuCao, @CanNang",

                        new SqlParameter("@TenDangNhap", model.TenDangNhap),
                        new SqlParameter("@MatKhauHash", model.MatKhau), 
                        new SqlParameter("@Email", model.Email),
                        new SqlParameter("@HoTen", model.HoTen),
                        new SqlParameter("@NgaySinh", model.NgaySinh),
                        new SqlParameter("@GioiTinh", model.GioiTinh),
                        new SqlParameter("@SoDienThoai", model.SoDienThoai),

                        new SqlParameter("@NhomMau", (object)model.NhomMau ?? DBNull.Value),
                        new SqlParameter("@ChieuCao", (object)model.ChieuCao ?? DBNull.Value),
                        new SqlParameter("@CanNang", (object)model.CanNang ?? DBNull.Value)
                    ).FirstOrDefault();

                    if (result != null && result.KetQua == 1)
                    {
                        TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        ViewBag.Error = result?.ThongBao ?? "Đăng ký thất bại.";
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Lỗi hệ thống: " + ex.Message;
                }
            }
            return View(model);
        }

        public class RegisterResult
        {
            public int KetQua { get; set; }
            public string ThongBao { get; set; }
        }
    }
}