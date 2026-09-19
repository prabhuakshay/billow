using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Billow.BusinessDetails;

/// <summary>
/// Turns a picture file into the logo as stored: a PNG at most <see cref="MaxSide"/> pixels on its
/// longest side, at 96 DPI so that WPF shows it at its size in pixels.
/// </summary>
public static class LogoImage
{
    /// <summary>The most pixels the logo's longest side may have. A smaller picture isn't enlarged.</summary>
    public const int MaxSide = 400;

    /// <summary>
    /// The picture in the file at <paramref name="path"/> (its first frame, for an animated GIF),
    /// shrunk to fit <see cref="MaxSide"/> and re-encoded as PNG. Null if the file can't be read or
    /// isn't a picture WPF can decode.
    /// </summary>
    public static byte[]? FromFile(string path)
    {
        try
        {
            using var file = File.OpenRead(path);
            var picture = BitmapDecoder.Create(file, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames[0];

            var longestSide = Math.Max(picture.PixelWidth, picture.PixelHeight);
            BitmapSource shrunk = longestSide <= MaxSide
                ? picture
                : new TransformedBitmap(picture, new ScaleTransform(
                    (double)MaxSide / longestSide, (double)MaxSide / longestSide));

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(At96Dpi(shrunk)));
            using var png = new MemoryStream();
            encoder.Save(png);
            return png.ToArray();
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or NotSupportedException
            or FileFormatException or ArgumentException or InvalidOperationException or OverflowException
            or COMException)
        {
            return null;
        }
    }

    /// <summary>The same pixels, marked as 96 DPI.</summary>
    private static BitmapSource At96Dpi(BitmapSource picture)
    {
        var stride = ((picture.PixelWidth * picture.Format.BitsPerPixel) + 7) / 8;
        var pixels = new byte[stride * picture.PixelHeight];
        picture.CopyPixels(pixels, stride, 0);
        return BitmapSource.Create(
            picture.PixelWidth, picture.PixelHeight, 96, 96, picture.Format, picture.Palette, pixels, stride);
    }
}
