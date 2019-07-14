using System;
using System.Collections.Generic;
using System.Linq;
using Mandarin.Common.Collections.Extensions;
using Mandarin.Common.Misc;

namespace Humper
{
    /// <summary>
    ///     Basic spacial hashing of world's boxes.
    /// </summary>
    public class Grid : IBroadPhase
    {
        private float CellSize { get; }

        private Cell[,] Cells { get; }

        public Grid(float width, float height, float cellSize = 64)
        {
            Cells = new Cell[(int)(width / cellSize) + 1, (int)(height / cellSize) + 1];
            CellSize = cellSize;
        }
        public void DrawDebug(Rect area, Action<Rect, float> drawCell, Action<Box> drawBox, Action<string, int, int, float> drawString)
        {
            // Drawing cells
            var cells = QueryCells(area);
            foreach(var cell in cells)
            {
                var count = cell.Count();
                var alpha = count > 0 ? 1f : 0.4f;
                drawCell(cell.Bounds, alpha);
                drawString(count.ToString(), (int)cell.Bounds.Center.X, (int)cell.Bounds.Center.Y, alpha);
            }
        }

        public void QueryBoxes(Rect area, ISet<Box> boxes)
        {
            var cells = QueryCells(area);
            cells.ForEach(p => boxes.AddRange(p.QueryBoxes(area)));
        }
        public Rect Bounds => new Rect(0, 0, Cells.GetLength(0) * CellSize, Cells.GetLength(1) * CellSize);

        public void Add(Box box)
        {
            var cells = QueryCells(box.Bounds);
            foreach(var cell in cells)
            {
                cell.Add(box);
            }
        }

        public void Update(Box box, Rect from)
        {
            var fromCells = QueryCells(from);
            var removed = false;
            foreach(var cell in fromCells)
            {
                removed |= cell.Remove(box);
            }

            if(removed)
            {
                Add(box);
            }
        }

        public bool Remove(Box box)
        {
            var cells = QueryCells(box.Bounds);

            var removed = false;
            foreach(var cell in cells)
            {
                removed |= cell.Remove(box);
            }

            return removed;
        }

        public List<Cell> QueryCells(Rect area)
        {
            var minX = (int)(area.Left / CellSize);
            var minY = (int)(area.Bottom / CellSize);
            var maxX = (int)(area.Right / CellSize);
            var maxY = (int)(area.Top / CellSize);

            minX = Math.Max(0, minX);
            minY = Math.Max(0, minY);
            maxX = Math.Min(Columns - 1, maxX);
            maxY = Math.Min(Rows - 1, maxY);

            var result = new List<Cell>();

            for(var x = minX; x <= maxX; x++)
            {
                for(var y = minY; y <= maxY; y++)
                {
                    var cell = Cells[x, y];

                    if(cell == null)
                    {
                        cell = new Cell(x, y, CellSize);
                        Cells[x, y] = cell;
                    }

                    result.Add(cell);
                }
            }

            return result;

        }

        public override string ToString()
        {
            return string.Format("[Grid: Width={0}, Height={1}, Columns={2}, Rows={3}]", Width, Height, Columns, Rows);
        }
        public class Cell
        {
            private readonly List<Box> _children = new List<Box>();

            public Rect Bounds { get; }

            public Cell(int x, int y, float cellSize)
            {
                Bounds = new Rect(x * cellSize, y * cellSize, cellSize, cellSize);
            }

            public void Add(Box box)
            {
                _children.Add(box);
            }

            public bool Remove(Box box)
            {
                var result = _children.Remove(box);
                return result;
            }

            public int Count()
            {
                return _children.Count;
            }
            public IEnumerable<Box> QueryBoxes(Rect area)
            {
                return _children.Where(p => p.Bounds.Overlaps(area));
            }
        }

        #region Size

        public float Width => Columns * CellSize;

        public float Height => Rows * CellSize;

        public int Columns => Cells.GetLength(0);

        public int Rows => Cells.GetLength(1);

        #endregion
    }
}