using System;
using System.Linq;
using NUnit.Framework;
using SubGame.Core.Grid;

namespace SubGame.Tests.EditMode
{
    [TestFixture]
    public class GridSystemTests
    {
        private GridSystem _grid;

        [SetUp]
        public void SetUp()
        {
            _grid = new GridSystem(10, 10, 10);
        }

        #region Constructor Tests

        [Test]
        public void Constructor_SetsPropertiesCorrectly()
        {
            var grid = new GridSystem(5, 10, 15);

            Assert.AreEqual(5, grid.Width);
            Assert.AreEqual(10, grid.Height);
            Assert.AreEqual(15, grid.Depth);
        }

        [Test]
        public void Constructor_ZeroWidth_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new GridSystem(0, 10, 10));
        }

        [Test]
        public void Constructor_NegativeHeight_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new GridSystem(10, -1, 10));
        }

        [Test]
        public void Constructor_NegativeDepth_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new GridSystem(10, 10, -5));
        }

        #endregion

        #region IsValidCoordinate Tests

        [Test]
        public void IsValidCoordinate_WithinBounds_ReturnsTrue()
        {
            var coord = new GridCoordinate(5, 5, 5);

            Assert.IsTrue(_grid.IsValidCoordinate(coord));
        }

        [Test]
        public void IsValidCoordinate_Origin_ReturnsTrue()
        {
            Assert.IsTrue(_grid.IsValidCoordinate(GridCoordinate.Zero));
        }

        [Test]
        public void IsValidCoordinate_MaxBounds_ReturnsTrue()
        {
            var coord = new GridCoordinate(9, 9, 9); // 10x10x10 grid, max index is 9

            Assert.IsTrue(_grid.IsValidCoordinate(coord));
        }

        [Test]
        public void IsValidCoordinate_NegativeX_ReturnsFalse()
        {
            var coord = new GridCoordinate(-1, 5, 5);

            Assert.IsFalse(_grid.IsValidCoordinate(coord));
        }

        [Test]
        public void IsValidCoordinate_NegativeY_ReturnsFalse()
        {
            var coord = new GridCoordinate(5, -1, 5);

            Assert.IsFalse(_grid.IsValidCoordinate(coord));
        }

        [Test]
        public void IsValidCoordinate_NegativeZ_ReturnsFalse()
        {
            var coord = new GridCoordinate(5, 5, -1);

            Assert.IsFalse(_grid.IsValidCoordinate(coord));
        }

        [Test]
        public void IsValidCoordinate_ExceedsWidth_ReturnsFalse()
        {
            var coord = new GridCoordinate(10, 5, 5); // Width is 10, so 10 is out of bounds

            Assert.IsFalse(_grid.IsValidCoordinate(coord));
        }

        [Test]
        public void IsValidCoordinate_ExceedsHeight_ReturnsFalse()
        {
            var coord = new GridCoordinate(5, 10, 5);

            Assert.IsFalse(_grid.IsValidCoordinate(coord));
        }

        [Test]
        public void IsValidCoordinate_ExceedsDepth_ReturnsFalse()
        {
            var coord = new GridCoordinate(5, 5, 10);

            Assert.IsFalse(_grid.IsValidCoordinate(coord));
        }

        #endregion

        #region Occupancy Tests

        [Test]
        public void IsOccupied_EmptyGrid_ReturnsFalse()
        {
            var coord = new GridCoordinate(5, 5, 5);

            Assert.IsFalse(_grid.IsOccupied(coord));
        }

        [Test]
        public void SetOccupied_MarksCoordinateAsOccupied()
        {
            var coord = new GridCoordinate(5, 5, 5);

            _grid.SetOccupied(coord);

            Assert.IsTrue(_grid.IsOccupied(coord));
        }

        [Test]
        public void SetOccupied_InvalidCoordinate_ThrowsArgumentException()
        {
            var coord = new GridCoordinate(-1, 0, 0);

            Assert.Throws<ArgumentException>(() => _grid.SetOccupied(coord));
        }

        [Test]
        public void ClearOccupied_RemovesOccupancy()
        {
            var coord = new GridCoordinate(5, 5, 5);
            _grid.SetOccupied(coord);

            _grid.ClearOccupied(coord);

            Assert.IsFalse(_grid.IsOccupied(coord));
        }

        [Test]
        public void ClearOccupied_UnoccupiedCoordinate_DoesNotThrow()
        {
            var coord = new GridCoordinate(5, 5, 5);

            Assert.DoesNotThrow(() => _grid.ClearOccupied(coord));
        }

        [Test]
        public void OccupiedCount_ReturnsCorrectCount()
        {
            _grid.SetOccupied(new GridCoordinate(0, 0, 0));
            _grid.SetOccupied(new GridCoordinate(1, 1, 1));
            _grid.SetOccupied(new GridCoordinate(2, 2, 2));

            Assert.AreEqual(3, _grid.OccupiedCount);
        }

        [Test]
        public void ClearAllOccupancy_RemovesAllOccupancy()
        {
            _grid.SetOccupied(new GridCoordinate(0, 0, 0));
            _grid.SetOccupied(new GridCoordinate(1, 1, 1));
            _grid.SetOccupied(new GridCoordinate(2, 2, 2));

            _grid.ClearAllOccupancy();

            Assert.AreEqual(0, _grid.OccupiedCount);
        }

        #endregion

        #region GetValidNeighbors Tests

        [Test]
        public void GetValidNeighbors_CenterOfGrid_ReturnsSixNeighbors()
        {
            var coord = new GridCoordinate(5, 5, 5);

            var neighbors = _grid.GetValidNeighbors(coord);

            Assert.AreEqual(6, neighbors.Count);
        }

        [Test]
        public void GetValidNeighbors_Corner_ReturnsThreeNeighbors()
        {
            var coord = GridCoordinate.Zero;

            var neighbors = _grid.GetValidNeighbors(coord);

            Assert.AreEqual(3, neighbors.Count);
        }

        [Test]
        public void GetValidNeighbors_Edge_ReturnsFourNeighbors()
        {
            var coord = new GridCoordinate(0, 0, 5); // On edge, not corner

            var neighbors = _grid.GetValidNeighbors(coord);

            Assert.AreEqual(4, neighbors.Count);
        }

        [Test]
        public void GetValidNeighbors_Face_ReturnsFiveNeighbors()
        {
            var coord = new GridCoordinate(0, 5, 5); // On face, not edge or corner

            var neighbors = _grid.GetValidNeighbors(coord);

            Assert.AreEqual(5, neighbors.Count);
        }

        #endregion

        #region GetCoordinatesInRange Tests

        [Test]
        public void GetCoordinatesInRange_RangeZero_ReturnsOnlyCenter()
        {
            var center = new GridCoordinate(5, 5, 5);

            var coords = _grid.GetCoordinatesInRange(center, 0);

            Assert.AreEqual(1, coords.Count);
            Assert.Contains(center, coords);
        }

        [Test]
        public void GetCoordinatesInRange_RangeOne_ReturnsSevenCoordinates()
        {
            var center = new GridCoordinate(5, 5, 5);

            var coords = _grid.GetCoordinatesInRange(center, 1);

            Assert.AreEqual(7, coords.Count); // Center + 6 neighbors
        }

        [Test]
        public void GetCoordinatesInRange_NegativeRange_ThrowsArgumentException()
        {
            var center = new GridCoordinate(5, 5, 5);

            Assert.Throws<ArgumentException>(() => _grid.GetCoordinatesInRange(center, -1));
        }

        [Test]
        public void GetCoordinatesInRange_NearEdge_ExcludesInvalidCoordinates()
        {
            var center = new GridCoordinate(0, 0, 0);

            var coords = _grid.GetCoordinatesInRange(center, 1);

            // At corner with range 1: center + 3 valid neighbors = 4
            Assert.AreEqual(4, coords.Count);
            Assert.IsTrue(coords.All(c => _grid.IsValidCoordinate(c)));
        }

        #endregion

        #region GetAllCoordinates Tests

        [Test]
        public void GetAllCoordinates_ReturnsCorrectCount()
        {
            var grid = new GridSystem(3, 4, 5);

            var coords = grid.GetAllCoordinates().ToList();

            Assert.AreEqual(60, coords.Count); // 3 * 4 * 5 = 60
        }

        [Test]
        public void TotalCells_ReturnsCorrectValue()
        {
            var grid = new GridSystem(3, 4, 5);

            Assert.AreEqual(60, grid.TotalCells);
        }

        [Test]
        public void GetAllCoordinates_AllCoordinatesAreValid()
        {
            var coords = _grid.GetAllCoordinates();

            Assert.IsTrue(coords.All(c => _grid.IsValidCoordinate(c)));
        }

        #endregion
    }
}
