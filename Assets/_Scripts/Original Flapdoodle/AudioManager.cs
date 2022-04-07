using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using static AppManager;
using static ImageCapture;

public class AudioManager : MonoBehaviour
{
    // ***** Singleton *****
    public static AudioManager Instance { get; private set; }

    public GameObject AudioPrefab;
    public GameObject AudioToolbar;

    private string microphone;
    private List<GameObject> audioObjects;
    private List<AudioClip> audioClips;

    private List<GameObject> selectedAudio;

    //public SpriteRenderer AudioIcon;
    //public Color selectedColor, deselectedColor;


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
        audioObjects = new List<GameObject>();
        audioClips = new List<AudioClip>();
        selectedAudio = new List<GameObject>();

        if (Microphone.devices.Length > 0)
        {
            microphone = Microphone.devices[0];
            Debug.Log("MICROPHONE NAME: " + microphone);
        }
        else { Debug.LogError("No microphones found"); }
    }

    public void OnRecordButtonPress()
    {
        if (AppManager.Instance.GetAppState() != AppState.Capture || ImageCapture.Instance.GetCaptureMode() != CaptureType.Audio) return;
        if (microphone == null) { return; }

        Debug.Log("OnRecordingButtonPress");
        if (Microphone.IsRecording(microphone))  // stop recording
        {
            Debug.Log("Stopping recording");
            Microphone.End(microphone);
            //ImageCapture.Instance.HighlightCaptureButton(false);
            AddAudioRecording();
        }

        else  // start recording
        {
            Debug.Log("Starting recording");
            AudioClip newClip = Microphone.Start(microphone, true, 10, 44100);
            audioClips.Add(newClip);

            //ImageCapture.Instance.HighlightCaptureButton(true);
        }
    }

    public void AddAudioRecording()
    {
        Transform activeDoodleParent = DoodleManager.Instance.GetActiveDoodleParent();
        GameObject newRecording = Instantiate(AudioPrefab, activeDoodleParent.position, activeDoodleParent.rotation, activeDoodleParent.transform);
        AudioSource audioSource = newRecording.GetComponent<AudioSource>();
        audioSource.clip = audioClips[audioClips.Count - 1];

        // sort of a silly workaround to the UnPause function :P 
        audioSource.Play();
        audioSource.Pause();

        audioObjects.Add(newRecording);

        AppManager.Instance.SetAppState(AppManager.AppState.SelectDefault);
    }

    public void DeleteAudioClip(GameObject audio)
    {
        if (audioObjects.Contains(audio))
        {
            audioObjects.Remove(audio);
            audioClips.Remove(audio.GetComponent<AudioSource>().clip);
            Destroy(audio);
        }
        else Debug.LogError("Audio clip not in list");
    }

    public void DeleteAudioClips()   // when delete button in UI is pressed
    {
        List<GameObject> selectedAudioCopy = new List<GameObject>();
        foreach (GameObject audio in selectedAudio) { selectedAudioCopy.Add(audio); }

        foreach (GameObject audio in selectedAudioCopy)
        {
            audioObjects.Remove(audio);
            Destroy(audio);
        }
        selectedAudio.Clear();
    }

    // LEAN TOUCH stuff
    public void SelectAudio(GameObject audio)
    {
        if (!selectedAudio.Contains(audio)) { selectedAudio.Add(audio); }
    }

    public void DeselectAudio(GameObject audio)
    {
        if (selectedAudio.Contains(audio)) { selectedAudio.Remove(audio); }
    }

    public void DeselectAllAudio()
    {
        List<GameObject> selectedAudioCopy = new List<GameObject>();
        foreach (GameObject audio in selectedAudio) { selectedAudioCopy.Add(audio); }

        foreach (GameObject audio in selectedAudioCopy) { audio.GetComponent<LeanSelectable>().Deselect(); }
        selectedAudio.Clear();
        HideAudioToolbar();
    }

    public void ShowAudioToolbar() { AudioToolbar.SetActive(true); }

    public void HideAudioToolbar()
    {
        if (selectedAudio.Count == 0)
        {
            AudioToolbar.SetActive(false);
        }
    }

    // FOR THE SAMPLE SCENE (doodles are hard coded in Unity, but need to add them to the audio list so they can be deleted)
    public void AddPrecreatedAudio(GameObject audioObj, AudioClip audioClip)
    {
        audioClips.Add(audioClip);
        audioObjects.Add(audioObj);
    }
}