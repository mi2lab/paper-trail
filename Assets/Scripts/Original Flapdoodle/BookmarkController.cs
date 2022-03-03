using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BookmarkController : MonoBehaviour
{
    public GameObject ButtonText;
    public GameObject SelectedQuad; // visual feedback to show when the bookmark has been selected (Lean Touch)
    //public GameObject DeleteButton;
    public VideoController videoController;

    public Material selectedMaterial;
    public Material deselectedMaterial;

    private double bookmarkTime;

    public void SetCheckpointTime(double newTime)
    {
        bookmarkTime = newTime;

        // set button text
        ButtonText.GetComponent<TextMeshPro>().text = "Time: " + bookmarkTime.ToString("F2");
    }

    public void SkipToBookmark()
    {
        videoController.SetVideoTime(bookmarkTime);
    }

    public void OnBookmarkSelect()
    {
        if (SelectedQuad)
        {
            SelectedQuad.GetComponent<MeshRenderer>().material = selectedMaterial;
            BookmarkManager.Instance.SelectBookmark(this.gameObject);
            BookmarkManager.Instance.ShowBookmarkToolbar();
        }
    }

    public void OnBookmarkDeselect()
    {
        if (SelectedQuad)
        {
            SelectedQuad.GetComponent<MeshRenderer>().material = deselectedMaterial;
            BookmarkManager.Instance.DeselectBookmark(this.gameObject);
            BookmarkManager.Instance.HideBookmarkToolbar();
        }
    }

    //public void OnBookmarkDeleteButtonPress()
    //{
    //    BookmarkManager.Instance.DeleteBookmark(this.gameObject);
    //}

    //public void ToggleDeleteButton(bool show) { DeleteButton.SetActive(show); }
}
