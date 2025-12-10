using System;
using System.ComponentModel.DataAnnotations;

namespace HoSoBenhAnDienTu.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        [Compare("MatKhau", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        public string XacNhanMatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime NgaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        public string GioiTinh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string SoDienThoai { get; set; }

        [Display(Name = "Nhóm máu")]
        public string NhomMau { get; set; } 

        [Display(Name = "Chiều cao (cm)")]
        [Range(1, 300, ErrorMessage = "Chiều cao không hợp lệ")]
        public decimal? ChieuCao { get; set; }

        [Display(Name = "Cân nặng (kg)")]
        [Range(1, 500, ErrorMessage = "Cân nặng không hợp lệ")]
        public decimal? CanNang { get; set; } 
    }
}