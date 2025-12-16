using UnityEngine;
using SkiaSharp;
using Svg.Skia;

public static class SvgRenderer
{
    public static Texture2D SvgToTexture(string xmlCode)
    {
        var svg = new SKSvg();
        svg.FromSvg(xmlCode);

        int width = (int)svg.Picture.CullRect.Width;
        int height = (int)svg.Picture.CullRect.Height;

        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var bitmap = new SKBitmap(info);
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(SKColors.Transparent);
        canvas.Scale(1, -1);
        canvas.Translate(0, -height);
        canvas.DrawPicture(svg.Picture);
        canvas.Flush();

        int byteCount = bitmap.ByteCount;
        byte[] raw = new byte[byteCount];
        System.Runtime.InteropServices.Marshal.Copy(bitmap.GetPixels(), raw, 0, byteCount);

        Texture2D texture = new(width, height, TextureFormat.RGBA32, false);
        texture.LoadRawTextureData(bitmap.Bytes);
        texture.Apply();

        return texture;
    }
}
