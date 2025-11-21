using System.Linq;
using NUnit.Framework;
using SubGame.Core.Entities;
using SubGame.Core.Grid;

namespace SubGame.Tests.EditMode
{
    /// <summary>
    /// Tests for Pathfinder class.
    /// </summary>
    [TestFixture]
    public class PathfinderTests
    {
        private GridSystem _gridSystem;
        private Pathfinder _pathfinder;

        [SetUp]
        public void Setup()
        {
            _gridSystem = new GridSystem(10, 10, 10);
            _pathfinder = new Pathfinder(_gridSystem);
        }

        #region FindPath Tests

        [Test]
        public void FindPath_AdjacentCells_ReturnsDirectPath()
        {
            var start = new GridCoordinate(0, 0, 0);
            var end = new GridCoordinate(1, 0, 0);

            var path = _pathfinder.FindPath(start, end);

            Assert.AreEqual(2, path.Count);
            Assert.AreEqual(start, path[0]);
            Assert.AreEqual(end, path[1]);
        }

        [Test]
        public void FindPath_SamePosition_ReturnsSingleElementPath()
        {
            var position = new GridCoordinate(5, 5, 5);

            var path = _pathfinder.FindPath(position, position);

            Assert.AreEqual(1, path.Count);
            Assert.AreEqual(position, path[0]);
        }

        [Test]
        public void FindPath_MultipleCells_ReturnsShortestPath()
        {
            var start = new GridCoordinate(0, 0, 0);
            var end = new GridCoordinate(3, 0, 0);

            var path = _pathfinder.FindPath(start, end);

            Assert.AreEqual(4, path.Count); // 0,1,2,3 = 4 cells
            Assert.AreEqual(start, path[0]);
            Assert.AreEqual(end, path[3]);
        }

        [Test]
        public void FindPath_InvalidStart_ReturnsEmptyPath()
        {
            var start = new GridCoordinate(-1, 0, 0);
            var end = new GridCoordinate(5, 0, 0);

            var path = _pathfinder.FindPath(start, end);

            Assert.AreEqual(0, path.Count);
        }

        [Test]
        public void FindPath_InvalidEnd_ReturnsEmptyPath()
        {
            var start = new GridCoordinate(0, 0, 0);
            var end = new GridCoordinate(100, 0, 0);

            var path = _pathfinder.FindPath(start, end);

            Assert.AreEqual(0, path.Count);
        }

        [Test]
        public void FindPath_OccupiedDestination_ReturnsEmptyPath()
        {
            var start = new GridCoordinate(0, 0, 0);
            var end = new GridCoordinate(3, 0, 0);
            _gridSystem.SetOccupied(end);

            var path = _pathfinder.FindPath(start, end);

            Assert.AreEqual(0, path.Count);
        }

        [Test]
        public void FindPath_AroundObstacle_FindsAlternatePath()
        {
            var start = new GridCoordinate(0, 0, 0);
            var end = new GridCoordinate(2, 0, 0);

            // Block direct path
            _gridSystem.SetOccupied(new GridCoordinate(1, 0, 0));

            var path = _pathfinder.FindPath(start, end);

            Assert.Greater(path.Count, 0);
            Assert.AreEqual(start, path[0]);
            Assert.AreEqual(end, path[path.Count - 1]);
            // Path should go around (e.g., via (0,1,0) or (0,0,1))
            Assert.Greater(path.Count, 3);
        }

        [Test]
        public void FindPath_CompletelyBlocked_ReturnsEmptyPath()
        {
            var start = new GridCoordinate(5, 5, 5);
            var end = new GridCoordinate(7, 5, 5);

            // Surround start position
            foreach (var neighbor in start.GetNeighbors())
            {
                if (_gridSystem.IsValidCoordinate(neighbor))
                {
                    _gridSystem.SetOccupied(neighbor);
                }
            }

            var path = _pathfinder.FindPath(start, end);

            Assert.AreEqual(0, path.Count);
        }

        [Test]
        public void FindPath_3DMovement_Works()
        {
            var start = new GridCoordinate(0, 0, 0);
            var end = new GridCoordinate(1, 1, 1);

            var path = _pathfinder.FindPath(start, end);

            Assert.AreEqual(4, path.Count); // Manhattan distance is 3, so 4 cells
            Assert.AreEqual(start, path[0]);
            Assert.AreEqual(end, path[3]);
        }

        #endregion

        #region GetReachablePositions Tests

        [Test]
        public void GetReachablePositions_Range1_Returns6Neighbors()
        {
            var start = new GridCoordinate(5, 5, 5);

            var reachable = _pathfinder.GetReachablePositions(start, 1);

            Assert.AreEqual(6, reachable.Count);
            Assert.IsFalse(reachable.Contains(start)); // Should not include start
        }

        [Test]
        public void GetReachablePositions_Range0_ReturnsEmpty()
        {
            var start = new GridCoordinate(5, 5, 5);

            var reachable = _pathfinder.GetReachablePositions(start, 0);

            Assert.AreEqual(0, reachable.Count);
        }

        [Test]
        public void GetReachablePositions_ExcludesOccupied()
        {
            var start = new GridCoordinate(5, 5, 5);
            var blocked = new GridCoordinate(6, 5, 5);
            _gridSystem.SetOccupied(blocked);

            var reachable = _pathfinder.GetReachablePositions(start, 1);

            Assert.AreEqual(5, reachable.Count);
            Assert.IsFalse(reachable.Contains(blocked));
        }

        [Test]
        public void GetReachablePositions_Range2_IncludesExtendedNeighbors()
        {
            var start = new GridCoordinate(5, 5, 5);

            var reachable = _pathfinder.GetReachablePositions(start, 2);

            // Should include positions 2 steps away
            Assert.That(reachable, Does.Contain(new GridCoordinate(7, 5, 5)));
            Assert.That(reachable, Does.Contain(new GridCoordinate(6, 6, 5)));
        }

        [Test]
        public void GetReachablePositions_BlockedPath_ExcludesUnreachable()
        {
            var start = new GridCoordinate(5, 5, 5);

            // Block all neighbors except one
            _gridSystem.SetOccupied(new GridCoordinate(6, 5, 5));
            _gridSystem.SetOccupied(new GridCoordinate(4, 5, 5));
            _gridSystem.SetOccupied(new GridCoordinate(5, 6, 5));
            _gridSystem.SetOccupied(new GridCoordinate(5, 4, 5));
            _gridSystem.SetOccupied(new GridCoordinate(5, 5, 6));
            // Leave (5, 5, 4) open

            var reachable = _pathfinder.GetReachablePositions(start, 3);

            // Can only reach positions through the one open path
            Assert.That(reachable, Does.Contain(new GridCoordinate(5, 5, 4)));
            Assert.That(reachable, Does.Contain(new GridCoordinate(5, 5, 3)));
            Assert.That(reachable, Does.Contain(new GridCoordinate(5, 5, 2)));
            Assert.IsFalse(reachable.Contains(new GridCoordinate(6, 5, 5)));
        }

        [Test]
        public void GetReachablePositions_AtEdge_RespectsGridBounds()
        {
            var start = new GridCoordinate(0, 0, 0);

            var reachable = _pathfinder.GetReachablePositions(start, 1);

            // Only 3 valid neighbors at corner
            Assert.AreEqual(3, reachable.Count);
        }

        #endregion

        #region PathExists Tests

        [Test]
        public void PathExists_ValidPath_ReturnsTrue()
        {
            var start = new GridCoordinate(0, 0, 0);
            var end = new GridCoordinate(5, 5, 5);

            Assert.IsTrue(_pathfinder.PathExists(start, end));
        }

        [Test]
        public void PathExists_NoPath_ReturnsFalse()
        {
            var start = new GridCoordinate(5, 5, 5);
            var end = new GridCoordinate(7, 5, 5);

            // Surround start
            foreach (var neighbor in start.GetNeighbors())
            {
                if (_gridSystem.IsValidCoordinate(neighbor))
                {
                    _gridSystem.SetOccupied(neighbor);
                }
            }

            Assert.IsFalse(_pathfinder.PathExists(start, end));
        }

        #endregion

        #region GetPathDistance Tests

        [Test]
        public void GetPathDistance_AdjacentCells_Returns1()
        {
            var start = new GridCoordinate(0, 0, 0);
            var end = new GridCoordinate(1, 0, 0);

            Assert.AreEqual(1, _pathfinder.GetPathDistance(start, end));
        }

        [Test]
        public void GetPathDistance_NoPath_ReturnsMinusOne()
        {
            var start = new GridCoordinate(0, 0, 0);
            var end = new GridCoordinate(100, 0, 0); // Invalid

            Assert.AreEqual(-1, _pathfinder.GetPathDistance(start, end));
        }

        [Test]
        public void GetPathDistance_SamePosition_ReturnsZero()
        {
            var position = new GridCoordinate(5, 5, 5);

            Assert.AreEqual(0, _pathfinder.GetPathDistance(position, position));
        }

        #endregion
    }
}
