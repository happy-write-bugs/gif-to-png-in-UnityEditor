using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using System.IO;

public class GifToPngTool
{
    #region ToPNG
    [MenuItem("Assets/GifToPNG/ToPNG", priority = 666, secondaryPriority = 1)]
    public static void ToPNG()
    {
        var gifPaths = GetSelectedGifPaths();
        foreach (var gifPath in gifPaths)
        {
            var frames = ToPNG(gifPath);
            Debug.Log($"成功:{gifPath}[{frames}]");
        }

        AssetDatabase.Refresh();
    }

    private static int ToPNG(string gifPath)
    {
        var gifImage = Image.FromFile(gifPath, true);
        var dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
        int frameCount = gifImage.GetFrameCount(dimension);
        
        for (int i = 0; i < frameCount; i++)
        {
            gifImage.SelectActiveFrame(dimension, i);
            var frame = new Bitmap(gifImage.Width, gifImage.Height);
            System.Drawing.Graphics.FromImage(frame).DrawImage(gifImage, Point.Empty);
            var frameTexture = new Texture2D(frame.Width, frame.Height, TextureFormat.RGBA32, false);
            for (int x = 0; x < frame.Width; x++)
            {
                for (int y = 0; y < frame.Height; y++)
                {
                    System.Drawing.Color sourceColor = frame.GetPixel(x, frame.Height - 1 - y);
                    frameTexture.SetPixel(x, y, new Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A));
                }
            }
            frameTexture.Apply();

            byte[] bytes = ImageConversion.EncodeToPNG(frameTexture);
            Object.DestroyImmediate(frameTexture);
            File.WriteAllBytes(gifPath.Replace(".gif", $"_{i}.png"), bytes);
        }

        return frameCount;
    }
    #endregion ToPNG

    #region ToPNG_AdaptiveSize
    [MenuItem("Assets/GifToPNG/ToPNG_AdaptiveSize", priority = 666, secondaryPriority = 2)]
    public static void ToPNG_AdaptiveSize()
    {
        var gifPaths = GetSelectedGifPaths();
        foreach (var gifPath in gifPaths)
        {
            var frames = ToPNG_AdaptiveSize(gifPath);
            Debug.Log($"成功:{gifPath}[{frames}]");
        }

        AssetDatabase.Refresh();
    }

    private static int ToPNG_AdaptiveSize(string gifPath)
    {
        var gifImage = Image.FromFile(gifPath, true);
        var dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
        int frameCount = gifImage.GetFrameCount(dimension);
        var tmpTexture2DList = new List<Texture2D>();

        Vector2Int min = new Vector2Int(gifImage.Width, gifImage.Height);
        Vector2Int max = Vector2Int.zero;
        for (int i = 0; i < frameCount; i++)
        {
            gifImage.SelectActiveFrame(dimension, i);
            var frame = new Bitmap(gifImage.Width, gifImage.Height);
            System.Drawing.Graphics.FromImage(frame).DrawImage(gifImage, Point.Empty);
            var frameTexture = new Texture2D(frame.Width, frame.Height, TextureFormat.RGBA32, false);
            for (int x = 0; x < frame.Width; x++)
            {
                for (int y = 0; y < frame.Height; y++)
                {
                    System.Drawing.Color sourceColor = frame.GetPixel(x, frame.Height - 1 - y);
                    frameTexture.SetPixel(x, y, new Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A));

                    if (sourceColor.A > 0f)
                    {
                        min.x = Mathf.Min(min.x, x);
                        min.y = Mathf.Min(min.y, y);
                        max.x = Mathf.Max(max.x, x);
                        max.y = Mathf.Max(max.y, y);
                    }
                }
            }
            frameTexture.Apply();
            tmpTexture2DList.Add(frameTexture);
        }

        var pngSize = max - min + Vector2Int.one;
        for (int i = 0; i < tmpTexture2DList.Count; i++)
        {
            var tmpTexture2D = tmpTexture2DList[i];
            var rightSizeTexture2D = new Texture2D(pngSize.x, pngSize.y, TextureFormat.RGBA32, false);
            for (int x = 0; x < rightSizeTexture2D.width; x++)
            {
                for (int y = 0; y < rightSizeTexture2D.height; y++)
                {
                    var color = tmpTexture2D.GetPixel(x + min.x, y+ min.y);
                    rightSizeTexture2D.SetPixel(x, y, color);
                }
            }
            rightSizeTexture2D.Apply();

            byte[] bytes = ImageConversion.EncodeToPNG(rightSizeTexture2D);
            Object.DestroyImmediate(rightSizeTexture2D);
            File.WriteAllBytes(gifPath.Replace(".gif", $"_{i}.png"), bytes);
        }

        foreach (var texture in tmpTexture2DList)
        {
            Object.DestroyImmediate(texture);
        }

        return frameCount;
    }
    #endregion ToPNG_AdaptiveSize

    public static HashSet<string> GetSelectedGifPaths()
    {
        // 获得明确选中的对象,包括文件夹
        var guids = Selection.assetGUIDs;
        var explicitPaths = new List<string>();
        for (int i = 0; i < guids.Length; i++)
        {
            explicitPaths.Add(AssetDatabase.GUIDToAssetPath(guids[i]));
        }

        // 找出选中的gif路径，包括被选中文件加下面所有的gif路径
        var paths = new HashSet<string>();
        var allAssetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (var path in allAssetPaths)
        {
            if (!path.EndsWith(".gif"))
                continue;

            foreach (var exPath in explicitPaths)
            {
                if (path.Contains(exPath))
                    paths.Add(path);
            }
        }

        return paths;
    }
}
