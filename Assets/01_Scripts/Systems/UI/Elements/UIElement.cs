using UnityEngine;

namespace UI.Elements
{
    /// <summary>
    /// Base class for all UI elements in the game.
    /// Provides a standard interface for updating UI components.
    /// Derived classes should override the UpdateDisplay method to implement specific UI behavior.
    /// </summary>
    public class UIElement : MonoBehaviour
    {

        /// <summary>
        /// Updates the UI element.
        /// </summary>
        public void UpdateElement()
        {
            UpdateDisplay();
        }

        /// <summary>
        /// Virtual method to update the display of the UI element.
        /// </summary>
        public virtual void UpdateDisplay() { }
    }
}
