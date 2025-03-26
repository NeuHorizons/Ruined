using UnityEngine;
using Pathfinding;
using System.Collections;

public class PathfinderInitializer : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(DelayedScan());
    }

    private IEnumerator DelayedScan()
    {
        yield return new WaitForSeconds(1f); // ✅ Waits 1 second before scanning
        AstarPath.active.Scan();
        Debug.Log("✅ A* Pathfinding Grid Scanned after 1 second!");
    }
}