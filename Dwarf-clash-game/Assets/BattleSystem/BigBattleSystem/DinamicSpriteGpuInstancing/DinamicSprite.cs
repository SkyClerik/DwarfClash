using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum AnimationQuadrant 
{
    TopLeft = 1,
    TopRight = 2,
    BottomLeft = 3,
    BottomRight = 4
}

public class DinamicSprite : MonoBehaviour
{
    [SerializeField] private Material _gpuInstancingMaterial;
    [SerializeField] private Texture2D _combinedAtlas;

    [Header("Sprite Quadrants (Auto-Populated)")]
    [SerializeField] private List<Sprite> _spritesQuadrant1 = new List<Sprite>();
    [SerializeField] private List<Sprite> _spritesQuadrant2 = new List<Sprite>();
    [SerializeField] private List<Sprite> _spritesQuadrant3 = new List<Sprite>();
    [SerializeField] private List<Sprite> _spritesQuadrant4 = new List<Sprite>();
    
    [SerializeField] private float _elementSize = 0.64f;
    [SerializeField] private int _elementCount = 10;
    [SerializeField] private int _frameInterval = 5;
    [SerializeField] private List<Transform> _targetTransforms;

    private List<Sprite> _currentAnimationList;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_combinedAtlas == null) 
        {
            _spritesQuadrant1.Clear();
            _spritesQuadrant2.Clear();
            _spritesQuadrant3.Clear();
            _spritesQuadrant4.Clear();
            return;
        }

        string path = AssetDatabase.GetAssetPath(_combinedAtlas);
        var allAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

        _spritesQuadrant1.Clear();
        _spritesQuadrant2.Clear();
        _spritesQuadrant3.Clear();
        _spritesQuadrant4.Clear();

        float centerX = _combinedAtlas.width / 2f;
        float centerY = _combinedAtlas.height / 2f;

        List<Sprite> allSprites = allAssets.OfType<Sprite>().ToList();

        allSprites.Sort((a, b) => a.name.CompareTo(b.name));
    
        foreach (var sprite in allSprites)
        {
            Vector2 center = sprite.rect.center;
            if (center.x < centerX && center.y >= centerY)
            {
                _spritesQuadrant1.Add(sprite);
            }
            else if (center.x >= centerX && center.y >= centerY)
            {
                _spritesQuadrant2.Add(sprite);
            }
            else if (center.x < centerX && center.y < centerY)
            {
                _spritesQuadrant3.Add(sprite);
            }
            else
            {
                _spritesQuadrant4.Add(sprite);
            }
        }

        if (_gpuInstancingMaterial != null)
        {
            _gpuInstancingMaterial.SetTexture("_MainTex", _combinedAtlas);
        }
        
        EditorUtility.SetDirty(this);
    }
#endif

    private struct ParticleData
    {
        public Vector3 position;
        public Vector4 uvRect;
        public float size;
        public float flip;
        public float active;
    }

    private ComputeBuffer _particlesBuffer;
    private ParticleData[] _particles;
    private Mesh _quadMesh;
    private MaterialPropertyBlock _mpb;

    private int _currentFrame;
    private int _currentSpriteIndex;
    private Vector4[] _spriteUVs;

    public void SetAnimationQuadrant(AnimationQuadrant quadrant)
    {
        List<Sprite> newSpriteList = null;
        switch (quadrant)
        {
            case AnimationQuadrant.TopLeft:
                newSpriteList = _spritesQuadrant1;
                break;
            case AnimationQuadrant.TopRight:
                newSpriteList = _spritesQuadrant2;
                break;
            case AnimationQuadrant.BottomLeft:
                newSpriteList = _spritesQuadrant3;
                break;
            case AnimationQuadrant.BottomRight:
                newSpriteList = _spritesQuadrant4;
                break;
        }

        if (newSpriteList != null && newSpriteList.Count > 0)
        {
            _currentAnimationList = newSpriteList;
            InitializeSpriteUVs();
            _currentSpriteIndex = 0;
            _currentFrame = 0;
        }
        else
        {
            Debug.LogWarning($"[DinamicSprite] Попытка переключиться на квадрант {(int)quadrant}, но в нем нет спрайтов.");
        }
    }
    
    private void Awake()
    {
        if (_targetTransforms.Count != _elementCount)
        {
            Debug.LogWarning($"Target Transforms count ({_targetTransforms.Count}) does not match Element Count ({_elementCount}). Adjusting element count.");
            _elementCount = _targetTransforms.Count;
        }
        
        _currentAnimationList = _spritesQuadrant1;

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
        if (_currentAnimationList == null || _currentAnimationList.Count == 0)
        {
            Debug.LogError("Current animation list is empty! Make sure the default quadrant (TopLeft) has sprites.");
            _spriteUVs = new Vector4[0];
            return;
        }

        _spriteUVs = new Vector4[_currentAnimationList.Count];
        for (int i = 0; i < _currentAnimationList.Count; i++)
        {
            Rect rect = _currentAnimationList[i].rect;
            Texture2D texture = _currentAnimationList[i].texture;
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
                uvRect = _spriteUVs.Length > 0 ? _spriteUVs[Random.Range(0, _spriteUVs.Length)] : Vector4.zero,
                flip = 0f,
                active = 1f
            };
        }

        // Переворачиваем первый элемент (тестовый код, временно закомментирован)
        // if (_elementCount > 0)
        // {
        //     _particles[0].flip = 1.0f;
        // }
        if (_elementCount > 1)
        {
            _particles[1].active = 0.0f;
        }

        _particlesBuffer = new ComputeBuffer(_elementCount, Marshal.SizeOf(typeof(ParticleData)));
        _particlesBuffer.SetData(_particles);

        _mpb = new MaterialPropertyBlock();
        _mpb.SetBuffer("_Particles", _particlesBuffer);

        if (_gpuInstancingMaterial != null && _combinedAtlas != null)
        {
            _gpuInstancingMaterial.SetTexture("_MainTex", _combinedAtlas);
        }
    }

    private void Update()
    {
        if (_targetTransforms == null || _targetTransforms.Count != _elementCount || _gpuInstancingMaterial == null || _spriteUVs == null || _spriteUVs.Length == 0)
        {
            return;
        }

        _currentFrame++;
        bool updateUVs = _currentFrame >= _frameInterval;
        if (updateUVs)
        {
            _currentFrame = 0;
            _currentSpriteIndex = (_currentSpriteIndex + 1) % _spriteUVs.Length;
        }

        for (int i = 0; i < _elementCount; i++)
        {
            if (i < _targetTransforms.Count && _targetTransforms[i] != null)
            {
                _particles[i].position = _targetTransforms[i].position;
            }
            if (updateUVs)
            {
                _particles[i].uvRect = _spriteUVs[_currentSpriteIndex];
            }
        }

        _particlesBuffer.SetData(_particles);
        
        Bounds bounds = CalculateBounds();

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
        float maxElementSize = _elementSize;

        for (int i = 1; i < _targetTransforms.Count; i++)
        {
            if (i < _targetTransforms.Count && _targetTransforms[i] != null)
            {
                bounds.Encapsulate(_targetTransforms[i].position);
            }
        }
        
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
