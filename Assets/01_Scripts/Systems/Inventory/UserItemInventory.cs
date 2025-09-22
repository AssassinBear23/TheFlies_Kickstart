using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.ItemInventory
{
    using Items.Consumables;
    using Items.Equipables;
    using Items;

    /// <summary>
    /// Manages the player's inventory, equipped items, and active consumables.
    /// This static class provides centralized access to the player's items and equipment state.
    /// </summary>
    public class UserItemInventory : MonoBehaviour
    {
        private static readonly List<AbstractInventoryItem> currentInventory = new();

        /// <summary>
        /// A read-only list of the items currently in the user's inventory.
        /// </summary>
        public static IReadOnlyList<AbstractInventoryItem> Inventory => currentInventory;

        /// <summary>
        /// The currently equipped fishing rod. Null if no rod is equipped.
        /// </summary>
        public static Rod EquippedRod { get; private set; }

        /// <summary>
        /// The currently equipped fishing lure. Null if no lure is equipped.
        /// </summary>
        public static Lure EquippedLure { get; private set; }

        /// <summary>
        /// The currently equipped fishing hook. Null if no hook is equipped.
        /// </summary>
        public static Hook EquippedHook { get; private set; }

        /// <summary>
        /// The currently active bait. Null if no bait is active.
        /// </summary>
        public static Bait ActiveBait { get; private set; }

        /// <summary>
        /// Adds an item to the player's inventory if it's not already present.
        /// </summary>
        /// <param name="item">The item to add to the inventory.</param>
        public static void AddItem(AbstractInventoryItem item)
        {
            if (!currentInventory.Contains(item))
                currentInventory.Add(item);
        }

        /// <summary>
        /// Removes an item from the player's inventory if it exists.
        /// </summary>
        /// <param name="item">The item to remove from the inventory.</param>
        public static void RemoveItem(AbstractInventoryItem item)
        {
            if (currentInventory.Contains(item))
                currentInventory.Remove(item);
        }

        /// <summary>
        /// Equips a fishing rod if the rod is in the player's inventory.
        /// </summary>
        /// <param name="rod">The fishing rod to equip.</param>
        /// <remarks>
        /// This method will replace any currently equipped rod with the new one.
        /// If the rod is not in the player's inventory, no action is taken.
        /// </remarks>
        public static void EquipRod(Rod rod)
        {
            if (!currentInventory.Contains(rod)) return;
            EquippedRod = rod;
            // Switch the visual representation of the rod in the inventory UI
        }

        /// <summary>
        /// Equips a fishing lure if the lure is in the player's inventory.
        /// </summary>
        /// <param name="lure">The fishing lure to equip.</param>
        /// <remarks>
        /// This method will replace any currently equipped lure with the new one.
        /// If the lure is not in the player's inventory, no action is taken.
        /// </remarks>
        public static void EquipLure(Lure lure)
        {
            if (!currentInventory.Contains(lure)) return;
            EquippedLure = lure;
            // Switch the visual representation of the lure in the inventory UI
        }

        /// <summary>
        /// Equips a fishing hook if the hook is in the player's inventory.
        /// </summary>
        /// <param name="hook">The fishing hook to equip.</param>
        /// <remarks>
        /// This method will replace any currently equipped hook with the new one.
        /// If the hook is not in the player's inventory, no action is taken.
        /// </remarks>
        public static void EquipHook(Hook hook)
        {
            if (!currentInventory.Contains(hook)) return;
            EquippedHook = hook;
            // Switch the visual representation of the hook in the inventory UI
        }

        /// <summary>
        /// Sets the active bait if no bait is currently active and the bait is in the inventory.
        /// </summary>
        /// <param name="bait">The bait to set as active.</param>
        /// <remarks>
        /// This method will only activate the bait if:
        /// 1. There is no currently active bait (ActiveBait is null)
        /// 2. The bait item exists in the player's inventory
        /// If either condition fails, no action is taken.
        /// </remarks>
        public static void SetActiveBait(Bait bait)
        {
            if (ActiveBait != null || !currentInventory.Contains(bait)) return; // Indicate that a bait is already active
            ActiveBait = bait;
            // Show the active bait in the inventory UI
        }
    }
}
