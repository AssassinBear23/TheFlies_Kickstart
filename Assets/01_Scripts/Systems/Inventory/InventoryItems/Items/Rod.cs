using UnityEngine;

namespace InventorySystem.Items.Equipables
{
    [CreateAssetMenu(fileName = "NewRod", menuName = "Inventory/Items/Equipable/Rod")]
    public class Rod : AbstractEquipable, Interfaces.IFishStrenghtModifier
    {
        public void ApplyStrengthModifier(float strengthModifier)
        {
            throw new System.NotImplementedException();
        }
    }
}
