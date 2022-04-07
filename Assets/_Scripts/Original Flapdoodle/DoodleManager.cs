using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using UnityEngine.UI;
using static AppManager;
using static ImageCapture;

public class DoodleManager : MonoBehaviour
{
    // ***** Singleton *****
    public static DoodleManager Instance { get; private set; }

    //public GameObject SceneContent;
    public List<Transform> DoodleParents;
    private List<Transform> activeDoodleParents; // markers that are currently tracked
    private Transform activeDoodleParent;   //  most recently tracked marker

    public GameObject ARDoodlePrefab;

    public GameObject AnimateFunctionsToolbar;

    // key: ID, value: doodle GameObject
    static Dictionary<int, GameObject> arDoodles;
    static int nextDoodleID = 0;

    public List<GameObject> selectedDoodles;   // which doodles are currently selected

    public Button RecordButton;
    private ColorBlock recordingColors; // for highlighting the recording button
    private ColorBlock defaultColors;

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

        arDoodles = new Dictionary<int, GameObject>();
        activeDoodleParents = new List<Transform>();
    }

    private void Start()
    {
        //arDoodles = new Dictionary<int, GameObject>();
        //activeDoodleParents = new List<Transform>();
        selectedDoodles = new List<GameObject>();

        activeDoodleParent = DoodleParents[0];  // make sure this is never null
        activeDoodleParent.parent.Find("ActiveMarkerLabel").gameObject.SetActive(true);

        defaultColors = RecordButton.colors;    // initialize ColorBlocks for highlighting the button
        recordingColors = RecordButton.colors;
        recordingColors.normalColor = recordingColors.selectedColor;
        recordingColors.highlightedColor = recordingColors.selectedColor;
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.R)) { ReparentDoodles(DoodleParents[0]); }
    //}

    /// <summary> Creates a doodle and places it inside the SceneContent object </summary>
    public void CreateDoodle(ref Texture2D tex, CaptureType captureMode)
    {
        Debug.Log("Creating doodle! ID: " + nextDoodleID);

        // instantiate doodle object
        GameObject doodle = Instantiate(ARDoodlePrefab, activeDoodleParent.position, activeDoodleParent.rotation, activeDoodleParent.transform);

        // create a new sprite and assign it to object
        Sprite doodleSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        SpriteRenderer spriteRenderer = doodle.transform.GetChild(0).GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = doodleSprite;

        // save it to list
        ARDoodle doodleScript = doodle.GetComponent<ARDoodle>();
        doodleScript.InitializeDoodle(nextDoodleID, captureMode);
        doodleScript.SetDoodleParent(activeDoodleParent);

        selectedDoodles.Add(doodle);    // doodle is selected when it's first created
        arDoodles.Add(nextDoodleID, doodle);

        ++nextDoodleID;

        doodle.GetComponent<LeanSelectable>().Select();
        AppManager.Instance.SetAppState(AppState.Selected);
    }

    public GameObject GetDoodle(int id)
    {
        if (arDoodles.ContainsKey(id)) { return arDoodles[id]; }

        else
        {
            Debug.Log("No doodle with ID " + id.ToString());
            return null;
        }
    }

    #region ******************** SELECT OPERATIONS ********************
    public void SelectDoodle(GameObject doodle)
    {
        if (!selectedDoodles.Contains(doodle))
        {
            selectedDoodles.Add(doodle);
        }

        if (AppManager.Instance.GetAppState() == AppState.SelectDefault) {
            AppManager.Instance.SetAppState(AppState.Selected);

            //HighlightActiveDoodleParent(null);  // activeDoodleParent hasn't changed
        }
    }

    public void DeselectDoodle(GameObject doodle) {
        if (selectedDoodles.Contains(doodle)) {
            selectedDoodles.Remove(doodle);
        }
        //HighlightActiveDoodleParent(null);  // activeDoodleParent hasn't changed
    }

    /// <summary> Deselect all doodles and save their positions </summary>
    public void PlaceAllDoodles(bool changeAppState = true)
    {
        List<GameObject> selectedDoodlesCopy = new List<GameObject>();
        foreach (GameObject doodle in selectedDoodles) { selectedDoodlesCopy.Add(doodle); }

        Debug.Log("Placing doodles!");
        foreach (GameObject doodle in selectedDoodlesCopy)
        {
            doodle.GetComponent<LeanSelectable>().Deselect();
            doodle.GetComponent<ARDoodle>().SetDoodlePosition(doodle.transform.localPosition);
        }

        selectedDoodles.Clear();
        if (changeAppState) { AppManager.Instance.SetAppState(AppState.SelectDefault); }
    }

    /// <summary> Resets position </summary>
    public void CancelDoodleTranslation()
    {
        foreach (GameObject doodle in selectedDoodles)
        {
            ARDoodle doodleScript = doodle.GetComponent<ARDoodle>();
            if (doodleScript.HasDoodleBeenPlaced())
            {
                doodle.GetComponent<LeanSelectable>().Deselect();
                doodle.GetComponent<ARDoodle>().ResetDoodlePosition();
            }
            else  { DeleteDoodle(doodleScript.GetDoodleID()); } // delete doodle if we've never placed it yet (coming here straight from capture)
        }

        selectedDoodles.Clear();
        AppManager.Instance.SetAppState(AppState.SelectDefault);
    }

    public void DuplicateSelectedDoodles()
    {
        Debug.Log("Duplicating selected doodles!");
        List<ARDoodle> duplicatedDoodles = new List<ARDoodle>();
        foreach (GameObject doodle in selectedDoodles)
        {
            Transform parent = doodle.GetComponent<ARDoodle>().GetDoodleParent(); ;
            GameObject copy = Instantiate(doodle, parent);
            copy.transform.localPosition = parent.position;

            // save it to list
            ARDoodle doodleScript = copy.GetComponent<ARDoodle>();
            doodleScript.InitializeDoodle(nextDoodleID, doodle.GetComponent<ARDoodle>().GetCaptureType());
            arDoodles.Add(nextDoodleID, copy);
            duplicatedDoodles.Add(doodleScript);

            ++nextDoodleID;
        }

        foreach (ARDoodle doodleScript in duplicatedDoodles) { doodleScript.OnDoodleSelect(); }
    }

    public void DeleteSelectedDoodles()
    {
        Debug.Log("Deleting selected doodles!");
        List<GameObject> selectedDoodlesCopy = new List<GameObject>();
        foreach (GameObject doodle in selectedDoodles) { selectedDoodlesCopy.Add(doodle); }

        foreach (GameObject doodle in selectedDoodlesCopy)
        {
            ARDoodle doodleScript = doodle.GetComponent<ARDoodle>();
            DeleteDoodle(doodleScript.GetDoodleID());
        }

        selectedDoodles.Clear();
    }

    private void DeleteDoodle(int id)
    {
        if (arDoodles.ContainsKey(id)) {
            ClippingMaskManager.Instance.DeleteMasksWithDoodle(id);
            Destroy(arDoodles[id]);
            arDoodles.Remove(id); 
        }

        else { Debug.LogError("No doodle with ID " + id.ToString()); }
    }

    public void FlipSelectedDoodleAxes()    // makes doodles either pop out of page or be aligned horizontally with marker
    {
        Debug.Log("Flipping selected doodles!");

        foreach (GameObject doodle in selectedDoodles)
        {
            Debug.Log("x: " + doodle.transform.localRotation.eulerAngles.x + " y: " + doodle.transform.localRotation.eulerAngles.y);
            if (doodle.transform.localRotation.eulerAngles.x > 250) // flip to perpencidular to marker along y (270 degrees)
            {
                Debug.Log("Axis 2");
                doodle.transform.localRotation = Quaternion.Euler(0, -90, 0);

            }
            else if (doodle.transform.localRotation.eulerAngles.y > 250)  // normal position – parallel to marker
            {
                Debug.Log("Axis 0");
                doodle.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            else // flip to perpencidular to marker along X
            {
                Debug.Log("Axis 1");
                doodle.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            }

            //if (doodle.transform.localRotation.x < 0) // we were rotated along X, now rotate along Y (both perpendicular) (2)
            //{
            //    doodle.transform.localRotation *= Quaternion.Euler(90, -90, 0);

            //}
            ////else if (doodle.transform.localRotation.y < 0)  // back to parallel with marker (0)
            ////{
            ////    doodle.transform.localRotation *= Quaternion.Euler(-90, 90, 0);
            ////}
            //else // we were parallel, now flip to perpencidular to marker along X axis (1)
            //{
            //    doodle.transform.localRotation *= Quaternion.Euler(-90, 0, 0);
            //}
        }
    }
    #endregion


    #region ******************** ANIMATIONS ********************
    public void OnRecordClick()
    {
        Debug.Log("OnRecordClick: " + selectedDoodles.Count);

        RecordButton.colors = recordingColors;

        foreach (GameObject doodle in selectedDoodles) {
            doodle.GetComponent<ARDoodleAnimationManager>().OnRecord();
        }
    }

    public void OnStopClick()
    {
        foreach (GameObject doodle in selectedDoodles)
        {
            doodle.GetComponent<ARDoodleAnimationManager>().OnStop();
        }

        UpdateAnimateFunctionsToolbar(AppManager.Instance.GetAppState());
        RecordButton.colors = defaultColors;
    }

    public void UpdateAnimateFunctionsToolbar(AppState newState)
    {
        bool hasAnimation = false;
        foreach (GameObject doodle in selectedDoodles)
        {
            if (newState == AppState.AnimateTranslation)
            {
                hasAnimation = doodle.GetComponent<ARDoodleAnimationManager>().HasTranslationAnimation() ? true : hasAnimation;
            }
            else if (newState == AppState.AnimateRotation)
            {
                hasAnimation = doodle.GetComponent<ARDoodleAnimationManager>().HasRotationAnimation() ? true : hasAnimation;
            }
        }

        if (hasAnimation)
        {
            AnimateFunctionsToolbar.transform.Find("RecordButton").gameObject.SetActive(false);
            AnimateFunctionsToolbar.transform.Find("StopButton").gameObject.SetActive(false);
            AnimateFunctionsToolbar.transform.Find("DeleteButton").gameObject.SetActive(true);
        }
        else
        {
            AnimateFunctionsToolbar.transform.Find("RecordButton").gameObject.SetActive(true);
            AnimateFunctionsToolbar.transform.Find("StopButton").gameObject.SetActive(true);
            AnimateFunctionsToolbar.transform.Find("DeleteButton").gameObject.SetActive(false);
        }
    }

    public void DeselectSelectedAnimations()
    {
        List<GameObject> doodlesToDeselect = new List<GameObject>();  // make a copy so we can loop without error
        foreach (GameObject doodle in selectedDoodles)
        {
            doodlesToDeselect.Add(doodle);
        }
        foreach (GameObject doodle in doodlesToDeselect)
        {
            doodle.GetComponent<LeanSelectable>().Deselect();
            //doodle.GetComponent<ARDoodle>().OnDoodleDeselect();
        }
        selectedDoodles.Clear();
    }

    public void DeleteSelectedAnimations()
    {
        foreach (GameObject doodle in selectedDoodles)
        {
            ARDoodleAnimationManager animManager = doodle.GetComponent<ARDoodleAnimationManager>();
            if (animManager) { animManager.DeleteAnimation(); }
        }
        DeselectSelectedAnimations();
    }

    public void PlayAllAnimations()
    {
        Debug.Log("playing all animations");
        foreach (Transform doodleParent in DoodleParents)
        {
            for (int i = 0; i < doodleParent.childCount; ++i)
            {
                ARDoodleAnimationManager animManager = doodleParent.GetChild(i).GetComponent<ARDoodleAnimationManager>();
                if (animManager) { animManager.RestartAnimationReplay(); }
            }
        }
    }

    public void StopAllAnimations()
    {
        Debug.Log("stopping all animations");
        foreach (Transform doodleParent in DoodleParents)
        {
            for (int i = 0; i < doodleParent.childCount; ++i)
            {
                ARDoodleAnimationManager animManager = doodleParent.GetChild(i).GetComponent<ARDoodleAnimationManager>();
                if (animManager) { animManager.StopAnimationReplay(); }
            }
        }
    }

    public void ToggleAllPivots(bool show)
    {
        foreach (GameObject doodle in selectedDoodles)
        {
            ARDoodleAnimationManager animManager = doodle.GetComponent<ARDoodleAnimationManager>();
            if (animManager)
            {
                animManager.Pivot.SetActive(show);
            }
        }
    }

    public void ToggleComponentsForRotationAnimation()
    {
        foreach (GameObject doodle in selectedDoodles)
        {
            doodle.GetComponent<LeanDragTranslateLocalAxis>().enabled = !doodle.GetComponent<LeanDragTranslateLocalAxis>().enabled;
            doodle.GetComponent<LeanTwistRotateAxis>().enabled = !doodle.GetComponent<LeanTwistRotateAxis>().enabled;
            doodle.GetComponent<LeanTwistRotatePivot>().enabled = !doodle.GetComponent<LeanTwistRotatePivot>().enabled;
        }
    }
    #endregion

    #region ******************** DOODLE PARENTING ********************
    public Transform GetActiveDoodleParent() { return activeDoodleParent; }

    public void AddDoodleParent(Transform parent)
    {
        if (activeDoodleParents.Contains(parent))
        {
            //Debug.Log("Something went wrong :P The marker is already in the list!!");
            return;
        }
        activeDoodleParents.Add(parent);

        //Transform oldParent = activeDoodleParent;
        activeDoodleParent.parent.Find("ActiveMarkerLabel").gameObject.SetActive(false);
        activeDoodleParent = parent;  // this becomes the new active parent, as it is the most recently tracked marker
        activeDoodleParent.parent.Find("ActiveMarkerLabel").gameObject.SetActive(true);
        //HighlightActiveDoodleParent(oldParent, null);
    }

    public void RemoveDoodleParent(Transform parent)
    {
        if (!activeDoodleParents.Contains(parent))
        {
            //Debug.Log("Something went wrong :P The marker was never tracked!!");
            return;
        }
        activeDoodleParents.Remove(parent);

        if (activeDoodleParent == parent)   // need to set a new parent
        {
            if (activeDoodleParents.Count > 0)  // set activeDoodleParent to the last element in this list (most recently tracked marker)
            {
                //Transform oldParent = activeDoodleParent;
                activeDoodleParent = activeDoodleParents[activeDoodleParents.Count - 1];
                activeDoodleParent.parent.Find("ActiveMarkerLabel").gameObject.SetActive(true);
                //HighlightActiveDoodleParent(oldParent, parent);
            }
            else
            {
                Debug.Log("We have no active parents, so we're keeping the same one");
            }
        }
    }

    public void ReparentDoodles(Transform newParent)
    {
        Debug.Log("Reparenting doodles!");
        foreach (GameObject doodle in selectedDoodles)
        {
            doodle.GetComponent<ARDoodle>().SetDoodleParent(newParent);
        }

        // HighlightActiveDoodleParent(activeDoodleParent);
    }

    //private void HighlightActiveDoodleParent(Transform oldActiveParent, Transform untrackedMarker = null)
    //{
    //    Debug.Log("Active doodle parent: " + activeDoodleParent.transform.parent.name);

    //    if (oldActiveParent) { oldActiveParent.transform.parent.GetComponent<MarkerController>().SetDeselectedColor(); }
    //    if (untrackedMarker) { untrackedMarker.transform.parent.GetComponent<MarkerController>().SetDeselectedColor(); }
    //    if (selectedDoodles.Count > 0)
    //    {
    //        Debug.Log("SelectedDoodles count:" + selectedDoodles.Count);
    //        activeDoodleParent.transform.parent.GetComponent<MarkerController>().SetSelectedColor();
    //    }
    //}
    #endregion

    // FOR THE SAMPLE SCENE (doodles are hard coded in Unity, but need to add them to the arDoodles dictionary so they can be deleted)
    public void AddPrecreatedDoodles(int id, ARDoodle doodleScript)
    {
        arDoodles.Add(id, doodleScript.gameObject);
        nextDoodleID = ++id;
    }
}


