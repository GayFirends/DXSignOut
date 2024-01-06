using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using FormatException = System.FormatException;

namespace DxSignOut.Utils;

internal static class QrCodeHelper
{
    public static string? Decode(string path)
    {
        using FileStream fileStream = File.OpenRead(path);
        using SKBitmap? sKBitmap = SKBitmap.Decode(fileStream);
        if (sKBitmap is null || sKBitmap.IsEmpty)
        {
            throw new FormatException();
        }

        int w = sKBitmap.Width;
        int h = sKBitmap.Height;
        byte[] bytes = new byte[w * h * 3];
        int byteIndex = 0;
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                SKColor color = sKBitmap.GetPixel(x, y);
                bytes[byteIndex + 0] = color.Red;
                bytes[byteIndex + 1] = color.Green;
                bytes[byteIndex + 2] = color.Blue;
                byteIndex += 3;
            }
        }

        RGBLuminanceSource rGBLuminanceSource = new(bytes, w, h);
        HybridBinarizer hybridBinarizer = new(rGBLuminanceSource);
        BinaryBitmap binaryBitmap = new(hybridBinarizer);
        QRCodeReader qRCodeReader = new();
        Result result = qRCodeReader.decode(binaryBitmap);
        return result is null ? throw new NullReferenceException() : result.Text;
    }
}