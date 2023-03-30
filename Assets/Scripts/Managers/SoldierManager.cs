using UnityEngine;

public class SoldierManager : StaticInstanceWithLogger<SoldierManager>
{
    // public List<Soldier> Soldiers { get; private set; }

    public void SpawnSoldiers()
    {
        this._logger.Log("Spawning soldiers");
    }
}
