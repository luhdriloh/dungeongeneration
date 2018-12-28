using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    private readonly float _edgeWeight;
    protected readonly Vector2 _pointOne;
    protected readonly Vector2 _pointTwo;

    public Edge(Vector2 one, Vector2 two)
    {
        // order which vertex gets set as one or two for hashing
        if ((two.x < one.x) || (Mathf.Abs(one.x - two.x) < Mathf.Epsilon && two.y < one.y))
        {
            Vector2 temp = one;
            one = two;
            two = temp;
        }

        _pointOne = one;
        _pointTwo = two;

        _edgeWeight = Vector2.Distance(one, two);
    }

    public float GetEdgeWeight()
    {
        return _edgeWeight;
    }

    public Vector2 GetVertex(int index)
    {
        return index == 0 ? _pointOne : _pointTwo;
    }

    public bool Equals(Edge edgeToCheck)
    {
        return _pointOne == edgeToCheck._pointOne && _pointTwo == edgeToCheck._pointTwo;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Edge);
    }

    public override int GetHashCode()
    {
        return this._pointOne.GetHashCode() * 17 + this._pointOne.GetHashCode();
    }
}
