using System;
using UnityEngine;

public class SafeArea : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Rect _currentSafeArea;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        
        ResizeToSafeArea();
    }

    void ResizeToSafeArea()
    {
        Rect safeArea = Screen.safeArea;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        _rectTransform.anchorMin = anchorMin;
        _rectTransform.anchorMax = anchorMax;

        _currentSafeArea = safeArea;
    }
}