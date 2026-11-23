using System;

namespace vanhaodev.uimanager.samples.kpopshop
{
    /// <summary>
    /// Data model for a shop item.
    /// </summary>
    [Serializable]
    public class ItemModel
    {
        public string Name;
        public float PriceUsd;
        public string ImageUrl;
        public string Description;

        public ItemModel() { }

        public ItemModel(string name, float priceUsd, string imageUrl, string description)
        {
            Name = name;
            PriceUsd = priceUsd;
            ImageUrl = imageUrl;
            Description = description;
        }
    }
}
