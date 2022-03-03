using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;
using static AppManager;

public class ARDoodleAnimationManager : MonoBehaviour
{
    public GameObject DoodleSprite;
    public GameObject Pivot;
    public GameObject ParticleSystem;
    private ParticleSystem particles;

    private List<Vector3> outerTranslations;   // operates on the parent objects

    private List<Vector3> innerTranslations;    // operates on the DoodleSprite object
    private List<Quaternion> innerRotations;    // operates on the DoodleSprite object

    private Coroutine translationCoroutine;
    private Coroutine rotationCoroutine;

    private bool record;
    private bool replay;

    private void Start()
    {
        outerTranslations = new List<Vector3>();
        innerTranslations = new List<Vector3>();
        innerRotations = new List<Quaternion>();

        particles = ParticleSystem.GetComponent<ParticleSystem>();
    }

    public void OnRecord()
    {
        if (AppManager.Instance.GetAppState() == AppState.AnimateTranslation)
        {
            outerTranslations.Clear();

            Debug.Log("Recording translation animation!");
            translationCoroutine = StartCoroutine(RecordTranslationAnimation());
            record = true;
        }
        else if (AppManager.Instance.GetAppState() == AppState.AnimateRotation)
        {

            innerRotations.Clear();
            innerTranslations.Clear();

            Debug.Log("Recording rotation animation!");
            rotationCoroutine = StartCoroutine(RecordRotationAnimation());
            record = true;
        }
        GetComponent<LeanPinchScale>().enabled = false; // disable scaling
    }

    public void OnStop()
    {
        record = false;

        if (AppManager.Instance.GetAppState() == AppState.AnimateTranslation)
        {
            translationCoroutine = StartCoroutine(ReplayTranslationAnimation());
            replay = true;
        }
        else if (AppManager.Instance.GetAppState() == AppState.AnimateRotation)
        {
            rotationCoroutine = StartCoroutine(ReplayRotationAnimation());
        }
        GetComponent<LeanPinchScale>().enabled = true; // enable scaling
    }

    private IEnumerator RecordTranslationAnimation()
    {
        yield return new WaitUntil(() => record);

        while (record)
        {
            outerTranslations.Add(transform.localPosition);
            yield return null;
        }

        transform.localPosition = outerTranslations[0];

        Debug.Log("Number of keyframes: " + outerTranslations.Count);
    }

    private IEnumerator ReplayTranslationAnimation()
    {
        yield return new WaitUntil(() => replay);
        yield return new WaitForSeconds(1f);    // wait a bit before replaying
        Debug.Log("Replaying translation!");

        int numKeyframes = outerTranslations.Count;
        int keyframeCount = 0;
        while (replay)
        {
            transform.localPosition = outerTranslations[keyframeCount];
            keyframeCount++;

            if (keyframeCount == numKeyframes) { keyframeCount = 0; }

            yield return null;
        }
    }

    private IEnumerator RecordRotationAnimation()
    {
        Pivot.GetComponent<LeanDragTranslateLocalAxis>().enabled = false;    // don't want to be moving the pivot here

        yield return new WaitUntil(() => record);

        while (record)
        {
            innerTranslations.Add(DoodleSprite.transform.localPosition);
            innerRotations.Add(DoodleSprite.transform.localRotation);
            yield return null;
        }

        DoodleSprite.transform.localRotation = innerRotations[0];   // reset the doodle to where it was before
        DoodleSprite.transform.localPosition = innerTranslations[0];  // TODO: test if these need to be localPosition / localRotations

        Pivot.GetComponent<LeanDragTranslateLocalAxis>().enabled = true;   // enable moving the pivot again
    }

    private IEnumerator ReplayRotationAnimation()
    {
        yield return new WaitUntil(() => replay);
        yield return new WaitForSeconds(1f);    // wait a bit before replaying
        Debug.Log("Replaying rotation!");

        int numKeyframes = innerRotations.Count;
        int keyframeCount = 0;
        while (replay)
        {
            DoodleSprite.transform.localRotation = innerRotations[keyframeCount]; // TODO: test if these need to be localPosition / localRotations
            DoodleSprite.transform.localPosition = innerTranslations[keyframeCount];
            keyframeCount++;

            if (keyframeCount == numKeyframes) { keyframeCount = 0; }

            yield return null;
        }
    }

    public bool HasTranslationAnimation() { Debug.Log("HasTranslationAnimation"); return outerTranslations.Count > 0; }
    public bool HasRotationAnimation() { return innerRotations.Count > 0; }

    public void DeleteAnimation() { StartCoroutine(DeleteAnimationForState()); }

    private IEnumerator DeleteAnimationForState()
    {
        Debug.Log("Deleting animations");
        if (AppManager.Instance.GetAppState() == AppState.AnimateTranslation)
        {
            replay = false;
            transform.localPosition = outerTranslations[0];   // reset original position
            outerTranslations.Clear();
            translationCoroutine = null;
        }

        else if (AppManager.Instance.GetAppState() == AppState.AnimateRotation)
        {
            replay = false;
            DoodleSprite.transform.localRotation = innerRotations[0];    // reset original rotation
            DoodleSprite.transform.localPosition = innerTranslations[0];    // reset original position
            innerRotations.Clear();
            innerTranslations.Clear();
            rotationCoroutine = null;
            Pivot.SetActive(false);
        }

        yield return new WaitForEndOfFrame();
        RestartAnimationReplay();
        DoodleManager.Instance.UpdateAnimateFunctionsToolbar(AppManager.Instance.GetAppState());
    }

    public void StopAnimationReplay()
    {
        replay = false;
        translationCoroutine = null;
        rotationCoroutine = null;
        Debug.Log("Stopping animation replay");

        if (HasTranslationAnimation() || HasRotationAnimation()) { particles.Play(); }
    }

    public void RestartAnimationReplay()
    {
        if (HasTranslationAnimation()) { translationCoroutine = StartCoroutine(ReplayTranslationAnimation()); }
        if (HasRotationAnimation()) { rotationCoroutine = StartCoroutine(ReplayRotationAnimation()); }

        replay = true;
        Debug.Log("Restarting animation replay");

        particles.Pause();
        particles.Clear();
    }
}
