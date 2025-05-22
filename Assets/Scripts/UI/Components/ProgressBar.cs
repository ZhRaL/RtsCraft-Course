using System;
using UnityEngine;

namespace UI
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] private Vector2 padding = new(9, 8);
        [SerializeField] private RectTransform mask;
        private RectTransform maskParentRectTransform;

        private void Awake()
        {
            if (mask == null)
            {
                Debug.LogError($"Progress bar {name} has no mask");
            }

            maskParentRectTransform = maskParentRectTransform.parent.GetComponent<RectTransform>();
            
        }

        public void SetProgress(float value)
        {
            Vector2 parentSize = maskParentRectTransform.sizeDelta;
            Vector2 targetSize = parentSize - padding * 2;

            targetSize.x *= Mathf.Clamp01(value);

            mask.offsetMin = padding;
            mask.offsetMax = new Vector2(padding.x + targetSize.x - parentSize.x, -padding.y);
        }
    }
}