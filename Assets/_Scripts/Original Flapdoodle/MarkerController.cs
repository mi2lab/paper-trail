using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DoodleManager;

public class MarkerController : MonoBehaviour
{
    public GameObject DoodleParent;
    public GameObject MarkerButton;
    public SpriteRenderer spriteRenderer;

    public Color SelectedColor;
    public Color DeselectedColor;

    private Coroutine markerAnimationCoroutine;

    private void OnEnable()
    {
        if (markerAnimationCoroutine != null) { StopCoroutine(markerAnimationCoroutine); }
        markerAnimationCoroutine = StartCoroutine(MarkerAnimation());
    }

    private IEnumerator MarkerAnimation()
    {
        while(true)
        {

            yield return null;
        }
    }

    public void OnMarkerButtonClick()
    {
        DoodleManager.Instance.PlaceAllDoodles();
    }

    public void SetSelectedColor() { spriteRenderer.color = SelectedColor; }

    public void SetDeselectedColor() { spriteRenderer.color = DeselectedColor; }
}
