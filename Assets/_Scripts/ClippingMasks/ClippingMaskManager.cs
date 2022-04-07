using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Lean.Touch;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

public class ClippingMaskManager : MonoBehaviour
{
    // ***** Singleton *****
    public static ClippingMaskManager Instance { get; private set; }

    public GameObject ClippingMaskPrefab;

    //public List<GameObject> DoodleParent;
    public Dictionary<GameObject, int> ClippingMasks;   // key: mask GameObject, value: doodleID

    public bool editMasks = false;
    public bool deleteMasks = false;

    public Button EditButton;
    public Button DeleteButton;

    public Color selectedColor;
    public Color deselectedColor;


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
    }

    void Start()
    {
        ClippingMasks = new Dictionary<GameObject, int>();
    }

    public void AddClippingMask()
    {
        if (DoodleManager.Instance.selectedDoodles.Count == 0) { StartCoroutine(AppManager.Instance.FlashAddClippingMaskInstructions()); }

        foreach (GameObject doodle in DoodleManager.Instance.selectedDoodles)
        {
            ARDoodle doodleScript = doodle.GetComponent<ARDoodle>();

            // add a clipping mask on top of each selected doodle
            GameObject newMask = Instantiate(ClippingMaskPrefab, doodle.transform.position - new Vector3(0, 0, 0.025f),
                doodle.transform.rotation, doodleScript.GetDoodleParent().parent.Find("ClippingMasks"));

            doodleScript.AddClippingMaskInteraction();   // doodle will store reference to its clipping mask

            ClippingMasks.Add(newMask, doodleScript.GetDoodleID());
        }

        EditButton.Select();
        ToggleMaskEditingMode(true); // allow you to edit the masks just after adding them
    }

    public void ToggleMaskEditingMode(bool edit)
    {
        Debug.Log("toggling mask editing mode: " + edit);
        if (edit) { ToggleMaskDeletionMode(false); }   // don't want to edit and delete at the same time
        editMasks = edit;

        // change color of button to indicate selected status
        ColorBlock newColors = EditButton.colors;
        if (editMasks) { newColors.normalColor = selectedColor; }
        else { newColors.normalColor = deselectedColor; }
        EditButton.colors = newColors;  // replace entire ColorBlock

        //ToggleAllMaskManipulation(edit);

        DoodleManager.Instance.PlaceAllDoodles(false); // don't want to change the AppState
    }

    public void ToggleMaskDeletionMode(bool delete)
    {
        Debug.Log("Toggling mask deletion mode: " + delete);
        if (delete) { ToggleMaskEditingMode(false); }  // don't want to edit and delete at the same time
        deleteMasks = delete;

        AppManager.Instance.ToggleDeleteClippingMaskInstructions(delete);

        // change color of button to indicate selected status
        ColorBlock newColors = DeleteButton.colors;
        if (deleteMasks) { newColors.normalColor = selectedColor; }
        else { newColors.normalColor = deselectedColor; }
        DeleteButton.colors = newColors;  // replace entire ColorBlock

        //ToggleAllMaskManipulation(delete);
    }

    public void ToggleAllMaskManipulation (bool manipulate)
    {
        foreach (GameObject mask in ClippingMasks.Keys)
        {
            mask.GetComponent<SpriteRenderer>().enabled = manipulate;
            mask.GetComponent<BoxCollider>().enabled = manipulate;
            mask.GetComponent<ObjectManipulator>().enabled = manipulate;
            mask.GetComponent<NearInteractionGrabbable>().enabled = manipulate;
        }
    }

    public void OnDeleteMask(GameObject mask)
    {
        Debug.Log("Deleting mask!");
        if (deleteMasks) {
            GameObject doodle = DoodleManager.Instance.GetDoodle(ClippingMasks[mask]);
            if (doodle) { doodle.GetComponent<ARDoodle>().RemoveClippingMaskInteraction(); }

            ClippingMasks.Remove(mask);
            Destroy(mask);
        }
    }

    public void DeleteMasksWithDoodle(int doodleID)  // doodle was deleted, so remove the mask and dont fuss with any of the other stuff in the previous function :D
    {
        List<GameObject> clippingMasksToDelete = new List<GameObject>();
        foreach (GameObject mask in ClippingMasks.Keys)
        {
            if (ClippingMasks[mask] == doodleID) { clippingMasksToDelete.Add(mask); }
        }

        foreach (GameObject mask in clippingMasksToDelete)
        {
            ClippingMasks.Remove(mask);
            Destroy(mask);
        }
    }

    public void OnReturn()
    {
        ToggleMaskDeletionMode(false);
        ToggleMaskEditingMode(false);
        ToggleAllMaskManipulation(false);
        DoodleManager.Instance.PlaceAllDoodles();
    }

    // FOR THE SAMPLE SCENE (doodles are hard coded in Unity, but need to add clipping masks through this script)
    public GameObject AddMasksForPrecreatedDoodle(ARDoodle doodleScript, Transform doodleParent)
    {
        Debug.Log("adding masks for precreated doodes");

        // add a clipping mask on top of each selected doodle
        GameObject newMask = Instantiate(ClippingMaskPrefab, doodleScript.gameObject.transform.position - new Vector3(0, 0, 0.01f),
            doodleScript.gameObject.transform.rotation, doodleParent);

        doodleScript.AddClippingMaskInteraction();   // doodle will store reference to its clipping mask

        ClippingMasks.Add(newMask, doodleScript.GetDoodleID());

        OnReturn(); // don't want to delete or edit right now

        return newMask;
    }
}
