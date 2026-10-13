using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class Test5 : MonoBehaviour
{
    [SerializeField] private Material _gpuInstancingMaterial;
    [SerializeField] private List<Sprite> _sprites;
    [SerializeField] private float _elementSize = 0.64f;
    [SerializeField] private int _elementCount = 10;
    [SerializeField] private int _frameInterval = 5;
    [SerializeField] private List<Transform> _targetTransforms;

    // Структура данных для частиц, должна совпадать со структурой в шейдере
    private struct ParticleData
    {
        public Vector3 position;
        public Vector4 uvRect;
        public float size;
        public float flip; // 0 for normal, 1 for flipped
        public float active; // 1 for active, 0 for inactive
    }

    private ComputeBuffer _particlesBuffer;
    private ParticleData[] _particles;
    private Mesh _quadMesh;
    private MaterialPropertyBlock _mpb;

    private int _currentFrame;
    private int _currentSpriteIndex;
    private Vector4[] _spriteUVs;

    private void Awake()
    {
        Debug.Log("Test4 Awake called!");

        if (_targetTransforms.Count != _elementCount)
        {
            Debug.LogWarning($"Target Transforms count ({_targetTransforms.Count}) does not match Element Count ({_elementCount}). Adjusting element count.");
            _elementCount = _targetTransforms.Count;
        }

        CreateQuadMesh();
        InitializeSpriteUVs();
        InitializeParticles();
    }

    private void CreateQuadMesh()
    {
        _quadMesh = new Mesh();
        _quadMesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0)
        };
        _quadMesh.uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        _quadMesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
        _quadMesh.RecalculateBounds();
    }

    private void InitializeSpriteUVs()
    {
        if (_sprites == null || _sprites.Count == 0)
        {
            Debug.LogError("Sprites list is empty!");
            return;
        }

        _spriteUVs = new Vector4[_sprites.Count];
        for (int i = 0; i < _sprites.Count; i++)
        {
            Rect rect = _sprites[i].rect;
            Texture2D texture = _sprites[i].texture;
            _spriteUVs[i] = new Vector4(
                rect.x / texture.width,
                rect.y / texture.height,
                rect.width / texture.width,
                rect.height / texture.height
            );
        }
    }

    private void InitializeParticles()
    {
        _particles = new ParticleData[_elementCount];
        for (int i = 0; i < _elementCount; i++)
        {
            _particles[i] = new ParticleData
            {
                position = _targetTransforms[i].position,
                size = _elementSize,
                uvRect = _spriteUVs[Random.Range(0, _spriteUVs.Length)],
                flip = 0f, // По умолчанию не перевернут
                active = 1f // По умолчанию активен
            };
        }

        // Переворачиваем первый элемент
        if (_elementCount > 0)
        {
            _particles[0].flip = 1.0f;
        }

        // Прячем второй элемент
        if (_elementCount > 1)
        {
            _particles[1].active = 0.0f;
        }

        // Marshal.SizeOf используется для получения размера структуры в байтах
        _particlesBuffer = new ComputeBuffer(_elementCount, Marshal.SizeOf(typeof(ParticleData)));
        _particlesBuffer.SetData(_particles);

        _mpb = new MaterialPropertyBlock();
        _mpb.SetBuffer("_Particles", _particlesBuffer);

        // Устанавливаем текстуру атласа в материал
        if (_gpuInstancingMaterial != null && _sprites.Count > 0)
        {
            _gpuInstancingMaterial.SetTexture("_MainTex", _sprites[0].texture);
        }
    }

    private void Update()
    {
        if (_targetTransforms == null || _targetTransforms.Count != _elementCount || _gpuInstancingMaterial == null)
        {
            return;
        }

        // Обновление данных
        _currentFrame++;
        bool updateUVs = _currentFrame >= _frameInterval;
        if (updateUVs)
        {
            _currentFrame = 0;
            _currentSpriteIndex = (_currentSpriteIndex + 1) % _spriteUVs.Length;
        }

        for (int i = 0; i < _elementCount; i++)
        {
            if (_targetTransforms[i] != null)
            {
                _particles[i].position = _targetTransforms[i].position;
            }
            if (updateUVs)
            {
                _particles[i].uvRect = _spriteUVs[_currentSpriteIndex];
            }
        }

        // Отправка обновленных данных в буфер
        _particlesBuffer.SetData(_particles);

        // Рассчитываем границы для отсечения (culling)
        Bounds bounds = CalculateBounds();

        // Отрисовка
        Graphics.DrawMeshInstancedProcedural(
            _quadMesh,
            0,
            _gpuInstancingMaterial,
            bounds,
            _elementCount,
            _mpb);
    }

    private Bounds CalculateBounds()
    {
        if (_targetTransforms == null || _targetTransforms.Count == 0)
        {
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        var bounds = new Bounds(_targetTransforms[0].position, Vector3.zero);
        float maxElementSize = _elementSize; // Для простоты можно взять максимальный размер

        for (int i = 1; i < _targetTransforms.Count; i++)
        {
            if (_targetTransforms[i] != null)
            {
                bounds.Encapsulate(_targetTransforms[i].position);
            }
        }

        // Расширяем границы на размер самого большого элемента, чтобы спрайты не обрезались по краям
        bounds.Expand(maxElementSize * 2);

        return bounds;
    }

    private void OnDestroy()
    {
        if (_particlesBuffer != null)
        {
            _particlesBuffer.Release();
            _particlesBuffer = null;
        }

        if (_quadMesh != null)
        {
            Destroy(_quadMesh);
        }
    }
}
