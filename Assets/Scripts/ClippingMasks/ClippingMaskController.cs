using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClippingMaskController : MonoBehaviour
{
    public void OnManipulationStart()
    {
        if (ClippingMaskManager.Instance.deleteMasks)
        {
            ClippingMaskManager.Instance.OnDeleteMask(gameObject);
        }
    }
}
