#define THIS_DEBUG
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Toolbox;

public class UserInterfaceRaycaster : Singleton<UserInterfaceRaycaster>
{
    private PointerEventData _pointer;
    private List<RaycastResult> _resultRaycast;
    public bool IsPickingMode
    {
        get
        {
            _pointer = new PointerEventData(EventSystem.current);
            _pointer.position = Input.mousePosition;
            _resultRaycast = new List<RaycastResult>();

            // this can hit ui with "picking mode" set to "position"
            EventSystem.current.RaycastAll(_pointer, _resultRaycast);

#if THIS_DEBUG
            foreach (var rayResult in _resultRaycast)
                Debug.Log($"name: {rayResult.gameObject.name}");
#endif

            if (_resultRaycast.Count > 0)
                return true;

            return false;
        }
    }
}
