using System;
using UnityEngine;

namespace vanhaodev.uimanager.samples.kpopshop
{
    /// <summary>
    /// Holds the player's bag data (money and purchased items).
    /// </summary>
    public class UserManager : MonoBehaviour
    {
        [SerializeField] private float _initialMoney = 100f;

        public BagModel Bag { get; private set; }
        public event Action OnBagChanged;

        private void Awake()
        {
            Bag = new BagModel(_initialMoney);
        }

        /// <summary>
        /// Attempts to buy an item. Fires OnBagChanged if the purchase succeeds.
        /// </summary>
        public bool TryBuy(ItemModel item)
        {
            if (Bag == null || !Bag.TryBuy(item)) return false;

            OnBagChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Add money to the bag. Fires OnBagChanged.
        /// </summary>
        public void AddMoney(float amount)
        {
            if (Bag == null || amount <= 0) return;
            Bag.MoneyUsd += amount;
            OnBagChanged?.Invoke();
        }
    }
}
