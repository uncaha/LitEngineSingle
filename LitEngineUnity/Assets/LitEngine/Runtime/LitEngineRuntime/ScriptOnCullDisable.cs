using UnityEngine;
namespace LitEngine
{

    public class ScriptOnCullDisable : MonoBehaviour
    {

        public bool mIsCanFolowCamShow = true;
        public ParticleSystem mParticsystem = null;

        protected void OnBecameInvisible()
        {
            mParticsystem.Stop();
        }
        protected void OnBecameVisible()
        {
            mParticsystem.Play();
        }
    }
}
