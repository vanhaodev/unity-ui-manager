using System;
using UnityEngine;
using UnityEngine.UI;
using vanhaodev.uimanager;
using vanhaodev.uimanager.effect;
using vanhaodev.uimanager.effect.templates;

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
        private bool _isShowWelcome = false;
        protected override void Awake()
        {
            base.Awake();
            _btnShop?.onClick.AddListener(OnShopClicked);
            _userManager ??= FindFirstObjectByType<UserManager>();
            SetAnimation(new TempSlideAnimation());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _btnShop?.onClick.RemoveListener(OnShopClicked);
        }

        public override void OnEnter()
        {
            if (_userManager != null)
                _userManager.OnBagChanged += RefreshPurchasedItems;

            RefreshPurchasedItems();
        }

        public override void OnExit()
        {
            if (_userManager != null)
                _userManager.OnBagChanged -= RefreshPurchasedItems;
        }

        private void OnShopClicked()
        {
            FindFirstObjectByType<UIManager>()?.ShowScreen<ScreenShop>();
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

        public override void Show(Action onComplete = null)
        {
            base.Show(() =>
            {
                if (_isShowWelcome == false)
                {
                    FindFirstObjectByType<UIManager>()?.ShowPopup<PopupNotice>(p =>
                    {
                        p.SetData("Welcome!", "Welcome to <b>K-pop Shop</b> sample XD" +
                                              "\nThis is a sample to help you better understand my UI Manager, " +
                                              "including built-in utilities for your game UI, " +
                                              "designed to stay simple and not overly complex.\n");
                    });
                    
                    FindAnyObjectByType<SoundManager>()?.PlayLoop("MainTheme", 0.3f);
                    _isShowWelcome = true;
                }

                onComplete?.Invoke();
            });
        }
    }
}
