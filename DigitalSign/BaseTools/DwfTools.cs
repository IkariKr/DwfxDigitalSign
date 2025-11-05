using DwfWatermark.Model;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using Bitmap = System.Drawing.Bitmap;

namespace DwfWatermark.BaseTools;

public static class DwfTools
{

	public static Bitmap CreateBitmap(int x, int y, bool isTransparent = true)
	{
		var image = new Bitmap(x, y);
		if (isTransparent)
		{
			using var graphics = Graphics.FromImage(image);
			graphics.Clear(Color.Transparent);
		}

		return image;
	}

	public static Bitmap MergeImage(this Bitmap image, Stream overlayStream, RectangleF destRect)
	{
		using var graphics = Graphics.FromImage(image);
		graphics.CompositingMode = CompositingMode.SourceCopy;
		graphics.CompositingQuality = CompositingQuality.HighQuality;
		using var overlayImage = new Bitmap(overlayStream);
		graphics.DrawImage(overlayImage, destRect);

		return image;
	}
}