using System.Collections.Generic;
using Toolbox;
using UnityEngine;

public class BattlefieldManager : Singleton<BattlefieldManager>
{
    [SerializeField]
    [Tooltip("Корневой объект для показа/скрытия сцены")]
    private GameObject _battleSceneRoot;

    private List<HeroBehaviour> _playerHeroes = new List<HeroBehaviour>();
    public List<HeroBehaviour> PlayerHeroes => _playerHeroes;

    private List<HeroBehaviour> _enemyHeroes = new List<HeroBehaviour>();
    public List<HeroBehaviour> EnemyHeroes => _enemyHeroes;

    [Header("Spawner")]

    private BattlefieldSpawner _spawner;
    [SerializeField]
    [Tooltip("Расстояние от центра до первого ряда")]
    private float _centerOffset = 3;
    [SerializeField]
    [Tooltip("Расстояние между отрядами в сетке")]
    private float _cellSpacing = 2;
    [SerializeField]
    private GameObject _heroPrefab;

#if DEBUG
    [SerializeField]
    private BattleTable _playerBattleTable;
    [SerializeField]
    private BattleTable _enemyBattleTable;
#endif

    private void Start()
    {
        _spawner = new BattlefieldSpawner(this, _centerOffset, _cellSpacing, _heroPrefab);

#if DEBUG
        StartBattle(_playerBattleTable, _enemyBattleTable);
#endif
    }

    public void StartBattle(BattleTable playerTeamTable, BattleTable enemyTeamTable)
    {
        FullReset();
        Debug.Log("BattlefieldManager командует начать бой!");
        _spawner.DoSpawning(playerTeamTable, enemyTeamTable);
        ShowScene();
    }

    public void EndBattle()
    {
        Debug.Log("BattlefieldManager командует завершить бой!");
        FullReset();
    }

    private void FullReset()
    {
        foreach (var hero in _playerHeroes)
        {
            if (hero != null)
                Destroy(hero.gameObject);
        }
        _playerHeroes.Clear();

        foreach (var hero in _enemyHeroes)
        {
            if (hero != null)
                Destroy(hero.gameObject);
        }
        _enemyHeroes.Clear();

        HideScene();
    }

    public void ShowScene() => _battleSceneRoot?.SetActive(true);

    public void HideScene() => _battleSceneRoot?.SetActive(false);
}
