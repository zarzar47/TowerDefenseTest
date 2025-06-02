using UnityEngine;

// This class represents a position on a grid, defined by its x and y coordinates.
// Basic helper class mostly used for testing and debugging but has potential for more complex grid-based logic in the future.
public class GridPosition
{
    public int x;
    public int y;

    public GridPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override string ToString()
    {
        return $"({x}, {y})";
    }

    public override bool Equals(object obj)
    {
        if (obj is GridPosition other)
        {
            return x == other.x && y == other.y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return (x, y).GetHashCode();
    }
}
