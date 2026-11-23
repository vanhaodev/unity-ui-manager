namespace vanhaodev.uimanager
{
    /// <summary>
    /// Hears the intro's <see cref="IntroCue"/>s — where a game hangs its sounds. Put components implementing
    /// this on the intro prefab; <see cref="IntroSequence"/> calls every one of them. The intro itself plays no
    /// audio, so it never needs the game's sound system or ignores the player's volume.
    /// </summary>
    public interface IIntroCueHandler
    {
        void OnIntroCue(IntroCue cue);
    }
}
