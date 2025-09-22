using UnityEngine;

namespace InventorySystem.Items.Equipables
{
    [CreateAssetMenu(fileName = "NewLure", menuName = "Inventory/Items/Equipable/Lure")]
    public class Lure : AbstractEquipable, Interfaces.ITypeModifier
    {
        [SerializeField] private float biteRateModifier = 1f;

        public void ApplyTypeModifier(FishType allowedType)
        {
            throw new System.NotImplementedException();
        }
    }
}