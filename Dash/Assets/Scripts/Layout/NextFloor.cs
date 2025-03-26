using UnityEngine;
using UnityEngine.SceneManagement;

public class NextFloor : MonoBehaviour
{
    public string sceneToLoad; // Scene to reload
    private bool playerInRange = false;
    public FloorManager floorManager; // Reference to FloorManager

    // In Awake, automatically look for a GameObject named "FloorManager" if none is assigned.
    private void Awake()
    {
        if (floorManager == null)
        {
            GameObject fm = GameObject.Find("FloorManager");
            if (fm != null)
            {
                floorManager = fm.GetComponent<FloorManager>();
                Debug.Log("FloorManager found and assigned.");
            }
            else
            {
                Debug.LogWarning("FloorManager not found in the scene.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player entered trigger (2D).");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player left trigger (2D).");
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E key pressed. Advancing floor and reloading scene.");
            EnemyDetection.ResetHiveMind();
            if (floorManager != null)
            {
                floorManager.EnterNextFloor();
            }
            else
            {
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
}