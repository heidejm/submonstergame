using System.Collections.Generic;

namespace SubGame.Core.Grid
{
    /// <summary>
    /// Provides pathfinding functionality for 3D grids using BFS.
    /// BFS guarantees shortest path in unweighted grids.
    /// </summary>
    public class Pathfinder
    {
        private readonly IGridSystem _gridSystem;

        /// <summary>
        /// Creates a new pathfinder for the given grid system.
        /// </summary>
        /// <param name="gridSystem">The grid system to pathfind on</param>
        public Pathfinder(IGridSystem gridSystem)
        {
            _gridSystem = gridSystem;
        }

        /// <summary>
        /// Finds the shortest path from start to end using BFS.
        /// </summary>
        /// <param name="start">Starting position</param>
        /// <param name="end">Target position</param>
        /// <returns>List of coordinates forming the path (including start and end), or empty if no path exists</returns>
        public IReadOnlyList<GridCoordinate> FindPath(GridCoordinate start, GridCoordinate end)
        {
            // If start or end is invalid, no path exists
            if (!_gridSystem.IsValidCoordinate(start) || !_gridSystem.IsValidCoordinate(end))
            {
                return new List<GridCoordinate>();
            }

            // If start equals end, return single-element path
            if (start.Equals(end))
            {
                return new List<GridCoordinate> { start };
            }

            // If end is occupied, no path exists (can't move there)
            if (_gridSystem.IsOccupied(end))
            {
                return new List<GridCoordinate>();
            }

            // BFS to find shortest path
            var queue = new Queue<GridCoordinate>();
            var cameFrom = new Dictionary<GridCoordinate, GridCoordinate>();
            var visited = new HashSet<GridCoordinate>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current.Equals(end))
                {
                    // Reconstruct path
                    return ReconstructPath(cameFrom, start, end);
                }

                foreach (var neighbor in _gridSystem.GetValidNeighbors(current))
                {
                    // Skip if already visited
                    if (visited.Contains(neighbor))
                    {
                        continue;
                    }

                    // Skip if occupied (unless it's the destination)
                    if (_gridSystem.IsOccupied(neighbor) && !neighbor.Equals(end))
                    {
                        continue;
                    }

                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }

            // No path found
            return new List<GridCoordinate>();
        }

        /// <summary>
        /// Gets all positions reachable from start within the given range.
        /// Uses BFS to find all positions within Manhattan distance.
        /// </summary>
        /// <param name="start">Starting position</param>
        /// <param name="range">Maximum movement range</param>
        /// <returns>Set of all reachable positions (excluding start)</returns>
        public IReadOnlyCollection<GridCoordinate> GetReachablePositions(GridCoordinate start, int range)
        {
            var reachable = new HashSet<GridCoordinate>();

            if (range <= 0 || !_gridSystem.IsValidCoordinate(start))
            {
                return reachable;
            }

            // BFS with distance tracking
            var queue = new Queue<(GridCoordinate pos, int dist)>();
            var visited = new HashSet<GridCoordinate>();

            queue.Enqueue((start, 0));
            visited.Add(start);

            while (queue.Count > 0)
            {
                var (current, distance) = queue.Dequeue();

                // Add to reachable if not the start and not occupied
                if (!current.Equals(start) && !_gridSystem.IsOccupied(current))
                {
                    reachable.Add(current);
                }

                // Don't explore further if at max range
                if (distance >= range)
                {
                    continue;
                }

                foreach (var neighbor in _gridSystem.GetValidNeighbors(current))
                {
                    if (visited.Contains(neighbor))
                    {
                        continue;
                    }

                    // Can move through empty cells
                    if (!_gridSystem.IsOccupied(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, distance + 1));
                    }
                }
            }

            return reachable;
        }

        /// <summary>
        /// Checks if a path exists from start to end.
        /// </summary>
        public bool PathExists(GridCoordinate start, GridCoordinate end)
        {
            return FindPath(start, end).Count > 0;
        }

        /// <summary>
        /// Gets the distance of the shortest path from start to end.
        /// </summary>
        /// <returns>Path length, or -1 if no path exists</returns>
        public int GetPathDistance(GridCoordinate start, GridCoordinate end)
        {
            var path = FindPath(start, end);
            return path.Count > 0 ? path.Count - 1 : -1;
        }

        /// <summary>
        /// Reconstructs the path from start to end using the cameFrom map.
        /// </summary>
        private static IReadOnlyList<GridCoordinate> ReconstructPath(
            Dictionary<GridCoordinate, GridCoordinate> cameFrom,
            GridCoordinate start,
            GridCoordinate end)
        {
            var path = new List<GridCoordinate>();
            var current = end;

            while (!current.Equals(start))
            {
                path.Add(current);
                current = cameFrom[current];
            }

            path.Add(start);
            path.Reverse();

            return path;
        }
    }
}
