using System.Collections.Generic;
using UnityEngine;

public class DrunkWalkerDungeonCreator : MonoBehaviour
{
    public GameObject _roomPrototype;

    public bool _overlapAllowed;
    public int _numberOfWalkers;
    public int _numberOfIterations;
    private HashSet<Vector2> _positionsVisited;
    private List<DrunkWalker> _drunkWalkers;

	private void Start ()
    {
        _positionsVisited = new HashSet<Vector2>();
        _drunkWalkers = new List<DrunkWalker>();

        for (int i = 0; i < _numberOfWalkers; i++)
        {
            _drunkWalkers.Add(new DrunkWalker(Vector2.zero));
        }

        _positionsVisited.Add(Vector2.zero);
        MakeTheDrunkardsWalk();
        CreateDungeon();
	}

    private void MakeTheDrunkardsWalk()
    {
        for (int i = 0; i < _numberOfIterations; i++)
        {
            foreach (DrunkWalker drunkWalker in _drunkWalkers)
            {
                Vector2 previousPosition = drunkWalker.Position;
                Vector2 newPosition;

                if (_overlapAllowed)
                {
                    _positionsVisited.Add(drunkWalker.WalkInRandomDirection());
                    continue;
                }

                // only do this if we have no overlap allowed
                for (int k = 0; k < 10; k++)
                {
                    drunkWalker.Position = previousPosition;
                    newPosition = drunkWalker.WalkInRandomDirection();
                    if (!_positionsVisited.Contains(newPosition))
                    {
                        _positionsVisited.Add(newPosition);
                        break;
                    }
                }


            }
        }
    }

    private void CreateDungeon()
    {
        foreach (Vector2 locationToFill in _positionsVisited)
        {
            Room room = Instantiate(_roomPrototype, locationToFill, Quaternion.identity).GetComponent<Room>();
            room.SetSize(1, 1);
            room.SetColor(new Color32(255, 165, 0, 255));
        }
    }
}

