using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolSystem : NetworkedStaticInstanceWithLogger<ObjectPoolSystem>
{
    private IDictionary<PoolType, ObjectPool<Transform>> _objectPoolMap = new Dictionary<PoolType, ObjectPool<Transform>>();
    private IDictionary<PoolType, Transform> _prefabMap = new Dictionary<PoolType, Transform>();
    [SerializeField] private Transform _pooledObjectsParent;

    [SerializeField] private Transform _bulletPrefab;
    [SerializeField] private Transform _muzzleFlashPrefab;

    private int _numOfPlayers = 1;

    public enum PoolType
    {
        Bullet,
        MuzzleFlash
    }

    protected override void Awake()
    {
        base.Awake();
        GameManager.OnStateChange += this.OnGameStateChange;
        RpcSystem.OnPlayerGameSceneLoaded += this.OnPlayerGameSceneLoaded;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        GameManager.OnStateChange -= this.OnGameStateChange;
        RpcSystem.OnPlayerGameSceneLoaded -= this.OnPlayerGameSceneLoaded;
    }

    private void Start()
    {
        foreach (int key in Enum.GetValues(typeof(PoolType)))
        {
            Transform prefab;

            switch (key)
            {
                case (int)PoolType.Bullet:
                    prefab = this._bulletPrefab;
                    break;
                case (int)PoolType.MuzzleFlash:
                    prefab = this._muzzleFlashPrefab;
                    break;
                default:
                    this._logger.Log($"The {key} transform doesn't exists", Logger.LogLevel.Error);
                    continue;
            }

            this._prefabMap[(PoolType)key] = prefab;
        }
    }

    private void OnPlayerGameSceneLoaded(ulong _, ulong[] connectedClientIds) => this._numOfPlayers = connectedClientIds.Length;

    private void OnGameStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.GameStarting:
                int bulletsToCachePerPlayer = 25;
                int muzzleFlashToCachePerPlayer = bulletsToCachePerPlayer / 5;
                int startingAmountBullet = bulletsToCachePerPlayer * this._numOfPlayers;
                int startingAmountMuzzleFlash = muzzleFlashToCachePerPlayer * this._numOfPlayers;
                this.InitPool(PoolType.Bullet, startingAmountBullet, startingAmountBullet, (int)(startingAmountBullet * 1.2f));
                this.InitPool(PoolType.MuzzleFlash, startingAmountMuzzleFlash, startingAmountMuzzleFlash, (int)(startingAmountMuzzleFlash * 1.2f));
                break;
        }
    }

    private void InitPool(PoolType type, int startingAmount, int startingSize, int maxSize)
    {
        if (this._objectPoolMap.ContainsKey(type))
        {
            this._logger.Log($"The {type} pool has already been initialized", Logger.LogLevel.Error);
            return;
        }
        if (!this._prefabMap.ContainsKey(type))
        {
            this._logger.Log($"The {type} prefab doesn't exist", Logger.LogLevel.Error);
            return;
        }

        this._prefabMap.TryGetValue(type, out Transform prefab);
        this._objectPoolMap[type] = new ObjectPool<Transform>(() => Instantiate(prefab, this._pooledObjectsParent),
        transform => transform.gameObject.SetActive(true),
        transform => transform.gameObject.SetActive(false),
        transform => Destroy(transform.gameObject),
        false, startingSize, maxSize);
        Transform[] transformArray = new Transform[startingAmount];

        for (int i = 0; i < startingAmount; i++)
        {
            this.TryGetObject(type, out Transform transform);
            transformArray[i] = transform;
        }
        for (int i = 0; i < startingAmount; i++)
        {
            this.ReleaseObject(type, transformArray[i]);
        }
    }

    public bool TryGetObject(PoolType type, out Transform transform)
    {
        if (!this._objectPoolMap.ContainsKey(type))
        {
            this._logger.Log($"The {type} pool has not been initialized", Logger.LogLevel.Error);
            transform = default(Transform);
            return false;
        }

        transform = this._objectPoolMap[type].Get();
        return true;
    }

    public void ReleaseObject(PoolType type, Transform transform)
    {
        if (!this._objectPoolMap.ContainsKey(type))
        {
            this._logger.Log($"The {type} pool has not been initialized", Logger.LogLevel.Error);
            return;
        }

        this._objectPoolMap[type].Release(transform);
    }
}
