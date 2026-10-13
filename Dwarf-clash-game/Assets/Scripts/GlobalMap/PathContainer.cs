using System.Collections.Generic;
using UnityEngine;
using Toolbox;

public class PathContainer : MonoBehaviour
{
    [SerializeField]
    [ReadOnly]
    private List<MapNodeCollision> _nodes = new List<MapNodeCollision>();
    [SerializeField]
    private MapNodeCollision _currentActiveNode;
    [SerializeField]
    private Sprite _activeSprite;
    [SerializeField]
    private Sprite _disableSprite;
    [SerializeField]
    private Sprite _complateSprite;

    private void Start()
    {
        _nodes.Clear();
        _nodes = gameObject.FindComponentsInAllChildren<MapNodeCollision>(includeInactive: true);
        int s = 0;
        for (int i = 0; i < _nodes.Count; i++)
        {
            if (_nodes[i] == _currentActiveNode)
            {
                _nodes[i].SetCollider = true;
                _nodes[i].SetSprite(_activeSprite);
                s = i;
            }
            else
            {
                if (i > s)
                {
                    _nodes[i].SetCollider = false;
                    _nodes[i].SetSprite(_complateSprite);
                }
                else
                {
                    _nodes[i].SetCollider = false;
                    _nodes[i].SetSprite(_disableSprite);
                }
            }
        }
    }
}
