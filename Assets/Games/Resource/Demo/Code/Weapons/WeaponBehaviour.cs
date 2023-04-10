using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    public enum GunType
    {
        //步枪
        RIFLE,

        //手枪
        PISTOL,

        //散弹枪
        SHOTGUN,

        //狙击枪
        SNIPERRIFLE,

        //冲锋枪
        SUBMACHINE


    }
    public abstract class WeaponBehaviour : MonoBehaviour
    {

        #region UNITY

        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void Update() { }

        #endregion

        #region GETTERS

        /// <summary>
        /// Returns the name of this weapon item. This is very helpful for things like displaying the name.
        /// </summary>
        public abstract string GetItemName();

        /// <summary>
        /// Get the weapon type and decide how to fire it
        /// </summary>
        /// <returns></returns>
        public abstract GunType GetWeaponType();

        /// <summary>
        /// Get the weapon bullet num
        /// </summary>
        /// <returns></returns>
        public abstract int GetBulletNum();

        /// <summary>
        /// Whether the firing animation is playing
        /// </summary>
        /// <returns></returns>
        public abstract bool IsPlayingFire();


        public abstract float GetRateOfFire();

        /// <summary>
        /// Returns the RuntimeAnimationController the Character needs to use when this Weapon is equipped!
        /// </summary>
        public abstract RuntimeAnimatorController GetAnimatorController();
        /// <summary>
        /// Returns the weapon's attachment manager component.
        /// </summary>
        public abstract AttachmentBehaviour GetAttachments();

        #endregion

        #region METHODS

        /// <summary>
        /// Fires the weapon.
        /// </summary>
        public abstract void Fire();
        /// <summary>
        /// Reloads the weapon.
        /// </summary>
        public abstract void Reload();

        /// <summary>
        /// Spawns a new casing prefab and ejects it from the weapon. Really cool stuff.
        /// </summary>
        public abstract void EjectCasing();


        /// <summary>
        /// Spawns a new Bullet prefab and ejects it from the weapon. 
        /// </summary>
        public abstract void EjectBullet();

        #endregion
    }
}