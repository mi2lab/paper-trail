using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ImageCapture : MonoBehaviour
{
    public enum CaptureType { Photo, Outline, Video, Audio }
    private CaptureType captureMode = CaptureType.Photo;

    public Button PhotoButton, OutlineButton, VideoButton, AudioButton;
    public CaptureGuideController captureGuideController;
    public GameObject CaptureButton, AcceptCropButton;
    private Button captureButtonScript;

    public Image ImagePreview;

    // ***** Singleton *****
    public static ImageCapture Instance { get; private set; }

    public float GrayscaleThreshold = 0.25f;

    private Vector2Int startingPoint, imageDimensions; // starting point, image dimensions in pixels – set by CaptureGuideController

    private int leftBound, rightBound, topBound, bottomBound; // dimensions of collider (like cropped image)

    public Color selectedColor, deselectedColor, recordingColor;

    private ColorBlock selectedColors, deselectedColors, recordingColors;

    public void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        // initialize ColorBlocks for highlighting the selected mode button
        selectedColors = PhotoButton.colors;
        selectedColors.normalColor = selectedColor;

        deselectedColors = OutlineButton.colors;
        deselectedColors.normalColor = deselectedColor;

        captureButtonScript = CaptureButton.GetComponent<Button>();
        recordingColors = captureButtonScript.colors;
        recordingColors.normalColor = recordingColor;
    }

    private void Update()   // FOR TESTING!!! :P 
    {
        if (Input.GetKey(KeyCode.C)) { TakeScreenshot(); }  
    }

    public void SetDimensions(Vector2Int newStart, Vector2Int newDim) {
        startingPoint = newStart;
        imageDimensions = newDim;
    }

    public CaptureType GetCaptureMode() { return captureMode; }

    public void SetCaptureMode(string modeString)
    {
        if (System.Enum.TryParse(modeString, out CaptureType newMode))
        {
            if (newMode != captureMode)
            {
                captureMode = newMode;
            }
            else { Debug.Log("Mode is already " + modeString); }
        }
        else { Debug.LogError("Mode " + modeString + " doesn't exist"); }

        SelectModeButton();

        // TODO TODO TODO fix this :D different for audio, and different now that we crop after taking pic
        if (captureMode != CaptureType.Video) { // need to show these elements if we hid them for Video mode
            //foreach (GameObject go in AppManager.Instance.CaptureGuides) { go.SetActive(true); }
        }
        else {  // hide these elements for video
            //foreach (GameObject go in AppManager.Instance.CaptureGuides) { go.SetActive(false); }
        }
    }

    public void SelectModeButton()
    {
        if (captureMode == CaptureType.Photo) {
            PhotoButton.colors = selectedColors;
            OutlineButton.colors = deselectedColors;
            VideoButton.colors = deselectedColors;
            AudioButton.colors = deselectedColors;
        }
        else if (captureMode == CaptureType.Outline) {
            PhotoButton.colors = deselectedColors;
            OutlineButton.colors = selectedColors;
            VideoButton.colors = deselectedColors;
            AudioButton.colors = deselectedColors;
        }
        else if (captureMode == CaptureType.Video) {
            PhotoButton.colors = deselectedColors;
            OutlineButton.colors = deselectedColors;
            VideoButton.colors = selectedColors;
            AudioButton.colors = deselectedColors;
        }
        else // audio
        {
            PhotoButton.colors = deselectedColors;
            OutlineButton.colors = deselectedColors;
            VideoButton.colors = deselectedColors;
            AudioButton.colors = selectedColors;
        }
    }

    public void HighlightCaptureButton(bool highlight) // make the capture button red for recording video / audio
    {
        if (highlight) { recordingColors.normalColor = recordingColor; }
        else { recordingColors.normalColor = deselectedColor; }

        CaptureButton.GetComponent<Button>().colors = recordingColors;
    }

    public void OnCaptureButtonClick()
    {
        switch(captureMode)
        {
            case CaptureType.Photo:
            case CaptureType.Outline:
                TakeScreenshot();
                break;
            case CaptureType.Video:
                VideoManager.Instance.OnRecordButtonPressed();
                break;
            case CaptureType.Audio:
                AudioManager.Instance.OnRecordButtonPress();
                break;
        }
    }

    public void TakeScreenshot()
    {
        Debug.Log("Taking screenshot!");
        StartCoroutine(CaptureScreenshot());
    }

    private IEnumerator CaptureScreenshot()
    {
        // hide UI elements while we take the pic
        AppManager.Instance.ToggleMainToolbar(false);
        AppManager.Instance.ToggleCaptureToolbar(false);
        AppManager.Instance.ToggleCaptureButton(false);

        yield return new WaitForEndOfFrame();

        Texture2D screenImage = new Texture2D(Screen.width, Screen.height);
        screenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
        screenImage.Apply();

        // create a sprite for the image preview
        Sprite imagePreview = Sprite.Create(screenImage, new Rect(0, 0, screenImage.width, screenImage.height),
            new Vector2(0.5f, 0.5f));
        ImagePreview.sprite = imagePreview;
        ImagePreview.gameObject.SetActive(true);

        AppManager.Instance.ToggleCaptureGuides(true);    // bring back capture guide
        AppManager.Instance.ToggleCaptureButton(true);
        CaptureButton.SetActive(false); // hide the capture button, show the button for accepting the crop
        AcceptCropButton.SetActive(true);
        //AppManager.Instance.ToggleMainToolbar(true);  // don't want to do this yet, force them to crop first

        captureGuideController.ResetCaptureGuide(); // resize guide to original dimensions
    }

    // Source: https://unitycoder.com/blog/2014/11/11/image-to-grayscale-script/
    public void CropAndRemoveBackground()  // called by CaptureGuide button
    {
        // crop (before doing background removal for Outline mode)
        // TODO fix this!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! :D
        //int xDim = (int)Mathf.Floor(ImageDimensions.x);
        //int yDim = (int)Mathf.Floor(ImageDimensions.y);
        //int xStart = (int)Mathf.Floor((Screen.width - xDim) / 2);
        //int yStart = (int)Mathf.Floor((Screen.height - yDim) / 2);

        // copy texture from the Image Preview
        Color[] screenImageCrop1 = ImagePreview.sprite.texture.GetPixels(startingPoint.x, startingPoint.y,
            imageDimensions.x, imageDimensions.y);
        Texture2D screenImageCropped = new Texture2D(imageDimensions.x, imageDimensions.y);
        screenImageCropped.SetPixels(screenImageCrop1);
        screenImageCropped.Apply();

        // now do background removal
        if (captureMode == CaptureType.Outline)
        {
            // image dimensions to set the collider
            leftBound = -1;
            bottomBound = screenImageCropped.height;
            rightBound = screenImageCropped.width;
            topBound = -1;

            Color32[] pixels = screenImageCropped.GetPixels32();
            for (int x = 0; x < screenImageCropped.width; x++)
            {
                for (int y = 0; y < screenImageCropped.height; y++)
                {
                    Color32 pixel = pixels[x + y * screenImageCropped.width];
                    int p = ((256 * 256 + pixel.r) * 256 + pixel.b) * 256 + pixel.g;
                    int b = p % 256;
                    p = Mathf.FloorToInt(p / 256);
                    int g = p % 256;
                    p = Mathf.FloorToInt(p / 256);
                    int r = p % 256;
                    float l = (0.2126f * r / 255f) + 0.7152f * (g / 255f) + 0.0722f * (b / 255f);

                    // if color is greater than the threshold, make this pixel transparent
                    //Color c = l > GrayscaleThreshold ? new Color(l, l, l, 0) : new Color(l, l, l, 1);

                    // if color is greater than the threshold, make this pixel transparent
                    // set black pixels to white, so we can change sprite color later
                    Color c;
                    if (l > GrayscaleThreshold) { c = new Color(l, l, l, 0); }
                    else
                    {
                        c = new Color(1, 1, 1, 1); // set black pixels to white, so we can change sprite color later

                        if (leftBound < 0) { leftBound = x; }   // set left bound, since we've found a colored pixel
                        rightBound = x; // keep setting rightBound as long as we find colored pixels
                        if (y < bottomBound) { bottomBound = y; }
                        if (y > topBound) { topBound = y; }
                    }
                    //Color c = l > GrayscaleThreshold ? new Color(l, l, l, 0) : new Color(1, 1, 1, 1);   // set black pixels to white, so we can change sprite color later
                    screenImageCropped.SetPixel(x, y, c);
                }
            }

            // crop again
            Color[] screenImageCrop2 = screenImageCropped.GetPixels(leftBound, bottomBound, rightBound - leftBound, topBound - bottomBound);
            Texture2D newImage = new Texture2D(rightBound - leftBound, topBound - bottomBound);
            newImage.SetPixels(screenImageCrop2);
            newImage.Apply();
            screenImageCropped = newImage;
        }
        else
        {
            screenImageCropped.Apply();
        }

        ImagePreview.gameObject.SetActive(false);   // hide the image preview
        AppManager.Instance.ToggleMainToolbar(true);
        CaptureButton.SetActive(true); // hide the accept crop button, switch back to capture button
        AcceptCropButton.SetActive(false);

        DoodleManager.Instance.CreateDoodle(ref screenImageCropped, captureMode);  // create doodle
    }
}
