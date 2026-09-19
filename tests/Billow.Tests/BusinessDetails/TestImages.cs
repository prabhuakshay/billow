using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Billow.Tests.BusinessDetails;

/// <summary>Writes picture files for a test to pick as the logo. Deleted on dispose.</summary>
public sealed class TestImages : IDisposable
{
    private readonly string _directory = Directory.CreateTempSubdirectory("billow-test-images-").FullName;

    /// <summary>A PNG file of the given size in pixels, at <paramref name="dpi"/> dots per inch.</summary>
    public string Png(int width, int height, double dpi = 96) =>
        Write(width, height, dpi, new PngBitmapEncoder(), ".png");

    /// <summary>A JPEG file of the given size in pixels.</summary>
    public string Jpeg(int width, int height) => Write(width, height, 96, new JpegBitmapEncoder(), ".jpg");

    /// <summary>A file named like a picture that holds text instead.</summary>
    public string NotAnImage()
    {
        var path = Path.Combine(_directory, $"{Guid.NewGuid():N}.png");
        File.WriteAllText(path, "This is not a picture.");
        return path;
    }

    /// <summary>The width and height in pixels of the picture in <paramref name="image"/>.</summary>
    public static (int Width, int Height) SizeOf(byte[] image)
    {
        var frame = Decode(image);
        return (frame.PixelWidth, frame.PixelHeight);
    }

    /// <summary>The dots per inch across and down of the picture in <paramref name="image"/>.</summary>
    public static (double DpiX, double DpiY) DpiOf(byte[] image)
    {
        var frame = Decode(image);
        return (Math.Round(frame.DpiX), Math.Round(frame.DpiY));
    }

    /// <summary>Whether <paramref name="image"/> starts with the PNG file signature.</summary>
    public static bool IsPng(byte[] image) =>
        image.AsSpan().StartsWith((byte[])[0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A]);

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    private static BitmapFrame Decode(byte[] image)
    {
        using var stream = new MemoryStream(image);
        return BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames[0];
    }

    private string Write(int width, int height, double dpi, BitmapEncoder encoder, string extension)
    {
        // A diagonal gradient, so the picture isn't one flat colour.
        var pixels = new byte[width * height * 4];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var i = ((y * width) + x) * 4;
                pixels[i] = (byte)(x * 255 / width);
                pixels[i + 1] = (byte)(y * 255 / height);
                pixels[i + 2] = 128;
            }
        }

        var bitmap = BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Bgr32, null, pixels, width * 4);
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        var path = Path.Combine(_directory, $"{Guid.NewGuid():N}{extension}");
        using var file = File.Create(path);
        encoder.Save(file);
        return path;
    }
}
