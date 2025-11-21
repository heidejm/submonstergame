using System;
using System.Collections.Generic;

namespace SubGame.Core.Grid
{
    /// <summary>
    /// Represents a coordinate in the 3D grid system.
    /// Immutable struct for safe use as dictionary keys and in collections.
    /// </summary>
    public readonly struct GridCoordinate : IEquatable<GridCoordinate>
    {
        /// <summary>
        /// The X coordinate (horizontal axis).
        /// </summary>
        public int X { get; }

        /// <summary>
        /// The Y coordinate (vertical axis - depth in ocean).
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// The Z coordinate (forward axis).
        /// </summary>
        public int Z { get; }

        /// <summary>
        /// The origin coordinate (0, 0, 0).
        /// </summary>
        public static readonly GridCoordinate Zero = new GridCoordinate(0, 0, 0);

        /// <summary>
        /// Creates a new grid coordinate.
        /// </summary>
        /// <param name="x">X coordinate (horizontal)</param>
        /// <param name="y">Y coordinate (vertical/depth)</param>
        /// <param name="z">Z coordinate (forward)</param>
        public GridCoordinate(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Calculates the Manhattan distance between two coordinates.
        /// This represents the minimum number of grid moves to get from a to b.
        /// </summary>
        /// <param name="a">First coordinate</param>
        /// <param name="b">Second coordinate</param>
        /// <returns>Manhattan distance (sum of absolute differences)</returns>
        public static int Distance(GridCoordinate a, GridCoordinate b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z);
        }

        /// <summary>
        /// Gets the 6 orthogonally adjacent neighbors (no diagonals).
        /// In 3D grid: +X, -X, +Y, -Y, +Z, -Z
        /// </summary>
        /// <returns>List of 6 neighboring coordinates</returns>
        public List<GridCoordinate> GetNeighbors()
        {
            return new List<GridCoordinate>
            {
                new GridCoordinate(X + 1, Y, Z),  // +X
                new GridCoordinate(X - 1, Y, Z),  // -X
                new GridCoordinate(X, Y + 1, Z),  // +Y (up)
                new GridCoordinate(X, Y - 1, Z),  // -Y (down)
                new GridCoordinate(X, Y, Z + 1),  // +Z
                new GridCoordinate(X, Y, Z - 1)   // -Z
            };
        }

        /// <summary>
        /// Adds two coordinates together (vector addition).
        /// </summary>
        public static GridCoordinate operator +(GridCoordinate a, GridCoordinate b)
        {
            return new GridCoordinate(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        /// <summary>
        /// Subtracts one coordinate from another (vector subtraction).
        /// </summary>
        public static GridCoordinate operator -(GridCoordinate a, GridCoordinate b)
        {
            return new GridCoordinate(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        /// <summary>
        /// Checks equality between two coordinates.
        /// </summary>
        public static bool operator ==(GridCoordinate left, GridCoordinate right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Checks inequality between two coordinates.
        /// </summary>
        public static bool operator !=(GridCoordinate left, GridCoordinate right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Checks if this coordinate equals another.
        /// </summary>
        public bool Equals(GridCoordinate other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        /// <summary>
        /// Checks if this coordinate equals another object.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is GridCoordinate other && Equals(other);
        }

        /// <summary>
        /// Gets a hash code for this coordinate.
        /// Uses a simple but effective hash combining technique.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + X;
                hash = hash * 31 + Y;
                hash = hash * 31 + Z;
                return hash;
            }
        }

        /// <summary>
        /// Returns a string representation of the coordinate.
        /// </summary>
        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }
    }
}
