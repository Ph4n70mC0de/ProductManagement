using SkiaSharp;
using ZXing;
using ZXing.Common;

namespace ProductManagement.Features.Helpers;

public static class BarcodeHelper
{
    private static readonly BarcodeWriter<SKBitmap> BarcodeWriter = new()
    {
        Format = BarcodeFormat.CODE_128,
        Options = new EncodingOptions
        {
            Width = 300,
            Height = 100
        }
    };

    public static byte[] GenerateBarcode(string sku, int width = 300, int height = 100)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be empty", nameof(sku));

        var options = BarcodeWriter.Options;
        options.Width = width;
        options.Height = height;

        using var bitmap = BarcodeWriter.Write(sku);
        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    public static string GenerateBarcodeDataUrl(string sku)
    {
        var bytes = GenerateBarcode(sku);
        var base64 = Convert.ToBase64String(bytes);
        return $"data:image/png;base64,{base64}";
    }
}