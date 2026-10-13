using UI;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BoxCollider), typeof(SpriteRenderer))]
public class MapNodeCollision : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    [ReadOnly]
    private BoxCollider _collider;

    [SerializeField]
    private BattleTable _battleTable;

    private SpriteRenderer _spriteRenderer;

    public bool SetCollider
    {
        set { _collider.enabled = value; }
    }

    private void OnValidate()
    {
        if (_collider == null)
            _collider = GetComponent<BoxCollider>();

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Этот метод вызывается при клике/тапе по этому коллайдеру.
    /// Требует наличия PhysicsRaycaster на камере.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"MapNodeCollision OnPointerClick");
        if (eventData.button == PointerEventData.InputButton.Left)
            HandleInteraction();
    }

    public void SetSprite(Sprite sprite)
    {
        _spriteRenderer.sprite = sprite;
    }


    private void HandleInteraction()
    {
        Debug.Log($"Interaction with {gameObject.name}");

        if (MainSceneUIManager.Instance.TryGetPage(out GameHudPage gameHudPage))
            gameHudPage.ShowBattleInfoArea(_battleTable);
    }
}
