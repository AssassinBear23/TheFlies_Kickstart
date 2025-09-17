using System.Collections.Generic;
using UnityEngine;

namespace Core.Data
{
    /// <summary>
    /// Data class for information regarding the fish found in a country.
    /// </summary>
    [CreateAssetMenu(fileName = "_CountryData", menuName = "Data/Fish/CountryData")]
    public class CountryData : ScriptableObject
    {
        //[SerializeField] private string countryName;
        [SerializeField] private Sprite countryIcon;
        [SerializeField] private List<FishData> availableFish;
    }
}