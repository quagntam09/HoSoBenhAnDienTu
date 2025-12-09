using System;

namespace HoSoBenhAnDienTu.Models
{
    // Class để gộp chung sự kiện: Khám, Tiêm, Phẫu thuật
    public class SuKienYTe
    {
        public DateTime Ngay { get; set; }
        public string LoaiSuKien { get; set; } // "Khám bệnh", "Tiêm chủng", "Phẫu thuật"
        public string TieuDe { get; set; }     // Tên bệnh, Tên vắc xin...
        public string NoiDung { get; set; }    // Bác sĩ, Nơi khám...
        public string IconClass { get; set; }  // Class CSS cho icon
        public string ColorClass { get; set; } // Màu sắc
    }

    // Class chứa toàn bộ dữ liệu cho trang Hồ sơ cá nhân
    public class HoSoTongQuatViewModel
    {
        public HoSoNguoiDung HoSo { get; set; }
        public View_CanhBaoSucKhoe CanhBao { get; set; } // Lấy từ View SQL
        public string BMI_Text { get; set; }
        public string BMI_Color { get; set; }
        public decimal BMI_Value { get; set; }
    }
}