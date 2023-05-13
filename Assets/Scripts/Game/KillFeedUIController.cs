using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KillFeedUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _killFeedText;
    private List<KillFeedItem> _killFeedItems = new();

    private const float _KILL_ITEM_DEFAULT_LIFE_SPAN = 9f;
    private const int _MAX_KILL_ITEM_COUNT = 4;
    private event Action _OnKillFeedListChange;

    private void Awake()
    {
        SoldierManager.OnPlayerDeath += this.OnPlayerDeath;
        this._OnKillFeedListChange += this.OnKillFeedListChange;
    }

    private void OnDestroy()
    {
        SoldierManager.OnPlayerDeath -= this.OnPlayerDeath;
        this._OnKillFeedListChange -= this.OnKillFeedListChange;
    }

    private void Update()
    {
        if (this._killFeedItems.Count == 0) { return; }
        List<KillFeedItem> itemsToRemove = new();

        for (int i = 0; i < this._killFeedItems.Count; i++)
        {
            KillFeedItem item = this._killFeedItems[i];
            this._killFeedItems[i] = item.DecreaseLifeSpan(Time.deltaTime);
            if (item.CurrentLifeSpan > 0) { continue; }

            itemsToRemove.Add(item);
        }

        foreach (KillFeedItem item in itemsToRemove)
        {
            this._killFeedItems.Remove(item);
            this._OnKillFeedListChange?.Invoke();
        }
    }

    private void OnKillFeedListChange()
    {
        this._killFeedText.text = "";

        foreach (KillFeedItem item in this._killFeedItems)
        {
            this._killFeedText.text += $"{item.KillerName} -> {item.KilledPlayerName}\n";
        }
    }

    private void OnPlayerDeath(ulong deadClientId, ulong killerClientId)
    {
        string killerPlayerName = MultiplayerSystem.Instance.GetPlayerUsername(killerClientId);
        string deadPlayerName = MultiplayerSystem.Instance.GetPlayerUsername(deadClientId);

        // Remove oldest item if max count is reached
        if (this._killFeedItems.Count == _MAX_KILL_ITEM_COUNT)
        {
            this._killFeedItems.RemoveAt(this._killFeedItems.Count - 1);
        }

        this._killFeedItems.Insert(0, new(killerPlayerName, deadPlayerName, _KILL_ITEM_DEFAULT_LIFE_SPAN));
        this._OnKillFeedListChange?.Invoke();
    }

    private struct KillFeedItem
    {
        public string KillerName { get; private set; }
        public string KilledPlayerName { get; private set; }
        public float CurrentLifeSpan { get; private set; }

        public KillFeedItem(string killerName, string killedPlayerName, float defaultLifeSpan)
        {
            this.KillerName = killerName;
            this.KilledPlayerName = killedPlayerName;
            this.CurrentLifeSpan = defaultLifeSpan;
        }

        public KillFeedItem DecreaseLifeSpan(float decreaseAmount)
        {
            this.CurrentLifeSpan = Math.Clamp(this.CurrentLifeSpan - decreaseAmount, 0, _KILL_ITEM_DEFAULT_LIFE_SPAN);
            return this;
        }
    }
}
