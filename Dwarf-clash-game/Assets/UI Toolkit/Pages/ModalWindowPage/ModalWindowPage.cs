using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ModalWindowPage : MonoBehaviour
{
    [SerializeField]
    private const string _contentWindowName = "content_window";

    private UIDocument _document;
    private VisualElement _contentWindow;

    void Start()
    {
        _document = GetComponent<UIDocument>();
        _contentWindow = _document.rootVisualElement.Q(_contentWindowName);
    }

    public void SetModalContent(VisualElement visualElement)
    {
        
    }
}