using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Lean.Touch
{
	/// <summary>This component allows you to translate the current GameObject relative to the camera using the finger drag gesture.
    /// ****** Extension of the LeanDragTranslate script to work along specified axes *****</summary>
	[HelpURL(LeanTouch.HelpUrlPrefix + "LeanDragTranslateLocalAxis")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Drag Translate Local Axis")]
	public class LeanDragTranslateLocalAxis : MonoBehaviour
	{
		/// <summary>The method used to find fingers to use with this component. See LeanFingerFilter documentation for more information.</summary>
		public LeanFingerFilter Use = new LeanFingerFilter(true);

		/// <summary>The camera the translation will be calculated using.\n\nNone = MainCamera.</summary>
		[Tooltip("The camera the translation will be calculated using.\n\nNone = MainCamera.")]
		public Camera Camera;

		/// <summary>If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.
		/// -1 = Instantly change.
		/// 1 = Slowly change.
		/// 10 = Quickly change.</summary>
		[Tooltip("If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.\n\n-1 = Instantly change.\n\n1 = Slowly change.\n\n10 = Quickly change.")]
		public float Dampening = -1.0f;

		/// <summary>This allows you to control how much momenum is retained when the dragging fingers are all released.
		/// NOTE: This requires <b>Dampening</b> to be above 0.</summary>
		[Tooltip("This allows you to control how much momenum is retained when the dragging fingers are all released.\n\nNOTE: This requires <b>Dampening</b> to be above 0.")]
		[Range(0.0f, 1.0f)]
		public float Inertia;

        public bool ConstrainXAxis = false;
        public bool ConstrainYAxis = false;
        public bool ConstrainZAxis = false;

        /// <summary>This event is called when the translation has just started (old position now equals current position).</summary>
        //public UnityEvent OnTranslationStarted { get { if (onTranslationStarted == null) onTranslationStarted = new UnityEvent(); return onTranslationStarted; } }
        //[FormerlySerializedAs("OnTranslationStarted")] [SerializeField] private UnityEvent onTranslationStarted;

        /// <summary>This event is called when the translation is complete (old position now equals current position).</summary>
        public UnityEvent OnTranslationComplete { get { if (onTranslationComplete == null) onTranslationComplete = new UnityEvent(); return onTranslationComplete; } }
        [FormerlySerializedAs("OnTranslationComplete")] [SerializeField] private UnityEvent onTranslationComplete;

        private Vector3 oldPosition;
        private bool translationComplete = true;

        [HideInInspector]
		[SerializeField]
		private Vector3 remainingTranslation;

		/// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually add a finger.</summary>
		public void AddFinger(LeanFinger finger)
		{
			Use.AddFinger(finger);
		}

		/// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove a finger.</summary>
		public void RemoveFinger(LeanFinger finger)
		{
			Use.RemoveFinger(finger);
		}

		/// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually remove all fingers.</summary>
		public void RemoveAllFingers()
		{
			Use.RemoveAllFingers();
		}
#if UNITY_EDITOR
		protected virtual void Reset()
		{
			Use.UpdateRequiredSelectable(gameObject);
		}
#endif
		protected virtual void Awake()
		{
			Use.UpdateRequiredSelectable(gameObject);
		}

		protected virtual void Update()
		{
            if (!oldPosition.Equals(transform.localPosition))
            {
                // Store
                oldPosition = transform.localPosition;
                translationComplete = false;
            }
            else   // Extension to original LeanDragTranslate script: invoke OnTranslationComplete event
            {
                if (!translationComplete)
                {
                    OnTranslationComplete.Invoke();
                    translationComplete = true;
                }
            }

			// Get the fingers we want to use
			var fingers = Use.GetFingers();

			// Calculate the screenDelta value based on these fingers
			var screenDelta = LeanGesture.GetScreenDelta(fingers);

			if (screenDelta != Vector2.zero)
			{
				// Perform the translation
				if (transform is RectTransform)
				{
					TranslateUI(screenDelta);
				}
				else
				{
					Translate(screenDelta);
				}
			}

			// Increment
			remainingTranslation += transform.localPosition - oldPosition;

			// Get t value
			var factor = LeanTouch.GetDampenFactor(Dampening, Time.deltaTime);

			// Dampen remainingDelta
			var newRemainingTranslation = Vector3.Lerp(remainingTranslation, Vector3.zero, factor);

			// Shift this transform by the change in delta
			transform.localPosition = oldPosition + remainingTranslation - newRemainingTranslation;

			if (fingers.Count == 0 && Inertia > 0.0f && Dampening > 0.0f)
			{
				newRemainingTranslation = Vector3.Lerp(newRemainingTranslation, remainingTranslation, Inertia);
			}

			// Update remainingDelta with the dampened value
			remainingTranslation = newRemainingTranslation;
        }

		private void TranslateUI(Vector2 screenDelta)
		{
			var camera = Camera;

			if (camera == null)
			{
				var canvas = transform.GetComponentInParent<Canvas>();

				if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
				{
					camera = canvas.worldCamera;
				}
			}

			// Screen position of the transform
			var screenPoint = RectTransformUtility.WorldToScreenPoint(camera, transform.position);

			// Add the deltaPosition
			screenPoint += screenDelta;

			// Convert back to world space
			var worldPoint = default(Vector3);

			if (RectTransformUtility.ScreenPointToWorldPointInRectangle(transform.parent as RectTransform, screenPoint, camera, out worldPoint) == true)
			{
				transform.position = worldPoint;
			}
		}

		private void Translate(Vector2 screenDelta)
		{
			// Make sure the camera exists
			var camera = LeanTouch.GetCamera(Camera, gameObject);

			if (camera != null)
			{
				// Screen position of the transform
				var screenPoint = camera.WorldToScreenPoint(transform.position);

				// Add the deltaPosition
				screenPoint += (Vector3)screenDelta;

				// Convert back to world space
				transform.position = GetConstrainedPosition(camera.ScreenToWorldPoint(screenPoint));
			}
			else
			{
				Debug.LogError("Failed to find camera. Either tag your camera as MainCamera, or set one in this component.", this);
			}
		}

        // Extension to original LeanDragTranslate script: if Constrain Axis is true, returns the current position component of the transform
        private Vector3 GetConstrainedPosition(Vector3 vec)
        {
            return new Vector3(ConstrainXAxis ? transform.position.x : vec.x, 
                               ConstrainYAxis ? transform.position.y : vec.y, 
                               ConstrainZAxis ? transform.position.z : vec.z);
        }
    }
}