using System.Collections.Generic;
using UnityEngine;

public class LevelGeneratorScript : MonoBehaviour
{
    public float Size;
    public GameObject floorPrefab;
    public GameObject wallPrefab;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // Functions For Generator;

    List<Vector2Int> GenerateFloorArray(int x, int y, int offsetx, int offsety)
    {
        List<Vector2Int>
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
