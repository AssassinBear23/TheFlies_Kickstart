using Core.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndResultPage : MonoBehaviour
{
    [SerializeField] private Image fishImage;
    [SerializeField] private TMP_Text fishNameText;
    [SerializeField] private TMP_Text WeightText;
    [SerializeField] private TMP_Text LengthText;
    [SerializeField] private TMP_Text PriceText;

    public void SetTextCorrect(CaughtFish fish)
    {
        fishImage.sprite = fish.Sprite;
        fishNameText.text = fish.FishName;
        WeightText.text = $"Weight: {fish.Weight:F2}Kg";
        LengthText.text = $"Length: {fish.Length:F0}Cm";
        //PriceText.text = CalculateFishPrice();
    }
}
