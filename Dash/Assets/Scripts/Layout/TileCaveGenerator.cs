using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TileCaveGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    [Tooltip("If true, the dungeon will generate automatically on Start(). Disable this if using an external generator.")]
    public bool autoGenerateOnStart = false;

    public int gridWidth = 50;
    public int gridHeight = 50;
    public float cellSize = 1f;

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

    // Tilemap and RuleTile references.
    [Header("Tilemap Settings")]
    public Tilemap caveTilemap;           // Tilemap for walls and floors.
    public Tilemap roomCenterTilemap;     // Separate Tilemap for room center tiles.
    public RuleTile floorTile;
    public RuleTile wallTile;
    public RuleTile roomCenterTile;

    // Spawner for room centers.
    [Header("Spawner Settings")]
    public GameObject roomCenterSpawnerPrefab; // The spawner prefab to be placed at room centers.

    void Start()
    {
        if(autoGenerateOnStart)
        {
            GenerateDungeon();
        }
    }

    /// <summary>
    /// Public method to generate the dungeon.
    /// This method encapsulates the full dungeon-generation process.
    /// </summary>
    public void GenerateDungeon()
    {
        GenerateRandomGrid();
        for (int i = 0; i < smoothingIterations; i++)
        {
            SmoothGrid();
        }
        CreateWiderRooms();
        ConnectDisconnectedCaves(); // Connect major cave regions first.
        ConnectRoomsWithMST();      // Connect all room centers using MST with natural tunnels.
        EnsureAllRoomsConnected();  // Ensure every room center is reachable via the cave network.
        RenderGrid();
        RenderRoomCenters();
        SpawnPlayer(playerPrefab);
    }

    void GenerateRandomGrid()
    {
        grid = new int[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Keep the border as walls.
                if (x == 0 || y == 0 || x == gridWidth - 1 || y == gridHeight - 1)
                    grid[x, y] = 1;
                else
                    grid[x, y] = Random.Range(0, 100) < fillPercentage ? 1 : 0;
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
                    if (grid[x, y] == 1)
                        count++;
            }
        }
        return count;
    }

    void CreateWiderRooms()
    {
        roomCenters.Clear();

        // Create the spawn room.
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
                        grid[x, y] = 0;
                }
            }
        }

        // Generate additional rooms.
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
                            grid[x, y] = 0;
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
            DigCurvedTunnel(bestMainPoint, bestOtherPoint);
        }
    }

    // Connect all room centers using a minimum spanning tree (MST) approach.
    void ConnectRoomsWithMST()
    {
        if (roomCenters.Count < 2)
            return;

        List<Vector2Int> connectedRooms = new List<Vector2Int>();
        List<Vector2Int> remainingRooms = new List<Vector2Int>(roomCenters);

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
            DigCurvedTunnel(bestConnected, bestRemaining);
            connectedRooms.Add(bestRemaining);
            remainingRooms.Remove(bestRemaining);
        }
    }

    // Render the cave grid using the Tilemap.
    void RenderGrid()
    {
        if (caveTilemap == null)
        {
            Debug.LogError("Cave Tilemap not assigned!");
            return;
        }
        caveTilemap.ClearAllTiles();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x + Mathf.RoundToInt(dungeonOffset.x), y + Mathf.RoundToInt(dungeonOffset.y), 0);
                // Render the border as walls.
                if (x == 0 || y == 0 || x == gridWidth - 1 || y == gridHeight - 1)
                    caveTilemap.SetTile(pos, wallTile);
                else
                    caveTilemap.SetTile(pos, grid[x, y] == 1 ? wallTile : floorTile);
            }
        }
    }

    // Render room centers: place a tile and, if applicable, spawn a spawner at that position.
    // The player's spawn room will not get a spawner.
    void RenderRoomCenters()
    {
        if (roomCenterTilemap == null)
        {
            Debug.LogWarning("Room Center Tilemap not assigned. Skipping room centers rendering.");
            return;
        }
        roomCenterTilemap.ClearAllTiles();
        foreach (Vector2Int center in roomCenters)
        {
            Vector3Int tilePos = new Vector3Int(center.x + Mathf.RoundToInt(dungeonOffset.x),
                                                 center.y + Mathf.RoundToInt(dungeonOffset.y), 0);
            // Place the room center tile.
            roomCenterTilemap.SetTile(tilePos, roomCenterTile);

            // If a spawner prefab is assigned and this is not the spawn room, instantiate it.
            if (roomCenterSpawnerPrefab != null && center != spawnRoomPosition)
            {
                Vector3 spawnPos = new Vector3(center.x * cellSize + dungeonOffset.x,
                                               center.y * cellSize + dungeonOffset.y, 0);
                Instantiate(roomCenterSpawnerPrefab, spawnPos, Quaternion.identity);
            }
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
                neighbors.Add(neighbor);
        }
        return neighbors;
    }

    // Carve out cells (with adjacent cells) for wider tunnels.
    void CarveCell(int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int newX = x + dx;
                int newY = y + dy;
                if (newX > 0 && newX < gridWidth - 1 && newY > 0 && newY < gridHeight - 1)
                    grid[newX, newY] = 0;
            }
        }
    }

    // Dig a curved tunnel using Perlin noise for natural curvature.
    void DigCurvedTunnel(Vector2Int start, Vector2Int end)
    {
        Vector2 startPos = new Vector2(start.x, start.y);
        Vector2 endPos = new Vector2(end.x, end.y);
        float distance = Vector2.Distance(startPos, endPos);
        int steps = Mathf.CeilToInt(distance);
        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector2 pos = Vector2.Lerp(startPos, endPos, t);
            float noiseFactor = 2f;
            float noiseX = (Mathf.PerlinNoise(t * 5f, 0f) - 0.5f) * noiseFactor;
            float noiseY = (Mathf.PerlinNoise(0f, t * 5f) - 0.5f) * noiseFactor;
            pos += new Vector2(noiseX, noiseY);
            int gridX = Mathf.RoundToInt(pos.x);
            int gridY = Mathf.RoundToInt(pos.y);
            CarveCell(gridX, gridY);
        }
    }

    // Ensure every room center is connected to the spawn room.
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

    // Spawn the player as a GameObject.
    void SpawnPlayer(GameObject playerPrefab)
    {
        if (playerPrefab == null)
            return;
        Vector3 spawnPosition = new Vector3(spawnRoomPosition.x * cellSize, spawnRoomPosition.y * cellSize, 0);
        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }
}
