


using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Game Mode Service.
    /// </summary>
    public class GameModeService : IGameModeService
    {
        #region FIELDS
        
        /// <summary>
        /// The Player Character.
        /// </summary>
        private CharacterBehaviour playerCharacter;
        
        #endregion
        
        #region FUNCTIONS
        
        public CharacterBehaviour GetPlayerCharacter()
        {
            //Make sure we have a player character that is good to go!
            if (playerCharacter == null)
                playerCharacter = UnityEngine.Object.FindObjectOfType<CharacterBehaviour>();        
            //Return.
            return playerCharacter;
        }

        /// <summary>
        /// GetEquippedWeapon.
        /// </summary>
        public WeaponBehaviour GetEquippedWeapon()
        {
            //Get Character.
            CharacterBehaviour characterBehaviour = GetPlayerCharacter();
            //Check Reference.
            if (characterBehaviour == null)
                return null;
            
            //Get Inventory.
            InventoryBehaviour inventoryBehaviour = characterBehaviour.GetInventory();
            //Check Reference.
            if (inventoryBehaviour == null)
                return null;
            
            //Return.
            return inventoryBehaviour.GetEquipped();
        }
        public CentralInput GetCentralInput()
        {
            //Get Character.
            CharacterBehaviour characterBehaviour = GetPlayerCharacter();
            //Check Reference.
            if (characterBehaviour == null)
                return null;

            //Get CentralInput.
            CentralInput input = characterBehaviour.GetCentralInput();
            //Check Reference.
            if (input == null)
            {
                Debug.Log("game mode service" + "input null");
            }

            //Return.
            return input;
        }

        #endregion
    }
}