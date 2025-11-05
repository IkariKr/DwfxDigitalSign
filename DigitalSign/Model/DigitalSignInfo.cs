using System.IO.Packaging;

namespace DwfWatermark.Model;

public class DigitalSignInfo
{
    public string IsVerity { get; set; }
    public string SignTime { get; set; }
    public string Subject { get; set; }
    public IEnumerable<string> SignedParts { get; set; }
    public X509CertificateInfo SignerCertificate { get; set; }
    
    public static DigitalSignInfo ToDigitalSignInfo(PackageDigitalSignature digitalSignature)
    {
        return new DigitalSignInfo
        {
            IsVerity = digitalSignature.Verify().ToString(),
            SignTime = digitalSignature.SigningTime.ToString("yy-MM-dd HH:mm:ss"),
            Subject = digitalSignature.Signer.Subject,
            SignedParts = digitalSignature.SignedParts.Select(x => x.ToString()),
            SignerCertificate = new X509CertificateInfo
            {
                ExpirationDate = digitalSignature.Signer.GetExpirationDateString(),
                EffectiveDate = digitalSignature.Signer.GetEffectiveDateString(),
            }
        };
    }
}

public class X509CertificateInfo
{
    public string ExpirationDate { get; set; }  //Returns the expiration date of this X.509v3 certificate.
    public string EffectiveDate { get; set; } //Returns the effective date of this X.509v3 certificate.
}