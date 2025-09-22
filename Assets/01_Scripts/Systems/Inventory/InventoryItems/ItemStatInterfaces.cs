using System.Collections.Generic;


namespace InventorySystem.Items.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFishStrenghtModifier
    {
        void ApplyStrengthModifier(float strengthModifier);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ITensionModifier
    {
        void ApplyTensionModifier(float tensionModifier);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface ITypeModifier
    {
        void ApplyTypeModifier(FishType allowedType);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IRarityModifier
    {
        void ApplyRarityModifier(Dictionary<FishRarity, float> rarityWeights);
    }
}
