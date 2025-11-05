using System.ComponentModel.DataAnnotations;

namespace DwfWatermark.Model;

public class DigitalSignRequest
{
    /// <summary>
    /// dwf
    /// </summary>
    [Required]
    [FileExtensions(Extensions = ".dwfx", ErrorMessage = "必须上传.dwfx文件")]
    public IFormFile? DwfxStream { get; set; }

    /// <summary>
    /// png 
    /// </summary>
    [Required]
    [FileExtensions(Extensions = ".pfx", ErrorMessage = "必须上传.pfx文件")]
    public IFormFile? PfxStream { get; set; }
    
    /// <summary>
    /// 1 - Sha1 2 - Sha256 3 - Sha384
    /// </summary>
    [Required]
    public int HashAlgorithmType { get; set; } = 1;

    public string Password { get; set; } = string.Empty;
    
}