using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minigame.UI
{
    /// <summary>
    /// A button that can be pressed and released, triggering events on pointer enter, pointer up, and pointer exit.
    /// </summary>
    public class PressurePlateButton : MonoBehaviour, IPointerEnterHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private FishingMinigame currentGameInstance;

        void Start()
        {
            Image img = GetComponent<Image>();
            img.alphaHitTestMinimumThreshold = 0.5f; // Only pixels with alpha > 0.5 count as hit
        }

        public void OnPointerEnter(PointerEventData eventData) => currentGameInstance.SetPress(true);

        public void OnPointerUp(PointerEventData eventData) => currentGameInstance.SetPress(false);

        public void OnPointerExit(PointerEventData eventData) => currentGameInstance.SetPress(false);

        private void OnValidate()
        {
            if (currentGameInstance == null)
            {
                Debug.LogWarning("CurrentGameInstance is not assigned in PressurePlateButton.", this);
            }
        }
    }
}