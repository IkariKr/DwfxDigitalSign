namespace DigitalSign.BaseTools;

public static class HashAlgorithmString
{
    //（SHA256符合FIPS 180-4标准） 
    public readonly static string Sha256 = "http://www.w3.org/2001/04/xmlenc#sha256";
    public readonly static string Sha384 = "http://www.w3.org/2001/04/xmldsig-more#sha384";
    public readonly static string Sha1 = "http://www.w3.org/2000/09/xmldsig#sha1";
}