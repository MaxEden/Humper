namespace Humper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Base;

    /// <summary>
    /// Basic spacial hashing of world's boxes.
    /// </summary>
    public class Grid
    {
        public class Cell
        {
            public Cell(int x, int y, float cellSize)
            {
                Bounds = new RectangleF(x * cellSize, y * cellSize, cellSize, cellSize);
            }

            public RectangleF Bounds { get; private set; }

            public IEnumerable<IBox> Children => children;

            private List<IBox> children = new List<IBox>();

            public void Add(IBox box)
            {
                children.Add(box);
            }

            public bool Contains(IBox box)
            {
                return children.Contains(box);
            }

            public bool Remove(IBox box)
            {
                return children.Remove(box);
            }

            public int Count()
            {
                return children.Count;
            }
        }

        public Grid(int width, int height, float cellSize)
        {
            Cells = new Cell[width, height];
            CellSize = cellSize;
        }

        public float CellSize { get; set; }

        #region Size

        public float Width => Columns * CellSize;

        public float Height => Rows * CellSize;

        public int Columns => Cells.GetLength(0);

        public int Rows => Cells.GetLength(1);

        #endregion

        public Cell[,] Cells { get; private set; }

        public IEnumerable<Cell> QueryCells(RectangleF area)
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

        public IEnumerable<IBox> QueryBoxes(RectangleF area)
        {
            var cells = QueryCells(area);
            return cells.SelectMany((cell) => cell.Children).Distinct();
        }

        public void Add(IBox box)
        {
            var cells = QueryCells(box.Bounds);

            foreach(var cell in cells)
            {
                if(!cell.Contains(box))
                    cell.Add(box);
            }
        }

        public void Update(IBox box, RectangleF from)
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

        public bool Remove(IBox box)
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