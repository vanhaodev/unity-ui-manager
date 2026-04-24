using UnityEngine;
using UnityEngine.UI;
using Vanhaodev.UIManager;

namespace vanhaodev.uimanager.samples.kpopshop
{
    public class ScreenHome : BaseScreen
    {
        [Header("Navigation")]
        [SerializeField] private Button _btnShop;

        [Header("Purchased Items")]
        [SerializeField] private Transform _itemContainer;
        [SerializeField] private OwnedItemUI _itemPrefab;

        private UserManager _userManager;

        protected override void Awake()
        {
            base.Awake();
            _btnShop?.onClick.AddListener(OnShopClicked);
            _userManager = GetComponent<UserManager>();

            FindObjectOfType<UIManager>()?.ShowPopup<PopupNotice>(p =>
            {
                p.SetData("Welcome!", "Welcome to <b>K-pop Shop</b> sample XD");
            });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _btnShop?.onClick.RemoveListener(OnShopClicked);
        }

        public override void OnEnter(object data = null)
        {
            Debug.Log("[ScreenHome] Entered");

            if (_userManager != null)
                _userManager.OnBagChanged += RefreshPurchasedItems;

            RefreshPurchasedItems();
        }

        public override void OnExit()
        {
            Debug.Log("[ScreenHome] Exited");
            if (_userManager != null)
                _userManager.OnBagChanged -= RefreshPurchasedItems;
        }

        private void OnShopClicked()
        {
            FindObjectOfType<UIManager>()?.ShowScreen<ScreenShop>();
        }

        private void RefreshPurchasedItems()
        {
            if (_itemContainer == null || _itemPrefab == null || _userManager?.Bag == null) return;

            // Clear existing children
            for (int i = _itemContainer.childCount - 1; i >= 0; i--)
                Destroy(_itemContainer.GetChild(i).gameObject);

            // Spawn purchased items
            foreach (var item in _userManager.Bag.PurchasedItems)
            {
                var ui = Instantiate(_itemPrefab, _itemContainer);
                ui.SetData(item);
            }
        }
    }
}
