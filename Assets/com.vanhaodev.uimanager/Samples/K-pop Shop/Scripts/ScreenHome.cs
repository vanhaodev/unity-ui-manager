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

        [Tooltip("Click for a random message of a random length — the point is to watch FloatingTextMessage's "
                 + "box size itself to whatever string it is handed.")]
        [SerializeField] private Button _btnTestMessage;

        // Deliberately graded from two characters to far more than fits, so one button walks the fitter
        // through every branch it has: hug the string, stop at Max Width, shrink the font, ellipsize.
        private static readonly string[] _testMessages =
        {
            "Hi",
            "Wow",
            "Nice one",
            "Feeling good today",
            "Let's dance until the sun comes up",
            "This box should hug the line exactly",
            "A longer line that starts pushing right up against the max width",
            "Here is a much longer message, long enough that the box has to stop growing and the font has to shrink a step",
            "And this last one simply runs on and on, well past anything a floating message has any business saying, so the width clamps, the font bottoms out at its minimum, and the tail gets cut off with an ellipsis",
        };

        private UserManager _userManager;
        private UIManager _uiManager;
        private bool _isShowWelcome = false;
        protected override void Awake()
        {
            base.Awake();
            _btnShop?.onClick.AddListener(OnShopClicked);
            _btnTestFloat?.onClick.AddListener(OnTestFloatClicked);
            _btnTestMessage?.onClick.AddListener(OnTestMessageClicked);
            _btnAddMoney?.onClick.AddListener(OnAddMoneyClicked);
            _userManager ??= FindFirstObjectByType<UserManager>();
            SetAnimation(new TempSlideAnimation());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _btnShop?.onClick.RemoveListener(OnShopClicked);
            _btnTestFloat?.onClick.RemoveListener(OnTestFloatClicked);
            _btnTestMessage?.onClick.RemoveListener(OnTestMessageClicked);
            _btnAddMoney?.onClick.RemoveListener(OnAddMoneyClicked);
        }

        public override void OnEnter()
        {
            if (_userManager != null)
            {
                _userManager.OnBagChanged += RefreshPurchasedItems;
                _userManager.OnBagChanged += RefreshMoney;
            }

            // Manually register the flyout target — the library no longer auto-discovers the manager.
            _uiManager ??= FindFirstObjectByType<UIManager>();
            if (_moneyFlyoutTarget != null)
            {
                _moneyFlyoutTarget.Register(_uiManager);
                _moneyFlyoutTarget.OnIconArrived += OnCoinArrived;
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

            if (_moneyFlyoutTarget != null)
            {
                _moneyFlyoutTarget.OnIconArrived -= OnCoinArrived;
                _moneyFlyoutTarget.Unregister();
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

        // Float a random message from the Mgs button. Every click hands FloatingTextMessage a string of a
        // different length, which is the whole point: the prefab authors no width, the fitter decides it.
        private void OnTestMessageClicked()
        {
            if (_btnTestMessage == null) return;
            _uiManager ??= FindFirstObjectByType<UIManager>();

            var message = _testMessages[UnityEngine.Random.Range(0, _testMessages.Length)];
            _uiManager?.ShowFloatingText<FloatingTextMessage>(t => t.SetText(message), _btnTestMessage.transform);
        }

        private void OnAddMoneyClicked()
        {
            if (_btnAddMoney == null || _userManager == null) return;
            _uiManager ??= FindFirstObjectByType<UIManager>();

            const float addAmountUsd = 10f;
            const int addAmountCents = (int)(addAmountUsd * 100); // 1000 cents

            // No flyout available: just add money.
            if (_uiManager == null || _coinSprite == null)
            {
                _userManager.AddMoney(addAmountUsd);
                return;
            }

            // Count-per-coin: coins fly from the button's RectTransform and the number ticks up as
            // each one lands (see OnCoinArrived). PlayFlyoutFromRect resolves the source canvas
            // camera internally, so the spawn point is correct across any render mode.
            // AddMoney persists to the bag; by the time coins land the per-coin bumps already match
            // the new total, so RefreshMoney's SetTargetValue reconciles to a no-op.
            _uiManager.PlayFlyoutFromRect(
                source: (RectTransform)_btnAddMoney.transform,
                targetKey: "money",
                amount: addAmountCents,
                icon: _coinSprite,
                onComplete: () => _userManager.AddMoney(addAmountUsd)
            );
        }

        // Count-per-coin: nudge the displayed total by each coin's value as it lands,
        // so the number rises in lockstep with the coins.
        private void OnCoinArrived(int valuePerIcon)
        {
            if (_moneyFlyoutTarget == null) return;
            _moneyFlyoutTarget.SetTargetValue(_moneyFlyoutTarget.CurrentTargetValue + valuePerIcon);
        }

        private void RefreshMoney()
        {
            // App owns the number: drive the count-up whenever the bag changes.
            // The flyout icons are purely visual (fly + shake); they no longer touch the value.
            if (_userManager?.Bag == null) return;
            _moneyFlyoutTarget?.SetTargetValue((long)(_userManager.Bag.MoneyUsd * 100));
        }

        private void InitMoney()
        {
            if (_userManager?.Bag == null) return;

            // Abbreviated money display (e.g. 12600 -> "12.6k")
            if (_moneyFlyoutTarget != null)
                _moneyFlyoutTarget.Formatter = FormatMoney;

            // Initial sync - set both display and target
            _moneyFlyoutTarget?.SetValue((long)(_userManager.Bag.MoneyUsd * 100));
        }

        /// <summary>Short money format: 950 -> "950", 12600 -> "12.6k", 3_400_000 -> "3.4m".</summary>
        private static string FormatMoney(long value)
        {
            if (value < 0) return "-" + FormatMoney(-value);
            if (value < 1_000) return value.ToString();
            if (value < 1_000_000) return Abbrev(value, 1_000, 'k');
            if (value < 1_000_000_000) return Abbrev(value, 1_000_000, 'm');
            return Abbrev(value, 1_000_000_000, 'b');
        }

        // One decimal, trailing ".0" trimmed: 12600 -> "12.6k", 12000 -> "12k".
        private static string Abbrev(long value, long unit, char suffix)
        {
            var scaled = value / (double)unit;
            var text = scaled.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
            if (text.EndsWith(".0")) text = text[..^2];
            return text + suffix;
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

                    // Two same-type notices: keepSameTypeOnTop keeps "Welcome!" on top so it is read
                    // first, with "Info" waiting behind it (instead of the newer one covering it).
                    ui?.ShowPopup<PopupNotice>(p =>
                    {
                        p.SetData("Welcome!",
                            "Welcome to <b>K-pop Shop</b> sample XD\n" +
                            "This is a sample to help you better understand my UI Manager.");
                    }, keepSameTypeOnTop: true);

                    ui?.ShowPopup<PopupNotice>(p =>
                    {
                        p.SetData("Info",
                            "Including built-in utilities for your game UI,\n" +
                            "designed to stay simple and not overly complex.");
                    }, keepSameTypeOnTop: true);
                    FindAnyObjectByType<SoundManager>()?.PlayLoop("MainTheme", 0.3f);
                    _isShowWelcome = true;
                }

                onComplete?.Invoke();
            });
        }
    }
}
