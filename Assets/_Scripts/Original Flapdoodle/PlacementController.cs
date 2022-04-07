using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static AppManager;

// ********** Code adapted from ARKit example provided by SI 659 **********

public class PlacementController : MonoBehaviour
{
    public GameObject PlacementIndicator;

    private ARRaycastManager arRaycastManager;
    private bool planeIsDetected;
    private Pose placementPose;

    void Start()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        UpdatePlacementPose();

        if (AppManager.Instance.GetAppState() == AppState.Capture)
        {
            UpdatePlacementIndicator();
        }
    }

    private void UpdatePlacementPose()
    {
        // ViewportToScreenPoint: https://docs.unity3d.com/ScriptReference/Camera.ViewportToScreenPoint.html 
        Vector3 screenCenter = Camera.main.ViewportToScreenPoint(new Vector2(0.5f, 0.5f));
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        // ARRaycastManager: https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@2.1/api/UnityEngine.XR.ARFoundation.ARRaycastManager.html
        arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon);

        planeIsDetected = hits.Count > 0;

        if (planeIsDetected)
        {
            // Pose: https://docs.unity3d.com/ScriptReference/Pose.html
            placementPose = hits[0].pose;
            // Transform.forward: https://docs.unity3d.com/ScriptReference/Transform-forward.html
            Vector3 cameraForward = Camera.main.transform.forward;
            // Vector3.normalized: https://docs.unity3d.com/ScriptReference/Vector3-normalized.html
            Vector3 cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            // Quaternion.LookRotation: https://docs.unity3d.com/ScriptReference/Quaternion.LookRotation.html
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }

    private void UpdatePlacementIndicator()
    {
        if (planeIsDetected)
        {
            PlacementIndicator.SetActive(true);
            PlacementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
        else
        {
            PlacementIndicator.SetActive(false);
        }
    }

    //public void HandlePlacementIndicatorSelect()
    //{
    //    Debug.Log("Placement indicator selected");

    //    if (appManager.GetAppState() == AppState.Capture)
    //    {
    //        Debug.Log("Taking photo!");
    //    }

    //}

}
