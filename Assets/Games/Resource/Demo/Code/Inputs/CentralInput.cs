using UnityEngine;
using UnityEngine.InputSystem;

namespace InfimaGames.Animated.ModernGuns
{

    public class CentralInput : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;
        public bool fire;
        public bool aim;
        public bool reload;
        public bool reloadEmpty;
        public bool inspect;
        public bool holster;
        public bool grenade;
        public bool knife;
        public bool lowered;
        public bool crouch;
        public bool escape;
        public bool changeAttachments;
        public bool changeSkins;
        public float scroll;
        public bool save;
        public bool cancel;


        public bool cursorLocked = true;
        private bool lastEState = false;


        private void Update()
        {

        }
        public void OnSave()
        {
            save = true;
        }

        public void OnCancel()
        {
            cancel = true;
        }

        public void OnMove(InputValue value)
        {
            move = value.Get<Vector2>();
        }

        public void OnLook(InputValue value)
        {
            look = value.Get<Vector2>() * 0.5f;
        }

        public void OnJump(InputValue value)
        {
            jump = value.isPressed;
        }

        public void OnSprint(InputValue value)
        {
            sprint = value.isPressed;
        }

        public void OnFire(InputValue value)
        {
            fire = value.isPressed;
        }

        public void OnAim(InputValue value)
        {
            aim = value.isPressed;
        }

        public void OnScroll(InputValue value)
        {
            scroll = value.Get<float>();
        }

        public void OnReload(InputValue value)
        {
            reload = value.isPressed;
        }

        public void OnReloadEmpty(InputValue value)
        {
            reloadEmpty = value.isPressed;
        }

        public void OnInspect(InputValue value)
        {
            inspect = true;
        }

        public void OnHolster(InputValue value)
        {
            holster = value.isPressed;
        }

        public void OnGrenade(InputValue value)
        {
            grenade = value.isPressed;
        }

        public void OnKnife(InputValue value)
        {
            knife = value.isPressed;
        }

        public void OnLowered(InputValue value)
        {
            lowered = value.isPressed;
        }

        public void OnChangeAttachment(InputValue value)
        {
            changeAttachments = true;

        }

        public void OnChangeSkins(InputValue value)
        {
            changeSkins = true;

        }

        public void OnEscape()
        {
            escape = true;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }

    }
}
