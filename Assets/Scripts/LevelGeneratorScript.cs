using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGeneratorScript : MonoBehaviour
{
    public float Size = 2;
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject doorwayPrefab;
    private List<Vector2Int> allFloors = new List<Vector2Int>();
    private List<Wall> allWalls = new List<Wall>();
    [SerializeField] public List<GameObject> allProps = new List<GameObject>(); 
    [SerializeField] public List<RoomTheme> roomThemes = new List<RoomTheme>();
    private Vector2Int[] directionArray = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(0, 1) };
    private Vector2Int[] invertedDirectionArray = new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1) };
    private int[] invertedRotationArray = new int[] { 2, 3, 0, 1 };
    

    // Start is called before the first frame update
    void Start()
    {
        GenerateRandomRooms(1, 1, 8);

    }

    // Update is called once per frame
    void Update()
    {

    }

    public string GetFloorTag(Vector2Int pos, List<roomClass> rooms)
    {
        foreach (roomClass room in rooms)
        {
            if(room.floors.Contains(pos))
            {
                return room.theme.themeName;
            }
        }
        return null;
    }


    public Color GetRandomColour()
    {
        return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    public List<Vector2Int> AttemptToFitRoom(int xsize, int ysize, int xoffset, int yoffset, List<Vector2Int> floorsToAvoid)
    {
        //create 4 lists of rooms generated from the functions input
        //check if any of the rooms are in the list of floors to avoid, if if they are, return null
        List<Vector2Int> room1 = GenerateFloorArray(xsize, ysize, xoffset, yoffset);
        if (!room1.Intersect(floorsToAvoid).Any()) { return room1; }
        List<Vector2Int> room2 = GenerateFloorArray(ysize, xsize, xoffset, yoffset);
        if (!room2.Intersect(floorsToAvoid).Any()) { return room2; }
        List<Vector2Int> room3 = GenerateFloorArray(xsize, ysize, xoffset - xsize + 1, yoffset - ysize + 1);
        if (!room3.Intersect(floorsToAvoid).Any()) { return room3; }
        List<Vector2Int> room4 = GenerateFloorArray(xsize, ysize, xoffset, yoffset - ysize + 1);
        if (!room4.Intersect(floorsToAvoid).Any()) { return room4; }
        List<Vector2Int> room5 = GenerateFloorArray(xsize, ysize, xoffset - xsize + 1, yoffset);
        if (!room5.Intersect(floorsToAvoid).Any()) { return room5; }
        //if we make it through all the rooms, return null
        return null;

    }


    // Functions For Generator;

    public void GenerateRandomRooms(int baseRoomSizeX, int baseRoomSizeY, int numberOfRooms)
    {



        //List<Vector2Int> PlacedFloors = new List<Vector2Int>();
        //List<Wall> PlacedWalls = new List<Wall>();

        List<Vector2Int> ElevatorFloors = new List<Vector2Int>();
        List<Wall> ElevatorWalls = new List<Wall>();

        //Initial Room
        ElevatorFloors = GenerateFloorArray(baseRoomSizeX, baseRoomSizeY, 0, 0);
        ElevatorWalls = GenerateWalls(ElevatorFloors);

        List<Wall> PlacedDoorways = new List<Wall>();

        List<roomClass> allRooms = new List<roomClass>();

        List<Vector2Int> doorPositions = new List<Vector2Int>();

        //Generate Hub Room
        List<Vector2Int> HubRoom = GenerateFloorArray(6 + Random.Range(0,4), 2 + Random.Range(0, 2), 0, 1);;

        allRooms.Add(new roomClass(HubRoom, GetThemeFromTag("Hub", roomThemes), GenerateWalls(HubRoom)));

        //Add Hub Doorway
        PlacedDoorways.Add(new Wall(3, Vector2Int.zero));
        doorPositions.Add(Vector2Int.zero);
        doorPositions.Add(new Vector2Int(0,1));

        //Door Posisitons


        for (int i = 0; i < numberOfRooms; i++)
        {
            bool success = false;
            int iterators = 0;

            while (success == false && iterators < 1024)
            {
                iterators++;

                roomClass testingRoom = allRooms[Random.Range(0, allRooms.Count())];
                Vector2Int testingPosition = testingRoom.floors[Random.Range(0, testingRoom.floors.Count())];;


                Vector2Int newDirection = Vector2Int.zero;
                int isEdgeTest = isEdge(testingPosition, HubRoom);
                RoomTheme roomTheme = new RoomTheme();
                roomTheme = roomThemes[Random.Range(0, roomThemes.Count)];


                if (isEdgeTest != -1 && testingPosition != Vector2Int.zero && roomTheme.roomsCanConnect.Contains(GetFloorTag(testingPosition, allRooms)) && Random.Range(0, 100) / 100f <= roomTheme.roomSpawnChance && roomTheme.MaxUses > 0)
                {
                    
                    while (roomTheme.restrictFromRandomSelection == true)
                    {
                        roomTheme = roomThemes[Random.Range(0, roomThemes.Count)];
                    }
                    newDirection = testingPosition + directionArray[isEdgeTest];

                    List<Vector2Int> CombinedFloors = new List<Vector2Int>();
                    foreach (roomClass roomtoTest in allRooms)
                    {
                        CombinedFloors.AddRange(roomtoTest.floors);
                    }
                    List<Vector2Int> newRoom = AttemptToFitRoom(Random.Range(0, 2) + roomTheme.sizeX, Random.Range(0, 2) + roomTheme.sizeY, newDirection.x, newDirection.y, CombinedFloors);
                    if (newRoom != null)
                    {
                        List<Wall> newRoomWalls = GenerateWalls(newRoom);
                        doorPositions.Add(testingPosition);
                        doorPositions.Add(newDirection);
                        PlacedDoorways.Add(new Wall(isEdgeTest, testingPosition));
                        PlacedDoorways.Add(new Wall(invertedRotationArray[isEdgeTest], newDirection));
                        //PlacedWalls.AddRange(newRoomWalls);
                        //PlacedFloors.AddRange(newRoom);
                        allRooms.Add(new roomClass(newRoom, roomTheme, newRoomWalls));
                        if (roomTheme.MaxUses > 0)
                        {
                            roomTheme.MaxUses--;
                        }
                        //DrawDebugString($"{GetThemeFromTag(GetFloorTag(newDirection, allRooms), roomThemes).MaxUses} : {GetFloorTag(newDirection, allRooms)}", posToVector(newDirection));
                        success = true;
                    }
                    else if (newRoom == null)
                    {
                        success = false;
                    }
                }
            }
        }

        foreach (Wall doorway in PlacedDoorways)
        {
            foreach (roomClass room in allRooms)
            {
                if (getWallAtTransform(doorway.pos, doorway.rot, room.walls) != -1)
                {
                    room.walls[getWallAtTransform(doorway.pos, doorway.rot, room.walls)].ChangeWallState(Wall.WallType.Doorway);
                }
            }

            
        }


        //remove duplicates from lists
        /*PlacedFloors = PlacedFloors.Distinct().ToList();
        PlaceFloors(PlacedFloors);*/
        //PlaceWalls(PlacedWalls);

        foreach (roomClass roomToPlace in allRooms)
        {
            //PlaceFloors(roomToPlace.floors.Distinct().ToList());
            PlaceWalls(roomToPlace.walls);

        }

        foreach (roomClass room in allRooms)
        {
            foreach (Vector2Int testFloor in room.floors)
            {
                int propEdgeTest = isEdge(testFloor, room.floors);
                if (propEdgeTest != -1 && !doorPositions.Contains(testFloor))
                {
                    if(Random.Range(0,100) / 100f <= room.theme.spawnChance)
                    {
                        PlaceProp(new Prop(testFloor, propEdgeTest, room.theme.props));
                    }
                    
                }
            }
            
        }

    }

    //Generates an array of vector2 ints based on the size and offset of the room you give it.
    public List<Vector2Int> GenerateFloorArray(int sizex, int sizey, int offsetx, int offsety)
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

    public RoomTheme GetThemeFromTag(string tag, List<RoomTheme> themes)
    {
        //loop through themes
        foreach (RoomTheme theme in themes)
        {
            if(theme.themeName == tag)
            {
                return theme;
            }
        }
        return new RoomTheme();
    }


    public void DebugWall(Wall wall)
    {
        Instantiate(wallPrefab, posToVector(wall.pos) + new Vector3(0.0f, 3f, 0.0f), Quaternion.Euler(-90f, intToRot(wall.rot), 0.0f), this.transform);
    }


    //Draws debug string, at desired location
    public void DrawDebugString(string Text, Vector3 position)
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
    public List<Wall> GenerateWalls(List<Vector2Int> floors)
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
                if (!floors.Contains(floor + testDirection) && !doesWallExist(floor, y))
                {
                    Wall wallToAdd = new Wall(y, floor);
                    walls.Add(wallToAdd);
                    allWalls.Add(wallToAdd);

                }
            }
        }

        return walls;
    }

    //Creates a debug box at a location, with the specified colour
    public void AddDebugBox(Vector3 position, Color colour = default(Color))
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.transform.position = position;
        box.transform.localScale = new Vector3(1, 1, 1);
        box.GetComponent<Renderer>().material.color = colour;
    }


    //Checks if a floor piece is on a corner
    public bool isCorner(Vector2Int floor, List<Vector2Int> floors)
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
    public int isEdge(Vector2Int floor, List<Vector2Int> testArray)
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
                if (index == localDirections.Length)
                {
                    index = 0;
                }
            }
        }
        return -1;
    }

    //Places the visuals for floors based on a list of vector 2 ints
    public void PlaceFloors(List<Vector2Int> inFloors)
    {
        foreach (Vector2Int floor in inFloors)
        {
            Instantiate(floorPrefab, posToVector(floor), Quaternion.Euler(-90f, 0.0f, 0.0f), this.transform).name = $"Floor: {floor}";
        }
    }

    //Places the wall visuals based on the list of walls
    public void PlaceWalls(List<Wall> inWalls)
    {
        foreach (Wall wall in inWalls)
        {
            Instantiate(getWallPrefabFromType(wall.wallType), posToVector(wall.pos), Quaternion.Euler(-90f, intToRot(wall.rot), 0.0f), this.transform).name = $"{wall.wallType}: {wall.pos} ";
        }
    }

    //Turns a vector 2 int into a vector 3 and takes into account size
    public Vector3 posToVector(Vector2Int pos)
    {
        return (new Vector3(pos.x * Size, 0.0f, pos.y * Size));
    }

    //Turns an int into a rotator.
    public float intToRot(int rot)
    {
        return rot * 90;
    }

    public int getWallAtTransform(Vector2Int pos, int rot, List<Wall> wallsList)
    {
        int i = 0;
        //loop through all walls
        foreach (Wall wall in wallsList)
        {

            //if the wall is at the position and rotation
            if (wall.pos == pos && wall.rot == rot)
            {
                //return the wall
                return i;
            }
            i++;

        }
        return -1;
    }

    public bool doesWallExist(Vector2Int pos, int rot)
    {
        foreach (Wall wall in allWalls)
        {
            if (wall.pos == pos && wall.rot == rot)
            {
                return true;
            }
            else if (wall.pos == pos + directionArray[rot] && wall.rot == invertedRotationArray[rot])
            {
                return true;
            }
        }
        return false;

    }
    GameObject getWallPrefabFromType(Wall.WallType wallType)
    {
        switch (wallType)
        {
            case Wall.WallType.Wall:
                return wallPrefab;
            case Wall.WallType.Window:
                return wallPrefab;
            case Wall.WallType.Doorway:
                return doorwayPrefab;
            default:
                return null;
        }
    }

    void PlaceProp(Prop prop)
    {
        if(prop != null)
        {
            Instantiate(prop.propPrefab, posToVector(prop.pos), Quaternion.Euler(-90f, intToRot(prop.rot), 0.0f), this.transform).name = $"Prop: {prop.propPrefab.name} ";
        }
    }
}


public class Wall
{
    public int rot;
    public Vector2Int pos;
    public WallType wallType;

    //define an enum of wall types
    public enum WallType
    {
        Wall,
        Doorway,
        Window
    }

    public void ChangeWallState(WallType wallType)
    {
        this.wallType = wallType;
    }


    public Wall(int rot, Vector2Int pos)
    {
        this.rot = rot;
        this.pos = pos;
    }
}

[System.Serializable]
public class Prop
{
    public GameObject propPrefab;
    public Vector2Int pos;
    public int rot;

    public Prop(Vector2Int pos, int rot, List<GameObject> props)
    {
        //get random element from props
        propPrefab = props[Random.Range(0, props.Count)];
        this.pos = pos;
        this.rot = rot;

        //
        
    }
}


[System.Serializable]
public class RoomTheme
{
    public string themeName;
    public float spawnChance = 1;
    public float roomSpawnChance = 1;
    public List<GameObject> props;
    public Color wallTint = Color.white;
    public Color floorTint = Color.white;
    public int sizeX = 1;
    public int sizeY = 1;
    public bool restrictFromRandomSelection;
    public int MaxUses = 1;
    public List<string> roomsCanConnect;
  
}

public class roomClass
{
    public List<Vector2Int> floors;
    public RoomTheme theme;
    public List<Wall> walls;
    bool selectedRandomly;
    public roomClass(List<Vector2Int> floors, RoomTheme theme, List<Wall> walls)
    {
        this.floors = floors;
        this.theme = theme;
        this.walls = walls;
    }
}