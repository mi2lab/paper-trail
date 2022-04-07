using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;

public class SampleSceneInitialize : MonoBehaviour
{
    public ARDoodle TorqueArrow, RodA, RodB, Hint1, Hint2;
    public VideoController GyroscopeVideo;
    public AudioSource Audio;
    public BookmarkController VideoBookmark;

    void Start()
    {
        TorqueArrow.InitializeDoodle(0, ImageCapture.CaptureType.Outline);
        RodA.InitializeDoodle(1, ImageCapture.CaptureType.Outline);
        RodB.InitializeDoodle(2, ImageCapture.CaptureType.Outline);

        Hint1.InitializeDoodle(3, ImageCapture.CaptureType.Photo);
        Hint2.InitializeDoodle(4, ImageCapture.CaptureType.Photo);

        DoodleManager.Instance.AddPrecreatedDoodles(0, TorqueArrow);
        DoodleManager.Instance.AddPrecreatedDoodles(1, RodA);
        DoodleManager.Instance.AddPrecreatedDoodles(2, RodB);
        DoodleManager.Instance.AddPrecreatedDoodles(3, Hint1);
        DoodleManager.Instance.AddPrecreatedDoodles(4, Hint2);

        // add clipping masks to hints
        GameObject mask1 = ClippingMaskManager.Instance.AddMasksForPrecreatedDoodle(Hint1, Hint1.transform.parent);
        GameObject mask2 = ClippingMaskManager.Instance.AddMasksForPrecreatedDoodle(Hint2, Hint2.transform.parent);

        // set scale and position of the clipping masks so they're centered over "HINT"
        mask1.transform.localPosition = new Vector3(6.63f, 2.12f, mask1.transform.position.z);
        mask1.transform.localScale = new Vector3(0.46f, 0.37f, 0.874f);
        mask2.transform.localPosition = new Vector3(12.1f, 2.12f, mask2.transform.position.z);
        mask2.transform.localScale = new Vector3(0.46f, 0.37f, 0.874f);

        // video bookmark config
        VideoBookmark.SetCheckpointTime(17.0);
        VideoBookmark.videoController = GyroscopeVideo;
        BookmarkManager.Instance.AddPrecreatedDoodles(VideoBookmark.gameObject);

        // audio config
        AudioManager.Instance.AddPrecreatedAudio(Audio.gameObject, Audio.clip);
        Audio.Play();
        Audio.Pause();
    }
}
