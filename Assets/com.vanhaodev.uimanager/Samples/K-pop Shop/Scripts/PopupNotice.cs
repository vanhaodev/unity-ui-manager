using TMPro;
using UnityEngine;
using Vanhaodev.UIManager;

namespace vanhaodev.uimanager.samples.kpopshop
{
    public class PopupNotice : BasePopup
    {
        [SerializeField] private TMP_Text _txtTitle;
        [SerializeField] private TMP_Text _txtContent;

        public void SetData(string title, string content)
        {
            if (_txtTitle != null) _txtTitle.text = title;
            if (_txtContent != null) _txtContent.text = content;
        }
    }
}
