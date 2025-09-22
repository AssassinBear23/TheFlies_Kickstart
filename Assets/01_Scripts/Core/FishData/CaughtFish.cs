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
        /// Reference to the base fish data scriptable object containing species information.
        /// </summary>
        public FishData BaseData { get; private set; }

        /// <summary>
        /// The weight of the caught fish in kilograms.
        /// </summary>
        public float Weight { get; private set; }

        /// <summary>
        /// The length of the caught fish in centimeters.
        /// </summary>
        public float Length { get; private set; }

        /// <summary>
        /// Creates a new caught fish instance with randomly generated weight and length based on the fish data ranges.
        /// </summary>
        /// <param name="baseData">The base fish data containing information about the fish species.</param>
        public CaughtFish(FishData baseData)
        {
            BaseData = baseData;

            // Roll values based on the SO's ranges
            Weight = Random.Range(baseData.WeightRangeKg.x, baseData.WeightRangeKg.y);
            Length = Random.Range(baseData.LengthRangeCm.x, baseData.LengthRangeCm.y);
        }
    }
}