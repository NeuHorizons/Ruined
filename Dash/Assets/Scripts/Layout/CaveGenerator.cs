using UnityEngine;
using System.Collections.Generic;

public class CaveGenerator : MonoBehaviour
{
    public int gridWidth = 50;
    public int gridHeight = 50;
    public float cellSize = 1f;

    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject roomCenterPrefab;
    public GameObject playerPrefab;
   
    public Vector2Int spawnRoomPosition;
    public Vector2 dungeonOffset = Vector2.zero;
    public int fillPercentage = 45;
    public int smoothingIterations = 5;
    public int roomCount = 5;
    public int roomRadius = 3;
    public float roomEdgeNoise = 0.5f;
    private List<Vector2Int> roomCenters = new List<Vector2Int>(); 

    private int[,] grid;

    void Start()
    {
        GenerateRandomGrid();
        for (int i = 0; i < smoothingIterations; i++)
        {
            SmoothGrid();
        }
        CreateWiderRooms();
        ConnectDisconnectedCaves(); // Connect major cave regions first
        ConnectRoomsWithMST();      // Connect all room centers using MST with natural tunnels
        EnsureAllRoomsConnected();  // Ensure every room center is reachable via the cave network
        RenderGrid();
        SpawnRoomCenters();
        SpawnPlayer(playerPrefab);
    }

    void GenerateRandomGrid()
    {
        grid = new int[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Always keep the border as walls.
                if (x == 0 || y == 0 || x == gridWidth - 1 || y == gridHeight - 1)
                {
                    grid[x, y] = 1;
                }
                else
                {
                    grid[x, y] = Random.Range(0, 100) < fillPercentage ? 1 : 0;
                }
            }
        }
    }

    void SmoothGrid()
    {
        int[,] newGrid = new int[gridWidth, gridHeight];

        for (int x = 1; x < gridWidth - 1; x++)
        {
            for (int y = 1; y < gridHeight - 1; y++)
            {
                int wallCount = CountWallsAround(x, y);
                if (wallCount > 4)
                    newGrid[x, y] = 1;
                else if (wallCount < 4)
                    newGrid[x, y] = 0;
                else
                    newGrid[x, y] = grid[x, y];
            }
        }
        grid = newGrid;
    }

    int CountWallsAround(int gridX, int gridY)
    {
        int count = 0;
        for (int x = gridX - 1; x <= gridX + 1; x++)
        {
            for (int y = gridY - 1; y <= gridY + 1; y++)
            {
                if (x >= 0 && y >= 0 && x < gridWidth && y < gridHeight)
                {
                    if (grid[x, y] == 1)
                        count++;
                }
            }
        }
        return count;
    }

    void CreateWiderRooms()
    {
        roomCenters.Clear();

        // Step 1: Create the spawn room.
        spawnRoomPosition = new Vector2Int(Random.Range(5, gridWidth - 5), Random.Range(5, gridHeight - 5));
        roomCenters.Add(spawnRoomPosition);
        int spawnRoomSize = 2;

        for (int x = spawnRoomPosition.x - spawnRoomSize; x <= spawnRoomPosition.x + spawnRoomSize; x++)
        {
            for (int y = spawnRoomPosition.y - spawnRoomSize; y <= spawnRoomPosition.y + spawnRoomSize; y++)
            {
                if (x > 0 && y > 0 && x < gridWidth - 1 && y < gridHeight - 1)
                {
                    float distance = Vector2Int.Distance(new Vector2Int(x, y), spawnRoomPosition);
                    if (distance <= spawnRoomSize)
                    {
                        grid[x, y] = 0; // Clear a round area for the spawn room.
                    }
                }
            }
        }

        // Step 2: Generate additional rooms.
        for (int i = 0; i < roomCount; i++)
        {
            Vector2Int roomCenter;
            do
            {
                roomCenter = new Vector2Int(Random.Range(5, gridWidth - 5), Random.Range(5, gridHeight - 5));
            }
            while (Vector2Int.Distance(roomCenter, spawnRoomPosition) < (roomRadius * 3));

            roomCenters.Add(roomCenter);

            int maxRadius = Random.Range(roomRadius - 1, roomRadius + 2);

            for (int x = roomCenter.x - maxRadius; x <= roomCenter.x + maxRadius; x++)
            {
                for (int y = roomCenter.y - maxRadius; y <= roomCenter.y + maxRadius; y++)
                {
                    if (x > 0 && y > 0 && x < gridWidth - 1 && y < gridHeight - 1)
                    {
                        float distance = Vector2Int.Distance(new Vector2Int(x, y), roomCenter);
                        float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * roomEdgeNoise;
                        if (distance <= maxRadius + noise)
                        {
                            grid[x, y] = 0;
                        }
                    }
                }
            }
        }
    }

    void ConnectDisconnectedCaves()
    {
        List<List<Vector2Int>> caveRegions = GetCaveRegions();

        if (caveRegions.Count <= 1)
            return;

        List<Vector2Int> mainCave = caveRegions[0];

        for (int i = 1; i < caveRegions.Count; i++)
        {
            List<Vector2Int> otherCave = caveRegions[i];

            // Skip connecting the spawn room's cave region.
            if (otherCave.Contains(spawnRoomPosition))
            {
                Debug.Log("Skipping connection for spawn room.");
                continue;
            }

            Vector2Int bestMainPoint = Vector2Int.zero;
            Vector2Int bestOtherPoint = Vector2Int.zero;
            float bestDistance = float.MaxValue;

            foreach (Vector2Int mainPoint in mainCave)
            {
                foreach (Vector2Int otherPoint in otherCave)
                {
                    float dist = Vector2Int.Distance(mainPoint, otherPoint);
                    if (dist < bestDistance)
                    {
                        bestDistance = dist;
                        bestMainPoint = mainPoint;
                        bestOtherPoint = otherPoint;
                    }
                }
            }
            // Use a natural, curved tunnel to join these cave regions.
            DigCurvedTunnel(bestMainPoint, bestOtherPoint);
        }
    }

    // Connect all room centers using a minimum spanning tree (MST) approach with natural, curved tunnels.
    void ConnectRoomsWithMST()
    {
        if (roomCenters.Count < 2)
            return;

        List<Vector2Int> connectedRooms = new List<Vector2Int>();
        List<Vector2Int> remainingRooms = new List<Vector2Int>(roomCenters);

        // Start with the first room (e.g., spawn room)
        connectedRooms.Add(remainingRooms[0]);
        remainingRooms.RemoveAt(0);

        while (remainingRooms.Count > 0)
        {
            float bestDistance = float.MaxValue;
            Vector2Int bestConnected = Vector2Int.zero;
            Vector2Int bestRemaining = Vector2Int.zero;

            foreach (Vector2Int roomA in connectedRooms)
            {
                foreach (Vector2Int roomB in remainingRooms)
                {
                    float distance = Vector2Int.Distance(roomA, roomB);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestConnected = roomA;
                        bestRemaining = roomB;
                    }
                }
            }
            // Carve a natural, curved tunnel between the rooms.
            DigCurvedTunnel(bestConnected, bestRemaining);
            connectedRooms.Add(bestRemaining);
            remainingRooms.Remove(bestRemaining);
        }
    }

    void SpawnRoomCenters()
    {
        if (roomCenterPrefab == null)
            return;

        foreach (Vector2Int center in roomCenters)
        {
            // Skip the spawn room to prevent enemy spawners from appearing there.
            if (center == spawnRoomPosition)
            {
                Debug.Log("Skipping enemy spawner in spawn room.");
                continue;
            }

            Vector3 spawnPosition = new Vector3((center.x * cellSize) + dungeonOffset.x, (center.y * cellSize) + dungeonOffset.y, 0);
            Instantiate(roomCenterPrefab, spawnPosition, Quaternion.identity);
        }
    }

    List<List<Vector2Int>> GetCaveRegions()
    {
        List<List<Vector2Int>> caveRegions = new List<List<Vector2Int>>();
        bool[,] visited = new bool[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == 0 && !visited[x, y])
                {
                    List<Vector2Int> region = FloodFill(new Vector2Int(x, y), visited);
                    caveRegions.Add(region);
                }
            }
        }
        return caveRegions;
    }

    List<Vector2Int> FloodFill(Vector2Int start, bool[,] visited)
    {
        List<Vector2Int> region = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        while (queue.Count > 0)
        {
            Vector2Int cell = queue.Dequeue();
            region.Add(cell);

            foreach (Vector2Int neighbor in GetNeighbors(cell))
            {
                if (!visited[neighbor.x, neighbor.y] && grid[neighbor.x, neighbor.y] == 0)
                {
                    visited[neighbor.x, neighbor.y] = true;
                    queue.Enqueue(neighbor);
                }
            }
        }
        return region;
    }

    List<Vector2Int> GetNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = cell + dir;
            if (neighbor.x >= 0 && neighbor.y >= 0 && neighbor.x < gridWidth && neighbor.y < gridHeight)
            {
                neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }

    // Carve out a cell and its adjacent cells for wider tunnels,
    // while ensuring we don't affect the border.
    void CarveCell(int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int newX = x + dx;
                int newY = y + dy;
                if (newX > 0 && newX < gridWidth - 1 && newY > 0 && newY < gridHeight - 1)
                {
                    grid[newX, newY] = 0;
                }
            }
        }
    }

    // Creates a natural, curved tunnel between two points using Perlin noise.
    void DigCurvedTunnel(Vector2Int start, Vector2Int end)
    {
        Vector2 startPos = new Vector2(start.x, start.y);
        Vector2 endPos = new Vector2(end.x, end.y);
        float distance = Vector2.Distance(startPos, endPos);
        int steps = Mathf.CeilToInt(distance);

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            // Basic linear interpolation along the line.
            Vector2 pos = Vector2.Lerp(startPos, endPos, t);
            // Add noise for natural curvature.
            float noiseFactor = 2f; // Adjust to control the curvature intensity.
            float noiseX = (Mathf.PerlinNoise(t * 5f, 0f) - 0.5f) * noiseFactor;
            float noiseY = (Mathf.PerlinNoise(0f, t * 5f) - 0.5f) * noiseFactor;
            pos += new Vector2(noiseX, noiseY);
            int gridX = Mathf.RoundToInt(pos.x);
            int gridY = Mathf.RoundToInt(pos.y);
            CarveCell(gridX, gridY);
        }
    }

    // Checks connectivity by flood filling from the spawn room and connecting any isolated room centers.
    void EnsureAllRoomsConnected()
    {
        bool[,] visited = new bool[gridWidth, gridHeight];
        List<Vector2Int> reachable = FloodFill(spawnRoomPosition, visited);

        foreach (Vector2Int roomCenter in roomCenters)
        {
            if (!reachable.Contains(roomCenter))
            {
                Vector2Int nearest = roomCenter;
                float bestDistance = float.MaxValue;
                foreach (Vector2Int tile in reachable)
                {
                    float dist = Vector2Int.Distance(roomCenter, tile);
                    if (dist < bestDistance)
                    {
                        bestDistance = dist;
                        nearest = tile;
                    }
                }
                DigCurvedTunnel(roomCenter, nearest);
            }
        }
    }

    void RenderGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 position = new Vector3((x * cellSize) + dungeonOffset.x, (y * cellSize) + dungeonOffset.y, 0);
                // Always render the border as wall prefabs.
                if (x == 0 || y == 0 || x == gridWidth - 1 || y == gridHeight - 1)
                {
                    Instantiate(wallPrefab, position, Quaternion.identity, transform);
                }
                else
                {
                    GameObject prefabToSpawn = grid[x, y] == 1 ? wallPrefab : floorPrefab;
                    Instantiate(prefabToSpawn, position, Quaternion.identity, transform);
                }
            }
        }
    }

    void SpawnPlayer(GameObject playerPrefab)
    {
        if (playerPrefab == null)
            return;

        Vector3 spawnPosition = new Vector3(spawnRoomPosition.x * cellSize, spawnRoomPosition.y * cellSize, 0);
        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }
}
