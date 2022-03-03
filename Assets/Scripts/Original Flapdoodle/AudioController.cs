using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public GameObject PlaybackButtons;
    public SpriteRenderer PlayPauseButtonRenderer;
    public TextMeshPro PlayPauseButtonText;
    public Sprite PlaySprite;
    public Sprite PauseSprite;

    private AudioSource recording;

    public GameObject SelectedQuad;
    public Material selectedMaterial;
    public Material deselectedMaterial;

    void Start()
    {
        recording = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            OnPlayButtonPressed();
        }
    }

    public void OnPlayButtonPressed()
    {
        if (!recording) { Debug.LogError("RECORDING NULL"); }
        if (recording.isPlaying)
        {
            Debug.Log("pausing recording");
            recording.Pause();
            PlayPauseButtonRenderer.sprite = PlaySprite;
            PlayPauseButtonText.text = "Play";
        }
        else
        { 
            Debug.Log("playing recording");
            recording.UnPause();
            PlayPauseButtonRenderer.sprite = PauseSprite;
            PlayPauseButtonText.text = "Pause";
        }
    }

    public void OnStopButtonPressed()
    {
        if (recording.isPlaying)
        {
            recording.Pause();
            recording.time = 0f;
            PlayPauseButtonRenderer.sprite = PlaySprite;
            PlayPauseButtonText.text = "Play";
        }
    }

    public void OnRestartButtonPressed()
    {
        recording.time = 0f;
    }

    public void OnAudioSelect()
    {
        if (SelectedQuad)
        {
            SelectedQuad.GetComponent<MeshRenderer>().material = selectedMaterial;
            AudioManager.Instance.SelectAudio(this.gameObject);
            AudioManager.Instance.ShowAudioToolbar();
        }
    }

    public void OnAudioDeselect()
    {
        if (SelectedQuad)
        {
            SelectedQuad.GetComponent<MeshRenderer>().material = deselectedMaterial;
            AudioManager.Instance.DeselectAudio(this.gameObject);
            AudioManager.Instance.HideAudioToolbar();
        }
    }
}