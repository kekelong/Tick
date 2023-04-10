using UnityEngine;
using System.Collections;

namespace InfimaGames.Animated.ModernGuns
{
	/// <summary>
	/// Main Character Component
	/// </summary>
	public class Character : CharacterBehaviour
	{

        #region FIELDS SERIALIZED

        [Title(label: "Player Root")]
        [Tooltip("Player Root node")]
        [SerializeField]
        private GameObject root;


        [Title(label: "Inventory")]	
		[Tooltip("Inventory.")]
		[SerializeField]
		private InventoryBehaviour inventory;


		[Title(label: "Animation")]
		[Tooltip("Determines how smooth the locomotion blendspace is.")]
		[SerializeField]
		private float dampTimeLocomotion = 0.15f;
		

		[Title(label: "Animation Procedural")]		
		[Tooltip("Character Animator.")]
		[SerializeField]
		private Animator characterAnimator;



        [Title(label: "Foot node")]
        [Tooltip("Foot trage, Used for touchdown detection")]
        [SerializeField]
        private GameObject foot;


        [Title(label: "neck node")]
        [Tooltip("This node determines the rotation of the character")]
        [SerializeField]
        private GameObject view;

        #endregion

        #region FIELDS
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        private bool grounded = true;
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        private float groundedRadius = 0.56f;
        [Tooltip("The height the player can jump")]
        private float jumpHeight = 1.4f;
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        private float gravity = -20.0f;
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        private float fallTimeout = 0.15f;
        [Tooltip("Move speed of the character in m/s")]
        private float moveSpeed = 5.5f;
        [Tooltip("Sprint speed of the character in m/s")]
        private float sprintSpeed = 5.0f;
        [Tooltip("How far in degrees can you move the camera up")]
        private float topClamp = -40.0f;
        [Tooltip("How far in degrees can you move the camera down")]
        private float bottomClamp = 30.0f;
        [Tooltip("Touchdown detection layer")]
        public LayerMask layerMask;
        [Tooltip("当前武器子弹的数量")]
        private int bulletNum;
        [Tooltip("当前武器")]
        private WeaponBehaviour equippedWeapon;
        [Tooltip("角色控制器")]
        private CharacterController controller;
        [Tooltip("Inputs")]
        private CentralInput input;
        [Tooltip("Game Start Service")]
        private IGameStartService gameStartService;


        private float cameraTargetYaw;
        private float cameraTargetPitch;
        private float viewingAngle;

        //表示角色是否正在瞄准。
        private bool aiming;
        //表示角色是否在放下武器。
        private bool lowered;
        //窗口是否锁定
        private bool cursorLocked = false;
        //表示角色是否正在奔跑:按下奔跑键并且按下前进键
        private bool running;
        // 表示角色武器是否收起
        private bool holstered;
        // 表示角色是否蹲伏
        private bool crouching;
        //最后一次开火时间，防止射速过快，不和谐
        private float lastShotTime;
        //表示玩家是否按住瞄准按钮。
        private bool holdingButtonAim;
        //表示玩家是否按住奔跑按钮。
        private bool holdingButtonRun;
        //是否正在换弹
        private bool isPlayingReload;
        //控制移动的三维速度
        private float Velocity_x;
        private float Velocity_y;
        private float Velocity_z;
        // timeout deltatime
        private float _fallTimeoutDelta;
        private float terminalVelocity = 53.0f;
        bool previousFlag = true;

        #endregion

        #region UNITY

        protected override void Awake()
		{
		    input = GetComponent<CentralInput>();

            //UpdateCursorState();
            cursorLocked = input.cursorLocked;

            //Initialize Inventory.
            inventory.Init();
			//Refresh!
			RefreshWeaponSetup();

            controller = transform.parent.GetComponent<CharacterController>();

            //Get DataLinker.
            var dataLinker = GetComponent<DataLinker>();
			//Check Reference.
			if (dataLinker == null)
				return;
			//Cache IGameStartService.
			gameStartService = ServiceLocator.Current.Get<IGameStartService>();
		}
        protected override void Start()
        {
            StartCoroutine(CheckRoleadAnimationState());
        }
        protected override void Update()
		{
			//if (gameStartService.HasStarted())
            if (bulletNum <= 0)
            {
                PlayReloadAnimation();
            }
            GroundedCheck();
            JumpAndGravity();
            CameraRotation();
            Movement();
            UpdateAction();
            UpdateAnimator();
            Vector3 world = root.transform.TransformDirection(Velocity_x, Velocity_y, Velocity_z);
            controller.Move(world * Time.deltaTime);
        }

        #endregion

        #region FUNCTIONS
        public override InventoryBehaviour GetInventory() => inventory;
        public override bool IsCursorLocked() => cursorLocked;
        public override CentralInput GetCentralInput() => input;
        public override int GetBulletNum() => bulletNum;

        #endregion

        #region METHODS

        //TODO
        private void StopRunning() => running = false;
        private void IsRunning() => running = input.sprint && input.move.y > 0 && input.move.x == 0;

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            if (Physics.CheckSphere(foot.transform.position, groundedRadius, ~layerMask))
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawWireSphere(foot.transform.position, groundedRadius);
        }

        private void GroundedCheck()
        {
            grounded = Physics.CheckSphere(foot.transform.position, groundedRadius, ~layerMask);
            characterAnimator.SetBool(AHashes.Grounded, grounded);
        }

        private void JumpAndGravity()
        {
            if (grounded)
            {
                /*
                 * FallTimeout进入下降状态所需要的时间，
                 * 如果角色当前处于地面上 ，则需要重置 _fallTimeoutDelta 的值为 FallTimeout。
                 * 这意味着，如果角色在空中停留的时间不超过 FallTimeout，就不会进入失重状态
                 * 避免在下楼梯的过程中，出现抖动
                 */
                // reset the fall timeout timer
                _fallTimeoutDelta = fallTimeout;

                characterAnimator.SetBool(AHashes.Jumping, false);
                characterAnimator.SetBool(AHashes.FreeFall, false);

                if (Velocity_y < 0.0f)
                {
                    Velocity_y = -2f;
                }

                // Jump
                if (input.jump)
                {
                    //计算出起跳的初速度
                    Velocity_y = Mathf.Sqrt(jumpHeight * -2f * gravity);

                    characterAnimator.SetBool(AHashes.Jumping, true);
                }
            }
            else
            {
                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    characterAnimator.SetBool(AHashes.FreeFall, true);
                }
            }

            if (Velocity_y < terminalVelocity)
            {
                //V = V0 + at
                Velocity_y += gravity * Time.deltaTime;
            }

        }

        private void UpdateAction()
        {
            #region Aiming

            //Update Aiming Value.
            aiming = cursorLocked && input.aim && !holstered;

            //If we're aiming, make sure that the character can never have its weapon lowered or run. We do this because otherwise it looks super odd.
            if (aiming)
            {
                //Stop Lowered.
                lowered = false;
                //Stop Running.
                StopRunning();
            }
            //Update Animator Aiming.
            characterAnimator.SetBool(AHashes.Aim, aiming);

            #endregion

            #region Crouching

            //Check Input.
            if (cursorLocked && input.crouch)
            {
                //Toggle.
                crouching = !crouching;
                //Set Bool.
                characterAnimator.SetBool(AHashes.Crouching, crouching);
            }

            #endregion

            #region Firing

            bool currentFlag = input.fire;

            //We can't fire while holstered, so we check this right here.
            if (!holstered && cursorLocked && !isPlayingReload)
            {
                // 获取开火模式
                GunType mode = equippedWeapon.GetWeaponType();
                //如果上一帧已经开过火设为true

                if (currentFlag)
                {
                    switch (mode)
                    {
                        // 步枪 冲锋枪
                        case GunType.RIFLE:
                        case GunType.SUBMACHINE:
                            TryFire();
                            StopRunning();
                            break;

                        // 手枪 只有按下开火键才会开火，但是长按只会开火一次
                        case GunType.PISTOL:
                            if (!previousFlag)
                            {
                                TryFire();
                                StopRunning();
                            }
                            break;

                        // 狙击枪，散弹枪。 只有按下开火键才会开火，长按只会开火一次，并且每次开火必须等上次开火动画完成才能进行
                        case GunType.SHOTGUN:
                        case GunType.SNIPERRIFLE:

                            if (!equippedWeapon.IsPlayingFire() && !previousFlag)
                            {
                                TryFire();
                                StopRunning();
                            }
                            break;

                        default:
                            break;
                    }
                }

                previousFlag = currentFlag;
            }

            #endregion

            #region Lowering

            if (input.lowered && cursorLocked)
            {
                //Toggle.
                lowered = !lowered;

                //Stop running no matter whether the weapon is lowered or not. This way our transitions get to play properly.
                StopRunning();
            }
            //Update Lowered Value.
            characterAnimator.SetBool(AHashes.Lowered, lowered);
            #endregion

            #region Reloading

            //We can't reload while being holstered.
            if (!holstered && cursorLocked)
            {
                //Pressing the reload button.
                if (input.reload)
                {
                    PlayReloadAnimation();

                }
                //Pressing the reload empty button.
                if (input.reloadEmpty)
                {
                    PlayReloadAnimation("Reload Empty");

                }

            }

            #endregion

            #region Inspect

            //Pressing Inspect Button.
            if (input.inspect && !holstered && cursorLocked)
                Inspect();

            #endregion

            #region Holster

            //Pressing Holster Button.
            if (cursorLocked && input.holster)
            {
                //Stop Running/Lowered. These make no sense while holstering, obviously.
                lowered = false;

                StopRunning();

                //Set.
                SetHolstered(!holstered);

                //Play Animation.
                characterAnimator.CrossFade(holstered ? "Holster" : "Unholster", 0.0f, 3, 0.0f);
            }

            #endregion

            #region Grenade Throw

            //Pressing Grenade Button.
            if (input.grenade && !holstered && cursorLocked)
                PlayGrenadeThrow();

            #endregion

            #region Knife

            //Pressing Knife Button.
            if (input.knife)
                PlayMelee();

            #endregion

            #region Inventory Switching

            //Make sure the cursor is locked, and we're not paused.
            if (cursorLocked)
            {
                //Scroll Forward.
                if (input.scroll > 0)
                {
                    ScrollInventory(1);
                    StopRunning();
                }


                //Scroll Backward.
                if (input.scroll < 0)
                {
                    ScrollInventory(-1);
                    StopRunning();
                }

            }
            #endregion
            #region
            if(input.inspect && !holstered && cursorLocked && !running)
            {
                Inspect();
                input.inspect = false;
            }

            #endregion

            #region Escape

            //Pressed Escape Button.
            if (input.escape)
            {
                //Toggle the cursor locked value.
                cursorLocked = !cursorLocked;
                //Update the cursor's state.
                UpdateCursorState();
                input.escape = false;
            }
            #endregion
        }

        private void Movement()
        {
            IsRunning();
            Vector2 move = input.move;

            #region Calculate Velocity

            if (!holstered && cursorLocked)
            {
                if (running)
                {
                    lowered = false;
                    Velocity_z = sprintSpeed;
                }
                else
                {
                    Velocity_x = move.x * moveSpeed;
                    Velocity_z = move.y * moveSpeed;

                }
            }

            #endregion

            #region Running	

            characterAnimator.SetBool(AHashes.Running, running);

            #endregion

            #region Move
            Vector2 axisMovement;
            if (cursorLocked)
            {
                //input system同时按下会出现0.7，处理成 1
                float x = move.x == 0 ? 0.0f : Mathf.Sign(move.x);
                float y = move.y == 0 ? 0.0f : Mathf.Sign(move.y);
                axisMovement = new Vector2(x, y);
            }
            else
            {
                axisMovement = Vector2.zero;
            }
            //Movement Value. This value affects absolute movement. Aiming movement uses this, as opposed to per-axis movement.
            float movementValue = Mathf.Clamp01(Mathf.Abs(axisMovement.x) + Mathf.Abs(axisMovement.y));
            characterAnimator.SetFloat(AHashes.Movement, holstered ? 0.0f : movementValue, dampTimeLocomotion, Time.deltaTime);

            //Horizontal Movement Float.
            characterAnimator.SetFloat(AHashes.Horizontal, holstered ? 0.0f : axisMovement.x, dampTimeLocomotion, Time.deltaTime);
            //Vertical Movement Float.
            characterAnimator.SetFloat(AHashes.Vertical, holstered ? 0.0f : axisMovement.y, dampTimeLocomotion, Time.deltaTime);

            #endregion

        }
        private float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void CameraRotation()
        {
            cameraTargetYaw += input.look.x;
            cameraTargetPitch += input.look.y * 0.5f;
            cameraTargetYaw = ClampAngle(cameraTargetYaw, float.MinValue, float.MaxValue);
            Mathf.Clamp(cameraTargetYaw, float.MinValue, float.MaxValue);
            Quaternion yam = Quaternion.Euler(0.0f, cameraTargetYaw, 0.0f);
            root.transform.rotation = yam;

            Vector3 relativePos = view.transform.position;
            float angle = -input.look.y * 0.1f;
            Vector3 axis = view.transform.right;
            transform.RotateAround(relativePos, axis, angle);

        }
        private void UpdateAnimator()
        {


            #region Calculate Aiming Alpha

            //Aiming Alpha.
            var aimingAlpha = 0.0f;

            //This entire weird chunk of code just makes sure that the aimingAlpha value is properly set to the correct percentage.
            if (characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                aimingAlpha = characterAnimator.GetNextAnimatorStateInfo(0).IsName("Aim") ? Mathf.Lerp(0, 1f, characterAnimator.GetAnimatorTransitionInfo(0).normalizedTime) : 0.0f;
            }
            else if (characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Aim"))
            {
                aimingAlpha = characterAnimator.GetNextAnimatorStateInfo(0).IsName("Idle") ? Mathf.Lerp(1f, 0, characterAnimator.GetAnimatorTransitionInfo(0).normalizedTime) : 1.0f;
            }

            //Update the aiming value, but use interpolation. This makes sure that things like firing can transition properly.
            characterAnimator.SetFloat(AHashes.AimingAlpha, aimingAlpha);
            //Update weapon aiming value too!
            if (equippedWeapon)
                equippedWeapon.GetComponent<Animator>().SetFloat(AHashes.AimingAlpha, aimingAlpha);

            #endregion

            #region Update Grip Index

            //Get AttachmentBehaviour. This will allow us to get attachments.
            AttachmentBehaviour attachmentBehaviour = equippedWeapon.GetAttachments();
            //Reference Check.
            if (attachmentBehaviour != null)
            {
                //Get Grip Attachment.
                var gripBehaviour = attachmentBehaviour.GetVariant<GripBehaviour>("Grip");
                //Update the Grip Index value. This value is what changes the animation used for the idle pose.
                characterAnimator.SetFloat(AHashes.GripIndex, gripBehaviour != null ? gripBehaviour.GetIndex() : 0, 0.05f, Time.deltaTime);
            }

            #endregion

        }
        private void ScrollInventory(float value)
		{
			//Get the next index to switch to.
			int indexNext = value > 0 ? inventory.GetNextIndex() : inventory.GetLastIndex();
			//Get the current weapon's index.
			int indexCurrent = inventory.GetEquippedIndex();
					
			//Make sure we're allowed to change, and also that we're not using the same index, otherwise weird things happen!
			if (indexCurrent != indexNext)
				StartCoroutine(nameof(Equip), indexNext);
		}

		/// <summary>
		/// Plays the inspect animation.
		/// </summary>
		private void Inspect()
		{
			//Play.
			characterAnimator.CrossFade("Inspect", 0.0f, 1, 0);
			
			//Stop Running/Lowered To Inspect. Doing this actually helps a lot with feel.
			lowered = false;
		}
        /// <summary>
        /// Fires the character's weapon.
        /// </summary>
        private void TryFire()
        {
            //当时间间隔小于武器的射速时，不允许开火
            if (!(Time.time - lastShotTime > 60.0f / equippedWeapon.GetRateOfFire()))
                return;

            //Save the shot time, so we can calculate the fire rate correctly.
            lastShotTime = Time.time;

            if (bulletNum > 0)
            {
                //Play firing animation.
                characterAnimator.CrossFade("Fire", 0.0f, 1, 0);
                //Stop Running/Lowered To Fire. Doing this actually helps a lot with feel.
                lowered = false;
                StopRunning();
                bulletNum--;
            }
        }

        /// <summary>
        /// Plays the reload animation.
        /// </summary>
        private void PlayReloadAnimation(string animName = "Reload")
        {
            #region Animation
            //Play the animation state!
            characterAnimator.Play(animName, 1, 0.0f);

            #endregion
            //Reload.
            equippedWeapon.Reload();
            //update bulletNum
            bulletNum = equippedWeapon.GetBulletNum();
            //Stop Running/Lowered.
            lowered = false;
        }

        /// <summary>
        /// Equip Weapon Coroutine.
        /// </summary>
        private IEnumerator Equip(int index = 0)
		{
			//Only if we're not holstered, holster. If we are already, we don't need to wait.
			if(!holstered)
			{
				//Play.
				characterAnimator.CrossFade("Holster Quick", 0.0f, 3, 0.0f);
				//Holster.
				SetHolstered();
				//Wait.
				yield return new WaitUntil(() => characterAnimator.GetCurrentAnimatorStateInfo(3).IsName("Holster Quick Completed"));
			}
			
			//Equip The New Weapon.
			inventory.Equip(index);
			//Refresh.
			RefreshWeaponSetup();
			
			//Rebind. If we don't do this we get some super weird errors with some animation curves not working properly.
			characterAnimator.Rebind();

			characterAnimator.CrossFade("Unholster Quick", 0.0f, 3, 0.0f);
			//Unholster. We do this just in case we were holstered.
			SetHolstered(false);
		}
		/// <summary>
		/// Refresh all weapon things to make sure we're all set up!
		/// </summary>
		private void RefreshWeaponSetup()
		{
			//Make sure we have a weapon. We don't want errors!
			if ((equippedWeapon = inventory.GetEquipped()) == null)
				return;
			
			//Update Animator Controller.
			//We do this to update all animations to a specific weapon's set.
			characterAnimator.runtimeAnimatorController = equippedWeapon.GetAnimatorController();
		}

		/// <summary>
		/// Updates the cursor state based on the value of the cursorLocked variable.
		/// </summary>
		private void UpdateCursorState()
		{
			//Update cursor visibility.
			Cursor.visible = !cursorLocked;
			//Update cursor lock state.
			Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
		}

		/// <summary>
		/// Plays The Grenade Throwing Animation.
		/// </summary>
		private void PlayGrenadeThrow()
		{
			//Play.
			characterAnimator.CrossFade("Grenade Throw", 0.0f,
				1, 0.0f);
			
			//Stop Running/Lowered.
			lowered = false;  
		}
		/// <summary>
		/// Play The Melee Animation.
		/// </summary>
		private void PlayMelee()
		{
			//Play Normal.
			characterAnimator.CrossFade("Knife Attack", 0, 1, 0.0f);
			
			//Stop Running/Lowered.
			lowered = false; 
		}
		

		private void SetHolstered(bool value = true)
		{
			//Update value.
			holstered = value;			
			//Update Animator.
			const string boolName = "Holstered";
			characterAnimator.SetBool(boolName, holstered);
		}
		//检查当前是否在播放换弹动画
        private IEnumerator CheckRoleadAnimationState()
        {

            // 声明一个变量来保存上一帧的动画状态
            bool previousPlayingFire = false;

            while (true)
            {

                AnimatorStateInfo state = characterAnimator.GetCurrentAnimatorStateInfo(1);
                bool currentPlayingFire = state.IsName("Reload") || state.IsName("Reload Empty");

                // 如果当前播放的动画与上一帧的不同，则更新playingFire变量的值
                if (currentPlayingFire != previousPlayingFire)
                {
                    isPlayingReload = currentPlayingFire;
                }

                // 保存当前的播放状态，以备下一帧更新
                previousPlayingFire = currentPlayingFire;

                // 等待下一帧
                yield return null;
            }
        }
        #endregion
    }
}