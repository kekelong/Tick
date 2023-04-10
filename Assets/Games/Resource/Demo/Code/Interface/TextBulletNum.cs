
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns.Interface
{

    public class TextBulletNum : ElementText
    {
        #region METHODS

        /// <summary>
        /// Tick.
        /// </summary>
        protected override void Tick()
        {

            //Base.
            base.Tick();

            //Get ObjectLinker.
            var objectLinker = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();

            int num = objectLinker.GetBulletNum();
            textMesh.text = $"Bullet: " + num;
        }

        #endregion
    }
}