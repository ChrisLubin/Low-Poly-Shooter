//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// This component simply helps us hide the player’s entire interface with the press of a button. There’s not much to this one really as it is only a few lines long and they just set an object active or inactive based on input.
    /// </summary>
    public class HideInterface : MonoBehaviour
    {
        #region FIELDS SERIALIZED

        [Tooltip("The player’s interface object. This is the actual Game Object that we’re going to be messing around with in this script.")]
        [SerializeField]
        private GameObject interfaceObject;

        #endregion

        #region UNITY

        private void Awake()
        {
            GameManager.OnStateChange += this.OnGameStateChange;
            MultiplayerSystem.OnHostDisconnect += this.OnHostDisconnect;
            SoldierManager.OnLocalPlayerSpawn += this.OnLocalPlayerSpawn;
            SoldierManager.OnLocalPlayerDeath += this.OnLocalPlayerDeath;
            SoldierKillStreakController.OnLocalPlayerKillStreakActivatedOrDeactivated += this.OnLocalPlayerKillStreakActivatedOrDeactivated;
        }

        private void Start()
        {
            this.SetActive(false);
        }

        private void OnDestroy()
        {
            GameManager.OnStateChange -= this.OnGameStateChange;
            MultiplayerSystem.OnHostDisconnect -= this.OnHostDisconnect;
            SoldierManager.OnLocalPlayerSpawn -= this.OnLocalPlayerSpawn;
            SoldierManager.OnLocalPlayerDeath -= this.OnLocalPlayerDeath;
            SoldierKillStreakController.OnLocalPlayerKillStreakActivatedOrDeactivated -= this.OnLocalPlayerKillStreakActivatedOrDeactivated;
        }

        private void OnGameStateChange(GameState state)
        {
            switch (state)
            {
                case GameState.GameOver:
                    this.SetActive(false);
                    break;
                default:
                    break;
            }
        }

        private void OnLocalPlayerSpawn() => this.SetActive(true);
        private void OnLocalPlayerDeath() => this.SetActive(false);
        private void OnHostDisconnect() => this.SetActive(false);
        private void OnLocalPlayerKillStreakActivatedOrDeactivated(bool wasActivated) => this.SetActive(!wasActivated);
        private void SetActive(bool isActive) => interfaceObject.SetActive(isActive);

        #endregion
    }
}
