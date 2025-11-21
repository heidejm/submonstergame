using System;
using System.Collections.Generic;

namespace SubGame.Core.Grid
{
    /// <summary>
    /// Implementation of a 3D grid system for tactical combat.
    /// Manages grid bounds, occupancy, and spatial queries.
    /// </summary>
    public class GridSystem : IGridSystem
    {
        private readonly HashSet<GridCoordinate> _occupiedCells;

        /// <inheritdoc/>
        public int Width { get; }

        /// <inheritdoc/>
        public int Height { get; }

        /// <inheritdoc/>
        public int Depth { get; }

        /// <summary>
        /// Creates a new 3D grid system with the specified dimensions.
        /// </summary>
        /// <param name="width">Width of the grid (X axis, must be positive)</param>
        /// <param name="height">Height of the grid (Y axis, must be positive)</param>
        /// <param name="depth">Depth of the grid (Z axis, must be positive)</param>
        /// <exception cref="ArgumentException">Thrown if any dimension is not positive</exception>
        public GridSystem(int width, int height, int depth)
        {
            if (width <= 0)
                throw new ArgumentException("Width must be positive", nameof(width));
            if (height <= 0)
                throw new ArgumentException("Height must be positive", nameof(height));
            if (depth <= 0)
                throw new ArgumentException("Depth must be positive", nameof(depth));

            Width = width;
            Height = height;
            Depth = depth;
            _occupiedCells = new HashSet<GridCoordinate>();
        }

        /// <inheritdoc/>
        public bool IsValidCoordinate(GridCoordinate coord)
        {
            return coord.X >= 0 && coord.X < Width &&
                   coord.Y >= 0 && coord.Y < Height &&
                   coord.Z >= 0 && coord.Z < Depth;
        }

        /// <inheritdoc/>
        public bool IsOccupied(GridCoordinate coord)
        {
            return _occupiedCells.Contains(coord);
        }

        /// <inheritdoc/>
        public List<GridCoordinate> GetValidNeighbors(GridCoordinate coord)
        {
            var neighbors = coord.GetNeighbors();
            var validNeighbors = new List<GridCoordinate>();

            foreach (var neighbor in neighbors)
            {
                if (IsValidCoordinate(neighbor))
                {
                    validNeighbors.Add(neighbor);
                }
            }

            return validNeighbors;
        }

        /// <inheritdoc/>
        public List<GridCoordinate> GetCoordinatesInRange(GridCoordinate center, int range)
        {
            if (range < 0)
                throw new ArgumentException("Range must be non-negative", nameof(range));

            var result = new List<GridCoordinate>();

            // Iterate through a bounding box and filter by Manhattan distance
            int minX = Math.Max(0, center.X - range);
            int maxX = Math.Min(Width - 1, center.X + range);
            int minY = Math.Max(0, center.Y - range);
            int maxY = Math.Min(Height - 1, center.Y + range);
            int minZ = Math.Max(0, center.Z - range);
            int maxZ = Math.Min(Depth - 1, center.Z + range);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        var coord = new GridCoordinate(x, y, z);
                        if (GridCoordinate.Distance(center, coord) <= range)
                        {
                            result.Add(coord);
                        }
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public void SetOccupied(GridCoordinate coord)
        {
            if (!IsValidCoordinate(coord))
                throw new ArgumentException($"Coordinate {coord} is outside grid bounds", nameof(coord));

            _occupiedCells.Add(coord);
        }

        /// <inheritdoc/>
        public void ClearOccupied(GridCoordinate coord)
        {
            _occupiedCells.Remove(coord);
        }

        /// <inheritdoc/>
        public IEnumerable<GridCoordinate> GetAllCoordinates()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        yield return new GridCoordinate(x, y, z);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the total number of cells in the grid.
        /// </summary>
        public int TotalCells => Width * Height * Depth;

        /// <summary>
        /// Gets the number of currently occupied cells.
        /// </summary>
        public int OccupiedCount => _occupiedCells.Count;

        /// <summary>
        /// Clears all occupancy data from the grid.
        /// </summary>
        public void ClearAllOccupancy()
        {
            _occupiedCells.Clear();
        }
    }
}
