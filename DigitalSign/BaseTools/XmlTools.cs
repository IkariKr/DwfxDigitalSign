using System.Xml.Linq;

namespace DigitalSign.BaseTools;

public class XmlTools
{
    public static XElement DwfRasterOverlayXml(
        string href,
        string size,
        string objectId,
        string transform,
        string width,
        string height,
        string nameSpace)
    {
        var xName = XName.Get("ImageResource", nameSpace);
        
        return new XElement(xName,
            new XAttribute("role", "raster overlay"),
            new XAttribute("mime", "image/png"),
            new XAttribute("href",href),
            new XAttribute("size",size),
            new XAttribute("objectId",objectId),
            new XAttribute("zOrder","1"),
            new XAttribute("transform",$"{transform} 0 0 0 0 {transform} 0 0 0 0 1 0 0 0 0 1"),
            new XAttribute("originalExtents",$"0 0 {width} {height}")
        );
    }
}