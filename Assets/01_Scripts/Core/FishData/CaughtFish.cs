using UnityEngine;

namespace Core.Data
{
    /// <summary>
    /// Represents a fish that has been caught by the player, with specific weight and length values.
    /// </summary>
    [System.Serializable]
    public class CaughtFish
    {
        /// <summary>
        /// The visual representation of the caught fish.
        /// </summary>
        public Sprite Sprite { get; private set; }

        /// <summary>
        /// The weight of the caught fish in kilograms.
        /// </summary>
        public float Weight { get; private set; }

        /// <summary>
        /// The length of the caught fish in centimeters.
        /// </summary>
        public float Length { get; private set; }

        /// <summary>
        /// The rarity classification of the caught fish.
        /// </summary>
        public FishRarity Rarity { get; private set; }

        /// <summary>
        /// The name of the fish species.
        /// </summary>
        public string FishName { get; private set; }

        /// <summary>
        /// The type of water the fish naturally inhabits (freshwater or saltwater).
        /// </summary>
        public FishType Type { get; private set; }

        /// <summary>
        /// Creates a new caught fish instance with randomly generated weight and length based on the fish data ranges.
        /// </summary>
        /// <param name="baseData">The base fish data containing information about the fish species.</param>
        public CaughtFish(FishData baseData)
        {
            FishName = baseData.FishName;
            Rarity = baseData.Rarity;
            Type = baseData.Type;
            Sprite = baseData.Sprite;

            // Roll values based on the SO's ranges
            Length = Random.Range(baseData.LengthRangeCm.x, baseData.LengthRangeCm.y);
            float normalLength = (Length - baseData.LengthRangeCm.x) / (baseData.LengthRangeCm.y - baseData.LengthRangeCm.x);
            Weight = Mathf.Lerp(baseData.WeightRangeKg.x, baseData.WeightRangeKg.y, normalLength);
        }
    }
}