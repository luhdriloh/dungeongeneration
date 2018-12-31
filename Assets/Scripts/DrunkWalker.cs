using System.Collections.Generic;
using UnityEngine;


public enum Direction
{
    NORTH = 0,
    EAST = 1,
    SOUTH = 2,
    WEST = 3
};


public class DrunkWalker
{
    private Vector2 _position;
    private readonly Dictionary<Direction, Vector2> _directionToMovementMapping = new Dictionary<Direction, Vector2>
    {
        { Direction.NORTH, new Vector2(0, 1) },
        { Direction.EAST, new Vector2(1, 0) },
        { Direction.SOUTH, new Vector2(0, -1) },
        { Direction.WEST, new Vector2(-1, 0) }
    };

    public DrunkWalker(Vector2 startPosition)
    {
        _position = startPosition;
    }

    public Vector2 WalkInRandomDirection()
    {
        Direction directionForWalkerToGo = (Direction)Random.Range(0, 4);
        _position += _directionToMovementMapping[directionForWalkerToGo];
        return _position;
    }
}
