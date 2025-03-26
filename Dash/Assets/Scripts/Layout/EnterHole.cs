using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterHole2D : MonoBehaviour
{
    // Name of the scene to load (make sure it's added to Build Settings)
    public string sceneToLoad;

    // Flag to check if the player is in range
    private bool playerInRange = false;

    // Called when another 2D collider enters the trigger area
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player entered trigger (2D).");
            // Optional: Display a UI prompt, e.g., "Press E to enter"
        }
    }

    // Called when another 2D collider exits the trigger area
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player left trigger (2D).");
            // Optional: Hide the UI prompt
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // Check if the player is in range and if the "E" key was pressed
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E key pressed. Loading scene: " + sceneToLoad);
            EnemyDetection.ResetHiveMind();
            SceneManager.LoadScene(sceneToLoad);
            
        }
    }
}