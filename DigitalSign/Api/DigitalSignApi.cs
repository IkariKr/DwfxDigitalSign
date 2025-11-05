using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using DigitalSign.BaseTools;
using DwfWatermark.BaseTools;
using DwfWatermark.Model;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSign.Api;

public static class DigitalSignApi
{
    public static string pattern = @"dwf/documents/.*/sections/.*_.*-.*/descriptor.xml$";

    public static WebApplication Watermark (this WebApplication app)
    {
        app.MapPost("/api/watermark", async ([AsParameters] WatermarkRequest request) => 
        {
            // 验证请求中是否包含必要的文件
            if (request.DwfxStream == null || request.PngStream == null
                                          || request.DwfxStream.Length == 0 
                                          || request.PngStream.Length == 0)
                return Results.BadRequest("缺少必要的文件流（dwfStream 或 pngStream）。");
            if (request.PngWidth == 0 || request.PngHeight == 0)
                return Results.BadRequest("PngWidth 或 PngHeight 为0");
            if (request.Clarity is < 1 or > 20)
                return Results.BadRequest("Clarity 为 1-20 之间");
            
            var pngGuid = $"{Guid.NewGuid():N}";
            
            try
            {
                // 读取 DWF 文件内容
                var dwfMemoryStream = new MemoryStream();
                await request.DwfxStream.CopyToAsync(dwfMemoryStream);
                dwfMemoryStream.Position = 0; // 重置流的位置

                // 读取 PNG 文件内容
                using var pngMemoryStream = new MemoryStream();
                await request.PngStream.CopyToAsync(pngMemoryStream); 
                pngMemoryStream.Position = 0; // 重置流的位置
                
                // TODO: 实现水印合并逻辑
                using (var zipArchive = new ZipArchive(dwfMemoryStream, ZipArchiveMode.Update, true)) // 使用using确保释放
                {
                    var selectEnts = zipArchive.Entries
                        .Where(ent => Regex.IsMatch(ent.FullName, pattern));

                    foreach (var descriptorEnt in selectEnts)
                    {
                        XDocument descriptorXd;
                        using var pngMs = new MemoryStream();
                        await using (var descriptorEntStream = descriptorEnt.Open())
                        {
                            descriptorXd = XDocument.Load(descriptorEntStream);
                            if (descriptorXd.Root?.Name.LocalName != "Page") continue;

                            // 修改XML内容
                            var resourcesXe = descriptorXd.Root.Nodes().Select(x => x as XElement).FirstOrDefault
                                (x => x?.Name.LocalName == "Resources");
                            if (resourcesXe == null) continue;
                            
                            var paperXe = descriptorXd.Root.Nodes().Select(x => x as XElement).FirstOrDefault
                                (x => x?.Name.LocalName == "Paper");
                            if (paperXe == null) return Results.BadRequest("未找到Paper元素");

                            if (!float.TryParse(paperXe.Attribute("width")?.Value, out var width)
                                || !float.TryParse(paperXe.Attribute("height")?.Value, out var height))
                                return Results.BadRequest("未找到Paper Width || Height");

                            var bitmap = DwfTools.CreateBitmap((int)width * request.Clarity,
                                (int)height * request.Clarity, true);
                            bitmap = bitmap.MergeImage(pngMemoryStream,
                                new RectangleF(
                                    request.PositionX * request.Clarity,
                                    request.PositionY * request.Clarity,
                                    request.PngWidth * request.Clarity,
                                    request.PngHeight * request.Clarity));

                            bitmap.Save(pngMs, ImageFormat.Png);
                            var href = Path.Combine(Path.GetDirectoryName(descriptorEnt.FullName)!,$"{pngGuid}.png").Replace("\\", "/")!;
                            
                            //创建带命名空间的ImageResource元素
                            var imageResource = XmlTools.DwfRasterOverlayXml(
                                href:"/" + href,
                                size: pngMs.Length.ToString(),
                                objectId: Guid.NewGuid().ToString(),
                                transform: (1.0f / request.Clarity).ToString(),
                                width: width.ToString(),
                                height: height.ToString(),
                                descriptorXd.Root.Name.NamespaceName);
                            resourcesXe.Add(imageResource);
                        } // 确保关闭原始条目流

                        descriptorEnt.Delete(); // 删除旧条目
                        ////创建新条目并写入修改后的内容
                        var newEntry = zipArchive.CreateEntry(descriptorEnt.FullName);
                        using (var newStream = newEntry.Open())
                        using (var writer = XmlWriter.Create(newStream, new XmlWriterSettings
                               {
                                   Encoding = Encoding.UTF8,
                                   ConformanceLevel = ConformanceLevel.Document
                               }))
                        {
                            descriptorXd.Save(writer);
                        }

                        var newPngEntry = zipArchive.CreateEntry(Path.Combine(Path.GetDirectoryName(descriptorEnt
                            .FullName)!, $"{pngGuid}.png"));

                        // 写入到 ZIP 条目
                        using (var entryStream = newPngEntry.Open())
                        {
                            entryStream.Write(pngMs.ToArray());
                        }
                        break;
                    }

                }

                dwfMemoryStream.Position = 0;
                // 返回生成的文件作为响应
                return Results.Stream(dwfMemoryStream, "application/octet-stream",
                    $"{Guid.NewGuid():N}.dwfx"); // 根据实际文件类型调整 MIME 类型和文件名
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: 500);
            } 
        }).DisableAntiforgery();

        return app;
    }

    public static WebApplication DigitalSign(this WebApplication app)
    {
        app.MapPost("/api/digitalsign", async ([FromForm] DigitalSignRequest request) =>
        {
            // 验证请求中是否包含必要的文件
            if (request.DwfxStream == null || request.PfxStream == null
                                           || request.DwfxStream.Length == 0
                                           || request.PfxStream.Length == 0)
                return Results.BadRequest("缺少必要的文件流（dwfStream 或 pfxStream）。");
            
            // 读取 DWF 文件内容
            var dwfMemoryStream = new MemoryStream();
            await request.DwfxStream.CopyToAsync(dwfMemoryStream);
            dwfMemoryStream.Position = 0; // 重置流的位置

            // 读取 PNG 文件内容
            using var pfxMemoryStream = new MemoryStream();
            await request.PfxStream.CopyToAsync(pfxMemoryStream);
            pfxMemoryStream.Position = 0; // 重置流的位置

            DigitalSignatureTools.SignPackageWithPfx(dwfMemoryStream, pfxMemoryStream, request.Password,
                request.HashAlgorithmType switch
                {
                    1 => HashAlgorithmString.Sha1,
                    2 => HashAlgorithmString.Sha256,
                    3 => HashAlgorithmString.Sha384,
                    _ => string.Empty,
                }, pattern);

            dwfMemoryStream.Position = 0;
            
            return Results.Stream(dwfMemoryStream, "application/octet-stream",
                $"{Guid.NewGuid():N}.dwfx");
            
        }).DisableAntiforgery();
        
        return app;
    }
    
    public static WebApplication Vertifysign(this WebApplication app)
    {
        app.MapPost("/api/vertifysign", async ([FromForm] IFormFile? file) =>
        {
            // 验证请求中是否包含必要的文件
            if(file is null || file.Length == 0)
                return Results.BadRequest("缺少必要的文件。");

            // 读取 DWF 文件内容
            var dwfMemoryStream = new MemoryStream();
            await file.CopyToAsync(dwfMemoryStream);
            dwfMemoryStream.Position = 0; // 重置流的位置

            try
            {
                var result = DigitalSignatureTools.VerifyDigitalSign(dwfMemoryStream);

                return Results.Ok(result.ToString());
            }
            catch (Exception exception)
            {
                return Results.Problem(exception.Message, statusCode: 500);
            }
            
        }).DisableAntiforgery();
        
        return app;
    }
    
    public static WebApplication Signinfo(this WebApplication app)
    {
        app.MapPost("/api/signinfo", async ([FromForm] IFormFile? file) =>
        {
            // 验证请求中是否包含必要的文件
            if(file is null || file.Length == 0)
                return Results.BadRequest("缺少必要的文件。");

            // 读取 DWF 文件内容
            var dwfMemoryStream = new MemoryStream();
            await file.CopyToAsync(dwfMemoryStream);
            dwfMemoryStream.Position = 0; // 重置流的位置

            try
            {
                var package = Package.Open(dwfMemoryStream, FileMode.Open, FileAccess.Read);
                return Results.Ok(DigitalSignatureTools.GetSignInfo(package));
            }
            catch (Exception exception)
            {
                return Results.Problem(exception.Message, statusCode: 500);
            }
            
        }).DisableAntiforgery();

        return app;
    }
    
}