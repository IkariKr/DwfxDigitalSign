using System.IO;
using System.IO.Packaging;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using DwfWatermark.Model;

namespace DigitalSign.BaseTools;

public class DigitalSignatureTools
{
    public static bool SignPackageWithPfx(
        MemoryStream packageStream,
        MemoryStream pfxStream,
        string? password,
        string hashAlgorithm = "http://www.w3.org/2001/04/xmlenc#sha256",
        string excludePattern = "")
    {
        hashAlgorithm ??= HashAlgorithmString.Sha1;
			
        var cert = GetCertByPfx(pfxStream, password);
        if (cert is null) return false;
        
        using var package = Package.Open(packageStream, FileMode.Open, FileAccess.ReadWrite);
        return SignPackage(package, hashAlgorithm, cert, excludePattern);
    }
    
    
    
    public static VerifyResult? VerifyDigitalSign(MemoryStream? packageStream)
    {
        if (packageStream == null || packageStream.Length == 0) return null;
        using var package = Package.Open(packageStream, FileMode.Open, FileAccess.Read);
		
        PackageDigitalSignatureManager dsm = new PackageDigitalSignatureManager(package);
		
        return dsm.VerifySignatures(true);
    }
    
    private static X509Certificate2? GetCertByPfx(MemoryStream pfxStream, string? password)
    {
        X509Certificate2 cert;
        try
        {
            cert = new X509Certificate2(
                pfxStream.ToArray(),
                password,
                X509KeyStorageFlags.Exportable | // 允许导出私钥
                X509KeyStorageFlags.PersistKeySet); // 持久化密钥容器
        }
        catch (CryptographicException ex)
        {
            throw new Exception($"证书加载失败：{ex.Message}");
        }
        
        var keyUsageExt = cert.Extensions
            .OfType<X509KeyUsageExtension>()
            .FirstOrDefault();

        if (cert.HasPrivateKey &&
            (keyUsageExt?.KeyUsages & X509KeyUsageFlags.DigitalSignature) ==
            X509KeyUsageFlags.DigitalSignature) return cert;

        throw new Exception("证书不包含有效签名密钥");
        
    }
    
    private static bool SignPackage(Package package, 
	    string hashAlgorithm, 
	    X509Certificate2? cert = null,
	    string excludePattern = "")
	{
		
		if (package == null)
			throw new ArgumentNullException("SignAllParts(package)");

		
		PackageDigitalSignatureManager dsm = new PackageDigitalSignatureManager(package)
		{
			CertificateOption = CertificateEmbeddingOption.InCertificatePart,
			HashAlgorithm = hashAlgorithm,
		};
		
		List<Uri> toSign = new List<Uri>();

		var ns = from part in package.GetParts()
			where RegexTools.IsMatch(part.Uri.ToString(), excludePattern)
			select part.Uri;
		var s = package.GetParts().Select(part => part.Uri);
		
		var existingSignatures = package.GetParts()
			.Where(part => part.Uri.ToString().StartsWith("/package/services/digital-signature/"))
			.Select(part => part.Uri)
			.ToList();
		
		if (excludePattern.IsNullOrEmpty())
		{
			toSign.AddRange(package.GetParts().Where(part => !existingSignatures.Contains(part.Uri)).Select(part => part.Uri));
		}
		else
		{
			toSign.AddRange(package.GetParts().Where(part => !existingSignatures.Contains(part.Uri) && !RegexTools
				.IsMatch(part.Uri.ToString(), excludePattern)).Select(part => part.Uri));
		}
		
		toSign.Add(dsm.SignatureOrigin); 
		toSign.Add(PackUriHelper.GetRelationshipPartUri(new Uri("/", UriKind.Relative)));
		
		try
		{
			var sign = cert is null ? dsm.Sign(toSign) : dsm.Sign(toSign, cert);
			
			if (sign == null || sign.Verify() != VerifyResult.Success)
			{
				throw new Exception($"签名后验证失败");
			}
			return true;
		}
		catch (CryptographicException ex)
		{
			throw new Exception($"签名失败：{ex.Message}");
		}
	}
	    
	public static IEnumerable<DigitalSignInfo> GetSignInfo(Package? package)
	{
		if(package == null)
			return Enumerable.Empty<DigitalSignInfo>();
		var dsm = new PackageDigitalSignatureManager(package);
		return dsm.Signatures.Select(x => DigitalSignInfo.ToDigitalSignInfo(x));
	}
    
}