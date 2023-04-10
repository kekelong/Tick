
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns.Interface
{

    public class TextFPS : ElementText
    {
        #region METHODS

        /// <summary>
        /// Tick.
        /// </summary>
        protected override void Tick()
        {

            textMesh.text = "FPS : " + 1 / Time.smoothDeltaTime;
        }        

        #endregion
    }
}