using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartManager : MonoBehaviour
{
    private bool gameStarted = false;
    void Start()
    {
        Time.timeScale = 0f; // Pause any background gameplay
    }

    public void LoadGameScene()
    {
        gameStarted = true;
        Time.timeScale = 1f; // Resume time (just in case)
        SceneManager.LoadScene("SampleScene");
    }
}
