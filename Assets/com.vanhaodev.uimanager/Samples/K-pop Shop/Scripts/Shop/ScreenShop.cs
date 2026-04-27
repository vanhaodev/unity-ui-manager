using System;
using TMPro;
using UnityEngine;
using vanhaodev.uimanager.effect.templates;

namespace vanhaodev.uimanager.samples.kpopshop
{
    public class ScreenShop : BaseScreen
    {
        [Header("Item Spawning")]
        [SerializeField] private Transform _itemContainer;
        [SerializeField] private ItemUI _itemPrefab;

        [Header("Money")]
        [SerializeField] private TMP_Text _txtMoney;

        private ShopManager _shopManager;
        private UserManager _userManager;
        private bool _itemsSpawned;

        protected override void Awake()
        {
            base.Awake();
            _shopManager = GetComponent<ShopManager>();
            SetAnimation(new TempSlideAnimation());
        }

        public override void Show(Action onComplete = null)
        {
            base.Show(onComplete);
            Debug.Log("[ScreenShop] Entered");

            _userManager ??= FindFirstObjectByType<UserManager>();
            if (_userManager != null)
                _userManager.OnBagChanged += RefreshMoney;

            if (!_itemsSpawned)
            {
                _itemsSpawned = true;
                SpawnItems();
            }

            RefreshMoney();
        }

        public override void OnExit()
        {
            Debug.Log("[ScreenShop] Exited");
            if (_userManager != null)
                _userManager.OnBagChanged -= RefreshMoney;
        }

        protected override void OnCloseClicked()
        {
            Manager?.ShowScreen<ScreenHome>();
        }

        private void SpawnItems()
        {
            if (_shopManager == null || _itemPrefab == null || _itemContainer == null) return;

            foreach (var item in _shopManager.Items)
            {
                var ui = Instantiate(_itemPrefab, _itemContainer);
                ui.gameObject.SetActive(true);
                ui.SetData(item);
            }
        }

        private void RefreshMoney()
        {
            if (_txtMoney == null || _userManager?.Bag == null) return;
            _txtMoney.text = $"<sprite=22> {_userManager.Bag.MoneyUsd:0.00}";
        }
    }
}
