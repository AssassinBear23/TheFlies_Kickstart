using System.Collections.Generic;
using UnityEngine;

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
    public virtual void UpdateDisplay()
    {

    }
}
