using System.Collections.Generic;

using UnityEngine;

/*
* An implementation of Prim's algorithm for generating a minimum spanning tree
* given an edge weighted undirected graph G.
* */
public class Prims
{
    private HashSet<Edge> _edgesChosen;
    private HashSet<Edge> _edgesToCheck;  
    private List<Vector2> _listOfVertices; // our vertices
    private HashSet<Vector2> _verticesChecked;
    private Vector2 _vertexToCheck;
    private Dictionary<Vector2, List<Vector2>> _graph;
    

    public Prims(Dictionary<Vector2, List<Vector2>> graph)
    {
        //initialize the various arrays and the minimum priority queue
        _edgesChosen = new HashSet<Edge>();
        _edgesToCheck = new HashSet<Edge>();
        _listOfVertices = new List<Vector2>();
        _verticesChecked = new HashSet<Vector2>();
        _graph = graph;

        foreach (Vector2 vertex in _graph.Keys)
        {
            _listOfVertices.Add(vertex);
        }
    }

    public HashSet<Edge> Prim()
    {
        // get random vertex to start at
        int randomIndexToStartTreeFrom = Random.Range(0, _graph.Keys.Count);

        // add first vertex to list
        _vertexToCheck = _listOfVertices[randomIndexToStartTreeFrom];
        _listOfVertices.RemoveAt(randomIndexToStartTreeFrom);
        _verticesChecked.Add(_vertexToCheck);

        // keep going until we have no vertices to check anymore
        while (_listOfVertices.Count > 0)
        {
            _vertexToCheck = Scan(_graph, _vertexToCheck);
        }

        return _edgesChosen;
    }

    /*
    * This method takes a vertex v, finds all the vertices connected to v and 
    * compares their weights to the weights we've already found and determines if any of the weights
    * are less than what we already have
    * */
    private Vector2 Scan(Dictionary<Vector2, List<Vector2>> graph, Vector2 vertexToCheck)
    {
        // scan all edges from our vertex to check
        foreach (Vector2 connectionTo in graph[vertexToCheck])
        {
            Edge newEdge = new Edge(vertexToCheck, connectionTo);

            // make sure we dont add edges to vertices already checked
            if (_verticesChecked.Contains(newEdge.GetVertex(0)) && _verticesChecked.Contains(newEdge.GetVertex(1)))
            {
                continue;
            }

            if (_edgesToCheck.Contains(newEdge) == false)
            {
                _edgesToCheck.Add(newEdge);
            }
        }

        // look through our edges to check and find one with the lowest cost
        //  to a vertex in our list of vertices
        float lowestCost = float.MaxValue;
        List<Edge> edgesToRemove = new List<Edge>();
        Edge lowestCostEdge = null;
        Vector2 vertexAdded = Vector2.zero;

        foreach (Edge edge in _edgesToCheck)
        {
            // remove edges whose vertices are already connected
            if (_verticesChecked.Contains(edge.GetVertex(0)) && _verticesChecked.Contains(edge.GetVertex(1)))
            {
                edgesToRemove.Add(edge);
                continue;
            }

            // check if we have the lowest cost edge
            if (edge.GetEdgeWeight() <= lowestCost)
            {
                vertexAdded = _verticesChecked.Contains(edge.GetVertex(0)) ? edge.GetVertex(1) : edge.GetVertex(0);
                lowestCostEdge = edge;
                lowestCost = edge.GetEdgeWeight();
            }
        }

        // remove any uneccessary edges
        edgesToRemove.ForEach(edge => _edgesToCheck.Remove(edge));

        // add our new vertex to list of checked vertices and remove it
        //  from list of vertices
        _verticesChecked.Add(vertexAdded);
        _listOfVertices.Remove(vertexAdded);

        // add new edge
        _edgesChosen.Add(lowestCostEdge);

        return vertexAdded;
    }
}