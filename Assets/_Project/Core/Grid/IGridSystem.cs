using System.Collections.Generic;

namespace SubGame.Core.Grid
{
    /// <summary>
    /// Interface for the 3D grid system.
    /// Defines operations for managing a 3D tactical grid.
    /// </summary>
    public interface IGridSystem
    {
        /// <summary>
        /// Gets the width of the grid (X axis).
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the height of the grid (Y axis - depth in ocean).
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the depth of the grid (Z axis).
        /// </summary>
        int Depth { get; }

        /// <summary>
        /// Checks if a coordinate is within the grid bounds.
        /// </summary>
        /// <param name="coord">Coordinate to check</param>
        /// <returns>True if coordinate is valid, false otherwise</returns>
        bool IsValidCoordinate(GridCoordinate coord);

        /// <summary>
        /// Checks if a coordinate is currently occupied by an entity.
        /// </summary>
        /// <param name="coord">Coordinate to check</param>
        /// <returns>True if occupied, false otherwise</returns>
        bool IsOccupied(GridCoordinate coord);

        /// <summary>
        /// Gets the valid neighboring coordinates for a given position.
        /// Only returns coordinates that are within grid bounds.
        /// </summary>
        /// <param name="coord">Center coordinate</param>
        /// <returns>List of valid neighboring coordinates</returns>
        List<GridCoordinate> GetValidNeighbors(GridCoordinate coord);

        /// <summary>
        /// Gets all valid coordinates within a certain range of a position.
        /// Uses Manhattan distance for range calculation.
        /// </summary>
        /// <param name="center">Center coordinate</param>
        /// <param name="range">Maximum Manhattan distance</param>
        /// <returns>List of coordinates within range (including center)</returns>
        List<GridCoordinate> GetCoordinatesInRange(GridCoordinate center, int range);

        /// <summary>
        /// Sets a coordinate as occupied.
        /// </summary>
        /// <param name="coord">Coordinate to mark as occupied</param>
        void SetOccupied(GridCoordinate coord);

        /// <summary>
        /// Clears the occupied status of a coordinate.
        /// </summary>
        /// <param name="coord">Coordinate to clear</param>
        void ClearOccupied(GridCoordinate coord);

        /// <summary>
        /// Gets all coordinates in the grid.
        /// </summary>
        /// <returns>Enumerable of all grid coordinates</returns>
        IEnumerable<GridCoordinate> GetAllCoordinates();
    }
}
