namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Game Mode Service.
    /// </summary>
    public interface IGameModeService : IGameService
    {
        #region FUNCTIONS
        
        /// <summary>
        /// Returns the Player Character.
        /// </summary>
        CharacterBehaviour GetPlayerCharacter();

        /// <summary>
        /// Returns the Player EquippedWeapon.
        /// </summary>
        WeaponBehaviour GetEquippedWeapon();

        /// <summary>
        /// Returns a reference to the input component.
        /// </summary>
        CentralInput GetCentralInput();
        #endregion
    }
}