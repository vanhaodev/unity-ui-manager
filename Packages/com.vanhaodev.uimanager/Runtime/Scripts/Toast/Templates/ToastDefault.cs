namespace vanhaodev.uimanager.template
{
    /// <summary>
    /// Default toast: message-only, slide-up from bottom.
    /// </summary>
    public class ToastDefault : BaseToast
    {
        protected override void Awake()
        {
            base.Awake();
            // SetAnimation(new TempToastSlideAnimation());
        }
    }
}
