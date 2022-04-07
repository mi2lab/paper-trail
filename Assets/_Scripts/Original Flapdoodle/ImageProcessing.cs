using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImageProcessing : MonoBehaviour
{
    // FOR TESTING
    private string IMAGE_FILENAME;

    public GameObject SceneContent;
    public GameObject ARDoodlePrefab;

    public float GrayscaleThreshold = 0.25f;

    private Sprite grayscaleSprite;
    Texture2D grayscaleTexture;

    void Start()
    {
        IMAGE_FILENAME = Application.dataPath + "/Images/piggy.png";
        LoadImage();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Creating grayscale image");
            ConvertToGrayscale();
        }
    }

    private void LoadImage()
    {
        byte[] imageBytes = File.ReadAllBytes(IMAGE_FILENAME);

        grayscaleTexture = new Texture2D(2, 2);
        grayscaleTexture.LoadImage(imageBytes);

        // create a new sprite and assign it to object
        GameObject doodle = Instantiate(ARDoodlePrefab, SceneContent.transform);

        grayscaleSprite = Sprite.Create(grayscaleTexture, new Rect(0, 0, grayscaleTexture.width, grayscaleTexture.height), new Vector2(0.5f, 0.5f));
        doodle.GetComponent<SpriteRenderer>().sprite = grayscaleSprite;
    }


    // Source: https://unitycoder.com/blog/2014/11/11/image-to-grayscale-script/
    private void ConvertToGrayscale()
    {
        Color32[] pixels = grayscaleTexture.GetPixels32();
        for (int x = 0; x < grayscaleTexture.width; x++)
        {
            for (int y = 0; y < grayscaleTexture.height; y++)
            {
                Color32 pixel = pixels[x + y * grayscaleTexture.width];
                int p = ((256 * 256 + pixel.r) * 256 + pixel.b) * 256 + pixel.g;
                int b = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int g = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int r = p % 256;
                float l = (0.2126f * r / 255f) + 0.7152f * (g / 255f) + 0.0722f * (b / 255f);

                // if color is greater than the threshold, make this pixel transparent
                Color c = l > GrayscaleThreshold ? new Color(l, l, l, 0) : new Color(l, l, l, 1);
                grayscaleTexture.SetPixel(x, y, c);
            }
        }
        grayscaleTexture.Apply(true);
        var bytes = grayscaleTexture.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "grayscale.png", bytes);
    }
}
