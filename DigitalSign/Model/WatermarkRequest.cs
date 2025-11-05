using DwfWatermark.BaseTools;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;

namespace DwfWatermark.Model;

public class WatermarkRequest
{
	/// <summary>
	/// dwf
	/// </summary>
    [Required]
    [FromForm]
	[FileExtensions(Extensions = ".dwfx", ErrorMessage = "必须上传.dwfx文件")]
    public IFormFile? DwfxStream { get; set; }

	/// <summary>
	/// png 
	/// </summary>
	[Required]
	[FromForm]
	[FileExtensions(Extensions = ".png", ErrorMessage = "必须上传.png文件")]
	public IFormFile? PngStream { get; set; }
	
	/// <summary>
	/// 插入点X
	/// </summary>
	[Required]
	[FromForm]
	[Range(0, int.MaxValue, ErrorMessage = "X 坐标必须为非负数")]
	public int PositionX { get; set; }
	/// <summary>
	/// 插入点Y
	/// </summary>
	[Required]
	[FromForm]
	[Range(0, int.MaxValue, ErrorMessage = "Y 坐标必须为非负数")]
    public int PositionY { get; set; }
	/// <summary>
	/// 图片宽度
	/// </summary>
	[Required]
	[FromForm]
	[Range(0, int.MaxValue, ErrorMessage = "宽度必须为正数")]
    public float PngWidth { get; set; } = 0;
	/// <summary>
	/// 图片高度
	/// </summary>
	[Required]
	[FromForm]
	[Range(0, int.MaxValue, ErrorMessage = "高度必须为正数")]
    public float PngHeight { get; set; } = 0;

	/// <summary>
	/// 清晰度
	/// </summary>
	[Required]
	[FromForm]
	[Range(1, 20, ErrorMessage = "倍数必须为1-20之间")]
	public int Clarity { get; set; } = 1;


}