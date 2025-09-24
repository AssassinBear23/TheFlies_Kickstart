using UnityEngine;
using UnityEngine.Events;

namespace Core.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [HideInInspector] public InputManager inputManager;
        [HideInInspector] public UIManager uiManager;

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
    }
}