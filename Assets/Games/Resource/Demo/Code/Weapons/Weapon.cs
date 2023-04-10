using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// Weapon. This class handles most of the things that weapons need.
    /// </summary>
    public class Weapon : WeaponBehaviour
    {
        #region FIELDS SERIALIZED

        [Title(label: "General")]

        [Tooltip("The name of this item. Mostly just for display purposes.")]
        [SerializeField]
        private string itemName = "SP60";

        [Title(label: "Firing")]

        [Tooltip("The number of bullets in the weapon")]
        [SerializeField]
        private int bulletNum = 35;
        [Tooltip("Amount of shots this weapon can shoot in a minute. It determines how fast the weapon shoots.")]
        [SerializeField]
        private int roundsPerMinutes = 200;

        [Title(label: "Casings")]

        [Tooltip("Determines the amount of time that has to pass right after firing for a casing prefab to get ejected from the weapon!")]
        [SerializeField]
        private float casingDelay;

        [Title(label: "Resources")]

        [Tooltip("The AnimatorController a player character needs to use while wielding this weapon.")]
        [SerializeField]
        public RuntimeAnimatorController controller;

        [Tooltip("Prefab spawned when firing this weapon as a casing.")]
        [SerializeField]
        private GameObject casingPrefab;

        [Tooltip("Bullet Prefab")]
        [SerializeField]
        private GameObject bulletPrefab;

        [Tooltip("Weapon Type")]
        [SerializeField]
        private GunType weaponType;

        #endregion

        #region FIELDS
        private Sockets so;
        /// <summary>
        /// Weapon Animator.
        /// </summary>
        private Animator animator;
        /// <summary>
        /// Attachment Manager.
        /// </summary>
        private AttachmentBehaviour attachment;

        /// <summary>
        /// The player character's camera.
        /// </summary>
        private Transform playerCamera;
        private bool playingFire;
        private bool playingReload;

        #endregion

        #region UNITY

        /// <summary>
        /// Awake.
        /// </summary>
        protected override void Awake()
        {
            //Get Animator.
            animator = GetComponent<Animator>();
            //Get Attachment Manager.
            attachment = GetComponent<AttachmentBehaviour>();

            //Get Sockets.
            so = GetComponent<Sockets>();
            //Check Reference.
            if (so == null)
                return;
        }
        protected override void Update()
        {
          
         

        }
        protected override void Start()
        {
            // 启动协程来持续监视动画状态机的播放状态
            StartCoroutine(CheckFireAnimationState());
        }

        #endregion

        #region GETTERS
        public override GunType GetWeaponType() => weaponType;
        public override bool IsPlayingFire() => playingFire;
        public override float GetRateOfFire() => roundsPerMinutes;
        /// <summary>
        /// GetItemName.
        /// </summary>
        public override string GetItemName() => itemName;

        public override int GetBulletNum() => bulletNum;
        /// <summary>
        /// GetAnimatorController.
        /// </summary>
        public override RuntimeAnimatorController GetAnimatorController() => controller;
        /// <summary>
        /// GetAttachmentManager.
        /// </summary>
        public override AttachmentBehaviour GetAttachments() => attachment;

        #endregion

        #region METHODS

        private IEnumerator CheckFireAnimationState()
        {

            // 声明一个变量来保存上一帧的动画状态
            bool previousPlayingFire = false;

            while (true)
            {
                // 获取当前动画状态机中名为"Fire"的动画状态
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                bool currentPlayingFire = state.IsName("Fire") && state.normalizedTime < 0.85;

                // 如果当前播放的动画与上一帧的不同，则更新playingFire变量的值
                if (currentPlayingFire != previousPlayingFire)
                {
                    playingFire = currentPlayingFire;
                }

                // 保存当前的播放状态，以备下一帧更新
                previousPlayingFire = currentPlayingFire;

                // 等待下一帧
                yield return null;
            }
        }

        /// <summary>
        /// Waits for the necessary casingDelay before spawning a casing to eject from the weapon. This is very helpful to showcase weapons like a sniper rifle, which might have a bolt pull after firing that is when the casing gets ejected.
        /// </summary>
        private IEnumerator WaitForCasing()
        {
            //Yield.
            yield return new WaitForSeconds(casingDelay);

            //Eject.
            EjectCasing();
        }

        /// <summary>
        /// Reload.
        /// </summary>
        public override void Reload()
        {
            //Set Reloading Bool. This helps cycled reloads know when they need to stop cycling.
            const string boolName = "Reloading";
            animator.SetBool(boolName, true);
        }

        /// <summary>
        /// EjectCasing.
        /// </summary>
        public override void EjectCasing()
        {
            //Get Eject Socket.
            Transform ejectSocket = so.GetSocketTransform("SOCKET_Eject");
            //Check Reference.
            if (ejectSocket == null)
                return;

            //Instantiate.
            Instantiate(casingPrefab, ejectSocket.position, ejectSocket.rotation);
        }
        public override void EjectBullet()
        {
            //Get Eject Socket.
            Transform e = so.GetSocketTransform("SOCKET_Barrel");
            //Check Reference.
            if (e == null)
                return;
            //Instantiate.
            GameObject bulletObj = Instantiate(bulletPrefab, e.position, Quaternion.identity);
            bulletObj.transform.forward = -e.transform.up;
        }
        /// <summary>
        /// Fire.
        /// </summary>
        public override void Fire()
        {
            //Get Muzzle.
            var muzzleBehaviour = attachment.GetVariant<MuzzleBehaviour>("Muzzle");
            //Check Reference.
            if (muzzleBehaviour == null)
                return;

            //Fire Muzzle.
            muzzleBehaviour.Fire();

            EjectBullet();
            //Make sure we're not still waiting for some other casing to spawn.
            StopCoroutine(nameof(WaitForCasing));

            //Spawn Casing.
            StartCoroutine(nameof(WaitForCasing));
        }

        #endregion
    }
}