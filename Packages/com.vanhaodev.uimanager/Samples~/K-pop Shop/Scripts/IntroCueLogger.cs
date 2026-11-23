using UnityEngine;

namespace vanhaodev.uimanager.samples.kpopshop
{
    /// <summary>
    /// Hears the intro's cues. Lives on the intro prefab beside its <see cref="IntroView"/> — the sequence finds
    /// every <see cref="IIntroCueHandler"/> there. This one only logs; a game plays its sounds here, through its
    /// own sound system so the player's volume applies.
    /// </summary>
    public sealed class IntroCueLogger : MonoBehaviour, IIntroCueHandler
    {
        public void OnIntroCue(IntroCue cue)
        {
            // e.g. AppearStart → "whoosh", Bounce → the logo's sting, HideStart → "pop".
            Debug.Log($"[K-pop Shop] Intro cue: {cue}");
        }
    }
}
