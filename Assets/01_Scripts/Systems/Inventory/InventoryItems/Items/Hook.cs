using UnityEngine;


namespace InventorySystem.Items.Equipables
{
    using Interfaces;

    [CreateAssetMenu(fileName = "NewHook", menuName = "Inventory/Items/Equipable/Hook")]
    public class Hook : AbstractEquipable, ITensionModifier
    {
        [SerializeField] private float tensionRateModifier = 1f;

        public void ApplyTensionModifier(float tensionModifier)
        {
            throw new System.NotImplementedException();
        }
    }
}