using UnityEngine;

namespace InventorySystem.Items
{
    /// <summary>
    /// Base class for all equipable items in the inventory system.
    /// </summary>
    public abstract class AbstractInventoryItem : ScriptableObject
    {
        [SerializeField] protected Sprite itemIcon;
        [SerializeField] protected string itemName;

        public Sprite ItemIcon => itemIcon;
        public string ItemName => itemName;


    }
}
