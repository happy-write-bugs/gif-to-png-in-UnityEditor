#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using UnityEditor;
using UnityEngine;

public class GifToPngTest : MonoBehaviour
{
    public float speed = 1;

    private string loadingGifPath;
    private Vector2 drawPosition;
    private UnityEngine.UI.RawImage raw;
    private List<Texture2D> gifFrames = new List<Texture2D>();
    public Object Gif = null;

    void OnEnable()
    {
        gifFrames.Clear();
        raw = GetComponent<UnityEngine.UI.RawImage>();

        loadingGifPath = AssetDatabase.GetAssetPath(this.Gif);
        drawPosition = transform.position;
        var gifImage = Image.FromFile(loadingGifPath, true);
        var dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
        int frameCount = gifImage.GetFrameCount(dimension);
        Vector2Int min = new Vector2Int(gifImage.Width, gifImage.Height);
        Vector2Int max = Vector2Int.zero;

        var unitRef = GraphicsUnit.Pixel;
        var rectf = gifImage.GetBounds(ref unitRef);
        Debug.Log(dimension);
        Debug.Log(rectf);
        
        for (int i = 0; i < frameCount; i++)
        {
            gifImage.SelectActiveFrame(dimension, i);
            var frame = (Bitmap)gifImage.Clone(); //new Bitmap(gifImage.Width, gifImage.Height);
            //System.Drawing.Graphics.FromImage(frame).DrawImage(gifImage, Point.Empty);
            //var frameTexture = new Texture2D(frame.Width, frame.Height);

            var unitRef2 = GraphicsUnit.Pixel;
            var rectf2 = frame.GetBounds(ref unitRef);
            Debug.Log(rectf);
            //return;
            for (int x = 0; x < frame.Width; x++)
                for (int y = 0; y < frame.Height; y++)
                {
                    System.Drawing.Color sourceColor = frame.GetPixel(x, frame.Height - 1 - y);
                    //frameTexture.SetPixel(x, y, new Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A));

                    if (sourceColor.A > 0f)
                    {
                        min.x = Mathf.Min(min.x, x);
                        min.y = Mathf.Min(min.y, y);
                        max.x = Mathf.Max(max.x, x);
                        max.y = Mathf.Max(max.y, y);
                    }
                }
            //frameTexture.Apply();
            //gifFrames.Add(frameTexture);
        }

        var pngSize = max - min + Vector2Int.one;
        for (int i = 0; i < frameCount; i++)
        {
            gifImage.SelectActiveFrame(dimension, i);
            var frame = (Bitmap)gifImage.Clone(); //new Bitmap(gifImage.Width, gifImage.Height);
            //System.Drawing.Graphics.FromImage(frame).DrawImage(gifImage, Point.Empty);
            var frameTexture = new Texture2D(pngSize.x, pngSize.y);
            for (int x = min.x; x < max.x + 1; x++)
                for (int y = min.y; y < max.y + 1; y++)
                {
                    System.Drawing.Color sourceColor = frame.GetPixel(x, max.y - y);
                    frameTexture.SetPixel(x - min.x, y - min.y, new Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A));
                    //frameTexture.SetPixel(x - min.x, y - min.y,
                    //    new Color32((byte)(((float)x * byte.MaxValue) / (max.x + 1f)), byte.MaxValue, byte.MaxValue, byte.MaxValue));
                }
            frameTexture.Apply();
            gifFrames.Add(frameTexture);
        }

        Debug.Log($"min:{min} max:{max} size:{max - min}");
    }

    public IEnumerator PlayGif()
    {
        var waitForSeconds = new WaitForSeconds(this.speed);
        for (int i = 0; i < gifFrames.Count; i++)
        {
            raw.texture = gifFrames[i];
            yield return waitForSeconds;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            //播放一次
            StartCoroutine(PlayGif());
        }
        //循环播放
        //var ok = gifFrames[(int)(Time.frameCount * speed) % gifFrames.Count];
    }
}
#endif //UNITY_EDITOR