using UnityEngine;
using System.Collections.Generic;

public class ProceduralMapGenerator : MonoBehaviour
{
    [Header("Room Configuration")]
    public int minRooms = 3;
    public int maxRooms = 6;
    public Vector2 minRoomSize = new Vector2(10, 10);
    public Vector2 maxRoomSize = new Vector2(20, 20);
    public float roomSpacing = 5f; // space between rooms

    [Header("Wall Prefab")]
    public GameObject wallPrefab; // your wall sprite
    public float wallThickness = 1f;

    [Header("Spawn Points")]
    public int spawnPointsPerRoom = 3;
    public GameObject spawnPointPrefab; // optional visual marker

    [Header("Player Spawn")]
    public GameObject playerPrefab; // lily or cosmos
    public GameObject secondPlayerPrefab; // the other character

    private List<Room> rooms = new List<Room>();
    private List<Vector3> spawnPoints = new List<Vector3>();

    [System.Serializable]
    public class Room
    {
        public Vector2 position;
        public Vector2 size;
        public Rect bounds;

        public Room(Vector2 pos, Vector2 sz)
        {
            position = pos;
            size = sz;
            bounds = new Rect(pos.x - sz.x / 2, pos.y - sz.y / 2, sz.x, sz.y);
        }
    }

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        ClearMap();
        GenerateRooms();
        GenerateWalls();
        GenerateSpawnPoints();
        SpawnPlayers();
    }

    void ClearMap()
    {
        rooms.Clear();
        spawnPoints.Clear();

        // destroy old walls and spawn markers
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    void GenerateRooms()
    {
        int roomCount = Random.Range(minRooms, maxRooms + 1);

        for (int i = 0; i < roomCount; i++)
        {
            Vector2 roomSize = new Vector2(
                Random.Range(minRoomSize.x, maxRoomSize.x),
                Random.Range(minRoomSize.y, maxRoomSize.y)
            );

            Vector2 roomPos = Vector2.zero;

            if (i == 0)
            {
                // first room at origin
                roomPos = Vector2.zero;
            }
            else
            {
                // place next room adjacent to existing rooms
                Room previousRoom = rooms[Random.Range(0, rooms.Count)];

                // pick a random side of the previous room
                int side = Random.Range(0, 4);
                switch (side)
                {
                    case 0: // right
                        roomPos = previousRoom.position + new Vector2(previousRoom.size.x / 2 + roomSize.x / 2 + roomSpacing, 0);
                        break;
                    case 1: // left
                        roomPos = previousRoom.position + new Vector2(-(previousRoom.size.x / 2 + roomSize.x / 2 + roomSpacing), 0);
                        break;
                    case 2: // top
                        roomPos = previousRoom.position + new Vector2(0, previousRoom.size.y / 2 + roomSize.y / 2 + roomSpacing);
                        break;
                    case 3: // bottom
                        roomPos = previousRoom.position + new Vector2(0, -(previousRoom.size.y / 2 + roomSize.y / 2 + roomSpacing));
                        break;
                }
            }

            Room newRoom = new Room(roomPos, roomSize);
            rooms.Add(newRoom);
        }
    }

    void GenerateWalls()
    {
        foreach (Room room in rooms)
        {
            // create four walls for each room
            CreateWall(room.position + new Vector2(0, room.size.y / 2), new Vector2(room.size.x, wallThickness), "TopWall");
            CreateWall(room.position + new Vector2(0, -room.size.y / 2), new Vector2(room.size.x, wallThickness), "BottomWall");
            CreateWall(room.position + new Vector2(room.size.x / 2, 0), new Vector2(wallThickness, room.size.y), "RightWall");
            CreateWall(room.position + new Vector2(-room.size.x / 2, 0), new Vector2(wallThickness, room.size.y), "LeftWall");
        }
    }

    void CreateWall(Vector2 position, Vector2 size, string wallName)
    {
        GameObject wall;

        if (wallPrefab != null)
        {
            wall = Instantiate(wallPrefab, position, Quaternion.identity, transform);
            wall.transform.localScale = new Vector3(size.x, size.y, 1);
        }
        else
        {
            // create simple box if no prefab
            wall = new GameObject(wallName);
            wall.transform.position = position;
            wall.transform.parent = transform;

            SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
            sr.color = Color.gray;
            sr.sprite = CreateSquareSprite();
            wall.transform.localScale = new Vector3(size.x, size.y, 1);
        }

        wall.tag = "Wall";
        wall.layer = LayerMask.NameToLayer("Wall");

        BoxCollider2D collider = wall.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = wall.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = false;
    }

    void GenerateSpawnPoints()
    {
        foreach (Room room in rooms)
        {
            for (int i = 0; i < spawnPointsPerRoom; i++)
            {
                // random position within room bounds (with padding)
                float padding = 2f;
                Vector3 spawnPos = new Vector3(
                    Random.Range(room.bounds.xMin + padding, room.bounds.xMax - padding),
                    Random.Range(room.bounds.yMin + padding, room.bounds.yMax - padding),
                    0
                );

                spawnPoints.Add(spawnPos);

                // create visual marker if prefab exists
                if (spawnPointPrefab != null)
                {
                    GameObject marker = Instantiate(spawnPointPrefab, spawnPos, Quaternion.identity, transform);
                    marker.name = "SpawnPoint_" + spawnPoints.Count;
                }
            }
        }
    }

    void SpawnPlayers()
    {
        if (rooms.Count == 0) return;

        // spawn in first room
        Room firstRoom = rooms[0];
        Vector3 player1Pos = firstRoom.position;
        Vector3 player2Pos = firstRoom.position + new Vector2(1.5f, 0);

        if (playerPrefab != null)
        {
            Instantiate(playerPrefab, player1Pos, Quaternion.identity);
        }

        if (secondPlayerPrefab != null)
        {
            Instantiate(secondPlayerPrefab, player2Pos, Quaternion.identity);
        }
    }

    public List<Vector3> GetSpawnPoints()
    {
        return spawnPoints;
    }

    public Vector3 GetRandomSpawnPoint()
    {
        if (spawnPoints.Count == 0) return Vector3.zero;
        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }

    // creates a simple white square sprite
    Sprite CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }

    // regenerate map on demand
    public void RegenerateMap()
    {
        GenerateMap();
    }
}