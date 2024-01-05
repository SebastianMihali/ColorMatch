using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slice : MonoBehaviour
{
    private const float PointEveryAngle = 22.5f;
    private const float Radius = 180;
    
    private Image _image;
    private EdgeCollider2D _collider;
    private Action<bool> _collisionCallback;

    private Color _color;

    public void Init(float size,float sizeAngle, float angle, Color color,Action<bool> callback)
    {
        _image = GetComponent<Image>();
        _collider = GetComponent<EdgeCollider2D>();

        _image.fillAmount = size;
        transform.localEulerAngles = new Vector3(0, 0, angle);

        _color = color;
        _image.color = _color;

        _collisionCallback = callback;
        
        // Calculating the number of edges for the collider. There will be an edge every PointEveryAngle degree
        int numberOfPoints = Mathf.CeilToInt(sizeAngle / PointEveryAngle);
        float angleOffset = sizeAngle / numberOfPoints;

        List<Vector2> colliderPoints = new();
        
        for (int i = 0; i <= numberOfPoints; i++)
        {
            
            // Calculating the position of the edge on the circonference
            float pointX = Radius * Mathf.Sin(Mathf.Deg2Rad*(angleOffset * i));
            float pointY = Radius * Mathf.Cos(Mathf.Deg2Rad*(angleOffset * i));
            
            colliderPoints.Add(new Vector2(pointX,pointY));
        }

        _collider.points = colliderPoints.ToArray();
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        Point point = other.gameObject.GetComponent<Point>();

        if (point != null)
        {
            _collisionCallback.Invoke(point.GetColor() == _color);
            point.Destroy();
        }
    }
}
