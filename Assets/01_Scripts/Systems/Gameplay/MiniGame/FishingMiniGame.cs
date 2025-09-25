using Core.Data;
using Core.Managers;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Minigame
{
    /// <summary>
    /// A fishing mini-game implementation that simulates catching a fish through player input.
    /// Players must tap/hold the screen to shrink a circle while managing line tension.
    /// The goal is to shrink the circle below a threshold without letting it grow too large
    /// or allowing the tension to reach maximum.
    /// </summary>
    public class FishingMinigame : MonoBehaviour
    {
        [Header("Difficulty Settings")]
        [Tooltip("The difficulty modifier based on fish stats")]
        [SerializeField] private float difficultyModifier = 1f;
        /// <summary>
        /// Represents the player's progress toward catching the fish.
        /// Positive values indicate progress toward catching, while negative values mean the fish is escaping.
        /// When this value reaches a certain threshold (likely 1.0), the fish is caught.
        /// If this value drops to -1.0 or below, the fish escapes and the player loses.
        /// </summary>
        [SerializeField] private float catchingProgress = 0.2f;
        [SerializeField, Range(0.01f, 0.7f)] private float catchingSpeed = .25f;
        [SerializeField, Range(0.01f, 0.7f)] private float escapingSpeed = .2f;
        /// <summary>
        /// Represents the current tension value in the system.
        /// </summary>
        /// <remarks>This value is used to track the current level of tension, which may influence other
        /// behaviors or calculations.</remarks>
        [SerializeField] private float currentTension;
        [SerializeField, Range(0.01f, 0.5f)] private float tensionIncreaseRate = 0.15f;
        [SerializeField, Range(0.01f, 0.5f)] private float tensionDecreaseRate = 0.1f;

        [Header("References")]
        [Tooltip("The Fishdata of the fish that the player is attempting to catch")]
        [SerializeField] private FishData referenceFish;
        [SerializeField] private CaughtFish fishInstance;
        [SerializeField] private Image circleRenderer;
        [SerializeField] private Image tensionFill;

        /// <summary>
        /// Tracks whether the player is currently holding down the input to reel in the fish.
        /// When true, the circle shrinks and tension increases. When false, the circle expands and tension decreases.
        /// </summary>
        private bool isHolding;

        #region Methods
        /// <summary>
        /// Sets the fish data for the current minigame instance.
        /// </summary>
        /// <param name="value">The <see cref="FishData"/> object representing the fish data to be set.</param>
        /// <exception cref="InvalidOperationException">Thrown if the fish data has already been set for this minigame instance.</exception>
        public void SetFishData(FishData value)
        {
            if (referenceFish == null)
            {
                referenceFish = value;
                GetInstanceAndModifier();
            }
            else throw new InvalidOperationException("Fish data can only be set once per minigame instance.");
        }

        private void Start()
        {
            GetInstanceAndModifier();
        }

        /// <summary>
        /// Initializes the fish instance and calculates the difficulty modifier.
        /// </summary>
        private void GetInstanceAndModifier()
        {
                fishInstance = new CaughtFish(referenceFish);
            difficultyModifier *= StatModifiers(referenceFish, fishInstance);            
        }

        /// <summary>
        /// Calculates a difficulty modifier based on the characteristics of the fish being caught.
        /// This modifier can affect various aspects of the fishing minigame such as shrink speed,
        /// tension increase rate, or circle size thresholds.
        /// </summary>
        /// <param name="fishData">The data of the fish that determines the difficulty modifier.</param>
        /// <returns>A float value representing the difficulty modifier. A higher value indicates increased difficulty.</returns>
        float StatModifiers(FishData fishData, CaughtFish fightingFish)
        {
            float normalizedWeight = (fightingFish.Weight - fishData.WeightRangeKg.x) / (fishData.WeightRangeKg.y - fishData.WeightRangeKg.x);
            float weightModifier = Mathf.Lerp(0.8f, 1.2f, normalizedWeight);
            float rarityModifier = // Rarity affects difficulty: Common (1.0), Uncommon (1.1), Rare (1.2), Epic (1.3), Legendary (1.5)
                fishData.Rarity switch
                {
                    FishRarity.Common => 1.0f,
                    FishRarity.Rare => 1.1f,
                    FishRarity.Epic => 1.2f,
                    _ => 1.0f
                };
            return weightModifier * rarityModifier;
        }


        /// <summary>
        /// Updates the fishing mini-game state each frame.
        /// Processes player input, updates the circle size, and checks lose conditions.
        /// This method runs every frame while the mini-game is active.
        /// </summary>
        private void Update()
        {
            if(fishInstance == null)
            {
                if(Input.anyKeyDown) GetInstanceAndModifier();
            }

            // If player is pressing -> shrink circle & build tension
            if (isHolding)
            {
                catchingProgress += catchingSpeed * Time.deltaTime;
                currentTension += tensionIncreaseRate * Time.deltaTime;
            }
            else
            {
                catchingProgress -= escapingSpeed * Time.deltaTime;
                currentTension -= tensionDecreaseRate * Time.deltaTime;
            }

            // Clamp tension between 0–1
            currentTension = Mathf.Clamp01(currentTension);

            // Update visuals
            UpdateCircleVisual();
            UpdateTensionUI();

            // Win condition
            if (catchingProgress >= 1f)
            {
                Win();
                return;
            }

            // Lose conditions
            CheckLoseCondition();
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateTensionUI()
        {
            tensionFill.fillAmount = currentTension;
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateCircleVisual()
        {
            circleRenderer.material.SetFloat("_Progress", 1 - catchingProgress);
        }


        /// <summary>
        /// Checks if any lose conditions for the fishing mini-game have been met.
        /// Monitors two failure states: excessive tension causing the rod to snap,
        /// or insufficient catching progress allowing the fish to escape.
        /// When either condition is met, the appropriate lose scenario is triggered.
        /// </summary>
        private void CheckLoseCondition()
        {
            if (currentTension >= 1)
            {
                Lose("Rod Snapped");
            }
            if (catchingProgress <= 0)
            {
                Lose("Fish Escaped");
            }
        }


        /// <summary>
        /// Handles the win condition when the player successfully catches the fish.
        /// Disables the mini-game and triggers appropriate visual feedback.
        /// </summary>
        void Win()
        {
            GameManager.Instance.HandleMinigameResult("Won", fishInstance);
            enabled = false;
            Destroy(gameObject);
        }

        /// <summary>
        /// Handles the lose condition when the player fails to catch the fish.
        /// </summary>
        /// <param name="failureReason">The reason for the failure (e.g., "Fish escaped!" or "Rod snapped!")</param>
        void Lose(string failureReason)
        {
            GameManager.Instance.HandleMinigameResult("Lost", reason: failureReason);
            enabled = false;
            Destroy(gameObject);
        }

        /// <summary>
        /// Sets the press state for the fishing mini-game, which controls whether the player is actively reeling in the fish.
        /// When true, the circle will shrink and tension will increase. When false, the circle will expand and tension will gradually decrease.
        /// </summary>
        /// <param name="state">True when the player is pressing/holding the input, false otherwise.</param>
        internal void SetPress(bool state)
        {
            isHolding = state;
        }
        #endregion Methods
    }
}