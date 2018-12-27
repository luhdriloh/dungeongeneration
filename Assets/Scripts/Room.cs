using UnityEngine;

public class Room : MonoBehaviour
{
    public float _defaultScale;
    public int _id;
    public int _width;
    public int _height;
    private BoxCollider2D _boxCollider;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigidbody;

    public void TurnOff()
    {
        gameObject.SetActive(false);
    }

    public void TurnOn()
    {
        gameObject.SetActive(true);
    }

    public void TurnOnCollision()
    {
        _boxCollider.enabled = true;
    }

    public bool PhysicsSleeping()
    {
        return _rigidbody.IsSleeping();
    }

    public void SetColor(Color newColor)
    {
        _spriteRenderer.color = newColor;
    }

    public void SetSize(int width, int height)
    {
        _width = width;
        _height = height;
        SetRoomSize();
    }

    public void RoundPosition()
    {
        float x = Mathf.RoundToInt(transform.position.x);
        float y = Mathf.RoundToInt(transform.position.y);

        transform.position = new Vector2(x, y);
        _boxCollider.enabled = false;
    }

    public Vector2 ReturnRoomMidpoint()
    {
        return new Vector2(Mathf.RoundToInt(transform.position.x + _width / 2), Mathf.RoundToInt(transform.position.y + _height / 2));
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _boxCollider.enabled = false;
    }
	
	private void FixedUpdate ()
    {
        int x = Mathf.RoundToInt(transform.position.x);
        int y = Mathf.RoundToInt(transform.position.y);

        if (_rigidbody.IsSleeping() && Mathf.Abs(transform.position.x - x) > Mathf.Epsilon && Mathf.Abs(transform.position.y - y) > Mathf.Epsilon)
        {
            Debug.Log("rounding");
            transform.position = new Vector2(x, y);
        }
	}

    private void SetRoomSize()
    {
        transform.localScale = new Vector3(_defaultScale * _width, _defaultScale * _height, 0f);
    }
}
