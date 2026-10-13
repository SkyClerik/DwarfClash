using UnityEngine;

public class FormationController : MonoBehaviour
{
    [SerializeField]
    private Transform _formationShape; // Родительский Transform формации (с дочерними позициями)

    private HeroBehaviour _heroBehaviour;

    public void InitTarget(HeroBehaviour heroBehaviour)
    {
        _heroBehaviour = heroBehaviour;
        _heroBehaviour.OnTargetAcquired += HeroBehaviour_OnTargetAcquired;
    }

    private void OnDestroy()
    {
        _heroBehaviour.OnTargetAcquired -= HeroBehaviour_OnTargetAcquired;
    }

    private void HeroBehaviour_OnTargetAcquired(HeroBehaviour hero, IUnitBehaviour target)
    {
        Vector3 direction = target.transform.position - hero.transform.position;
        direction.z = 0;
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.right, direction);
        _formationShape.rotation = targetRotation;
    }
}
