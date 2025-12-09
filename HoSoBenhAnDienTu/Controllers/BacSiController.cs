using System;
using System.Linq;
using System.Web.Mvc;
using HoSoBenhAnDienTu.Models;
using System.Transactions;

namespace HoSoBenhAnDienTu.Controllers
{
    public class BacSiController : Controller
    {
        
        HoSoSucKhoeCaNhan_FullEntities db = new HoSoSucKhoeCaNhan_FullEntities();

        public ActionResult DanhSachBenhNhan()
        {
            if (Session["RoleID"]?.ToString() != "1") return RedirectToAction("Login", "Account");
            return View(db.HoSoNguoiDung.ToList());
        }

        public ActionResult TheoDoiSucKhoe(int id)
        {
            if (Session["RoleID"] == null || Session["RoleID"].ToString() != "1")
                return RedirectToAction("Login", "Account");

            var benhNhan = db.HoSoNguoiDung
                             .Include("DiUng")
                             .Include("TienSuGiaDinh")
                             .FirstOrDefault(h => h.MaHoSo == id);

            if (benhNhan == null) return HttpNotFound();

            return View(benhNhan);

        }

        [HttpPost]
        public JsonResult GetChartData(int maHoSo)
        {


            var data = db.View_BieuDoSucKhoe
                         .Where(v => v.MaHoSo == maHoSo)
                         .OrderBy(v => v.NgayKham) 
                         .Select(v => new {
                             Ngay = v.NgayKham,
                             HA_Tren = v.HuyetApTamThu,
                             HA_Duoi = v.HuyetApTamTruong,
                             Tim = v.NhipTim,
                             Can = v.CanNang
                         })
                         .ToList();

            var result = data.Select(x => new {
                Ngay = x.Ngay.ToString("dd/MM/yyyy"), 
                HA_Tren = x.HA_Tren,
                HA_Duoi = x.HA_Duoi,
                Tim = x.Tim,
                Can = x.Can
            });

            return Json(result);
        }
        public ActionResult KhamBenh(int id)
        {
            if (Session["RoleID"] == null || Session["RoleID"].ToString() != "1")
                return RedirectToAction("Login", "Account");

            var benhNhan = db.HoSoNguoiDung.Find(id);
            if (benhNhan == null) return HttpNotFound();

            return View(benhNhan);
        }

        [HttpPost]
        public JsonResult GetPatientChartData(int maHoSo)
        {
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

        
        [HttpGet]
        public JsonResult GetThuocSuggestions(string term)
        {
            var data = db.DanhMucThuoc
                         .Where(t => t.TenThuoc.Contains(term) || t.HoatChat.Contains(term))
                         .Select(t => new {
                             label = t.TenThuoc,          
                             value = t.TenThuoc,          
                             donVi = t.DonViTinh,         
                             hamLuong = t.HamLuong       
                         })
                         .Take(10) 
                         .ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LuuKetQuaKham(
    int maHoSo, string lyDoKham, string chanDoan, string loiDan, string noiKham,
    int? huyetApTamThu, int? huyetApTamTruong, int? nhipTim, decimal? nhietDo, decimal? canNang,
    string[] tenthuoc, string[] hamluong, int[] soluong, string[] donvi, string[] cachdung, string[] ghichu)
        {
            using (var scope = new System.Transactions.TransactionScope())
            {
                try
                {
                    string tenBacSi = Session["TenHienThi"] != null ? Session["TenHienThi"].ToString() : "Bác sĩ chỉ định";

                    var result = db.Proc_ThemMoiLanKham(
                        maHoSo,
                        DateTime.Now,
                        noiKham,
                        "Phòng khám chuyên khoa",
                        tenBacSi,
                        lyDoKham,
                        chanDoan,
                        loiDan,
                        DateTime.Now.AddDays(7)
                    ).FirstOrDefault();

                    if (result != null)
                    {
                        int newMaLanKham = Convert.ToInt32(result);

                        db.Proc_CapNhatChiSoSinhTon(
                            newMaLanKham,
                            huyetApTamThu,
                            huyetApTamTruong,
                            nhipTim,
                            nhietDo,
                            canNang
                        );

                        if (tenthuoc != null && tenthuoc.Length > 0)
                        {
                            for (int i = 0; i < tenthuoc.Length; i++)
                            {
                                if (!string.IsNullOrEmpty(tenthuoc[i]))
                                {
                                    db.Proc_KeDonThuoc(
                                        newMaLanKham,
                                        tenthuoc[i],
                                        hamluong[i],
                                        soluong[i],
                                        donvi[i],
                                        cachdung[i],
                                        ghichu[i] 
                                    );
                                }
                            }
                        }

                        scope.Complete(); 
                        TempData["Success"] = "Đã lưu hồ sơ và đơn thuốc thành công!";
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi khi lưu: " + ex.Message;
                }
            }

            return RedirectToAction("KhamBenh", new { id = maHoSo });
        }
    }
}
