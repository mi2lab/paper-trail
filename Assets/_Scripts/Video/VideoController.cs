using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Lean.Touch;

public class VideoController : MonoBehaviour
{
    public GameObject PlayIcon;
    public GameObject PauseIcon;

    public Material selectedMaterial;
    public Material deselectedMaterial;

    private VideoPlayer videoPlayer;
    private AudioSource audioSource;

    private void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        audioSource = GetComponent<AudioSource>();

        //transform.parent.GetComponent<LeanSelectable>().Select();   // want this to be selected when first created
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.P))
    //    {
    //        OnPlaybackButtonClick();
    //    }
    //    if (Input.GetKeyDown(KeyCode.B))
    //    {
    //        BookmarkManager.Instance.AddBookmark(GetComponent<VideoController>());
    //    }
    //}

    public void OnPlaybackButtonClick()
    {
        Debug.Log("Toggling video playback");
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
        else
        {
            videoPlayer.Play();
        }

        PlayIcon.SetActive(!PlayIcon.activeSelf);
        PauseIcon.SetActive(!PauseIcon.activeSelf);
    }

    public double GetVideoTime()
    {
        return videoPlayer.time;
    }

    public void SetVideoTime(double newTime)
    {
        videoPlayer.time = newTime;
    }

    public void OnVideoSelect()
    {
        if (videoPlayer)
        {
            videoPlayer.GetComponent<MeshRenderer>().material = selectedMaterial;
            VideoManager.Instance.SelectVideo(transform.parent.gameObject);
            VideoManager.Instance.ShowVideoToolbar();
        }
    }

    public void OnVideoDeselect()
    {
        if (videoPlayer)
        {
            videoPlayer.GetComponent<MeshRenderer>().material = deselectedMaterial;
            VideoManager.Instance.DeselectVideo(transform.parent.gameObject);
            VideoManager.Instance.HideVideoToolbar();
        }
    }

    public void OnBookmarkButtonClick()
    {
        BookmarkManager.Instance.AddBookmark(GetComponent<VideoController>());
    }
}