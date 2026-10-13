using Data;
using UnityEngine;

public class BattlefieldSpawner
{
    private BattlefieldManager _battlefieldManager;
    private float _centerOffset = 3;
    private float _cellSpacing = 2;
    private GameObject _heroPrefab;

    public BattlefieldSpawner(BattlefieldManager battlefieldManager, float centerOffset, float cellSpacing, GameObject heroPrefab)
    {
        _battlefieldManager = battlefieldManager;
        _centerOffset = centerOffset;
        _cellSpacing = cellSpacing;
        _heroPrefab = heroPrefab;
    }

    public void DoSpawning(BattleTable playerTable, BattleTable enemyTable)
    {
        //TODO: Получить список префабов для создания
        if (_heroPrefab == null)
        {
            Debug.LogError("Hero Prefab is not assigned in BattlefieldManager!");
            return;
        }

        SpawnPlayerHeroes(playerTable, _centerOffset, _cellSpacing);
        SpawnEnemyHeroes(enemyTable, _centerOffset, _cellSpacing);
    }

    private void SpawnPlayerHeroes(BattleTable playerTable, float centerOffset, float cellSpacing)
    {
        int numRows = playerTable.Table.Length;
        int numCols = playerTable.Table.Length > 0 ? playerTable.Table[0].HeroBoxes.Length : 0;

        for (int r = 0; r < numRows; r++)
        {
            for (int c = 0; c < numCols; c++)
            {
                HeroDefinition heroDef = playerTable.Table[r].HeroBoxes[c]?.baseHeroDefenition;
                if (heroDef != null)
                {
                    float x = -(centerOffset + (numCols - 1 - c) * cellSpacing);
                    float z = (numRows - 1 - r) * cellSpacing;
                    Vector3 position = new Vector3(x, 0, z);

                    if (TryInstantiateHero(heroDef, position, out HeroBehaviour heroBehaviour))
                    {
                        heroBehaviour.Team = TeamGroup.PLAYER;
                        _battlefieldManager.PlayerHeroes.Add(heroBehaviour);
                    }
                }
            }
        }
    }

    private void SpawnEnemyHeroes(BattleTable enemyTable, float centerOffset, float cellSpacing)
    {
        int numRows = enemyTable.Table.Length;
        int numCols = enemyTable.Table.Length > 0 ? enemyTable.Table[0].HeroBoxes.Length : 0;

        for (int r = 0; r < numRows; r++)
        {
            for (int c = 0; c < numCols; c++)
            {
                HeroDefinition heroDef = enemyTable.Table[r].HeroBoxes[c]?.baseHeroDefenition;
                if (heroDef != null)
                {
                    float x = centerOffset + c * cellSpacing;
                    float z = (numRows - 1 - r) * cellSpacing;
                    Vector3 position = new Vector3(x, 0, z);

                    if (TryInstantiateHero(heroDef, position, out HeroBehaviour heroBehaviour))
                    {
                        heroBehaviour.Team = TeamGroup.ENEMY;
                        _battlefieldManager.EnemyHeroes.Add(heroBehaviour);
                    }
                }
            }
        }
    }

    private bool TryInstantiateHero(HeroDefinition heroDef, Vector3 position, out HeroBehaviour heroBehaviour)
    {
        GameObject heroInstance = GameObject.Instantiate(_heroPrefab, position, Quaternion.identity, parent: null);
        heroInstance.name = $"Hero_{heroDef.Name}";

        if (heroInstance.TryGetComponent(out heroBehaviour))
            return true;

        return false;
    }
}
