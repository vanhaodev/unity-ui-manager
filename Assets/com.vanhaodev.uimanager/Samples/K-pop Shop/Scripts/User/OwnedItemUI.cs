using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using vanhaodev.uimanager;

namespace vanhaodev.uimanager.samples.kpopshop
{
    /// <summary>
    /// UI for a purchased item in the home screen. Click to show details notice.
    /// </summary>
    public class OwnedItemUI : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private Image _imgThumbnail;
        [SerializeField] private TMP_Text _txtName;
        [SerializeField] private TMP_Text _txtDescription;
        [SerializeField] private TMP_Text _txtPrice;

        [Header("Button")]
        [SerializeField] private Button _btnClick;

        private ItemModel _item;
        private UIManager _uiManager;

        private void Awake()
        {
            _btnClick?.onClick.AddListener(OnClicked);
        }

        private void OnEnable()
        {
            ImageLoader.Load(this, _imgThumbnail, _item.ImageUrl);
        }

        private void OnDestroy()
        {
            _btnClick?.onClick.RemoveListener(OnClicked);
        }

        public void SetData(ItemModel item)
        {
            _item = item;
            if (item == null) return;

            if (_txtName != null) _txtName.text = item.Name;
            if (_txtDescription != null) _txtDescription.text = item.Description;
            if (_txtPrice != null) _txtPrice.text = $"<sprite=22> {item.PriceUsd:0.00}";
        }

        private void OnClicked()
        {
            if (_item == null) return;

            _uiManager ??= FindObjectOfType<UIManager>();
            _uiManager?.ShowPopup<PopupNotice>(p => p.SetData(_item.Name, _item.Description));
        }
    }
}
