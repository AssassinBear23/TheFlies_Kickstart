using UnityEngine;
using UnityEngine.SceneManagement;


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
