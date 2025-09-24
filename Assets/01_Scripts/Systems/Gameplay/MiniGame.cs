using UnityEngine;
using UnityEngine.UI;

public class FishingMinigame : MonoBehaviour
{
    [Header("Circle Settings")]
    [SerializeField] private RectTransform circle;
    [SerializeField] private float shrinkSpeed = 50f;
    [SerializeField] private float minSizeThreshold = 40f; // win condition
    [SerializeField] private float maxSizeThreshold = 200f; // lose condition

    [Header("Tension Settings (optional)")]
    [SerializeField] private Slider tensionBar;
    [SerializeField] private float tensionIncreaseRate = 0.2f;
    [SerializeField] private float tensionDecreaseRate = 0.1f;
    [SerializeField] private float maxTension = 1f;

    private bool isHolding;
    private float tension;

    void Update()
    {
        // Input (tap/hold)
        isHolding = Input.GetMouseButton(0); // tap/hold on screen

        // Circle resize
        float delta = (isHolding ? -shrinkSpeed : shrinkSpeed * 0.5f) * Time.deltaTime;
        circle.sizeDelta += new Vector2(delta, delta);

        // Clamp size
        float size = Mathf.Clamp(circle.sizeDelta.x, minSizeThreshold * 0.5f, maxSizeThreshold * 1.2f);
        circle.sizeDelta = new Vector2(size, size);

        // Win condition
        if (circle.sizeDelta.x <= minSizeThreshold)
        {
            Win();
            return;
        }

        // Lose condition: circle too big
        if (circle.sizeDelta.x >= maxSizeThreshold)
        {
            Lose("Fish escaped!");
            return;
        }

        // Optional: Tension system
        if (tensionBar != null)
        {
            if (isHolding)
                tension += tensionIncreaseRate * Time.deltaTime;
            else
                tension -= tensionDecreaseRate * Time.deltaTime * 0.7f;

            tension = Mathf.Clamp(tension, 0, maxTension);
            tensionBar.value = tension;

            if (tension >= maxTension)
            {
                Lose("Rod snapped!");
                return;
            }
        }
    }

    void Win()
    {
        Debug.Log("Fish caught!");
        // Trigger animation, send caughtFish to inventory, show UI
        enabled = false;
    }

    void Lose(string reason)
    {
        Debug.Log("Lost: " + reason);
        // Trigger escape animation, reset minigame
        enabled = false;
    }
}
