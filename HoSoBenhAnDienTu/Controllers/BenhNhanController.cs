using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HoSoBenhAnDienTu.Models; 

namespace HoSoBenhAnDienTu.Controllers
{
    public class BenhNhanController : Controller
    {
        private HoSoSucKhoeCaNhan_FullEntities db = new HoSoSucKhoeCaNhan_FullEntities();

        private bool KiemTraQuyen()
        {
            if (Session["UserID"] == null || Session["RoleID"] == null) return false;
            return Session["RoleID"].ToString() == "2";
        }

        public ActionResult HoSoCaNhan()
        {
            if (!KiemTraQuyen()) return RedirectToAction("Login", "Account");

            if (Session["MaHoSo"] == null) return RedirectToAction("Login", "Account");
            int maHoSo = (int)Session["MaHoSo"];

            var model = new HoSoTongQuatViewModel();


            model.HoSo = db.HoSoNguoiDung
                           .Include("DiUng")
                           .Include("TienSuGiaDinh")
                           .FirstOrDefault(h => h.MaHoSo == maHoSo);

            if (model.HoSo == null) return HttpNotFound();

            model.CanhBao = db.View_CanhBaoSucKhoe
                              .Where(cb => cb.HoTen == model.HoSo.HoTen) 
                              .OrderByDescending(cb => cb.NgayKham)
                              .FirstOrDefault();

            if (model.HoSo.ChieuCaoCoDinh > 0 && model.HoSo.CanNangHienTai > 0)
            {
                decimal chieuCaoMet = (decimal)model.HoSo.ChieuCaoCoDinh / 100m;
                model.BMI_Value = Math.Round((decimal)model.HoSo.CanNangHienTai / (chieuCaoMet * chieuCaoMet), 2);

                if (model.BMI_Value < 18.5m)
                {
                    model.BMI_Text = "Gầy";
                    model.BMI_Color = "info"; 
                }
                else if (model.BMI_Value < 22.9m)
                {
                    model.BMI_Text = "Bình thường";
                    model.BMI_Color = "success"; 
                }
                else if (model.BMI_Value < 29.9m)
                {
                    model.BMI_Text = "Thừa cân";
                    model.BMI_Color = "warning";
                }
                else
                {
                    model.BMI_Text = "Béo phì";
                    model.BMI_Color = "danger"; 
                }
            }
            else
            {
                model.BMI_Value = 0;
                model.BMI_Text = "Chưa có dữ liệu";
                model.BMI_Color = "secondary";
            }

            return View(model);
        }
        public ActionResult LichSuYTe()
        {
            if (!KiemTraQuyen()) return RedirectToAction("Login", "Account");
            int maHoSo = (int)Session["MaHoSo"];

            var timeline = new List<SuKienYTe>();

            var dsKham = db.LanKham.Where(x => x.MaHoSo == maHoSo).ToList();
            foreach (var k in dsKham)
            {
                timeline.Add(new SuKienYTe
                {
                    Ngay = k.NgayKham,
                    LoaiSuKien = "Khám bệnh",
                    TieuDe = k.ChanDoanChinh ?? "Khám tổng quát",
                    NoiDung = $"Tại: {k.NoiKham} - BS: {k.BacSiKham}",
                    IconClass = "fa-stethoscope", 
                    ColorClass = "info"
                });
            }

            var dsTiem = db.LichSuTiemChung.Where(x => x.MaHoSo == maHoSo).ToList();
            foreach (var t in dsTiem)
            {
                if (t.NgayTiem.HasValue)
                {
                    timeline.Add(new SuKienYTe
                    {
                        Ngay = t.NgayTiem.Value,
                        LoaiSuKien = "Tiêm chủng",
                        TieuDe = $"Vắc xin: {t.TenVacXin}",
                        NoiDung = $"Mũi số: {t.MuiSo} - Tại: {t.DiaDiemTiem}",
                        IconClass = "fa-syringe",
                        ColorClass = "success"
                    });
                }
            }

            var dsMo = db.LichSuPhauThuat.Where(x => x.MaHoSo == maHoSo).ToList();
            foreach (var m in dsMo)
            {
                if (m.NgayThucHien.HasValue)
                {
                    timeline.Add(new SuKienYTe
                    {
                        Ngay = m.NgayThucHien.Value,
                        LoaiSuKien = "Phẫu thuật",
                        TieuDe = m.TenPhauThuat,
                        NoiDung = $"Tại: {m.NoiThucHien} - BS: {m.BacSiPhauThuat}",
                        IconClass = "fa-procedures",
                        ColorClass = "danger"
                    });
                }
            }

            return View(timeline.OrderByDescending(x => x.Ngay).ToList());
        }

        public ActionResult LichSuKhamBenh()
        {
            if (!KiemTraQuyen()) return RedirectToAction("Login", "Account");
            int maHoSo = (int)Session["MaHoSo"];

            // Sử dụng View SQL để có sẵn trạng thái 'Sắp tái khám'
            var lichSu = db.View_TongHopLichSuKham
                           .Where(x => x.MaHoSo == maHoSo)
                           .OrderByDescending(x => x.NgayKham)
                           .ToList();

            return View(lichSu);
        }

        public ActionResult ChiTietLanKham(int id)
        {
            if (!KiemTraQuyen()) return RedirectToAction("Login", "Account");
            int maHoSo = (int)Session["MaHoSo"];

            var lanKham = db.LanKham.Find(id);

            if (lanKham == null || lanKham.MaHoSo != maHoSo)
            {
                return RedirectToAction("LichSuKhamBenh");
            }

            ViewBag.ChiSo = db.ChiSoKham.FirstOrDefault(x => x.MaLanKham == id);
            ViewBag.DonThuoc = db.DonThuoc.Where(x => x.MaLanKham == id).ToList();
            ViewBag.TaiLieu = db.TaiLieuKham.Where(x => x.MaLanKham == id).ToList();

            return View(lanKham);
        }

        [HttpGet]
        public ActionResult CapNhatThongTin()
        {
            if (!KiemTraQuyen()) return RedirectToAction("Login", "Account");
            int maHoSo = (int)Session["MaHoSo"];

            var hoSo = db.HoSoNguoiDung.Find(maHoSo);
            return View(hoSo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatThongTin(HoSoNguoiDung model)
        {
            if (!KiemTraQuyen()) return RedirectToAction("Login", "Account");
            int maHoSo = (int)Session["MaHoSo"];

            try
            {
                var hoSoGoc = db.HoSoNguoiDung.Find(maHoSo);
                if (hoSoGoc != null)
                {
                    hoSoGoc.SoDienThoaiLienHe = model.SoDienThoaiLienHe;
                    hoSoGoc.ChieuCaoCoDinh = model.ChieuCaoCoDinh;
                    hoSoGoc.CanNangHienTai = model.CanNangHienTai;

                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("HoSoCaNhan");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
            }

            return View(model);
        }
        public ActionResult NhatKySucKhoe()
        {
            if (!KiemTraQuyen()) return RedirectToAction("Login", "Account");
            int maHoSo = (int)Session["MaHoSo"];

            var listNhatKy = db.NhatKySucKhoe
                               .Where(nk => nk.MaHoSo == maHoSo)
                               .OrderByDescending(n => n.NgayGhi)
                               .ToList();
            return View(listNhatKy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GhiNhatKy(string trieuChung, string chiSoTuDo)
        {
            if (KiemTraQuyen() && Session["MaHoSo"] != null)
            {
                var nk = new NhatKySucKhoe();
                nk.MaHoSo = (int)Session["MaHoSo"];
                nk.NgayGhi = DateTime.Now;
                nk.TrieuChung = trieuChung;
                nk.ChiSoTuDo = chiSoTuDo;

                db.NhatKySucKhoe.Add(nk);
                db.SaveChanges();
            }
            return RedirectToAction("NhatKySucKhoe");
        }

        public ActionResult ThongTinBaoHiem()
        {
            if (!KiemTraQuyen()) return RedirectToAction("Login", "Account");
            int maHoSo = (int)Session["MaHoSo"];

            var danhSachThe = db.TheBaoHiem.Where(t => t.MaHoSo == maHoSo).ToList();
            return View(danhSachThe);
        }

        public ActionResult LichSuDungThuoc()
        {
            if (!KiemTraQuyen()) return RedirectToAction("Login", "Account");
            int maHoSo = (int)Session["MaHoSo"];

            var tuThuoc = db.View_LichSuDungThuoc
                            .Where(x => x.MaHoSo == maHoSo)
                            .OrderByDescending(x => x.LanDungGanNhat)
                            .ToList();
            return View(tuThuoc);
        }
        public ActionResult BieuDoSucKhoe()
        {
            if (!KiemTraQuyen()) return RedirectToAction("Login", "Account");

            int maHoSo = (int)Session["MaHoSo"];

            var hoSo = db.HoSoNguoiDung.Find(maHoSo);

            return View(hoSo);
        }

        [HttpPost]
        public JsonResult GetMyChartData()
        {
            if (Session["MaHoSo"] == null) return Json(new List<object>()); 
            int maHoSo = (int)Session["MaHoSo"];

            var data = db.View_BieuDoSucKhoe
                         .Where(v => v.MaHoSo == maHoSo)
                         .OrderBy(v => v.NgayKham)
                         .Select(v => new {
                             Ngay = v.NgayKham,
                             HA_Tren = v.HuyetApTamThu,
                             HA_Duoi = v.HuyetApTamTruong,

                             Tim = v.NhipTim,

                             Can = v.CanNang
                         }).ToList();

            var result = data.Select(x => new {
                Ngay = x.Ngay.ToString("dd/MM/yyyy"),
                HA_Tren = x.HA_Tren,
                HA_Duoi = x.HA_Duoi,

                Tim = x.Tim,

                Can = x.Can
            });

            return Json(result);
        }
    }
}