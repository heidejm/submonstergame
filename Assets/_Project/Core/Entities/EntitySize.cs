using System;
using System.Collections.Generic;
using SubGame.Core.Grid;

namespace SubGame.Core.Entities
{
    /// <summary>
    /// Represents the size of an entity in grid cells.
    /// Position refers to the "anchor" cell (typically bottom-front-left corner).
    /// </summary>
    public readonly struct EntitySize : IEquatable<EntitySize>
    {
        /// <summary>
        /// Width in grid cells (X axis).
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height in grid cells (Y axis - vertical).
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Depth in grid cells (Z axis).
        /// </summary>
        public int Depth { get; }

        /// <summary>
        /// A 1x1x1 entity (single cell).
        /// </summary>
        public static readonly EntitySize One = new EntitySize(1, 1, 1);

        /// <summary>
        /// Creates a new entity size.
        /// </summary>
        /// <param name="width">Width in cells (X axis, minimum 1)</param>
        /// <param name="height">Height in cells (Y axis, minimum 1)</param>
        /// <param name="depth">Depth in cells (Z axis, minimum 1)</param>
        public EntitySize(int width, int height, int depth)
        {
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            Depth = Math.Max(1, depth);
        }

        /// <summary>
        /// Creates a uniform cubic size.
        /// </summary>
        /// <param name="size">Size in all dimensions</param>
        public EntitySize(int size) : this(size, size, size)
        {
        }

        /// <summary>
        /// Gets the total number of cells this entity occupies.
        /// </summary>
        public int TotalCells => Width * Height * Depth;

        /// <summary>
        /// Returns true if this is a single-cell entity (1x1x1).
        /// </summary>
        public bool IsSingleCell => Width == 1 && Height == 1 && Depth == 1;

        /// <summary>
        /// Gets all grid coordinates this entity would occupy given an anchor position.
        /// The anchor is the minimum corner (lowest X, Y, Z values).
        /// </summary>
        /// <param name="anchor">The anchor position (bottom-front-left corner)</param>
        /// <returns>All coordinates occupied by the entity</returns>
        public IEnumerable<GridCoordinate> GetOccupiedCells(GridCoordinate anchor)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        yield return new GridCoordinate(anchor.X + x, anchor.Y + y, anchor.Z + z);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the center offset from the anchor position.
        /// Useful for positioning visuals.
        /// </summary>
        public (float x, float y, float z) GetCenterOffset()
        {
            return (
                (Width - 1) / 2f,
                (Height - 1) / 2f,
                (Depth - 1) / 2f
            );
        }

        public bool Equals(EntitySize other)
        {
            return Width == other.Width && Height == other.Height && Depth == other.Depth;
        }

        public override bool Equals(object obj)
        {
            return obj is EntitySize other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Width;
                hash = hash * 31 + Height;
                hash = hash * 31 + Depth;
                return hash;
            }
        }

        public static bool operator ==(EntitySize left, EntitySize right) => left.Equals(right);
        public static bool operator !=(EntitySize left, EntitySize right) => !left.Equals(right);

        public override string ToString() => $"{Width}x{Height}x{Depth}";
    }
}
