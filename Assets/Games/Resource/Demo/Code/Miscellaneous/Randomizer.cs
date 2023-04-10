using UnityEngine;
using UnityEngine.Windows;

namespace InfimaGames.Animated.ModernGuns
{
    /// <summary>
    /// 随机化角色/武器上的附件和皮肤
    /// </summary>
    public class Randomizer : MonoBehaviour
    {
        #region FIELDS SERIALIZED

        [Title(label: "References")]

        [Tooltip("Inventory Reference.")]
        [SerializeField]
        private InventoryBehaviour inventoryBehaviour;

        [Tooltip("Character Skinner Reference. We use this one to randomize the character skin when pressing the correct input.")]
        [SerializeField]
        private CharacterSkinner characterSkinner;

        [Tooltip("Reference to the character's main Animator component.")]
        [SerializeField]
        private Animator animator;

        //Inputs Data. 
        private CentralInput input;
        #endregion

        #region UNITY

        private void Start()
        {
            input = GetComponent<CentralInput>();

        }
        /// <summary>
        /// Update.
        /// </summary>
        private void Update()
        {

            //Get Equipped.
            WeaponBehaviour equipped = inventoryBehaviour.GetEquipped();
            //Check Reference.
            if (equipped == null)
                return;

            //Randomize Attachments.
            if (input.changeAttachments)
            {
                //Play Animation.
                Play();
                //Randomize Attachments.
                equipped.GetAttachments().Randomize();

                //Allows us to let every behaviour know that we're randomizing things.
                equipped.SendMessage("OnRandomize", SendMessageOptions.DontRequireReceiver);
                input.changeAttachments = false;
            }
            //Randomize Skins.
            if (input.changeSkins)
            {
                //Play Animation.
                Play();

                //Get WeaponSkinner.
                var skinner = equipped.GetComponent<WeaponSkinner>();
                //Check Reference.
                if (skinner != null)
                {
                    //Randomize.
                    skinner.Randomize();
                    //Apply.
                    skinner.Apply();
                }

                //Randomize.
                characterSkinner.Randomize();
                //Apply.
                characterSkinner.Apply();

                input.changeSkins = false;
            }
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Plays the randomizing animation.
        /// </summary>
        private void Play()
        {
            //Crossfade Randomize Animation.
            animator.CrossFade("Randomize", 0.0f, animator.GetLayerIndex("Randomization"), 0.0f);
        }

        #endregion
    }
}