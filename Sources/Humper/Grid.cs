using System.Collections;

namespace Humper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Base;

    /// <summary>
    /// Basic spacial hashing of world's boxes.
    /// </summary>
    public class Grid: IBroadPhase
    {
        public class Cell
        {
            public Cell(int x, int y, float cellSize)
            {
                Bounds = new Rect(x * cellSize, y * cellSize, cellSize, cellSize);
            }

            public Rect Bounds { get; private set; }

            public IEnumerable<Box> Children => children;

            private HashSet<Box> children = new HashSet<Box>();

            public void Add(Box box)
            {
                children.Add(box);
            }

            public bool Contains(Box box)
            {
                return children.Contains(box);
            }

            public bool Remove(Box box)
            {
                return children.Remove(box);
            }

            public int Count()
            {
                return children.Count;
            }
        }

        public Grid(float width, float height, float cellSize = 64)
        {
            Cells = new Cell[(int)(width/cellSize)+1, (int)(height/cellSize)+1];
            CellSize = cellSize;
        }

        private float CellSize { get; set; }

        #region Size

        public float Width => Columns * CellSize;

        public float Height => Rows * CellSize;

        public int Columns => Cells.GetLength(0);

        public int Rows => Cells.GetLength(1);

        #endregion

        private Cell[,] Cells { get; set; }

        public IEnumerable<Cell> QueryCells(Rect area)
        {
            var minX = (int)(area.Left / CellSize);
            var minY = (int)(area.Top / CellSize);
            var maxX = (int)(area.Right / CellSize) + 1;
            var maxY = (int)(area.Bottom / CellSize) + 1;

            minX = Math.Max(0, minX);
            minY = Math.Max(0, minY);
            maxX = Math.Min(Columns - 1, maxX);
            maxY = Math.Min(Rows - 1, maxY);

            List<Cell> result = new List<Cell>();

            for(int x = minX; x <= maxX; x++)
            {
                for(int y = minY; y <= maxY; y++)
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
        public void DrawDebug(Rect area, Action<int, int, int, int, float> drawCell, Action<Box> drawBox, Action<string, int, int, float> drawString)
        {
            // Drawing cells
            var cells = QueryCells(area);
            foreach (var cell in cells)
            {
                var count = cell.Count();
                var alpha = count > 0 ? 1f : 0.4f;
                drawCell((int)cell.Bounds.X, (int)cell.Bounds.Y, (int)cell.Bounds.Width, (int)cell.Bounds.Height, alpha);
                drawString(count.ToString(), (int)cell.Bounds.Center.X, (int)cell.Bounds.Center.Y,alpha);
            }
        }

        public IEnumerable<Box> QueryBoxes(Rect area)
        {
            var cells = QueryCells(area);
            return cells.SelectMany((cell) => cell.Children).Distinct();
        }
        public Rect Bounds => new Rect(0,0,Width*CellSize,Height*CellSize);

        public void Add(Box box)
        {
            var cells = QueryCells(box.Bounds);

            foreach(var cell in cells)
            {
                if(!cell.Contains(box))
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
                Add(box);
        }

        public bool Remove(Box box)
        {
            var cells = this.QueryCells(box.Bounds);

            var removed = false;
            foreach(var cell in cells)
            {
                removed |= cell.Remove(box);
            }

            return removed;
        }

        public override string ToString()
        {
            return string.Format("[Grid: Width={0}, Height={1}, Columns={2}, Rows={3}]", Width, Height, Columns, Rows);
        }
    }
}