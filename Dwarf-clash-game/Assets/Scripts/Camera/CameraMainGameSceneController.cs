using UnityEngine;

public class CameraMainGameSceneController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Камера, которой управляем зумом")]
    private Camera _camera;

    [SerializeField]
    [Tooltip("Минимальные координаты карты (левый нижний угол)")]
    private Vector2 _mapMinBounds = new Vector2(-10f, -10f);
    [SerializeField]
    [Tooltip("Максимальные координаты карты (правый верхний угол)")]
    private Vector2 _mapMaxBounds = new Vector2(10f, 10f);
    [SerializeField]
    [Tooltip("Чувствительность объекта при управлении касанием")]
    private float _touchSensitivity = 0.1f;
    [SerializeField]
    [Tooltip("Чувствительность объекта при управлении мышью")]
    private float _mouseSensitivity = 0.1f;
    [SerializeField]
    [Tooltip("Минимальный orthographic size для зума")]
    private float _minOrthoSize = 1f;
    [SerializeField]
    [Tooltip("Максимальный orthographic size для зума")]
    private float _maxOrthoSize = 10f;
    [SerializeField]
    [Tooltip("Чувствительность зума колесом мыши")]
    private float _zoomSensitivity = 1f;
    [SerializeField]
    [Tooltip("Z-координата плоскости, на которой центрируется зум (для 2D)")]
    private float _targetZ = 0f;
    [SerializeField]
    [Tooltip("Режим камеры: true для 2D (движение X/Y), false для 3D (движение X/Z)")]
    public bool _is2DMode = true;

    private Vector2 _lastTouchPosition;
    private bool _isTouching = false;
    private float _initialPinchDistance;
    private float _initialOrthoSize;

    private const string _mouseX = "Mouse X";
    private const string _mouseY = "Mouse Y";

    private void Awake()
    {
        if (_camera == null)
        {
            Debug.LogWarning("Camera reference is not set! Trying to assign Camera.main.");
            _camera = Camera.main;
        }
    }

    private void Update()
    {
        if (_camera == null)
            return;

        // Перемещение объекта (карты)
        if (Input.touchCount == 1)
        {
            Touch touch = Input.touches[0];
            if (touch.phase == TouchPhase.Began)
            {
                _lastTouchPosition = touch.position;
                _isTouching = true;
            }
            else if (touch.phase == TouchPhase.Moved && _isTouching)
            {
                Vector3 delta = touch.position - _lastTouchPosition;
                _lastTouchPosition = touch.position;
                if (_is2DMode)
                {
                    this.transform.position += new Vector3(delta.x * _touchSensitivity, -delta.y * _touchSensitivity, 0);
                }
                else
                {
                    this.transform.position += new Vector3(-delta.x * _touchSensitivity, 0, delta.y * _touchSensitivity);
                }
                ClampObjectPosition();
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                _isTouching = false;
            }
        }

        if (Input.GetMouseButton(1))
        {
            float x = Input.GetAxis(_mouseX);
            float y = Input.GetAxis(_mouseY);
            if (_is2DMode)
            {
                this.transform.position += new Vector3(x * _mouseSensitivity, -y * _mouseSensitivity, 0);
            }
            else
            {
                this.transform.position += new Vector3(-x * _mouseSensitivity, 0, y * _mouseSensitivity);
            }
            ClampObjectPosition();
        }

        // Зум колесом мыши с центрированием
        if (Input.mouseScrollDelta.y != 0)
        {
            float targetPlane = _is2DMode ? _targetZ : 0f;
            ZoomAtPoint(Input.mousePosition, -Input.mouseScrollDelta.y * _zoomSensitivity, targetPlane);
        }

        // Зум pinch
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.touches[0];
            Touch touch1 = Input.touches[1];

            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                _initialPinchDistance = Vector2.Distance(touch0.position, touch1.position);
                _initialOrthoSize = _camera.orthographicSize;
            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                float currentDistance = Vector2.Distance(touch0.position, touch1.position);
                Vector2 pinchCenter = (touch0.position + touch1.position) / 2;
                if (_initialPinchDistance > 0 && currentDistance > 0)
                {
                    float newSize = _initialOrthoSize * (_initialPinchDistance / currentDistance);
                    float targetPlane = _is2DMode ? _targetZ : 0f;
                    ZoomToSizeAtPoint(pinchCenter, newSize, targetPlane);
                }
            }
        }
    }

    private void ZoomAtPoint(Vector2 screenPos, float sizeDelta, float targetPlane)
    {
        Vector3 worldPoint = _camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, targetPlane));
        float oldSize = _camera.orthographicSize;
        _camera.orthographicSize += sizeDelta;
        ClampOrthoSize();
        if (Mathf.Approximately(_camera.orthographicSize, oldSize)) return;
        Vector3 newWorldPoint = _camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, targetPlane));
        Vector3 delta = worldPoint - newWorldPoint;
        this.transform.position += delta;
        ClampObjectPosition();
    }

    private void ZoomToSizeAtPoint(Vector2 screenPos, float newSize, float targetPlane)
    {
        Vector3 worldPoint = _camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, targetPlane));
        float oldSize = _camera.orthographicSize;
        _camera.orthographicSize = Mathf.Clamp(newSize, _minOrthoSize, _maxOrthoSize);
        if (Mathf.Approximately(_camera.orthographicSize, oldSize)) return;
        Vector3 newWorldPoint = _camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, targetPlane));
        Vector3 delta = worldPoint - newWorldPoint;
        this.transform.position += delta;
        ClampObjectPosition();
    }

    private void ClampObjectPosition()
    {
        float aspectRatio = (float)Screen.width / Screen.height;
        float halfWidth = _camera.orthographicSize * aspectRatio;
        float halfHeight = _camera.orthographicSize;

        float minX = _mapMinBounds.x + halfWidth;
        float maxX = _mapMaxBounds.x - halfWidth;
        float minY, maxY, minZ, maxZ;

        if (_is2DMode)
        {
            minY = _mapMinBounds.y + halfHeight;
            maxY = _mapMaxBounds.y - halfHeight;
            minZ = maxZ = this.transform.position.z;
        }
        else
        {
            minY = maxY = this.transform.position.y;
            minZ = _mapMinBounds.y + halfHeight;
            maxZ = _mapMaxBounds.y - halfHeight;
        }

        if (minX > maxX) minX = maxX = (_mapMinBounds.x + _mapMaxBounds.x) / 2;
        if (minY > maxY) minY = maxY = (_mapMinBounds.y + _mapMaxBounds.y) / 2;
        if (!_is2DMode && minZ > maxZ) minZ = maxZ = (_mapMinBounds.y + _mapMaxBounds.y) / 2;

        float x = Mathf.Clamp(this.transform.position.x, minX, maxX);
        float y = Mathf.Clamp(this.transform.position.y, minY, maxY);
        float z = _is2DMode ? this.transform.position.z : Mathf.Clamp(this.transform.position.z, minZ, maxZ);
        this.transform.position = new Vector3(x, y, z);
    }

    private void ClampOrthoSize()
    {
        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, _minOrthoSize, _maxOrthoSize);
    }
}
