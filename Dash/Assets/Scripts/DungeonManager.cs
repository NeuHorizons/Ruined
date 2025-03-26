using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonManager : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsDungeonScene())
            gameObject.SetActive(false);
        else
        {
            gameObject.SetActive(true);
            InitializeDungeon();
        }
    }

    bool IsDungeonScene()
    {
        return SceneManager.GetActiveScene().name.Contains("Dungeon");
    }

    void InitializeDungeon()
    {
        // Set up floor generation, enemy spawners, etc.
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}