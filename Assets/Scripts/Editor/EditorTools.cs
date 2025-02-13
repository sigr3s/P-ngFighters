﻿using UnityEditor;
using UnityEngine;

public class EditorTools : MonoBehaviour
{
    #region Static methods
    [MenuItem("Tools/Ui/Anchors to corners")]
    static void AnchorsToCorners() 
    {
        foreach (Transform transform in Selection.transforms) 
        {
            RectTransform t = transform as RectTransform;
            RectTransform pt = Selection.activeTransform.parent as RectTransform;

            if (t != null && pt != null)
            {
                Vector2 newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / pt.rect.width, t.anchorMin.y + t.offsetMin.y / pt.rect.height);
                Vector2 newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / pt.rect.width, t.anchorMax.y + t.offsetMax.y / pt.rect.height);

                t.anchorMin = newAnchorsMin;
                t.anchorMax = newAnchorsMax;
                t.offsetMin = t.offsetMax = Vector2.zero;
            }
        }
    }
    #endregion
}