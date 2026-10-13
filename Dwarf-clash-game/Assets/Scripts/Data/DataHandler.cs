using Data;
using Toolbox;
using UnityEngine;

public class DataHandler : Singleton<DataHandler>
{
    [SerializeField]
    private GameDataHub _dataHub;

    public GameDataHub DataHub => _dataHub;
}
