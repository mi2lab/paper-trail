using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ImageCapture;

public class CaptureGuideController : MonoBehaviour
{
    public DraggableHandle topScript, bottomScript, leftScript, rightScript;
    public RectTransform topTransform, bottomTransform, leftTransform, rightTransform;
    private float ORIGINAL_WIDTH, ORIGINAL_HEIGHT;

    //private const float MIN_WIDTH = 300;
    //private const float MIN_HEIGHT = 300;
    private const float MAX_WIDTH = 2200;
    private const float MAX_HEIGHT = 1620;
    private const float LINE_WIDTH = 20;

    private void Awake()
    {
        ORIGINAL_WIDTH = rightTransform.position.x - leftTransform.position.x;
        ORIGINAL_HEIGHT = topTransform.position.y - bottomTransform.position.y;
        Debug.Log("width: " + ORIGINAL_WIDTH + " height: " + ORIGINAL_HEIGHT);

        // set correct dimensions to start with
        ImageCapture.Instance.SetDimensions(new Vector2Int((int)leftTransform.position.x, (int)bottomTransform.position.y),
            new Vector2Int((int)ORIGINAL_WIDTH, (int)ORIGINAL_HEIGHT));
    }

    public void UpdateGuides(DraggableHandle handle)
    {
        if (handle == rightScript || handle == leftScript)  // update top & bottom
        {
            if (handle == rightScript && (rightTransform.anchoredPosition.x > (MAX_WIDTH / 2) || rightTransform.anchoredPosition.x < leftTransform.anchoredPosition.x)) // out of bounds condition
            {
                rightTransform.anchoredPosition = new Vector2(MAX_WIDTH / 2, rightTransform.anchoredPosition.y);
            }
            else if (handle == leftScript && (leftTransform.anchoredPosition.x < -MAX_WIDTH / 2 || leftTransform.anchoredPosition.x > rightTransform.anchoredPosition.x)) // out of bounds condition
            {
                leftTransform.anchoredPosition = new Vector2(-MAX_WIDTH / 2, leftTransform.anchoredPosition.y);
            }

            topTransform.position = new Vector2((rightTransform.position.x + leftTransform.position.x) / 2, topTransform.position.y);
            topScript.UpdateLineLength(new Vector2(rightTransform.position.x - leftTransform.position.x, LINE_WIDTH));

            bottomTransform.position = new Vector2((rightTransform.position.x + leftTransform.position.x) / 2, bottomTransform.position.y);
            bottomScript.UpdateLineLength(new Vector2(rightTransform.position.x - leftTransform.position.x, LINE_WIDTH));
        }
        else // update left and right
        {
            if (handle == topScript && (topTransform.anchoredPosition.y > MAX_HEIGHT / 2 || topTransform.anchoredPosition.y < bottomTransform.anchoredPosition.y)) // out of bounds condition
            {
                topTransform.anchoredPosition = new Vector2(topTransform.anchoredPosition.x, MAX_HEIGHT / 2);
            }
            else if (handle == bottomScript && (bottomTransform.anchoredPosition.y < -MAX_HEIGHT / 2 || bottomTransform.anchoredPosition.y > topTransform.anchoredPosition.y)) // out of bounds condition
            {
                bottomTransform.anchoredPosition = new Vector2(bottomTransform.anchoredPosition.x, -MAX_HEIGHT / 2);
            }

            leftTransform.position = new Vector2(leftTransform.position.x, (topTransform.position.y + bottomTransform.position.y) / 2);
            leftScript.UpdateLineLength(new Vector2(LINE_WIDTH, topTransform.position.y - bottomTransform.position.y));

            rightTransform.position = new Vector2(rightTransform.position.x, (topTransform.position.y + bottomTransform.position.y) / 2);
            rightScript.UpdateLineLength(new Vector2(LINE_WIDTH, topTransform.position.y - bottomTransform.position.y));
        }
    }

    public void ResetCaptureGuide()
    {
        topTransform.anchoredPosition = new Vector2(0, ORIGINAL_HEIGHT / 2);
        bottomTransform.anchoredPosition = new Vector2(0, -ORIGINAL_HEIGHT / 2);
        rightTransform.anchoredPosition = new Vector2(ORIGINAL_WIDTH / 2, 0);
        leftTransform.anchoredPosition = new Vector2(-ORIGINAL_WIDTH / 2, 0);

        topScript.UpdateLineLength(new Vector2(ORIGINAL_WIDTH, LINE_WIDTH));
        bottomScript.UpdateLineLength(new Vector2(ORIGINAL_WIDTH, LINE_WIDTH));
        rightScript.UpdateLineLength(new Vector2(LINE_WIDTH, ORIGINAL_HEIGHT));
        leftScript.UpdateLineLength(new Vector2(LINE_WIDTH, ORIGINAL_HEIGHT));

        ImageCapture.Instance.SetDimensions(new Vector2Int((int)leftTransform.position.x, (int)bottomTransform.position.y),
            new Vector2Int((int)(rightTransform.position.x - leftTransform.position.x),
            (int)(topTransform.position.y - bottomTransform.position.y)));
    }

    public void ToggleHandlerManipulation(DraggableHandle handle = null)
    {
        if (handle == rightScript)
        {
            rightScript.allowedToDrag = true;
            leftScript.allowedToDrag = false;
            topScript.allowedToDrag = false;
            bottomScript.allowedToDrag = false;
        }
        else if (handle == leftScript)
        {
            rightScript.allowedToDrag = false;
            leftScript.allowedToDrag = true;
            topScript.allowedToDrag = false;
            bottomScript.allowedToDrag = false;
        }
        else if (handle == topScript)
        {
            rightScript.allowedToDrag = false;
            leftScript.allowedToDrag = false;
            topScript.allowedToDrag = true;
            bottomScript.allowedToDrag = false;
        }
        else if (handle == bottomScript)
        {
            rightScript.allowedToDrag = false;
            leftScript.allowedToDrag = false;
            topScript.allowedToDrag = false;
            bottomScript.allowedToDrag = true;
        }

        else // null, everyone should be allowed to drag
        {
            rightScript.allowedToDrag = true;
            leftScript.allowedToDrag = true;
            topScript.allowedToDrag = true;
            bottomScript.allowedToDrag = true;

            // update the capture dimensions
            ImageCapture.Instance.SetDimensions(new Vector2Int((int)leftTransform.position.x, (int)bottomTransform.position.y),
            new Vector2Int((int)(rightTransform.position.x - leftTransform.position.x),
            (int)(topTransform.position.y - bottomTransform.position.y)));
        }
    }
}
