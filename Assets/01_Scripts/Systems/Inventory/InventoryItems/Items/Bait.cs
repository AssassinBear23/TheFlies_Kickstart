using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.Items.Consumables
{
    [CreateAssetMenu(fileName = "NewBait", menuName = "Inventory/Items/Consumable/Bait")]
    public class Bait : AbstractConsumable, Interfaces.IRarityModifier
    {
        public void ApplyRarityModifier(Dictionary<FishRarity, float> rarityWeights)
        {
            throw new System.NotImplementedException();
        }
    }
}