using Data;
using Toolbox;
using UnityEngine;

public class Player : Singleton<Player>
{
    [SerializeField]
    private PlayerDefinition _playerDefinition;

    public PlayerDefinition PlayerDefinition => _playerDefinition;

}
