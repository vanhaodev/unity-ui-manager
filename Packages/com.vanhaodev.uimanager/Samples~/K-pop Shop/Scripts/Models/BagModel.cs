using System;
using System.Collections.Generic;

namespace vanhaodev.uimanager.samples.kpopshop
{
    /// <summary>
    /// Data model for player's bag. Holds purchased items and current balance.
    /// </summary>
    [Serializable]
    public class BagModel
    {
        public List<ItemModel> PurchasedItems = new();
        public float MoneyUsd;

        public BagModel() { }

        public BagModel(float initialMoney)
        {
            MoneyUsd = initialMoney;
        }

        /// <summary>
        /// Attempts to buy an item. Returns true if purchase succeeds.
        /// </summary>
        public bool TryBuy(ItemModel item)
        {
            if (item == null || MoneyUsd < item.PriceUsd) return false;

            MoneyUsd -= item.PriceUsd;
            PurchasedItems.Add(item);
            return true;
        }

        public bool HasItem(ItemModel item)
        {
            return item != null && PurchasedItems.Contains(item);
        }
    }
}
