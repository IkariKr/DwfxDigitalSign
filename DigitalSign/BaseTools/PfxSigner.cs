using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace TestDwfSign.BaseTools;


// public class PfxSigner
// {
// 	public static X509Certificate2 LoadCertificate(string pfxPath, string password)
// 	{
// 		return new X509Certificate2(
// 			pfxPath,
// 			password,
// 			X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet
// 		);
// 	}
// 	
// 	public static string SignFile(string filePath, X509Certificate2 cert)
// 	{
// 		byte[] fileBytes = File.ReadAllBytes(filePath);
// 		using (RSA rsa = cert.GetRSAPrivateKey())
// 		{
// 			if (rsa == null) throw new InvalidOperationException("证书未包含有效私钥");
// 			byte[] signature = rsa.SignData(fileBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
// 			return Convert.ToBase64String(signature);
// 		}
// 	}
// 	
// 	public static bool VerifySignature(string filePath, string signature, X509Certificate2 cert)
// 	{
// 		byte[] fileBytes = File.ReadAllBytes(filePath);
// 		byte[] signatureBytes = Convert.FromBase64String(signature);
// 		using (RSA rsa = cert.GetRSAPublicKey())
// 		{
// 			return rsa.VerifyData(
// 				fileBytes,
// 				signatureBytes,
// 				HashAlgorithmName.SHA256,
// 				RSASignaturePadding.Pkcs1
// 			);
// 		}
// 	}
//
// }
