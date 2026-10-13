using UnityEngine;

public interface IUnitBehaviour
{
    public Transform transform { get; }
    public bool IsDead { get; }
    public void Damage(float damage);
}