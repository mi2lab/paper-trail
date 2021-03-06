// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.Experimental.UI.BoundsControl
{
    /// <summary> 
    /// Shareable configuration for the <see cref="BoxDisplay" /> of <see cref="BoundsControl"/>
    /// This class provides all data members needed to create a solid box display for bounds control
    /// </summary>
    [CreateAssetMenu(fileName = "BoxDisplayConfiguration", menuName = "Mixed Reality Toolkit/Experimental/Bounds Control/Box Display Configuration")]
    public class BoxDisplayConfiguration : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Material used to display the bounding box. If set to null no bounding box will be displayed")]
        private Material boxMaterial = null;

        /// <summary>
        /// Material used to display the bounding box. If set to null no bounding box will be displayed
        /// </summary>
        public Material BoxMaterial
        {
            get { return boxMaterial; }
            set
            {
                if (boxMaterial != value)
                {
                    boxMaterial = value;
                    configurationChanged.Invoke();
                }
            }
        }

        [SerializeField]
        [Tooltip("Material used to display the bounding box when grabbed. If set to null no change will occur when grabbed.")]
        private Material boxGrabbedMaterial = null;

        /// <summary>
        /// Material used to display the bounding box when grabbed. If set to null no change will occur when grabbed.
        /// </summary>
        public Material BoxGrabbedMaterial
        {
            get { return boxGrabbedMaterial; }
            set
            {
                if (boxGrabbedMaterial != value)
                {
                    boxGrabbedMaterial = value;
                    configurationChanged.Invoke();
                }
            }
        }

        [SerializeField]
        [Tooltip("When an axis is flattened what value to set that axis's scale to for display.")]
        private float flattenAxisDisplayScale = 0.0f;

        /// <summary>
        /// When an axis is flattened what value to set that axis's scale to for display.
        /// </summary>
        public float FlattenAxisDisplayScale
        {
            get { return flattenAxisDisplayScale; }
            set
            {
                if (flattenAxisDisplayScale != value)
                {
                    flattenAxisDisplayScale = value;
                    configurationChanged.Invoke();
                }
            }
        }

        internal protected UnityEvent configurationChanged = new UnityEvent();
    }
}
