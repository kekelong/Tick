using Unity.VisualScripting;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Character Abstract Behaviour.
    /// </summary>
    public abstract class CharacterBehaviour : MonoBehaviour
    {
        #region UNITY

        /// <summary>
        /// Awake.
        /// </summary>
        protected virtual void Awake() { }
        /// <summary>
        /// Start.
        /// </summary>
        protected virtual void Start() { }

        /// <summary>
        /// Update.
        /// </summary>
        protected virtual void Update() { }

        #endregion

        #region GETTERS

        /// <summary>
        /// Returns a reference to the Inventory component.
        /// </summary>
        public abstract InventoryBehaviour GetInventory();

        /// <summary>
        /// Returns true if the game cursor is locked.
        /// </summary>
        public abstract bool IsCursorLocked();

        /// <summary>
        /// 获取当前剩余子弹
        /// </summary>
        /// <returns></returns>
        public abstract int GetBulletNum();

        /// <summary>
        /// Returns a reference to the input component.
        /// </summary>
        public abstract CentralInput GetCentralInput();
        #endregion
    }
}