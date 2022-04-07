// SOURCE: NextGen Video Recorder example code

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pmjo.NextGenRecorder;
using UnityEngine.Video;
using Lean.Touch;

public class VideoManager : MonoBehaviour
{
    // ***** Singleton *****
    public static VideoManager Instance { get; private set; }


    public GameObject VideoPrefab;
    public GameObject VideoToolbar;         // toolbar for deleting videos

   // private List<GameObject> videoObjects;
    private List<GameObject> selectedVideos;

    //private string microphone;

    private GameObject videoRecording;  // temp variable, will be reassigned for each new video
    //private AudioSource audioRecording; // temp variable, will be reassigned for each new video

    private bool isRecording = false;
    private float startDelaySeconds = 0.1f;
    //private string filepath;

    public bool isDeletingBookmarks = false; // meant to solve the error where the video is deleted before its bookmarks

    private void Awake()
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

        selectedVideos = new List<GameObject>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            OnRecordButtonPressed();
        }
    }

    public void OnRecordButtonPressed()
    {
        if (!Recorder.IsSupported)
        {
            Debug.LogError("Video Recorder is not supported");
            return;
        }

        isRecording = !isRecording;
        if (isRecording)    // starting recording
        {
            ImageCapture.Instance.HighlightCaptureButton(true);

            videoRecording = Instantiate(VideoPrefab, DoodleManager.Instance.GetActiveDoodleParent().parent.Find("Videos"));
            videoRecording.SetActive(false);    // hide until we've finished recording the video
            MicrophoneRecorder.Instance.audioSource = videoRecording.GetComponent<AudioSource>();
            // record audio
            //audioRecording = videoRecording.transform.Find("VideoPlayer").GetComponent<AudioSource>();
            //audioRecording.clip = Microphone.Start(microphone, true, 10, 44100);

            StartCoroutine(RecordVideo());
        }
        else   // stopping recording
        {
            //Microphone.End(microphone);
            ImageCapture.Instance.HighlightCaptureButton(false);
            AppManager.Instance.SetAppState(AppManager.AppState.SelectDefault);
        }
    }

    public void SelectVideo(GameObject video)
    {
        if (!selectedVideos.Contains(video)) { selectedVideos.Add(video); }
    }

    public void DeselectVideo(GameObject video)
    {
        if (selectedVideos.Contains(video)) { selectedVideos.Remove(video); }
    }

    public void DeselectAllVideos()
    {
        List<GameObject> selectedVideosCopy = new List<GameObject>();
        foreach (GameObject video in selectedVideos) { selectedVideosCopy.Add(video); }

        foreach (GameObject video in selectedVideosCopy) { video.GetComponent<LeanSelectable>().Deselect(); }
        selectedVideos.Clear();
        HideVideoToolbar();
    }

    public void DeleteVideos()
    {
        isDeletingBookmarks = true;
        StartCoroutine(DeleteVideosAndBookmarks());
    }

   private IEnumerator DeleteVideosAndBookmarks()
    {
        isDeletingBookmarks = true;

        List<GameObject> selectedVideosCopy = new List<GameObject>();
        foreach (GameObject video in selectedVideos)
        {
            selectedVideosCopy.Add(video);
            BookmarkManager.Instance.DeleteAllBookmarksForVideo(video.transform.GetChild(0).GetComponent<VideoController>()); // delete corresponding bookmarks
        }

        yield return new WaitUntil(() => isDeletingBookmarks == false);

        foreach (GameObject video in selectedVideosCopy)
        {
            Destroy(video);
        }
        selectedVideos.Clear();
    }

    public void ShowVideoToolbar() { VideoToolbar.SetActive(true); }

    public void HideVideoToolbar() {
        if (selectedVideos.Count == 0)
        {
            VideoToolbar.SetActive(false);
        }
    }

    public void ReparentVideos(Transform newParent)
    {
        Debug.Log("Reparenting videos!");
        foreach (GameObject video in selectedVideos)
        {
            video.transform.SetParent(newParent);
        }
    }

    #region NextGen Recorder stuff
    private void OnEnable()
    {
        Recorder.RecordingStarted += RecordingStarted;
        Recorder.RecordingStopped += RecordingStopped;
        Recorder.RecordingExported += RecordingExported;
    }

    private void OnDisable()
    {
        Recorder.RecordingStarted -= RecordingStarted;
        Recorder.RecordingStopped -= RecordingStopped;
        Recorder.RecordingExported -= RecordingExported;
    }

    private IEnumerator RecordVideo()
    {
        yield return new WaitUntil(() => isRecording);
        Recorder.PrepareRecording();

        yield return new WaitForSeconds(startDelaySeconds);

        Recorder.StartRecording();

        yield return new WaitUntil(() => !isRecording);

        Recorder.StopRecording();
    }

    private void RecordingStarted(long sessionId)
    {
        Debug.Log("Recording " + sessionId + " was started.");
    }

    private void RecordingStopped(long sessionId)
    {
        Debug.Log("Recording " + sessionId + " was stopped.");

        Recorder.ExportRecordingSession(sessionId);
    }

    void RecordingExported(long sessionId, string path, Recorder.ErrorCode errorCode)
    {
        if (errorCode == Recorder.ErrorCode.NoError)
        {
            Debug.Log("Recording exported to " + path + ", session id " + sessionId);

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            //CopyFileToDesktop(path, "MyAwesomeRecording.mp4");
#elif UNITY_IOS || UNITY_TVOS
            PlayVideo(path);
#endif

            //string timeStamp = Time.time.ToString().Replace(".", "").Replace(":", "");
            //string filename = string.Format("TestVideo_{0}.mp4", timeStamp);
            //filepath = System.IO.Path.Combine(Application.persistentDataPath, filename);
            //filepath = filepath.Replace("/",

            // spawn a new video player
            VideoPlayer videoPlayer = videoRecording.transform.Find("VideoPlayer").GetComponent<VideoPlayer>();
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = path;

            //videoPlayer.GetComponent<AudioSource>().clip = MicrophoneRecorder.Instance.audioSource.clip;
            videoPlayer.audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, videoPlayer.GetComponent<AudioSource>());

            //videoObjects.Add(videoRecording);  // save to list
            videoRecording.SetActive(true);

            // Sharing.SaveToPhotos(path);
        }
        else
        {
            Debug.Log("Failed to export recording, error code " + errorCode + ", session id " + sessionId);
        }
    }

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    private static void CopyFileToDesktop(string path, string fileName)
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string dstPath = Path.Combine(desktopPath, fileName);

        File.Copy(path, dstPath, true);

        Debug.Log("Recording " + fileName + " copied to the desktop");
    }

#elif UNITY_IOS || UNITY_TVOS
    private static void PlayVideo(string path)
    {
        if (!path.Contains("file://"))
        {
            path = "file://" + path;
        }

        Handheld.PlayFullScreenMovie(path);
    }

#endif
    #endregion
}
