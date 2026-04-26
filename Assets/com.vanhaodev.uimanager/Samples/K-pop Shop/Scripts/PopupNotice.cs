using TMPro;
using UnityEngine;
using vanhaodev.uimanager;
using vanhaodev.uimanager.effect;

namespace vanhaodev.uimanager.samples.kpopshop
{
    public class PopupNotice : BasePopup
    {
        [SerializeField] private TMP_Text _txtTitle;
        [SerializeField] private TMP_Text _txtContent;

        protected override void Awake()
        {
            base.Awake();
            SetAnimation(new FadeAnimation());
        }

        public void SetData(string title, string content)
        {
            if (_txtTitle != null) _txtTitle.text = title;
            if (_txtContent != null) _txtContent.text = content;
        }
    }
}
