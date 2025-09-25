using UnityEngine;

namespace Minigame.UI
{
    /// <summary>
    /// A button that behaves erratically, moving to a random position within its parent container when hovered over.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ErraticButton : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 50f;
        [SerializeField] private float moveRange = 200f;
        private RectTransform rect;
        private float offset;

        private void Awake() => rect = GetComponent<RectTransform>();

        private void Update()
        {
            offset += Time.deltaTime * moveSpeed;
            float x = Mathf.Sin(offset) * moveRange;
            rect.anchoredPosition = new Vector2(x, rect.anchoredPosition.y);
        }
    }
}