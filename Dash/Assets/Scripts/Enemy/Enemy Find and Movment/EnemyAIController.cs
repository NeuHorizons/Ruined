using UnityEngine;
using Pathfinding;

public class EnemyAIController : MonoBehaviour
{
    public MonoBehaviour movementScript; 
    private AIPath aiPath;               
    private Transform player;
    public float attackRadius = 5f;      
    private bool usingPathfinding = false;
    
    
    private EnemyDetection enemyDetection;

    private void Start()
    {
        aiPath = GetComponent<AIPath>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        enemyDetection = GetComponent<EnemyDetection>();

        aiPath.enabled = false; 
        if (movementScript != null)
            movementScript.enabled = false; 
    }

    private void Update()
    {
        
        if (enemyDetection != null && !enemyDetection.isAlerted)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        
        if (distanceToPlayer > attackRadius && !usingPathfinding)
        {
            StartPathfinding();
        }
        
        else if (distanceToPlayer <= attackRadius && usingPathfinding)
        {
            StartCombatMovement();
        }

        if (usingPathfinding && aiPath != null)
        {
            
            aiPath.destination = player.position;
        }
    }

    private void StartPathfinding()
    {
        usingPathfinding = true;
        if (movementScript != null)
            movementScript.enabled = false; 

        if (aiPath != null)
            aiPath.enabled = true; 
    }

    private void StartCombatMovement()
    {
        usingPathfinding = false;
        if (aiPath != null)
            aiPath.enabled = false; 

        if (movementScript != null)
            movementScript.enabled = true; 
    }
}
