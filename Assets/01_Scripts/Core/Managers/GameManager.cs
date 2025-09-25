using Core.Data;
using Minigame;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        /*[HideInInspector]*/
        public InputManager inputManager;
        /*[HideInInspector]*/
        public UIManager uiManager;

        [SerializeField] private GameObject minigamePrefab;
        [SerializeField] private Transform uiRoot;

        [Header("Data")]
        [SerializeField] private CountryData countryData;

        [Space(20), Header("Events"), Space(10)]
        /// <summary>
        /// Event that is called at the start of the game to setup all managers in correct order.
        /// </summary>
        [SerializeField] private UnityEvent managerSetupSequence;
        [SerializeField] private UnityEvent miniGameWinEvent;
        [SerializeField] private UnityEvent miniGameLoseEvent;

        public CaughtFish LastCaughtFish { get; private set; }

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

        /// <summary>
        /// Starts a fishing minigame with a specific fish.
        /// </summary>
        /// <param name="fishData"></param>
        public void StartFishingMinigame(FishData fishData)
        {
            var instance = Instantiate(minigamePrefab, uiRoot);
            instance.GetComponent<FishingMinigame>().SetFishData(fishData);
        }

        /// <summary>
        /// Starts a fishing minigame with a random fish.
        /// </summary>
        public void StartFishingMinigameRandom()
        {
            var instance = Instantiate(minigamePrefab, uiRoot);
            int roll = Random.Range(0, countryData.AvailableFish.Count);
            instance.GetComponent<FishingMinigame>().SetFishData(countryData.AvailableFish[roll]);
        }

        /// <summary>
        /// Handles the result of a minigame, invoking appropriate events based on win/loss.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="caughtFish"></param>
        /// <param name="reason"></param>
        public void HandleMinigameResult(string result, CaughtFish caughtFish = null, string reason = null)
        {
            if (result == "Won" && caughtFish != null)
            {
                LastCaughtFish = caughtFish;
                Debug.Log($"Caught {caughtFish.FishName} ({caughtFish.Weight}kg, {caughtFish.Length}cm)");
                miniGameWinEvent?.Invoke();
            }
            else
            {
                miniGameLoseEvent?.Invoke();
                Debug.Log($"Lost: {reason}");
            }
        }
    }
}