using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using pmjo.NextGenRecorder;
// using pmjo.NextGenRecorder.Sharing;

namespace pmjo.Examples
{
    public class WebCamRecorder : Recorder.VideoRecorderBase
    {
        public enum CameraType
        {
            FrontFacing,
            BackFacing
        }

        public enum CameraPlaybackType
        {
            AspectFill,
            AspectFit
        }

        public enum RecordingFrameRate
        {
            ApplicationFrameRate,
            CameraFrameRate
        }

        public enum RecordingSize
        {
            CroppedToScreenAspect,
            ScreenSize
        }

        public Sprite spriteOn;
        public Sprite spriteOff;
        public Button recordingButton;
        public Button switchCameraButton;
        public Material blitMaterial;
        public RawImage cameraFeed;

        public int preferredCameraWidth = 1920;
        public int preferredCameraHeight = 1080;
        public int preferredCameraFrameRate = 30;
        public CameraType preferredCameraType = CameraType.BackFacing;

        public CameraPlaybackType cameraPlaybackType = CameraPlaybackType.AspectFit;
        public RecordingFrameRate recordingFrameRate = RecordingFrameRate.CameraFrameRate;
        public RecordingSize recordingSize = RecordingSize.CroppedToScreenAspect;
        public int applicationFrameRate = 30;

        private WebCamTexture mWebCamTexture;
        private string mWebCamDeviceName = null;
        private RenderTexture mRecordingTexture;
        private Image mRecordingButtonImage;
        private Vector4 mBlitSize = new Vector4(1, 1, 0, 0);
        private int mBlitSizeProperty;
        private int mBlitAngleProperty;
        private bool mIsFrontFacing;

        void Awake()
        {
            Application.targetFrameRate = applicationFrameRate;

            // Try to get preferred camera type
            bool requestFrontFacing = (preferredCameraType == CameraType.FrontFacing);
            mWebCamDeviceName = TryGetCamera(requestFrontFacing, out mIsFrontFacing);

            if (mWebCamDeviceName == null)
            {
                Debug.Log("Failed to get camera");
                return;
            }

            mRecordingButtonImage = recordingButton.GetComponent<Image>();

            // Get shader property ids
            mBlitSizeProperty = Shader.PropertyToID("_Size");
            mBlitAngleProperty = Shader.PropertyToID("_Angle");

            // Enable switch camera button if both front camera and back camera found
            switchCameraButton.interactable = HasCamera(true) && HasCamera(false);

            // Enable recording button if recording supported
            recordingButton.interactable = Recorder.IsSupported;

            // Set recorder framerate
            Recorder.TargetFrameRate = (recordingFrameRate == RecordingFrameRate.ApplicationFrameRate) ? Application.targetFrameRate : preferredCameraFrameRate;
            Recorder.FrameSkipping = false;
        }

        IEnumerator Start()
        {
            if (mWebCamTexture == null && mWebCamDeviceName != null)
            {
                yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

                // Create webcam texture if camera permission is granted
                if (Application.HasUserAuthorization(UserAuthorization.WebCam))
                {
                    mWebCamTexture = new WebCamTexture(preferredCameraWidth, preferredCameraHeight, preferredCameraFrameRate);
                    mWebCamTexture.deviceName = mWebCamDeviceName;
                    mWebCamTexture.Play();
                }
                else
                {
                    Debug.Log("No camera permission, not starting");
                }
            }
        }

        void OnEnable()
        {
            Recorder.RecordingStarted += RecordingStarted;
            Recorder.RecordingStopped += RecordingStopped;
            Recorder.RecordingExported += RecordingExported;
        }

        void OnDisable()
        {
            Recorder.RecordingStopped -= RecordingStopped;
            Recorder.RecordingStarted -= RecordingStarted;
            Recorder.RecordingExported -= RecordingExported;
        }

        private bool mWasPlaying = false;

        void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                mWasPlaying = mWebCamTexture.isPlaying;

                if (mWasPlaying)
                {
                    mWebCamTexture.Stop();
                }
            }
            else
            {
                if (mWasPlaying)
                {
                    mWebCamTexture.Play();
                }
            }
        }

        private void RecordingStarted(long sessionId)
        {
            mRecordingButtonImage.sprite = spriteOn;
        }

        private void RecordingStopped(long sessionId)
        {
            mRecordingButtonImage.sprite = spriteOff;

            Recorder.ExportRecordingSession(sessionId);
        }

        private void RecordingExported(long sessionId, string path, Recorder.ErrorCode errorCode)
        {
            if (errorCode == Recorder.ErrorCode.NoError)
            {
                Debug.Log("Recording exported to " + path + ", session id " + sessionId);

    #if UNITY_EDITOR_OSX ||  UNITY_STANDALONE_OSX
                CopyFileToDesktop(path, "MyAwesomeRecording.mp4");
    #elif UNITY_IOS ||  UNITY_TVOS
                PlayVideo(path);
    #endif

                // Or save to photos using the Sharing API (triggers save to file dialog on macOS)
                // Remember to uncomment using pmjo.NextGenRecorder.Sharing at the top of the file
                // Sharing.SaveToPhotos(path, "My Awesome Album");

                // Or share using the Sharing API (only available on iOS)
                // Sharing.ShowShareSheet(path, true);
            }
            else
            {
                Debug.Log("Failed to save video: " + errorCode.ToString());
            }
        }

        void Update()
        {
            if (WebCamTextureNotInitialized())
            {
                return;
            }

            float angle = mWebCamTexture.videoRotationAngle;

            // Get source orientation and size
            bool srcIsLandscape = mWebCamTexture.videoRotationAngle == 0 || (mWebCamTexture.videoRotationAngle % 180) == 0;
            int srcWidth, srcHeight;

            if (srcIsLandscape)
            {
                srcWidth = mWebCamTexture.width;
                srcHeight = mWebCamTexture.height;
            }
            else
            {
                srcWidth = mWebCamTexture.height;
                srcHeight = mWebCamTexture.width;
            }

            // Get destination orientation and size
            int dstWidth = Screen.width;
            int dstHeight = Screen.height;

            // Calculate aspect fit or fill
            CalculateBlitSize(srcWidth, srcHeight, dstWidth, dstHeight, ref mBlitSize);

            if (recordingSize == RecordingSize.CroppedToScreenAspect)
            {
                dstWidth = Mathf.RoundToInt(srcWidth * (1 / mBlitSize.x));
                dstHeight = Mathf.RoundToInt(srcHeight * (1 / mBlitSize.y));
            }

            // Debug.Log("CAMERA ANGLE " + mWebCamTexture.videoRotationAngle + " MIRROR " + mWebCamTexture.videoVerticallyMirrored);
            // Debug.Log("CAMERA WIDTH " + mWebCamTexture.width + " HEIGHT " + mWebCamTexture.height);

            // Flip camera feed if needed
            if (mWebCamTexture.videoVerticallyMirrored)
            {
                if (srcIsLandscape)
                {
                    mBlitSize.y *= -1;
                }
                else
                {
                    mBlitSize.x *= -1;
                }
            }

            // Mirror front facing camera
            if (mIsFrontFacing)
            {
                mBlitSize.x *= -1;
            }

            // Create recording texture that will be correctly oriented
            if (mRecordingTexture == null || mRecordingTexture.width != dstWidth || mRecordingTexture.height != dstHeight)
            {
                mRecordingTexture = new RenderTexture(dstWidth, dstHeight, 0, RenderTextureFormat.Default);
                // mRecordingTexture.filterMode = FilterMode.Bilinear;
                mRecordingTexture.Create();

                cameraFeed.texture = mRecordingTexture;
                cameraFeed.enabled = true;
            }

            // Set shader parameters
            blitMaterial.SetVector(mBlitSizeProperty, mBlitSize);
            blitMaterial.SetFloat(mBlitAngleProperty, Mathf.Deg2Rad * angle);

            // Set recording texture if not yet set
            if (RecordingTexture != mRecordingTexture)
            {
                RecordingTexture = mRecordingTexture;
            }

            // Blit webcam texture to the recording texture
            Graphics.Blit(mWebCamTexture, mRecordingTexture, blitMaterial);

            if (recordingFrameRate == RecordingFrameRate.ApplicationFrameRate || (mWebCamTexture.isPlaying && mWebCamTexture.didUpdateThisFrame))
            {
                BlitRecordingTexture();
            }
        }

        private void CalculateBlitSize(float srcWidth, float srcHeight, float dstWidth, float dstHeight, ref Vector4 blitSize)
        {
            float screenAspect = dstWidth / dstHeight;
            float textureAspect = srcWidth / srcHeight;

            if ((cameraPlaybackType == CameraPlaybackType.AspectFit) ^ (screenAspect > textureAspect))
            {
                blitSize.x = 1.0f;
                blitSize.y = screenAspect * (1.0f / textureAspect);
            }
            else
            {
                blitSize.x = (1.0f / screenAspect) * textureAspect;
                blitSize.y = 1.0f;
            }
        }

        public void RecordingButtonClick()
        {
            if (WebCamTextureNotInitialized())
            {
                return;
            }

            if (!Recorder.IsRecording)
            {
                Recorder.StartRecording();
            }
            else
            {
                Recorder.StopRecording();
            }
        }

        public void SwitchCameraButtonClick()
        {
            mWebCamTexture.Stop();
            mWebCamDeviceName = TryGetCamera(!mIsFrontFacing, out mIsFrontFacing);
            mWebCamTexture.deviceName = mWebCamDeviceName;
            mWebCamTexture.Play();
        }

        private bool WebCamTextureNotInitialized()
        {
            return mWebCamTexture == null || mWebCamTexture.width <= 16 || mWebCamTexture.height <= 16;
        }

        private bool HasCamera(bool frontFacing)
        {
            WebCamDevice[] devices = WebCamTexture.devices;

            for (int i = 0; i < devices.Length; i++)
            {
                if (devices[i].isFrontFacing == frontFacing)
                {
                    return true;
                }
            }

            return false;
        }

        private string TryGetCamera(bool frontFacing, out bool isFrontFacing)
        {
            WebCamDevice[] devices = WebCamTexture.devices;

            // Find preferred camera type
            for (int i = 0; i < devices.Length; i++)
            {
                if (devices[i].isFrontFacing == frontFacing)
                {
                    isFrontFacing = devices[i].isFrontFacing;
                    return devices[i].name;
                }
            }

            // If preferred type was not found, return first camera
            if (devices.Length > 0)
            {
                isFrontFacing = devices[0].isFrontFacing;
                return devices[0].name;
            }

            // If there is no camera, return null
            isFrontFacing = frontFacing;
            return null;
        }

    #if UNITY_EDITOR_OSX ||  UNITY_STANDALONE_OSX
        private static void CopyFileToDesktop(string path, string fileName)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string dstPath = Path.Combine(desktopPath, fileName);

            File.Copy(path, dstPath, true);

            Debug.Log("Recording " + fileName + " copied to the desktop");
        }

    #elif UNITY_IOS ||  UNITY_TVOS
        private static void PlayVideo(string path)
        {
            if (!path.Contains("file://"))
            {
                path = "file://" + path;
            }

            Handheld.PlayFullScreenMovie(path);
        }

    #endif
    }
}
