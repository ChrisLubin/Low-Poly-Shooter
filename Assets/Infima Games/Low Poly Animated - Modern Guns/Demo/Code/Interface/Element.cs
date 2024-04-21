//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.Animated.ModernGuns.Interface
{
    /// <summary>
    /// Interface Element.
    /// </summary>
    public abstract class Element : MonoBehaviour
    {
        #region FIELDS

        /// <summary>
        /// Game Mode Service.
        /// </summary>
        protected IGameModeService gameModeService;

        /// <summary>
        /// Player Character.
        /// </summary>
        protected CharacterBehaviour characterBehaviour;
        /// <summary>
        /// Player Character Inventory.
        /// </summary>
        protected InventoryBehaviour inventoryBehaviour;

        /// <summary>
        /// Equipped Weapon.
        /// </summary>
        protected WeaponBehaviour equippedWeaponBehaviour;

        #endregion

        #region UNITY

        /// <summary>
        /// Awake.
        /// </summary>
        protected virtual void Awake()
        {
            SoldierManager.OnLocalPlayerSpawn += this.OnLocalPlayerSpawn;
            SoldierManager.OnLocalPlayerDeath += this.OnLocalPlayerDeath;
        }

        private void OnDestroy()
        {
            SoldierManager.OnLocalPlayerSpawn -= this.OnLocalPlayerSpawn;
            SoldierManager.OnLocalPlayerDeath -= this.OnLocalPlayerDeath;
        }

        /// <summary>
        /// Update.
        /// </summary>
        private void Update()
        {
            //Ignore if we don't have an Inventory.
            if (Equals(inventoryBehaviour, null))
                return;

            //Get Equipped Weapon.
            equippedWeaponBehaviour = inventoryBehaviour.GetEquipped();

            //Tick.
            Tick();
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Tick.
        /// </summary>
        protected virtual void Tick() { }

        private void OnLocalPlayerSpawn()
        {
            //Get Game Mode Service. Very useful to get Game Mode references.
            gameModeService = ServiceLocator.Current.Get<IGameModeService>();

            //Get Player Character.
            characterBehaviour = gameModeService.GetPlayerCharacter();
            //Get Player Character Inventory.
            inventoryBehaviour = characterBehaviour.GetInventory();
        }

        private void OnLocalPlayerDeath()
        {
            characterBehaviour = null;
            inventoryBehaviour = null;
        }

        #endregion
    }
}
