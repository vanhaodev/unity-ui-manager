using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using vanhaodev.uimanager;
using vanhaodev.uimanager.template;
using vanhaodev.uimanager.templates;

namespace vanhaodev.uimanager.samples.kpopshop
{
    /// <summary>
    /// UI component for displaying a shop item. Click to open buy confirmation popup.
    /// </summary>
    public class ItemUI : MonoBehaviour
    {
        [Header("Image")]
        [SerializeField] private Image _imgThumbnail;

        [Header("Texts")]
        [SerializeField] private TMP_Text _txtName;
        [SerializeField] private TMP_Text _txtDescription;
        [SerializeField] private TMP_Text _txtPrice;

        [Header("Button")]
        [SerializeField] private Button _btnBuy;

        private ItemModel _item;
        private UIManager _uiManager;
        private ShopManager _shopManager;

        private void Awake()
        {
            _btnBuy?.onClick.AddListener(OnBuyClicked);
        }

        private void OnDestroy()
        {
            _btnBuy?.onClick.RemoveListener(OnBuyClicked);
        }

        public void SetData(ItemModel item)
        {
            _item = item;
            if (item == null) return;

            if (_txtName != null) _txtName.text = item.Name;
            if (_txtDescription != null) _txtDescription.text = item.Description;
            if (_txtPrice != null) _txtPrice.text = $"<sprite=22> {item.PriceUsd:0.00}";

            ImageLoader.Load(this, _imgThumbnail, item.ImageUrl);
        }

        private void OnBuyClicked()
        {
            if (_item == null) return;

            _uiManager ??= FindFirstObjectByType<UIManager>();
            _shopManager ??= FindFirstObjectByType<ShopManager>();
            _uiManager?.ShowPopup<PopupBuyConfirm>(p => p.SetData(_item, OnBuyConfirmed));
        }

        private void OnBuyConfirmed(ItemModel item)
        {
            _uiManager?.LoadingBlock<LoadingBlockDefault>(onSetup: (lb) => { lb.SetMessage("Buying..."); },
                action: async () =>
                {
                    await Task.Delay(3500);
                    _shopManager?.BuyItem(item);
                });
        }
    }
}
