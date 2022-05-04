using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGeneratorScript : MonoBehaviour
{
    public float Size = 2;
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public List<Vector2Int> allFloors = new List<Vector2Int>();
    public List<Wall> allWalls = new List<Wall>();
    private Vector2Int[] directionArray = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(0, 1) };

    // Start is called before the first frame update
    void Start()
    {
        /*allFloors = GenerateFloorArray(4, 4, 2, 2);
        PlaceFloors(allFloors);
        allWalls = GenerateWalls(allFloors);
        PlaceWalls(allWalls);*/
        GenerateRandomRooms(4, 4, 1);
    }

    // Update is called once per frame
    void Update()
    {

    }


    Color GetRandomColour()
    {
        return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    List<Vector2Int> AttemptToFitRoom(int xsize, int ysize, int xoffset, int yoffset, List<Vector2Int> floorsToAvoid)
    {
        //create 4 lists of rooms generated from the functions input
        List<Vector2Int> room1 = GenerateFloorArray(xsize, ysize, xoffset, yoffset);
        List<Vector2Int> room2 = GenerateFloorArray(ysize, xsize, xoffset, yoffset);
        List<Vector2Int> room3 = GenerateFloorArray(xsize, ysize, xoffset - (xsize-1), yoffset - (ysize-1));
        List<Vector2Int> room4 = GenerateFloorArray(xsize, ysize, xoffset , yoffset - (ysize + 1));
        List<Vector2Int> room5 = GenerateFloorArray(xsize, ysize, xoffset - (xsize + 1), yoffset);

        //check if any of the rooms are in the list of floors to avoid, if if they are, return null
        if (!room1.Intersect(floorsToAvoid).Any()) { return room1; }
        if (!room2.Intersect(floorsToAvoid).Any()) { return room2; }
        if (!room3.Intersect(floorsToAvoid).Any()) { return room3; }
        if (!room4.Intersect(floorsToAvoid).Any()) { return room4; }
        if (!room5.Intersect(floorsToAvoid).Any()) { return room5; }

        //if we make it through all the rooms, return null
        return null;

    }


    // Functions For Generator;

    void GenerateRandomRooms(int baseRoomSizeX, int baseRoomSizeY, int numberOfRooms)
    {
        //List of floors
        List<Vector2Int> PlacedFloors = new List<Vector2Int>();
        //list of walls
        List<Wall> PlacedWalls = new List<Wall>();
        PlacedFloors = GenerateFloorArray(baseRoomSizeX, baseRoomSizeY, 0, 0);
        //Create walls for inital room
        PlacedWalls = GenerateWalls(PlacedFloors);
        //Place walls
        PlaceWalls(PlacedWalls);

        for (int i = 0; i < numberOfRooms; i++)
        {
            bool success = false;
            int iterators = 0;

            Vector2Int testingPosition = PlacedFloors[Random.Range(0, PlacedFloors.Count)];
            int isEdgeTest = isEdge(testingPosition, PlacedFloors);
            Vector2Int newDirection = testingPosition + directionArray[isEdgeTest];

/*            if (isEdgeTest != -1)
            {
                AddDebugBox(posToVector(testingPosition));
                AddDebugBox(posToVector(newDirection));
            }
*/
            List<Vector2Int> newRoom = AttemptToFitRoom(3, 3, newDirection.x, newDirection.y, PlacedFloors);

            PlaceFloors(newRoom);
            PlaceWalls(GenerateWalls(newRoom));


/*
            while (success == false && iterators < 0)
            {
                iterators++;

                





            }*/

            //remove duplicates from lists
            PlacedFloors = PlacedFloors.Distinct().ToList();
            PlaceFloors(PlacedFloors);
            PlaceWalls(PlacedWalls);
        }


    }

    //Generates an array of vector2 ints based on the size and offset of the room you give it.
    List<Vector2Int> GenerateFloorArray(int sizex, int sizey, int offsetx, int offsety)
    {
        List<Vector2Int> vector2Ints = new List<Vector2Int>();
        //Main creation loop
        for (int x = 0; x < sizex; x++)
        {
            vector2Ints.Add(new Vector2Int(x + offsetx, 0 + offsety));

            for (int y = 0; y < sizey; y++)
            {
                if (y != 0)
                {
                    vector2Ints.Add(new Vector2Int(x + offsetx, y + offsety));
                }

            }
        }
        //Return floor pieces
        return vector2Ints;
    }


    //Draws debug string, at desired location
    void DrawDebugString(string Text, Vector3 position)
    {
        //Generate text that always faces the camera
        GameObject text = new GameObject();
        text.transform.position = position;
        text.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        text.AddComponent<TextMesh>();
        text.GetComponent<TextMesh>().text = Text;
        text.GetComponent<TextMesh>().fontSize = 10;
        text.GetComponent<TextMesh>().alignment = TextAlignment.Center;
        text.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
        text.GetComponent<TextMesh>().characterSize = 0.1f;
        text.GetComponent<TextMesh>().color = Color.red;
        text.GetComponent<TextMesh>().font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;


    }

    //Generates an array of walls, that can be placed later, walls are only placed on edges.
    List<Wall> GenerateWalls(List<Vector2Int> floors)
    {

        List<Wall> walls = new List<Wall>();

        foreach (Vector2Int floor in floors)
        {

            if (isCorner(floor, floors))
            {
                /*DrawDebugString("Poop", posToVector(floor));*/
            }

            for (int y = 0; y < 4; y++)
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

    //Creates a debug box at a location, with the specified colour
    void AddDebugBox(Vector3 position, Color colour = default(Color))
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.transform.position = position;
        box.transform.localScale = new Vector3(1, 1, 1);
        box.GetComponent<Renderer>().material.color = colour;
    }


    //Checks if a floor piece is on a corner
    bool isCorner(Vector2Int floor, List<Vector2Int> floors)
    {
        int count = 0;
        foreach (Vector2Int test in directionArray)
        {
            if (!floors.Contains(floor + test))
            {
                count++;
            }
        }
        if (count == 2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    //Checks if a floor is on the edge
    int isEdge(Vector2Int floor, List<Vector2Int> testArray)
    {
        Vector2Int[] localDirections = directionArray;
        int index = Random.Range(0, localDirections.Length);
        for (int i = 0; i < 4; i++)
        {
            Vector2Int testDirection = directionArray[index];
            if (!testArray.Contains(floor + testDirection))
            {
                return index;
            }
            else
            {
                index++;
                if(index == localDirections.Length)
                {
                    index = 0;
                }
            }
        }
        return -1;
    }

    //Places the visuals for floors based on a list of vector 2 ints
    void PlaceFloors(List<Vector2Int> inFloors)
    {
        foreach (Vector2Int floor in inFloors)
        {
            Instantiate(floorPrefab, posToVector(floor), Quaternion.Euler(-90f, 0.0f, 0.0f), this.transform);
        }
    }

    //Places the wall visuals based on the list of walls
    void PlaceWalls(List<Wall> inWalls)
    {
        foreach (Wall wall in inWalls)
        {
            Instantiate(wallPrefab, posToVector(wall.pos), Quaternion.Euler(-90f, intToRot(wall.rot), 0.0f), this.transform);
        }
    }

    //Turns a vector 2 int into a vector 3 and takes into account size
    Vector3 posToVector(Vector2Int pos)
    {
        return (new Vector3(pos.x * Size, 0.0f, pos.y * Size));
    }

    //Turns an int into a rotator.
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
