using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using static AppManager;
using static ImageCapture;

public class ARDoodle : MonoBehaviour
{
    public GameObject DoodleSprite;
    private SpriteRenderer doodleRenderer;  // child of this object
    private CaptureType captureType;

    private int doodleID;

    private Vector3 doodlePosition; // localPosition
    private bool hasDoodleBeenPlacedBefore = false;  // i'm so great at naming things

    public Color regularColorPhoto = Color.white;
    public Color regularColorOutline = Color.black;
    public Color highlightColor = new Color(0.99216f, 0.6902f, 0.8549f);

    // for doodle transitions
    public AnimationCurve movementTimeCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
    public float movementDuration = 2.0f;

    private Transform doodleParent;

    private bool hasMaskInteraction;    // clipping masks

    public void InitializeDoodle(int newID, CaptureType captureMode)
    {
        doodleRenderer = DoodleSprite.GetComponent<SpriteRenderer>();
        SetDoodleID(newID);
        SetColliderSize();
        captureType = captureMode;
    }

    public int GetDoodleID() { return doodleID; }

    private void SetDoodleID(int newID) { doodleID = newID; }

    public void OnDoodleSelect() 
    {
        DoodleManager.Instance.SelectDoodle(gameObject);
        SetHighlightColor();
    }

    public void OnDoodleDeselect() 
    {
        if (AppManager.Instance.GetAppState() == AppState.AnimateDefault || AppManager.Instance.GetAppState() == AppState.Selected)
        {
            DoodleManager.Instance.DeselectDoodle(gameObject);
        }
        SetRegularColor();
    }

    public Vector3 GetDoodlePosition() { return doodlePosition; }

    public bool HasDoodleBeenPlaced() { return hasDoodleBeenPlacedBefore; }

    public void SetDoodlePosition(Vector3 newPosition) 
    {
        doodlePosition = newPosition;
        hasDoodleBeenPlacedBefore = true;
    }

    public void ResetDoodlePosition() { transform.localPosition = doodlePosition; }

    private void SetColliderSize()
    {
        Vector2 spriteSize = doodleRenderer.sprite.bounds.size;
        BoxCollider col = gameObject.GetComponent<BoxCollider>();
        col.size = new Vector3(spriteSize.x, spriteSize.y, 0.1f);

        Debug.Log("Sprite size: " + spriteSize.x.ToString() + " " + spriteSize.y.ToString());
    }

    public void SetRegularColor() {
        if (captureType == CaptureType.Outline)
        {
            doodleRenderer.color = regularColorOutline;
        }
        else { doodleRenderer.color = regularColorPhoto; }
    }

    public void SetHighlightColor() { doodleRenderer.color = highlightColor; }

    public CaptureType GetCaptureType() { return captureType; }

    public Transform GetDoodleParent() { return doodleParent; }

    public void SetDoodleParent(Transform newParent) {
        doodleParent = newParent;
        transform.SetParent(doodleParent);
        StartCoroutine(DoodleMovementAnimation(newParent));
        //transform.position = doodleParent.position;
        //transform.rotation = doodleParent.rotation;
    }

    // SOURCE: https://forum.unity.com/threads/a-smooth-ease-in-out-version-of-lerp.28312/
    private IEnumerator DoodleMovementAnimation(Transform endpoint)
    {
        Debug.Log("Starting movement!");
        float animationTime = 0;
        var startPos = this.transform.position;
        var startRot = this.transform.rotation;
        var endPos = endpoint.position;
        var endRot = endpoint.rotation;
        var animationTimeLength = movementTimeCurve[movementTimeCurve.length - 1].time;

        while (animationTime < animationTimeLength)
        {
            animationTime += (Time.deltaTime / movementDuration);
            this.transform.position = Vector3.Lerp(startPos, endPos, movementTimeCurve.Evaluate(animationTime));
            this.transform.rotation = Quaternion.Slerp(startRot, endRot, movementTimeCurve.Evaluate(animationTime));
            yield return null;
        }

        yield return null;

        // make sure we got to the position
        transform.position = endpoint.position;
        transform.rotation = endpoint.rotation;
        Debug.Log("Finished movement!");
    }

    #region Clipping mask interaction
    public void AddClippingMaskInteraction()
    {
        hasMaskInteraction = true;
        doodleRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
    }

    public void RemoveClippingMaskInteraction()
    {
        hasMaskInteraction = false;
        doodleRenderer.maskInteraction = SpriteMaskInteraction.None;
    }

    //private IEnumerator OpacityAnimation(Color fromColor, Color toColor, bool isSelected)
    //{
    //    float elapsedTime = 0f;
    //    float waitTime = 1f;

    //    while (elapsedTime < waitTime)
    //    {
    //        doodleRenderer.color = Color.Lerp(fromColor, toColor, elapsedTime / waitTime);
    //        elapsedTime += Time.deltaTime;

    //        yield return null;
    //    }
    //    doodleRenderer.color = toColor;
    //    yield return null;

    //    if (isSelected) { doodleRenderer.maskInteraction = SpriteMaskInteraction.None; }
    //    else { doodleRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask; }
    //}
    #endregion
}
