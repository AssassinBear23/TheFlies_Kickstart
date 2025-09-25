using Core.Data;
using Minigame;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        /*[HideInInspector]*/ public InputManager inputManager;
        /*[HideInInspector]*/ public UIManager uiManager;

        [SerializeField] private GameObject minigamePrefab;
        [SerializeField] private Transform uiRoot;

        [Space(20), Header("Events"), Space(10)]
        /// <summary>
        /// Event that is called at the start of the game to setup all managers in correct order.
        /// </summary>
        [SerializeField] private UnityEvent managerSetupSequence;

        private void Awake()
        {
            managerSetupSequence?.Invoke();
        }

        /// <summary>
        /// Setup of the singleton instance.
        /// </summary>
        public void SetupInstance()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("Multiple instances of GameManager detected. Destroying duplicate.");
                Destroy(this);
            }
        }

        public void StartFishingMinigame(FishData fishData)
        {
            var instance = Instantiate(minigamePrefab, uiRoot);
            instance.GetComponent<FishingMinigame>().SetSetFishData(fishData);
        }

        public void HandleMinigameResult(string result, CaughtFish caughtFish = null, string reason = null)
        {
            if (result == "Won" && caughtFish != null)
            {
                Debug.Log($"Caught {caughtFish.FishName} ({caughtFish.Weight}kg, {caughtFish.Length}cm)");
                // TODO: add fish to FishInventory here
            }
            else
            {
                Debug.Log($"Lost: {reason}");
            }
        }
    }
}