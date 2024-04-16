//Copyright 2022, Infima Games. All Rights Reserved.

using Unity.Netcode;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns.Interface
{
    /// <summary>
    /// The Crosshair component helps with making sure that the on-screen crosshair changes its appearance based on different events in the asset.
    /// </summary>
    public class Crosshair : MonoBehaviour
    {
        #region FIELDS SERIALIZED

        [Tooltip("Reference to the RectTransform that this component needs to modify in order to make the entire crosshair look bigger//smaller.")]
        [SerializeField]
        private RectTransform holderTransform;

        [Tooltip("Reference to the CanvasGroup component needed to modify the entire crosshair’s alpha value.")]
        [SerializeField]
        private CanvasGroup canvasGroup;

        #endregion

        #region FIELDS

        private Character character;
        /// <summary>
        /// Represents the current size of the crosshair.
        /// </summary>
        private float currentSize = 50.0f;
        /// <summary>
        /// Represents the current opacity of the crosshair.
        /// </summary>
        private float currentOpacity = 1.0f;

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

        private void OnDestroy()
        {
            GameManager.OnStateChange -= this.OnGameStateChange;
            MultiplayerSystem.OnHostDisconnect -= this.OnHostDisconnect;
            SoldierManager.OnLocalPlayerSpawn -= this.OnLocalPlayerSpawn;
            SoldierManager.OnLocalPlayerDeath -= this.OnLocalPlayerDeath;
            SoldierKillStreakController.OnLocalPlayerKillStreakActivatedOrDeactivated -= this.OnLocalPlayerKillStreakActivatedOrDeactivated;
        }

        #endregion

        /// <summary>
        /// Tick.
        /// </summary>
        private void Update()
        {
            if (!SoldierManager.IsLocalPlayerAlive) { return; }

            //Get AimingAlpha value from the character's animator. This is the value from [0, 1] that represents how much we're aiming.
            float aimingAlpha = character.AimingAlpha;
            //Modify the size based on that alpha so we can hide the crosshair when aiming.
            currentSize = Mathf.Lerp(50.0f, 0.0f, aimingAlpha);
            //Modify the opacity based on that alpha so we can hide the crosshair when aiming.
            currentOpacity = Mathf.Lerp(1.0f, 0.0f, aimingAlpha);

            //Update Horizontal Size.
            holderTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentSize);
            //Update Vertical Size.
            holderTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currentSize);
            //Update Alpha.
            canvasGroup.alpha = currentOpacity;
        }

        public void SetActive(bool isActive)
        {
            this.holderTransform.gameObject.SetActive(isActive);
            this.canvasGroup.gameObject.SetActive(isActive);
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

        private void OnLocalPlayerSpawn()
        {
            if (!SoldierManager.Instance.TryGetPlayer(NetworkManager.Singleton.LocalClientId, out SoldierController player)) { return; }

            this.character = player.GetComponent<Character>();
            this.SetActive(true);
        }

        private void OnLocalPlayerDeath() => this.SetActive(false);
        private void OnHostDisconnect() => this.SetActive(false);
        private void OnLocalPlayerKillStreakActivatedOrDeactivated(bool wasActivated) => this.SetActive(!wasActivated);
    }
}
