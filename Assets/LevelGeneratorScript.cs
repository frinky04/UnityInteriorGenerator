using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGeneratorScript : MonoBehaviour
{
    public float Size = 2;
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public List<Vector2Int> allfloors = new List<Vector2Int>();
    public List<Wall> allwalls = new List<Wall>();
    private Vector2Int[] directionArray = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(0, 1) };

    // Start is called before the first frame update
    void Start()
    {
        allfloors = GenerateFloorArray(4,4,0,0);
        PlaceFloors(allfloors);
        allwalls = GenerateWalls(allfloors);
        PlaceWalls(allwalls);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Functions For Generator;

    List<Vector2Int> GenerateFloorArray(int sizex, int sizey, int offsetx, int offsety)
    {
        List<Vector2Int> vector2Ints = new List<Vector2Int>();
        //Main creation loop
        for (int x = 0; x < sizex; x++)
        {
            vector2Ints.Add(new Vector2Int(x, 0));

            for (int y = 0; y < sizey; y++)
            {
                if(y != 0)
                {
                    vector2Ints.Add(new Vector2Int(x, y));
                }
                
            }
        }
        //Return floor pieces
        return vector2Ints;
    }
    List<Wall> GenerateWalls(List<Vector2Int> floors)
    {

        List<Wall> walls = new List<Wall>();

        foreach (Vector2Int floor in floors)
        {
            for(int y = 0; y < 4; y++)
            {
                Vector2Int testDirection = directionArray[y];
                if (!floors.Contains(floor + testDirection))
                {
                    walls.Add(new Wall(y, floor));
                }
            }
        }

        return walls;
    }

    int isEdge(Vector2Int floor, List<Vector2Int> testArray)
    {
        Vector2Int[] localDirections = directionArray;
        for (int i = 0; i < 4; i++)
        {
            Vector2Int testDirection = directionArray[i];
            if (!testArray.Contains(floor + testDirection))
            {
                return i;
            }
        }
        return -1;
    }

    void PlaceFloors(List<Vector2Int> inFloors)
    {
        foreach (Vector2Int floor in inFloors)
        {
            Instantiate(floorPrefab, posToVector(floor), Quaternion.Euler(-90f, 0.0f, 0.0f), this.transform);
        }
    }

    void PlaceWalls(List<Wall> inWalls)
    {
        foreach (Wall wall in inWalls)
        {
            Instantiate(wallPrefab, posToVector(wall.pos), Quaternion.Euler(-90f, intToRot(wall.rot), 0.0f), this.transform);
        }
    }

    Vector3 posToVector(Vector2Int pos)
    {
        return (new Vector3(pos.x * Size, 0.0f, pos.y * Size));
    }

    float intToRot(int rot)
    {
        return rot * 90;
    }
}


public class Wall
{
    public int rot;
    public Vector2Int pos;
    public bool isDoorway;

    public Wall(int rot, Vector2Int pos)
    {
        this.rot = rot;
        this.pos = pos;
    }
}
