using System.Drawing;
using System.Drawing.Imaging;

namespace ProductManagement.Features.Helpers;

public static class BarcodeHelper
{
    public static byte[] GenerateBarcode(string sku, int width = 300, int height = 100)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be empty", nameof(sku));

        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        
        graphics.Clear(Color.White);
        
        using var font = new Font("Consolas", 10, FontStyle.Regular);
        using var brush = new SolidBrush(Color.Black);
        
        var stringFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        
        graphics.DrawString($"|{sku}|", font, brush, new Rectangle(0, 0, width, height), stringFormat);
        
        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    public static string GenerateBarcodeDataUrl(string sku)
    {
        var bytes = GenerateBarcode(sku);
        var base64 = Convert.ToBase64String(bytes);
        return $"data:image/png;base64,{base64}";
    }
}