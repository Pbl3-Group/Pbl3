// File: Models/HoSoUngVien.cs
namespace HeThongTimViec.Models;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("HoSoUngVien")]
public class HoSoUngVien
{
    [Key]
    [ForeignKey("NguoiDung")]
    public int NguoiDungId { get; set; }

    [StringLength(255)]
    public string? TieuDeHoSo { get; set; }

    public string? GioiThieuBanThan { get; set; }

    [StringLength(255)]
    public string? ViTriMongMuon { get; set; }

    [EnumDataType(typeof(LoaiLuong))]
    public LoaiLuong? LoaiLuongMongMuon { get; set; }

    public ulong? MucLuongMongMuon { get; set; } // BIGINT UNSIGNED

    [EnumDataType(typeof(TrangThaiTimViec))]
    public TrangThaiTimViec TrangThaiTimViec { get; set; }

    public bool ChoPhepTimKiem { get; set; }

    [StringLength(255)]
    [Url]
    public string? UrlCvMacDinh { get; set; }

    public virtual NguoiDung NguoiDung { get; set; } = null!;
}