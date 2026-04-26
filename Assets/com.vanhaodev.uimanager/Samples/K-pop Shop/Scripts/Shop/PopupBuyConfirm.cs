using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using vanhaodev.uimanager;

namespace vanhaodev.uimanager.samples.kpopshop
{
    /// <summary>
    /// Confirmation popup shown before buying a shop item.
    /// </summary>
    public class PopupBuyConfirm : BasePopup
    {
        [Header("Image")]
        [SerializeField] private Image _imgThumbnail;

        [Header("Texts")]
        [SerializeField] private TMP_Text _txtName;
        [SerializeField] private TMP_Text _txtPrice;

        [Header("Buttons")]
        [SerializeField] private Button _btnConfirm;
        [SerializeField] private Button _btnCancel;

        private ItemModel _item;
        private Action<ItemModel> _onConfirm;

        protected override void Awake()
        {
            base.Awake();
            _btnConfirm?.onClick.AddListener(OnConfirmClicked);
            _btnCancel?.onClick.AddListener(OnCancelClicked);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _btnConfirm?.onClick.RemoveListener(OnConfirmClicked);
            _btnCancel?.onClick.RemoveListener(OnCancelClicked);
        }

        public void SetData(ItemModel item, Action<ItemModel> onConfirm = null)
        {
            _item = item;
            _onConfirm = onConfirm;

            if (item == null) return;
            if (_txtName != null) _txtName.text = item.Name;
            if (_txtPrice != null) _txtPrice.text = $"${item.PriceUsd:0.00}";
        }

        public override void Show(Action onComplete = null)
        {
            base.Show(onComplete);
            ImageLoader.Load(this, _imgThumbnail, _item.ImageUrl);
        }

        private void OnConfirmClicked()
        {
            _onConfirm?.Invoke(_item);
            Manager?.ClosePopup(this);
        }

        private void OnCancelClicked()
        {
            Manager?.ClosePopup(this);
        }
    }
}
