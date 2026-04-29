using TMPro;
using UnityEngine;
using vanhaodev.uimanager.effect;

namespace vanhaodev.uimanager.templates
{
    public class LoadingBlockDefault : BaseLoadingBlock
    {
        [SerializeField] private TMP_Text _text;

        protected override void Awake()
        {
            base.Awake();
            var animation = GetComponent<ILoadingBlockAnimation>();
            if (animation != null)
                SetLoadingBlockAnimation(animation);
        }

        public void SetMessage(string message)
        {
            if (_text != null)
                _text.text = message;
        }
    }
}
