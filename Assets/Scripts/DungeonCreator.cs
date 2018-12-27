﻿using System.Collections.Generic;
using UnityEngine;

using HullDelaunayVoronoi.Primitives;
using HullDelaunayVoronoi.Delaunay;


public class DungeonCreator : MonoBehaviour
{
    public GameObject _roomPrototype;
    public int _numberOfRooms;
    public int _roomSizeMean;
    public int _roomSizeStandardDeviation;
    public int _circleRadius;
    public int _minRoomSizeRatio;
    public int _roomMinSize;
    public int _corridorSize;
    public float _percentEdgesToAddBack;

    public float _mainRoomSizeThreshold;

    private List<Room> _roomsList;
    private List<Room> _mainRoomsList;
    private List<Vector2> _vertices;
    private HashSet<Edge> _mstEdges;

    private Dictionary<Vector2, Room> _midpointToRoomMapping;
    private Dictionary<Vector2, List<Vector2>> _roomConnections;

    private DelaunayTriangulation2 _delaunayTriangulation;
    private Material _lineMaterial;


    private Color[] _roomColors =
    {
        new Color32(0, 17, 109, 255),
        new Color32(27, 52, 186, 255),
        new Color32(0, 109, 255, 255),
        new Color32(255, 223, 0, 255),
        new Color32(183, 29, 29, 255)
    };

    private void Start ()
    {
        Physics2D.autoSimulation = false;
        _roomsList = new List<Room>();
        _mainRoomsList = new List<Room>();
        _vertices = new List<Vector2>();
        _midpointToRoomMapping = new Dictionary<Vector2, Room>();
        _roomConnections = new Dictionary<Vector2, List<Vector2>>();
        _lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));

        GenerateRooms();
        SeperateRooms();
        RoundRoomPositions();
        FindMainRooms();
        CreateGraph();

        _mstEdges = MST();
        AddBackSomeEdgesToMST();

        CreateCorridors();
        AddBackIntersectingRooms();
    }

    private void GenerateRooms()
    {
        int newWidth, newHeight;

        for (int i = 0; i < _numberOfRooms; i++)
        {
            Room room = Instantiate(_roomPrototype, GetRandomPointInCircle(_circleRadius, Vector2.zero), Quaternion.identity).GetComponent<Room>();

            // get room size
            newWidth = Mathf.RoundToInt(_roomSizeMean + NextGaussianDouble() * _roomSizeStandardDeviation);
            newHeight = Mathf.RoundToInt(_roomSizeMean + NextGaussianDouble() * _roomSizeStandardDeviation);

            newWidth = newWidth < _roomMinSize ? _roomMinSize : newWidth;
            newHeight = newHeight < _roomMinSize ? _roomMinSize : newHeight;

            if (newWidth / newHeight > _minRoomSizeRatio)
            {
                newHeight = Mathf.RoundToInt(newWidth / _minRoomSizeRatio);
            }

            if (newHeight / newWidth > _minRoomSizeRatio)
            {
                newWidth = Mathf.RoundToInt(newHeight / _minRoomSizeRatio);
            }

            room.SetSize(newWidth, newHeight);
            room.SetColor(GetRandomRoomColor());
            room.transform.position -= new Vector3(newWidth / 2, newHeight / 2, 0f);
            room._id = i;

            _roomsList.Add(room);
        }
    }

    private void SeperateRooms()
    {
        Time.timeScale = 50;
        foreach (Room room in _roomsList)
        {
            room.TurnOnCollision();
        }

        int numAwake = NumOfRoomsAwake();
        int physicsTimesSimulated = 0;

        if (Physics2D.autoSimulation == false)
        {
            while (numAwake > 0)
            {
                for (int i = 0; i < 50; i++)
                {
                    Physics2D.Simulate(Time.fixedDeltaTime);
                }

                physicsTimesSimulated += 50;
                numAwake = NumOfRoomsAwake();
            }
        }

        Debug.Log("times simulated: " + physicsTimesSimulated);
        Time.timeScale = 1;
    }

    private void RoundRoomPositions()
    {
        foreach (Room room in _roomsList)
        {
            room.RoundPosition();
        }
    }

    private void FindMainRooms()
    {
        int sizeThreshold = Mathf.RoundToInt((_roomSizeMean * _mainRoomSizeThreshold) * (_roomSizeMean * _mainRoomSizeThreshold));
        foreach (Room room in _roomsList)
        {
            if (GetRoomSize(room) >= sizeThreshold)
            {
                room.SetColor(_roomColors[2]);
                _mainRoomsList.Add(room);
            }
            else
            {
                room.SetColor(Color.white);
            }
        }
    }

    private void CreateGraph()
    {
        List<Vertex2> vertices = new List<Vertex2>();
        Vector2 roomMidpoint;

        foreach (Room room in _mainRoomsList)
        {
            roomMidpoint = room.ReturnRoomMidpoint();
            _midpointToRoomMapping.Add(roomMidpoint, room);
            vertices.Add(new Vertex2(roomMidpoint.x, roomMidpoint.y));
            _vertices.Add(roomMidpoint);
        }

        _delaunayTriangulation = new DelaunayTriangulation2();
        _delaunayTriangulation.Generate(vertices);

        foreach (DelaunayCell<Vertex2> cell in _delaunayTriangulation.Cells)
        {
            Simplex<Vertex2> nodeVertices = cell.Simplex;

            Vector2 one = new Vector2(nodeVertices.Vertices[0].X, nodeVertices.Vertices[0].Y);
            Vector2 two = new Vector2(nodeVertices.Vertices[1].X, nodeVertices.Vertices[1].Y);
            Vector2 three = new Vector2(nodeVertices.Vertices[2].X, nodeVertices.Vertices[2].Y);

            List<Vector2> vertexList = new List<Vector2> { one, two, three };

            // add our vertices to our dictionary if they are not already there
            for (int i = 0; i < vertexList.Count; i++)
            {
                if (_roomConnections.ContainsKey(vertexList[i]) == false)
                {
                    _roomConnections.Add(vertexList[i], new List<Vector2>());
                }

                AddVerticesIfNonExistant(vertexList[i], vertexList);
            }
        }
    }

    private HashSet<Edge> MST()
    {
        Prims mst = new Prims(_roomConnections);
        HashSet<Edge> edges = mst.Prim();
        return edges;
    }

    private void AddBackSomeEdgesToMST()
    {
        List<Edge> allEdges = new List<Edge>();

        foreach (Vector2 vertex in _roomConnections.Keys)
        {
            foreach (Vector2 vertexTo in _roomConnections[vertex])
            {
                Edge newEdge = new Edge(vertex, vertexTo);
                if (allEdges.Contains(newEdge) == false)
                {
                    allEdges.Add(newEdge);
                }
            }
        }

        // shuffle vertices
        Shuffle<Edge>(allEdges);
        int numEdgesToAddBack = Mathf.RoundToInt(_percentEdgesToAddBack * allEdges.Count);

        // add back a couple edges to make the dungeon more interesting
        for (int i = 0; i < numEdgesToAddBack; i++)
        {
            _mstEdges.Add(allEdges[i]);
        }
    }

    private void CreateCorridors()
    {
        // create and l shape or a straight line
        foreach (Edge edge in _mstEdges)
        {
            Room one = _midpointToRoomMapping[edge.GetVertex(0)];
            Room two = _midpointToRoomMapping[edge.GetVertex(1)];

            one.SetColor(Color.red);
            two.SetColor(Color.red);

            // if the rooms intersect vertically and horizontally then they are 
            //  right next to each other
            Debug.Log("horizontally: " + RoomsIntersectHorizontally(one, two));
            Debug.Log("vertically: " + RoomsIntersectVertically(one, two));

            break;
        }
    }

    private void AddBackIntersectingRooms()
    { 
    
    }

    // HELPER FUNCTIONS //

    private Vector2 GetRandomPointInCircle(float radius, Vector2 center)
    {
        float theta = 2 * Mathf.PI * Random.value;
        float distanceFromCenter = radius * Mathf.Sqrt(Random.value);

        float x = center.x + distanceFromCenter * Mathf.Cos(theta);
        float y = center.y + distanceFromCenter * Mathf.Sin(theta);

        return new Vector2(x, y);
    }

    private float NextGaussianDouble()
    {
        float u, v, S;

        do
        {
            u = 2.0f * Random.value - 1.0f;
            v = 2.0f * Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0);
        
        float fac = Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
        return u * fac;
    }

    private int NumOfRoomsAwake()
    {

        int numAwake = 0;

        foreach (Room room in _roomsList)
        {
            numAwake += room.PhysicsSleeping() ? 0 : 1;
        }

        return numAwake;
    }

    private Color GetRandomRoomColor()
    {
        int index = Random.Range(0, _roomColors.Length);
        return _roomColors[index];
    }

    private int GetRoomSize(Room room)
    {
        return room._height * room._width;    
    }


    private void AddVerticesIfNonExistant(Vector2 vertex, List<Vector2> verticesToAdd)
    {
        foreach (Vector2 vertexToAdd in verticesToAdd)
        {
            if (vertex == vertexToAdd)
            {
                continue;
            }
            else if (_roomConnections[vertex].Contains(vertexToAdd) == false)
            {
                _roomConnections[vertex].Add(vertexToAdd);
            }
        }
    }

    private void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private bool RoomsIntersectVertically(Room one, Room two)
    {
        float roomOneLeftSideXValue = one.transform.position.x;
        float roomOneRightSideXValue = roomOneLeftSideXValue + one._width;

        float roomTwoLeftSideXValue = two.transform.position.x;
        float roomTwoRightSideXValue = roomTwoLeftSideXValue + two._width;

        return (roomOneLeftSideXValue <= roomTwoRightSideXValue && roomOneRightSideXValue >= roomTwoLeftSideXValue);
    }

    private bool RoomsIntersectHorizontally(Room one, Room two)
    {
        float roomOneBottomSideYValue = one.transform.position.y;
        float roomOneTopSideYValue = roomOneBottomSideYValue + one._height;

        float roomTwoBottomSideYValue = two.transform.position.y;
        float roomTwoTopSideYValue = roomTwoBottomSideYValue + two._height;

        return (roomOneBottomSideYValue <= roomTwoTopSideYValue && roomOneTopSideYValue >= roomTwoBottomSideYValue);
    }

    // GRAPH FUNCTIONS //

    private void OnDrawGizmos()
    {
        if (_delaunayTriangulation == null || _delaunayTriangulation.Cells.Count == 0 || _delaunayTriangulation.Vertices.Count == 0)
        {
            return;
        }

        // first lets just draw the circles around our main rooms
        Gizmos.color = Color.yellow;
        foreach (Vector2 vertex in _vertices)
        {
            Gizmos.DrawSphere(new Vector3(vertex.x, vertex.y, 0), 3);
        }

        //DrawEdges();
        DrawMST();
    }

    private void DrawMST()
    {
        if (_mstEdges.Count == 0)
        {
            return;
        }

        Gizmos.color = Color.green;

        foreach (Edge edge in _mstEdges)
        {
            Gizmos.DrawLine(edge.GetVertex(0), edge.GetVertex(1));
        }
    }

    private void DrawEdges()
    {
        Gizmos.color = Color.red;

        foreach (Vector2 vertex in _roomConnections.Keys)
        {
            foreach (Vector2 vertexTo in _roomConnections[vertex])
            {
                Gizmos.DrawLine(vertex, vertexTo);
            }
        }
    }
}
