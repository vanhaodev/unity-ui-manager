using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using vanhaodev.uimanager.samples.kpopshop.animation;

namespace vanhaodev.uimanager.samples.kpopshop
{
    public class ScreenHome : BaseScreen
    {
        [Header("Navigation")]
        [SerializeField] private Button _btnShop;

        [Header("Money")]
        [SerializeField] private TMP_Text _txtMoney;
        [SerializeField] private FlyoutTarget _moneyFlyoutTarget;
        [SerializeField] private Button _btnAddMoney;
        [SerializeField] private Sprite _coinSprite;

        [Header("Purchased Items")]
        [SerializeField] private Transform _itemContainer;
        [SerializeField] private OwnedItemUI _itemPrefab;

        [Header("Floating Text Test")]
        [Tooltip("Spam this button to test FloatingTextAlbum stacking: each click floats a random album.")]
        [SerializeField] private Button _btnTestFloat;

        // Self-contained album data for the test button (Home has no ShopManager — it lives in the Shop screen).
        private static readonly (string Name, string Url)[] _testAlbums =
        {
            ("NewJeans - New Jeans (1st EP)", "https://cdn-images.dzcdn.net/images/cover/3bf93527469ed115356e2663e234c8f2/1900x1900-000000-80-0-0.jpg"),
            ("NewJeans - OMG", "https://upload.wikimedia.org/wikipedia/vi/1/10/NewJeans_OMG_cover.jpg"),
            ("NewJeans - Get Up", "https://upload.wikimedia.org/wikipedia/vi/6/62/Get_Up_NJ.jpg"),
            ("BabyMonster - BABYMONS7ER", "https://upload.wikimedia.org/wikipedia/en/6/63/BabyMonster_-_BabyMons7er.jpg"),
            ("BabyMonster - DRIP", "https://colorcodedlyrics.com/wp-content/uploads/2024/10/BABYMONSTER-DRIP-AlbumArt.png"),
        };

        private UserManager _userManager;
        private UIManager _uiManager;
        private bool _isShowWelcome = false;
        protected override void Awake()
        {
            base.Awake();
            _btnShop?.onClick.AddListener(OnShopClicked);
            _btnTestFloat?.onClick.AddListener(OnTestFloatClicked);
            _btnAddMoney?.onClick.AddListener(OnAddMoneyClicked);
            _userManager ??= FindFirstObjectByType<UserManager>();
            SetAnimation(new TempSlideAnimation());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _btnShop?.onClick.RemoveListener(OnShopClicked);
            _btnTestFloat?.onClick.RemoveListener(OnTestFloatClicked);
            _btnAddMoney?.onClick.RemoveListener(OnAddMoneyClicked);
        }

        public override void OnEnter()
        {
            if (_userManager != null)
            {
                _userManager.OnBagChanged += RefreshPurchasedItems;
                _userManager.OnBagChanged += RefreshMoney;
            }

            RefreshPurchasedItems();
            InitMoney();
            PreloadTestAlbumCovers();
        }

        public override void OnExit()
        {
            if (_userManager != null)
            {
                _userManager.OnBagChanged -= RefreshPurchasedItems;
                _userManager.OnBagChanged -= RefreshMoney;
            }
        }

        private void OnShopClicked()
        {
            FindFirstObjectByType<UIManager>()?.ShowScreen<ScreenShop>();
        }

        // Warm the sprite cache so spam-clicking the test button shows covers instantly.
        private void PreloadTestAlbumCovers()
        {
            foreach (var album in _testAlbums)
                ImageLoader.LoadSprite(this, album.Url, null);
        }

        // Float a random album from the test button — spam it to see the stacking behaviour.
        private void OnTestFloatClicked()
        {
            if (_btnTestFloat == null) return;
            var album = _testAlbums[UnityEngine.Random.Range(0, _testAlbums.Length)];
            _uiManager ??= FindFirstObjectByType<UIManager>();

            // Float from the test button so you can move it to any corner/edge and watch the clamp/flip.
            ImageLoader.LoadSprite(this, album.Url, cover =>
                _uiManager?.ShowFloatingText<FloatingTextAlbum>(
                    t => t.SetAlbum(album.Name, cover), _btnTestFloat.transform));
        }

        private void OnAddMoneyClicked()
        {
            if (_btnAddMoney == null || _userManager == null) return;
            _uiManager ??= FindFirstObjectByType<UIManager>();

            const float addAmountUsd = 10f;
            const int addAmountCents = (int)(addAmountUsd * 100); // 1000 cents

            // Play flyout effect from button to money field
            // AddMoney is called AFTER flyout completes to avoid interfering with lerp
            if (_uiManager != null && _coinSprite != null)
            {
                _uiManager.PlayFlyout(
                    sourceWorldPos: _btnAddMoney.transform.position,
                    targetKey: "money",
                    amount: addAmountCents,
                    icon: _coinSprite,
                    onComplete: () => _userManager.AddMoney(addAmountUsd)
                );
            }
            else
            {
                _userManager.AddMoney(addAmountUsd);
            }
        }

        private void RefreshMoney()
        {
            // Flyout handles the visual update via NotifyIconArrived
            // This just ensures bag state is in sync (called by OnBagChanged)
        }

        private void InitMoney()
        {
            if (_userManager?.Bag == null) return;

            // Initial sync - set both display and target
            _moneyFlyoutTarget?.SetValue((long)(_userManager.Bag.MoneyUsd * 100));
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
                    var ui = FindFirstObjectByType<UIManager>();

                    ui?.ShowPopup<PopupNotice>(p =>
                    {
                        p.SetData("Welcome!",
                            "Welcome to <b>K-pop Shop</b> sample XD\n" +
                            "This is a sample to help you better understand my UI Manager.");
                    });

                    ui?.ShowPopup<PopupNotice>(p =>
                    {
                        p.SetData("Info",
                            "Including built-in utilities for your game UI,\n" +
                            "designed to stay simple and not overly complex.");
                    });
                    FindAnyObjectByType<SoundManager>()?.PlayLoop("MainTheme", 0.3f);
                    _isShowWelcome = true;
                }

                onComplete?.Invoke();
            });
        }
    }
}
