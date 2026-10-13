using Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class HeroTargetFinder
{
    [SerializeField]
    private float _zTolerance = 0.1f;

    private HeroBehaviour _owner;
    private HeroDefinition _heroDefinition;
    private List<HeroBehaviour> _enemyHeroes;

    public HeroTargetFinder(HeroBehaviour owner, List<HeroBehaviour> enemyHeroes)
    {
        _owner = owner;
        _heroDefinition = _owner.Definition;
        _enemyHeroes = enemyHeroes;
    }

    public HeroBehaviour GetClosestEnemyAtSameZ()
    {
        if (_enemyHeroes.Count == 0)
            return null;

        float myX = _owner.transform.position.x;
        float myZ = _owner.transform.position.z;
        HeroBehaviour closestTarget = null;
        float minDistance = float.MaxValue;

        foreach (HeroBehaviour potentialTarget in _enemyHeroes)
        {
            if (!potentialTarget.isActiveAndEnabled || potentialTarget.IsDead)
                continue;

            float targetZ = potentialTarget.transform.position.z;
            float zDiff = Mathf.Abs(myZ - targetZ);
            if (zDiff > _zTolerance)
                continue;

            float targetX = potentialTarget.transform.position.x;
            float distanceX = Mathf.Abs(myX - targetX);
            if (distanceX < minDistance)
            {
                minDistance = distanceX;
                closestTarget = potentialTarget;
            }
        }
        return closestTarget;
    }

    public HeroBehaviour UpdateFinding()
    {
        if (_enemyHeroes.Count == 0)
            return null;

        var targetWeights = new Dictionary<HeroBehaviour, int>();

        foreach (HeroBehaviour hero in _enemyHeroes)
        {
            if (!hero.isActiveAndEnabled || !hero.IsGhost)
                continue;

            var aggressionWeight = _heroDefinition.AggressionWeights.FirstOrDefault(aw => aw.HeroDefinition == hero.Definition);

            if (aggressionWeight != null)
                targetWeights[hero] = aggressionWeight.Weight;
            else
                targetWeights[hero] = 1;
        }

        if (targetWeights.Count == 0)
            return null;

        int maxWeight = targetWeights.Values.Max();

        var maxWeightTargets = targetWeights.Where(kvp => kvp.Value == maxWeight).ToList();

        HeroBehaviour chosenTarget = null;
        float minDistance = float.MaxValue;

        foreach (var kvp in maxWeightTargets)
        {
            float distance = Vector3.Distance(_owner.transform.position, kvp.Key.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                chosenTarget = kvp.Key;
            }
        }

        return chosenTarget;
    }
}
