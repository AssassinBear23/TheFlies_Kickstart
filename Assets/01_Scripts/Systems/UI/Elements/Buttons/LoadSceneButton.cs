using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Elements.Buttons
{
    /// <summary>
    /// Represents a button that loads a specified scene when triggered.
    /// </summary>
    /// <remarks>This class extends the <see cref="Button"/> class to provide functionality for loading
    /// scenes. The scene name can be specified, or the default scene "MainMenu" will be loaded if no name is
    /// provided.</remarks>
    public class LoadSceneButton : Button
    {
        /// <summary>
        /// Loads the scene with the given name.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load. Default value is "MainMenu".</param>
        public void LoadSceneByName(string sceneName = "MainMenu")
        {
            {
                try
                {
                    Time.timeScale = 1;
                    SceneManager.LoadScene(sceneName);
                }
                catch
                {
                    Debug.LogError("Scene not found. Please check the scene name.");
                }
            }
        }
    }
}