using System.Collections.Generic;
using UnityEngine;
using Vanhaodev.UIManager;

namespace vanhaodev.uimanager.samples.kpopshop
{
    /// <summary>
    /// Holds the list of items available in the shop and handles buy logic.
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        [SerializeField] private List<ItemModel> _items = new();

        private UserManager _userManager;
        private UIManager _uiManager;

        public IReadOnlyList<ItemModel> Items => _items;

        /// <summary>
        /// Attempts to buy an item. Shows a notice popup if the player cannot afford it.
        /// </summary>
        public void BuyItem(ItemModel item)
        {
            if (item == null) return;

            _userManager ??= FindObjectOfType<UserManager>();
            if (_userManager?.Bag == null) return;

            if (_userManager.Bag.MoneyUsd < item.PriceUsd)
            {
                ShowNotEnoughMoneyNotice(item);
                return;
            }

            _userManager.TryBuy(item);
        }

        private void ShowNotEnoughMoneyNotice(ItemModel item)
        {
            _uiManager ??= FindObjectOfType<UIManager>();
            _uiManager?.ShowPopup<PopupNotice>(p =>
            {
                p.SetData(
                    "Not enough money",
                    $"You need <b>${item.PriceUsd:0.00}</b> to buy <b>{item.Name}</b>."
                );
            });
        }

        private void Awake()
        {
            if (_items.Count == 0)
                InitDefaultItems();
        }

        private void InitDefaultItems()
        {
            // NewJeans albums
            _items.Add(new ItemModel(
                name: "NewJeans - New Jeans (1st EP)",
                priceUsd: 19.99f,
                imageUrl: "",
                description: "NewJeans debut EP featuring Attention, Hype Boy, Cookie, and Hurt."
            ));
            _items.Add(new ItemModel(
                name: "NewJeans - OMG",
                priceUsd: 17.99f,
                imageUrl: "",
                description: "Second single album with title tracks OMG and Ditto."
            ));
            _items.Add(new ItemModel(
                name: "NewJeans - Get Up",
                priceUsd: 21.99f,
                imageUrl: "",
                description: "Second EP including Super Shy, ETA, Cool With You, and New Jeans."
            ));

            // BabyMonster albums
            _items.Add(new ItemModel(
                name: "BabyMonster - BABYMONS7ER",
                priceUsd: 18.99f,
                imageUrl: "",
                description: "BabyMonster's 1st mini album debut release."
            ));
            _items.Add(new ItemModel(
                name: "BabyMonster - DRIP",
                priceUsd: 22.99f,
                imageUrl: "",
                description: "BabyMonster's 1st full album with title track DRIP."
            ));
        }

        public ItemModel GetItem(int index)
        {
            return index >= 0 && index < _items.Count ? _items[index] : null;
        }
    }
}
