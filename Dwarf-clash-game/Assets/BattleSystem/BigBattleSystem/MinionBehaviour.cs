using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MinionBehaviour : UnitBase, IUnitBehaviour
{
    [SerializeField]
    private float _attackDistance = .3f;
    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private Transform _firePoint;
    [SerializeField]
    private GameObject _projectilePrefab;
    [SerializeField]
    [ReadOnly]
    private GameObject _targetDebug;
    private HeroBehaviour _ownerHero;

    private IUnitBehaviour _target;
    private bool _isAttacking = false;
    private bool _isDead = false;
    private bool _isReadyInit = false;

    private MinionDefinition _definition;
    private Transform _modelRootTransform;
    private Vector3 _defaultModelScale = Vector3.zero;
    private Transform _inShapePoint;

    private const string _idleAnimID = "idle";
    private const string _attackAnimID = "attack";
    private const string _dieAnimID = "dead";
    private const string _walkAnimID = "walk";

    public bool IsDead
    {
        get
        {
            if (_definition.CurHealth < 0)
                _isDead = true;

            return _isDead;
        }
    }

    public event Action<float> OnDamage;
    private void RaiseOnDamage(float damage) { OnDamage?.Invoke(damage); }

    void Start()
    {
        SetAnim(_idleAnimID);
    }

    public void Init(HeroBehaviour ownerHero, MinionDefinition definition, Transform inShapePosition)
    {
        _ownerHero = ownerHero;
        _definition = Instantiate(definition);
        //_ownerSquad = squad;
        //_ownerSquad.Minions.Add(this);
        _inShapePoint = inShapePosition;

        //_ownerSquad.Hero.OnTargetAcquired += OnTargetAcquired;

        _isReadyInit = true;
        MoveToMySlot();
    }

    private void OnDestroy()
    {
        //_ownerSquad.Hero.OnTargetAcquired -= OnTargetAcquired;
    }

    private void OnTargetAcquired(HeroBehaviour heroBehaviour, IUnitBehaviour target)
    {
        if (target is HeroBehaviour targetHero)
        {
            FindNearestTarget(targetHero.Minions, targetHero);
        }
    }

    private void FindNearestTarget(List<MinionBehaviour> enemyMinions, HeroBehaviour enemyHero)
    {
        if (enemyMinions.Count > 0)
        {
            _target = (IUnitBehaviour)enemyMinions.OrderBy(m => Vector3.Distance(transform.position, m.transform.position)).First();
            _targetDebug = _target?.transform.gameObject;
        }
        else
        {
            _target = enemyHero;
            _targetDebug = _target?.transform.gameObject;
        }

        if (_target == null || _target.IsDead)
        {
            MoveToMySlot();
            return;
        }
    }

    void Update()
    {
        if (IsDead) return;

        if (!_isReadyInit) return;

        if (_target != null && _target.IsDead)
        {
            //Debug.Log($"Minion {gameObject.name}: Target {_target} is dead, resetting.");
            _target = null;
            if (_ownerHero.Target != null && _ownerHero.Target is HeroBehaviour target)
            {
                if (target?.Minions != null)
                    FindNearestTarget(target.Minions, target);
            }
        }

        if (_target != null)
        {
            float dist = Vector3.Distance(transform.position, _target.transform.position);
            if (dist > _attackDistance)
            {
                MoveTowardsTarget(_target.transform);
            }
            else
            {
                if (!_isAttacking)
                    StartCoroutine(AttackCoroutine());
            }
        }
        else
        {
            if (_isAttacking)
            {
                StopAllCoroutines();
                _isAttacking = false;
                SetAnim(_idleAnimID);
            }
            else
            {
                MoveToMySlot();
            }
        }
    }

    public void MoveToMySlot()
    {
        float dist = Vector3.Distance(transform.position, _inShapePoint.position);
        if (dist <= 0.01f)
        {
            LookAtTarget(_ownerHero.transform);
            SetAnim(_idleAnimID);
        }
        else
        {
            MoveTowardsTarget(_inShapePoint);
        }
    }

    private void MoveTowardsTarget(Transform target)
    {
        LookAtTarget(target);
        SetAnim(_walkAnimID);
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * _definition.MoveSpeed * Time.deltaTime;
    }

    private IEnumerator AttackCoroutine()
    {
        _isAttacking = true;
        SetAnim(_attackAnimID);

        // Подождем один кадр, чтобы анимация начала играть
        yield return null;

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        float realDuration = stateInfo.length / _animator.speed;

        // Ждем всю анимацию
        yield return new WaitForSeconds(realDuration);

        _target?.Damage(_definition.Damage);
        EndAttack();
    }

    private void EndAttack()
    {
        if (_target != null && _target.IsDead)
            _target = null;

        _isAttacking = false;
        SetAnim(_idleAnimID);

        if (_ownerHero.Target != null && _ownerHero.Target is HeroBehaviour target)
        {
            if (target.Minions == null)
                return;

            FindNearestTarget(target.Minions, target);
        }
    }

    public void Damage(float damage)
    {
        if (IsDead) return;

        RaiseOnDamage(damage);

        _definition.CurHealth -= damage;

        if (_definition.CurHealth <= 0)
            OnDeath();
    }

    private void OnDeath()
    {
        if (IsDead) return;
        _isDead = true;
        SetAnim(_dieAnimID);
        StopAllCoroutines();

        gameObject.SetActive(false);
        _ownerHero.Minions.Remove(this);
    }

    public void LookAtTarget(Transform target)
    {
        TrySetDefaultModelScale();
        float direction = (target.position - transform.position).x > 0 ? 1 : -1;
        _modelRootTransform.localScale = new Vector3(direction * _defaultModelScale.x, _defaultModelScale.y, _defaultModelScale.z);
    }

    private void TrySetDefaultModelScale()
    {
        if (_defaultModelScale == Vector3.zero)
        {
            _modelRootTransform = _animator.transform;
            _defaultModelScale = _modelRootTransform.localScale;
            _defaultModelScale.x = Mathf.Abs(_defaultModelScale.x);
        }
    }

    public override void OnDieAnimationCompleted()
    {
        gameObject.SetActive(false);
    }

    private void SetAnim(string animName)
    {
        _animator.SetBool(_idleAnimID, false);
        _animator.SetBool(_attackAnimID, false);
        _animator.SetBool(_walkAnimID, false);
        _animator.SetBool(_dieAnimID, false);

        _animator.SetBool(animName, true);
    }

    private void OnDrawGizmos()
    {
        if (_target != null && _target.transform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _target.transform.position);
        }
    }
}
