// SOURCE: https://dev.to/matthewodle/simple-ui-element-dragging-script-in-unity-c-450p

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableHandle : EventTrigger
{
    [SerializeField]
    public Vector2 DraggingConstraint;  // value is 0 if we don't want to scale in this direction

    private CaptureGuideController captureController;

    public bool allowedToDrag;  // set by CaptureGuideController, lets user only drag 1 handle at a time :P
    private bool dragging;

    public RectTransform Line;  // image

    private void Start()
    {
        captureController = transform.parent.GetComponent<CaptureGuideController>();
    }

    public void Update()
    {
        if (dragging && allowedToDrag)
        {
            transform.position = new Vector3(DraggingConstraint.x == 1 ? Input.mousePosition.x : transform.position.x,
                 DraggingConstraint.y == 1 ? Input.mousePosition.y : transform.position.y, transform.position.z);

            captureController.UpdateGuides(this);
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        dragging = true;
        captureController.ToggleHandlerManipulation(this);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        dragging = false;
        captureController.ToggleHandlerManipulation();  
    }

    public void UpdateLineLength(Vector2 newDimensions)
    {
        Line.sizeDelta = newDimensions;
    }
}