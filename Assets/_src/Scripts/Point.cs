using System;
using UnityEngine;
using UnityEngine.UI;

public class Point : MonoBehaviour
{
    private CircleCollider2D _collider;
    private ParticleSystem _particleSystem;
    private Material _material;
    private Image _image;
    private Color _color;
    private float _speed;

    private Vector3 _direction;
    private bool _move;

    private Action<Point> _destroyCallback;

    public void Create( Action<Point> destroyCallback)
    {
        _move = false;
        _image = GetComponentInChildren<Image>();
        _destroyCallback = destroyCallback;

        _particleSystem = GetComponentInChildren<ParticleSystem>();
        
        _collider = GetComponent<CircleCollider2D>();
        _collider.enabled = false;
        
        Renderer particleRenderer = _particleSystem.GetComponent<Renderer>();

        _material = Instantiate(particleRenderer.material);
        particleRenderer.material = _material;
    }

    public void Init(Color color,Vector2 position,float speed,Vector3 direction)
    {
        gameObject.SetActive(true);
        _image.gameObject.SetActive(true);
        _particleSystem.Stop();
        
        transform.localPosition = position;
        
        _material.SetColor("_BaseColor",color);
        _color = color;
        _image.color = _color;
        _speed = speed;
        _direction = direction;

        _move = true;
        
        _collider.enabled = true;
    }

    private void Update()
    {
        if (_move)
        {
            transform.localPosition += _direction * (_speed * Time.deltaTime);
        }
    }

    public Color GetColor() => _color;

    public void Destroy()
    {
        // Disable the collider so if the player moves the circle it wont collide with a new color sending so the game over
        _collider.enabled = false;
        
        _move = false;
        _image.gameObject.SetActive(false);
        
        // Playing the explosion particle
        _particleSystem.Play();
        
        // Hiding and adding to the pool after .5 seconds which is the duration of the particle
        Invoke("Hide",0.5f);
    }

    public void Hide()
    {
        _particleSystem.Stop();
        gameObject.SetActive(false);
        _destroyCallback.Invoke(this);
    }
}
