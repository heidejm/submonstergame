using NUnit.Framework;
using SubGame.Core.Grid;

namespace SubGame.Tests.EditMode
{
    [TestFixture]
    public class GridCoordinateTests
    {
        [Test]
        public void Constructor_SetsPropertiesCorrectly()
        {
            var coord = new GridCoordinate(1, 2, 3);

            Assert.AreEqual(1, coord.X);
            Assert.AreEqual(2, coord.Y);
            Assert.AreEqual(3, coord.Z);
        }

        [Test]
        public void Zero_ReturnsOriginCoordinate()
        {
            var zero = GridCoordinate.Zero;

            Assert.AreEqual(0, zero.X);
            Assert.AreEqual(0, zero.Y);
            Assert.AreEqual(0, zero.Z);
        }

        [Test]
        public void Distance_SameCoordinate_ReturnsZero()
        {
            var coord = new GridCoordinate(5, 5, 5);

            int distance = GridCoordinate.Distance(coord, coord);

            Assert.AreEqual(0, distance);
        }

        [Test]
        public void Distance_AdjacentCoordinates_ReturnsOne()
        {
            var a = new GridCoordinate(0, 0, 0);
            var b = new GridCoordinate(1, 0, 0);

            int distance = GridCoordinate.Distance(a, b);

            Assert.AreEqual(1, distance);
        }

        [Test]
        public void Distance_CalculatesManhattanDistanceCorrectly()
        {
            var a = new GridCoordinate(0, 0, 0);
            var b = new GridCoordinate(3, 4, 5);

            int distance = GridCoordinate.Distance(a, b);

            Assert.AreEqual(12, distance); // 3 + 4 + 5 = 12
        }

        [Test]
        public void Distance_NegativeCoordinates_CalculatesCorrectly()
        {
            var a = new GridCoordinate(-1, -2, -3);
            var b = new GridCoordinate(1, 2, 3);

            int distance = GridCoordinate.Distance(a, b);

            Assert.AreEqual(12, distance); // |1-(-1)| + |2-(-2)| + |3-(-3)| = 2+4+6 = 12
        }

        [Test]
        public void GetNeighbors_ReturnsSixNeighbors()
        {
            var coord = new GridCoordinate(5, 5, 5);

            var neighbors = coord.GetNeighbors();

            Assert.AreEqual(6, neighbors.Count);
        }

        [Test]
        public void GetNeighbors_ContainsCorrectNeighbors()
        {
            var coord = new GridCoordinate(5, 5, 5);

            var neighbors = coord.GetNeighbors();

            Assert.Contains(new GridCoordinate(6, 5, 5), neighbors); // +X
            Assert.Contains(new GridCoordinate(4, 5, 5), neighbors); // -X
            Assert.Contains(new GridCoordinate(5, 6, 5), neighbors); // +Y
            Assert.Contains(new GridCoordinate(5, 4, 5), neighbors); // -Y
            Assert.Contains(new GridCoordinate(5, 5, 6), neighbors); // +Z
            Assert.Contains(new GridCoordinate(5, 5, 4), neighbors); // -Z
        }

        [Test]
        public void Equals_SameCoordinates_ReturnsTrue()
        {
            var a = new GridCoordinate(1, 2, 3);
            var b = new GridCoordinate(1, 2, 3);

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a == b);
        }

        [Test]
        public void Equals_DifferentCoordinates_ReturnsFalse()
        {
            var a = new GridCoordinate(1, 2, 3);
            var b = new GridCoordinate(1, 2, 4);

            Assert.IsFalse(a.Equals(b));
            Assert.IsTrue(a != b);
        }

        [Test]
        public void GetHashCode_SameCoordinates_ReturnsSameHash()
        {
            var a = new GridCoordinate(1, 2, 3);
            var b = new GridCoordinate(1, 2, 3);

            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Addition_AddsCoordinatesCorrectly()
        {
            var a = new GridCoordinate(1, 2, 3);
            var b = new GridCoordinate(4, 5, 6);

            var result = a + b;

            Assert.AreEqual(new GridCoordinate(5, 7, 9), result);
        }

        [Test]
        public void Subtraction_SubtractsCoordinatesCorrectly()
        {
            var a = new GridCoordinate(5, 7, 9);
            var b = new GridCoordinate(1, 2, 3);

            var result = a - b;

            Assert.AreEqual(new GridCoordinate(4, 5, 6), result);
        }

        [Test]
        public void ToString_ReturnsFormattedString()
        {
            var coord = new GridCoordinate(1, 2, 3);

            string result = coord.ToString();

            Assert.AreEqual("(1, 2, 3)", result);
        }
    }
}
