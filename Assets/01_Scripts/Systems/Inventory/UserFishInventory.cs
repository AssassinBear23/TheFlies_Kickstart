using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.FishInventory
{
    using Core.Data;

    /// <summary>
    /// Manages the player's caught fish inventory.
    /// This static class provides centralized access to the player's caught fish.
    /// </summary>
    public class FishInventory : MonoBehaviour
    {
        private static readonly List<CaughtFish> caughtFishList = new();

        public static IReadOnlyList<CaughtFish> CaughtFish => caughtFishList;

        public static void AddFish(CaughtFish fish)
        {
            caughtFishList.Add(fish);
            // TODO: Trigger UI update, log event, etc.
        }

        public static void RemoveFish(CaughtFish fish)
        {
            caughtFishList.Remove(fish);
            // TODO: Handle selling/releasing
        }
    }
}