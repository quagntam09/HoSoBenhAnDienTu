using HoSoBenhAnDienTu.Models;
using System;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace HoSoBenhAnDienTu.Controllers
{
    public class BacSiController : Controller
    {
        
        HoSoSucKhoeCaNhan_FullEntities db = new HoSoSucKhoeCaNhan_FullEntities();


        public ActionResult DanhSachBenhNhan(string searchString, string sortOrder, string filterGender)
        {
            if (Session["RoleID"]?.ToString() != "1") return RedirectToAction("Login", "Account");

            var patients = db.HoSoNguoiDung.AsQueryable();

            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentGender = filterGender;
            ViewBag.CurrentSort = sortOrder;

            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.AgeSortParm = sortOrder == "Age" ? "age_desc" : "Age";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (!String.IsNullOrEmpty(searchString))
            {
                patients = patients.Where(s => s.HoTen.Contains(searchString) || s.MaHoSo.ToString().Contains(searchString));
            }

            if (!String.IsNullOrEmpty(filterGender))
            {
                patients = patients.Where(s => s.GioiTinh == filterGender);
            }

            switch (sortOrder)
            {
                case "name_desc":
                    patients = patients.OrderByDescending(s => s.HoTen);
                    break;
                case "Age": 
                    patients = patients.OrderBy(s => s.NgaySinh);
                    break;
                case "age_desc":
                    patients = patients.OrderByDescending(s => s.NgaySinh);
                    break;
                case "Date":
                    patients = patients.OrderBy(s => s.LanKham.Max(lk => lk.NgayKham));
                    break;
                case "date_desc": 
                    patients = patients.OrderByDescending(s => s.LanKham.Max(lk => lk.NgayKham));
                    break;
                default: 
                    patients = patients.OrderBy(s => s.HoTen);
                    break;
            }

            return View(patients.ToList());
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
        
        public ActionResult LichSuKhamBenh(int id)
        {
            if (Session["RoleID"]?.ToString() != "1") return RedirectToAction("Login", "Account");

            var benhNhan = db.HoSoNguoiDung.Find(id);
            if (benhNhan == null) return HttpNotFound();

            var lichSu = db.LanKham
                           .Where(lk => lk.MaHoSo == id)
                           .OrderByDescending(lk => lk.NgayKham)
                           .ToList();

            ViewBag.BenhNhan = benhNhan;
            return View(lichSu);
        }

        public ActionResult ChiTietLanKham(int id)
        {
            if (Session["RoleID"]?.ToString() != "1") return RedirectToAction("Login", "Account");

            var lanKham = db.LanKham.Find(id);
            if (lanKham == null) return HttpNotFound();

            ViewBag.ChiSo = db.ChiSoKham.FirstOrDefault(x => x.MaLanKham == id);
            ViewBag.DonThuoc = db.DonThuoc.Where(x => x.MaLanKham == id).ToList();
            ViewBag.TaiLieu = db.TaiLieuKham.Where(x => x.MaLanKham == id).ToList();
            ViewBag.BenhNhan = db.HoSoNguoiDung.Find(lanKham.MaHoSo);

            return View(lanKham);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadTaiLieuBacSi(int maLanKham, HttpPostedFileBase fileUpload, string tenTaiLieu)
        {
            if (Session["RoleID"]?.ToString() != "1") return RedirectToAction("Login", "Account");

            if (fileUpload != null && fileUpload.ContentLength > 0)
            {
                try
                {
                    string fileName = System.IO.Path.GetFileName(fileUpload.FileName);
                    string uniqueFileName = "BS_" + DateTime.Now.Ticks + "_" + fileName;
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/Uploads/TaiLieuKham/"), uniqueFileName);
                    fileUpload.SaveAs(path);

                    var taiLieu = new TaiLieuKham();
                    taiLieu.MaLanKham = maLanKham;
                    taiLieu.TenTaiLieu = string.IsNullOrEmpty(tenTaiLieu) ? fileName : tenTaiLieu;
                    taiLieu.LoaiFile = System.IO.Path.GetExtension(fileUpload.FileName);
                    taiLieu.DuongDanFile = "/Content/Uploads/TaiLieuKham/" + uniqueFileName;
                    taiLieu.NgayUpload = DateTime.Now;

                    db.TaiLieuKham.Add(taiLieu);
                    db.SaveChanges();
                    TempData["Success"] = "Đã thêm tài liệu thành công!";
                }
                catch (Exception ex) { TempData["Error"] = "Lỗi: " + ex.Message; }
            }
            return RedirectToAction("ChiTietLanKham", new { id = maLanKham });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XoaTaiLieuBacSi(int maTaiLieu)
        {
            if (Session["RoleID"]?.ToString() != "1") return RedirectToAction("Login", "Account");

            var taiLieu = db.TaiLieuKham.Find(maTaiLieu);
            if (taiLieu != null)
            {
                int maLanKham = taiLieu.MaLanKham ?? 0;
                try
                {
                    string fullPath = Server.MapPath("~" + taiLieu.DuongDanFile);
                    if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);

                    db.TaiLieuKham.Remove(taiLieu);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa tài liệu.";
                }
                catch { }

                if (maLanKham > 0) return RedirectToAction("ChiTietLanKham", new { id = maLanKham });
            }
            return RedirectToAction("DanhSachBenhNhan");
        }

        public ActionResult XemNhatKy(int id)
        {
            if (Session["RoleID"]?.ToString() != "1") return RedirectToAction("Login", "Account");

            var benhNhan = db.HoSoNguoiDung.Find(id);
            if (benhNhan == null) return HttpNotFound();

            var nhatKy = db.NhatKySucKhoe
                           .Where(nk => nk.MaHoSo == id)
                           .OrderByDescending(nk => nk.NgayGhi)
                           .ToList();

            ViewBag.BenhNhan = benhNhan;
            return View(nhatKy);
        }
    }
}
