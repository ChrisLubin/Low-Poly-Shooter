using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class KillFeedUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _killFeedText;
    private List<KillFeedItem> _killFeedItems = new();

    private const float _KILL_ITEM_DEFAULT_LIFE_SPAN = 9f;
    private const int _MAX_KILL_ITEM_COUNT = 4;
    private event Action _OnKillFeedListChange;

    private IDictionary<DamageType, string[]> _damageTypeToDescriptionMap;

    private void Awake()
    {
        this.InitializeMaps();
        MultiplayerSystem.OnPlayerDisconnect += this.OnPlayerDisconnect;
        SoldierManager.OnPlayerDeath += this.OnPlayerDeath;
        this._OnKillFeedListChange += this.OnKillFeedListChange;
    }

    private void OnDestroy()
    {
        MultiplayerSystem.OnPlayerDisconnect -= this.OnPlayerDisconnect;
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

    private void InitializeMaps()
    {
        string[] explosiveDescriptions = new[] { "EXPLODED", "OBLITERATED", "BLASTED", "ANNIHILATED", "ERADICATED", "DECIMATED" };

        this._damageTypeToDescriptionMap = new Dictionary<DamageType, string[]>()
        {
            { DamageType.Bullet, new[] { "Shot", "Deleted", "Eliminated", "Terminated", "Incapacitated", "Neutralized" } },
            { DamageType.Grenade, explosiveDescriptions },
            { DamageType.Missile, explosiveDescriptions },
        };
    }

    private void OnKillFeedListChange()
    {
        this._killFeedText.text = "";

        foreach (KillFeedItem item in this._killFeedItems)
        {
            this._killFeedText.text += $"{item.Text}\n";
        }
    }

    private void OnPlayerDeath(ulong deadClientId, ulong killerClientId, DamageType latestDamageType)
    {
        string killerPlayerName = MultiplayerSystem.Instance.GetPlayerUsername(killerClientId);
        string deadPlayerName = MultiplayerSystem.Instance.GetPlayerUsername(deadClientId);
        string killFeedText;

        // Remove oldest item if max count is reached
        if (this._killFeedItems.Count == _MAX_KILL_ITEM_COUNT)
            this._killFeedItems.RemoveAt(this._killFeedItems.Count - 1);

        if (killerClientId == deadClientId)
            killFeedText = $"{killerPlayerName} <color=red>SUICIDED</color>";
        else
            killFeedText = $"{killerPlayerName} <color=red>{this.GetKillDescription(latestDamageType)}</color> {deadPlayerName}";

        this._killFeedItems.Insert(0, new(killFeedText, _KILL_ITEM_DEFAULT_LIFE_SPAN));
        this._OnKillFeedListChange?.Invoke();
    }

    private void OnPlayerDisconnect(PlayerData player)
    {
        string playerLeftText = $"{player.Username} left";
        bool isPlayerLeftInKillFeed = this._killFeedItems.Any(item => item.Text == playerLeftText);
        if (isPlayerLeftInKillFeed) { return; }

        this._killFeedItems.Add(new(playerLeftText, _KILL_ITEM_DEFAULT_LIFE_SPAN));
        this._OnKillFeedListChange?.Invoke();
    }

    private string GetKillDescription(DamageType damageType) => this._damageTypeToDescriptionMap[damageType].GetRandomElement();

    private struct KillFeedItem
    {
        public string Text { get; private set; }
        public float CurrentLifeSpan { get; private set; }

        public KillFeedItem(string text, float defaultLifeSpan)
        {
            this.Text = text;
            this.CurrentLifeSpan = defaultLifeSpan;
        }

        public KillFeedItem DecreaseLifeSpan(float decreaseAmount)
        {
            this.CurrentLifeSpan = Math.Clamp(this.CurrentLifeSpan - decreaseAmount, 0, _KILL_ITEM_DEFAULT_LIFE_SPAN);
            return this;
        }
    }
}
