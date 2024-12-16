using UnityEngine;

public enum Direction
{
    Left,
    Right,
    Front,
    Back,
    Top,
    Bottom
}

public static class DirectionExtensions
{
    public static Vector3Int ToVector3Int(this Direction direction)
    {
        return direction switch
        {
            Direction.Left => new Vector3Int(-1, 0, 0),
            Direction.Right => new Vector3Int(1, 0, 0),
            Direction.Front => new Vector3Int(0, 0, 1),
            Direction.Back => new Vector3Int(0, 0, -1),
            Direction.Top => new Vector3Int(0, 1, 0),
            Direction.Bottom => new Vector3Int(0, -1, 0),
            _ => Vector3Int.zero
        };
    }
}