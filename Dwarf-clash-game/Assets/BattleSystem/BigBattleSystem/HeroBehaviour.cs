using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeamGroup : byte
{
    PLAYER = 10,
    ENEMY = 20,
}

public class HeroBehaviour : UnitBase, IUnitBehaviour
{
    public enum AnimationType : byte
    {
        Idle = 10,
        Walk = 20,
        Attack = 30,
        Dead = 40
    }

    private TeamGroup _team;
    [SerializeField]
    private Data.HeroDefinition _definition;
    [SerializeField]
    [Tooltip("Дистанция атаки")]
    private float _attackDistance = 2f;
    [SerializeField]
    private GameObject _viewRootObject;
    [SerializeField]
    private SkeletonAnimation _animation;

    private HeroTargetFinder _targetFinder;

    protected IUnitBehaviour _target;
    public IUnitBehaviour Target
    {
        get => _target;
        set
        {
            if (_target != value)
            {
                _target = value;
                _targetDebug = null;
                if (_target != null)
                {
                    RaiseOnTargetAcquired(thisHero: this, _target);
                    _targetDebug = _target.transform.gameObject;
                    LookAtTarget();
                }
            }
        }
    }
    [SerializeField]
    [ReadOnly]
    private GameObject _targetDebug;

    private bool _isMoving = false;
    private bool _isAttacking = false;
    private Vector3 _defaultModelScale = Vector3.zero;
    private bool _isDead = false;
    private bool _isGhost = true;


    [SerializeField]
    private List<MinionBehaviour> _minions = new List<MinionBehaviour>();
    public List<MinionBehaviour> Minions { get => _minions; set => _minions = value; }

    public Data.HeroDefinition Definition => _definition;
    public bool IsDead => _isDead;
    public bool IsGhost { get => _isGhost; set => _isGhost = value; }
    public TeamGroup Team { get => _team; set => _team = value; }

    public event Action<float> OnDamage;
    public event Action<IUnitBehaviour> OnEngageCombat;
    public event Action OnCombatEnd;
    public event Action<HeroBehaviour, IUnitBehaviour> OnTargetAcquired;
    public event Action OnMovementStarted;

    private void RaiseOnDamage(float damage) { OnDamage?.Invoke(damage); }
    private void RaiseOnEngageCombat(IUnitBehaviour iUnitBehaviour) { OnEngageCombat?.Invoke(iUnitBehaviour); }
    private void RaiseOnCombatEnd() { OnCombatEnd?.Invoke(); }
    private void RaiseOnTargetAcquired(HeroBehaviour thisHero, IUnitBehaviour target) { OnTargetAcquired?.Invoke(thisHero, target); }
    private void RaiseOnMovementStarted() { OnMovementStarted?.Invoke(); }

    void Start()
    {
        _definition = Instantiate(_definition);

        SetAnimation(AnimationType.Idle, true);

        SetDefaultModelScale();
        FindFirstTarget();

        void SetDefaultModelScale()
        {
            if (_defaultModelScale == Vector3.zero)
            {
                _defaultModelScale = _viewRootObject.transform.localScale;
                _defaultModelScale.x = Mathf.Abs(_defaultModelScale.x);
            }
        }

        void FindFirstTarget()
        {
            List<HeroBehaviour> enemies = _team == TeamGroup.PLAYER ? BattlefieldManager.Instance.EnemyHeroes : BattlefieldManager.Instance.PlayerHeroes;
            _targetFinder = new HeroTargetFinder(this, enemies);
            Target = _targetFinder.GetClosestEnemyAtSameZ();
        }
    }


    public void StartAttack()
    {
        if (IsDead)
            return;

        RaiseOnEngageCombat(_target);
        _isAttacking = true;
        LookAtTarget();
        SetAnimation(AnimationType.Attack, false);
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        _isAttacking = true;
        Spine.TrackEntry trackEntry = _animation.AnimationState.GetCurrent(0);

        if (trackEntry != null)
            yield return new WaitForSeconds(trackEntry.AnimationEnd);

        _target?.Damage(_definition.Damage);
        EndAttack();
    }

    private void EndAttack()
    {
        RaiseOnCombatEnd();
        _isAttacking = false;
        SetAnimation(AnimationType.Idle, true);
    }

    private void Update()
    {
        if (IsDead || !gameObject.activeInHierarchy)
            return;

        if (_target == null || !_target.transform.gameObject.activeInHierarchy || _target.IsDead)
        {
            StopAllCoroutines();
            EndAttack();
            Target = _targetFinder.UpdateFinding();
        }

        if (_target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, _target.transform.position);
            if (distanceToTarget > _attackDistance)
                MoveToTargetSimple();
            else
            {
                _isMoving = false;
                if (!_isDead)
                    if (!_isAttacking) StartAttack();
            }
        }
    }

    private void MoveToTargetSimple()
    {
        if (IsDead)
            return;

        if (!_isAttacking)
            StopAllCoroutines();

        if (!_isMoving)
        {
            _isMoving = true;
            RaiseOnMovementStarted();
        }

        SetAnimation(AnimationType.Walk, true);

        Vector3 direction = (_target.transform.position - transform.position).normalized;
        transform.position += direction * _definition.MoveSpeed * Time.deltaTime;
    }

    public void LookAtTarget()
    {
        float direction = (_target.transform.position - transform.position).x > 0 ? 1 : -1;
        _viewRootObject.transform.localScale = new Vector3(direction * _defaultModelScale.x, _defaultModelScale.y, _defaultModelScale.z);
    }

    private void OnDestroy()
    {
        OnEngageCombat = null;
        OnCombatEnd = null;
    }
    
    public void Damage(float damage)
    {
        if (_isDead) return;

        RaiseOnDamage(damage);

        _definition.CurHealth -= damage;
        if (_definition.CurHealth <= 0)
        {
            _isDead = true;
            OnDeath();
        }
    }

    public void OnDeath()
    {
        StopAllCoroutines();
        if (_isAttacking)
            EndAttack();

        SetAnimation(AnimationType.Dead, loop: false);
        StartCoroutine(DeactivateAfterDeathAnimation());
    }
    
    private IEnumerator DeactivateAfterDeathAnimation()
    {
        Spine.TrackEntry trackEntry = _animation.AnimationState.GetCurrent(0);
        if (trackEntry != null)
            yield return new WaitForSeconds(trackEntry.AnimationEnd);
    }

    public override void OnDieAnimationCompleted()
    {
        _viewRootObject.SetActive(false);
    }

    private void SetAnimation(AnimationType animType, bool loop)
    {
        if (_animation == null)
            return;

        string animationName = animType.ToString();

        var currentAnimation = _animation.AnimationState.GetCurrent(0);
        if (currentAnimation != null && currentAnimation.Animation.Name == animationName)
            return;

        _animation.AnimationState.SetAnimation(0, animationName, loop);
    }

    private void OnDrawGizmos()
    {
        if (_target != null && _target.transform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, _target.transform.position);
        }
    }
}
