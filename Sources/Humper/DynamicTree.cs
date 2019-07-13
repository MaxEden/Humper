/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Humper.Base;
using Mandarin.Common.Misc;

namespace Humper
{
    /// <summary>
    ///     A dynamic tree arranges data in a binary tree to accelerate
    ///     queries such as volume queries and ray casts. Leafs are proxies
    ///     with an AABB. In the tree we expand the proxy AABB by Settings.b2_fatAABBFactor
    ///     so that the proxy AABB is bigger than the client object. This allows the client
    ///     object to move by small amounts without triggering a tree update.
    ///     Nodes are pooled and relocatable, so we use node indices rather than pointers.
    /// </summary>
    public partial class DynamicTree : IBroadPhase
    {
        public Vector2 RectExtension                    = Vector2.One * 4;
        public Vector2 DisplacementPredictionMultiplier = Vector2.One * 10;

        private readonly List<Node>  _nodes        = new List<Node>();
        private readonly Stack<Node> _raycastStack = new Stack<Node>();
        private          Node        _root;
        private          int         _uk = 1;

        /// <summary>
        ///     Compute the height of the binary tree in O(N) time. Should not be called often.
        /// </summary>
        public int Height
        {
            get
            {
                if(_root == null)
                {
                    return 0;
                }

                return _root.Height;
            }
        }

        /// <summary>
        ///     Get the ratio of the sum of the node areas to the root area.
        /// </summary>
        public float AreaRatio
        {
            get
            {
                if(_root == null)
                {
                    return 0.0f;
                }

                var rootArea = _root.Rect.Perimeter;

                var totalArea = 0.0f;
                foreach(var node in _nodes)
                {
                    if(node.Height < 0)
                    {
                        // Free node in pool
                        continue;
                    }
                    totalArea += node.Rect.Perimeter;
                }

                return totalArea / rootArea;
            }
        }

        /// <summary>
        ///     Get the maximum balance of an node in the tree. The balance is the difference
        ///     in height of the two children of a node.
        /// </summary>
        public int MaxBalance
        {
            get
            {
                var maxBalance = 0;
                foreach(var node in _nodes)
                {
                    if(node.Height <= 1)
                    {
                        continue;
                    }

                    Debug.Assert(node.IsLeaf == false);

                    var child1 = node.Child_1;
                    var child2 = node.Child_2;
                    var balance = Math.Abs(child2.Height - child1.Height);
                    maxBalance = Math.Max(maxBalance, balance);
                }

                return maxBalance;
            }
        }

        public void Add(Box box)
        {
            var node = AllocateNode();

            node.Box = box;
            node.Tree = this;
            node.SetRect(box.IsActive ? box.Bounds.Expand(RectExtension) : box.Bounds);

            InsertLeaf(node);
        }
        public IList<Box> QueryBoxes(Rect area)
        {
            var result = new List<Box>();
            QueryBoxes(_root, area, result);
            return result;
        }
        private void QueryBoxes(Node node, Rect rect, IList<Box> boxes)
        {
            if(node == null) return;
            if(!node.Rect.Overlaps(rect)) return;

            if(node.IsLeaf)
            {
                boxes.Add(node.Box);
                return;
            }

            QueryBoxes(node.Child_1, rect, boxes);
            QueryBoxes(node.Child_2, rect, boxes);
        }
        Rect IBroadPhase.Bounds => _root?.Rect ?? Rect.Empty;
        public bool Remove(Box box)
        {
            return RemoveProxy(GetProxy(box));
        }
        public void Update(Box box, Rect from)
        {
            MoveProxy(GetProxy(box), from, box.Bounds.Position - from.Position);
        }
        void IBroadPhase.DrawDebug(Rect area, Action<Rect, float> drawCell, Action<Box> drawBox, Action<string, int, int, float> drawString)
        {
            drawCell(_root.Rect, 1);
            foreach(var node in QueryNodes(area))
            {
                drawCell(node.Rect, 0.5f);
            }
        }

        /// <summary>
        ///     Destroy a proxy. This asserts if the id is invalid.
        /// </summary>
        private bool RemoveProxy(Node node)
        {
            Debug.Assert(node.IsLeaf);
            Debug.Assert(node.Tree == this);

            RemoveLeaf(node);
            return FreeNode(node);
        }

        /// <summary>
        ///     Move a proxy with a swepted AABB. If the proxy has moved outside of its fattened AABB,
        ///     then the proxy is removed from the tree and re-inserted. Otherwise
        ///     the function returns immediately.
        /// </summary>
        /// <param name="node">The proxy id.</param>
        /// <param name="rect">The AABB.</param>
        /// <param name="displacement">The displacement.</param>
        /// <returns>true if the proxy was re-inserted.</returns>
        private bool MoveProxy(Node node, Rect rect, Vector2 displacement)
        {
            Debug.Assert(node.IsLeaf);

            if(node.Rect.Contains(rect))
            {
                return false;
            }

            RemoveLeaf(node);


            var newRect = rect;

            if(node.Box.IsActive)
            {
                newRect = newRect
                    .Offset(DisplacementPredictionMultiplier * displacement)
                    .Expand(RectExtension);

                newRect = Rect.Union(rect, newRect);
            }

            node.SetRect(newRect);

            InsertLeaf(node);
            return true;
        }

        public IEnumerable<Node> QueryNodes(Rect rect)
        {
            var nodes = new List<Node>();
            QueryNodes(_root, rect, nodes);
            return nodes;
        }
        private void QueryNodes(Node node, Rect rect, IList<Node> nodes)
        {
            if(node == null) return;
            if(!node.Rect.Overlaps(rect)) return;

            nodes.Add(node);

            QueryNodes(node.Child_1, rect, nodes);
            QueryNodes(node.Child_2, rect, nodes);
        }

        public List<Box> RayCastBoxes(Rect.RayCastInput input)
        {
            var boxes = new List<Box>();
            RayCastBoxes(_root, input, boxes);
            return boxes;
        }

        private void RayCastBoxes(Node node, Rect.RayCastInput input, List<Box> boxes)
        {
            if(node == null)
            {
                return;
            }

            // Build a bounding box for the segment.
            var segmentRect = Rect.FromMinMax(
                Vector2.Min(input.From, input.To),
                Vector2.Max(input.From, input.To));

            if(!node.Rect.Overlaps(segmentRect))
            {
                return;
            }

            var ray = input.To - input.From;

            Debug.Assert(ray.LengthSquared() > 0.0f);
            ray.Normalize();

            // v is perpendicular to the segment.
            var v = new Vector2(-ray.Y, ray.X);
            var absV = Vector2.Abs(v);
            
            // Separating axis for segment (Gino, p80).
            // |dot(v, p1 - c)| > dot(|v|, h)
            var separation = Math.Abs(Vector2.Dot(v, input.From - node.Rect.Center)) - Vector2.Dot(absV, node.Rect.Extents);

            if(separation > 0.0f)
            {
                return;
            }

            if(node.IsLeaf)
            {
                boxes.Add(node.Box);
            }
            else
            {
                RayCastBoxes(node.Child_1, input, boxes);
                RayCastBoxes(node.Child_2, input, boxes);
            }
        }

        private Node AllocateNode()
        {
            var node = new Node();
            _nodes.Add(node);
            node.Id = _uk++;
            return node;
        }

        private bool FreeNode(Node node)
        {
            return _nodes.Remove(node);
        }

        private void MakeRoot(Node node)
        {
            node.MakeOrphan();
            _root = node;
        }

        private void InsertLeaf(Node leaf)
        {
            //try
            //{
            if(_root == null)
            {
                MakeRoot(leaf);
                return;
            }

            //ValidateStructure(leaf);

            // Find the best sibling for this node
            var leafRect = leaf.Rect;
            var current = _root;
            while(current.IsLeaf == false)
            {
                var child1 = current.Child_1;
                var child2 = current.Child_2;

                var area = current.Rect.Perimeter;

                var combinedRect = Rect.Union(current.Rect, leafRect);
                var combinedArea = combinedRect.Perimeter;

                // Cost of creating a new parent for this node and the new leaf
                var cost = 2.0f * combinedArea;

                // Minimum cost of pushing the leaf further down the tree
                var inheritanceCost = 2.0f * (combinedArea - area);

                // Cost of descending into child1
                float cost1;
                if(child1.IsLeaf)
                {
                    var rect = Rect.Union(leafRect, child1.Rect);
                    cost1 = rect.Perimeter + inheritanceCost;
                }
                else
                {
                    var rect = Rect.Union(leafRect, child1.Rect);
                    var oldArea = child1.Rect.Perimeter;
                    var newArea = rect.Perimeter;
                    cost1 = newArea - oldArea + inheritanceCost;
                }

                // Cost of descending into child2
                float cost2;
                if(child2.IsLeaf)
                {
                    var rect = Rect.Union(leafRect, child2.Rect);
                    cost2 = rect.Perimeter + inheritanceCost;
                }
                else
                {
                    var rect = Rect.Union(leafRect, child2.Rect);
                    var oldArea = child2.Rect.Perimeter;
                    var newArea = rect.Perimeter;
                    cost2 = newArea - oldArea + inheritanceCost;
                }

                // Descend according to the minimum cost.
                if(cost < cost1 && cost1 < cost2)
                {
                    break;
                }

                // Descend
                if(cost1 < cost2)
                {
                    current = child1;
                }
                else
                {
                    current = child2;
                }

                //ValidateStructure(current);
            }

            var sibling = current;

            // Create a new parent.
            var oldParent = sibling.Parent;
            var newParent = AllocateNode();

            if(oldParent != null)
            {
                // The sibling was not the root.
                oldParent.SetChild(sibling, newParent);
            }
            else
            {
                // The sibling was the root.
                MakeRoot(newParent);
            }

            newParent.SetChild_1(sibling);
            newParent.SetChild_2(leaf);

            //ValidateStructure(sibling, leaf, oldParent, newParent);

            // Walk back up the tree fixing heights and AABBs
            current = leaf.Parent;
            while(current != null)
            {
                current = Balance(current);

                Debug.Assert(current.Child_1 != null);
                Debug.Assert(current.Child_2 != null);

                current = current.Parent;
                //ValidateStructure(sibling, leaf, oldParent, newParent);
            }

            //}
            //finally
            //{
            //    Validate();
            //}
        }

        private void RemoveLeaf(Node leaf)
        {
            //try
            //{
            if(leaf == _root)
            {
                _root = null;
                FreeNode(leaf);
                return;
            }

            var parent = leaf.Parent;
            var grandParent = parent.Parent;

            Node sibling;
            if(parent.Child_1 == leaf)
            {
                sibling = parent.Child_2;
            }
            else
            {
                sibling = parent.Child_1;
            }

            //ValidateStructure(grandParent, parent, leaf);

            if(grandParent != null)
            {
                // Destroy parent and connect sibling to grandParent.
                if(grandParent.Child_1 == parent)
                {
                    grandParent.SetChild_1(sibling);
                }
                else
                {
                    grandParent.SetChild_2(sibling);
                }
                //ValidateStructure(grandParent);
                FreeNode(parent);

                // Adjust ancestor bounds.
                var current = grandParent;
                while(current != null)
                {
                    //ValidateStructure(current);
                    current = Balance(current);

                    current = current.Parent;
                }
            }
            else
            {
                MakeRoot(sibling);
                FreeNode(parent);
            }
            //}
            //finally
            //{
            //    Validate();
            //}
        }

        /// <summary>
        ///     Perform a left or right rotation if node A is imbalanced.
        /// </summary>
        /// <param name="iA"></param>
        /// <returns>the new root index.</returns>
        private Node Balance(Node A)
        {
            //    try
            //    {
            if(A.IsLeaf || A.Height < 2)
            {
                return A;
            }

            var B = A.Child_1;
            var C = A.Child_2;

            var balance = C.Height - B.Height;


            //ValidateStructure(A, B, C);

            // Rotate C up
            if(balance > 1)
            {
                var F = C.Child_1;
                var G = C.Child_2;

                // Swap A and C
                Node.Swap(C, A, ref _root);

                // Rotate
                if(F.Height > G.Height)
                {
                    C.SetChild_2(F);
                    A.SetChild_2(G);
                }
                else
                {
                    C.SetChild_2(G);
                    A.SetChild_2(F);
                }


                //ValidateStructure(A, B, C, F, G);
                return C;
            }

            // Rotate B up
            if(balance < -1)
            {
                var D = B.Child_1;
                var E = B.Child_2;

                // Swap A and B
                Node.Swap(B, A, ref _root);

                // Rotate
                if(D.Height > E.Height)
                {
                    B.SetChild_2(D);
                    A.SetChild_1(E);
                }
                else
                {
                    B.SetChild_2(E);
                    A.SetChild_1(D);
                }

                //ValidateStructure(A, B, C, D, E);
                return B;
            }

            //ValidateStructure(A, B, C);

            return A;
            //}
            //finally
            //{
            //    Validate();
            //}
        }

        /// <summary>
        ///     Compute the height of a sub-tree.
        /// </summary>
        /// <param name="nodeId">The node id to use as parent.</param>
        /// <returns>The height of the tree.</returns>
        public int ComputeHeight(Node node)
        {
            if(node.IsLeaf)
            {
                return 0;
            }

            var height1 = ComputeHeight(node.Child_1);
            var height2 = ComputeHeight(node.Child_2);
            return 1 + Math.Max(height1, height2);
        }

        /// <summary>
        ///     Compute the height of the entire tree.
        /// </summary>
        /// <returns>The height of the tree.</returns>
        public int ComputeHeight()
        {
            if(_root == null && _nodes.Count == 0) return 0;
            var height = ComputeHeight(_root);
            return height;
        }

        public void ValidateStructure(params Node[] nodes)
        {
            foreach(var node in nodes)
            {
                if(node == null) continue;
                ValidateStructure(node);
            }
        }

        public void ValidateStructure(Node node)
        {
            if(node == null)
            {
                return;
            }

            if(node == _root)
            {
                Debug.Assert(node.Parent == null);
            }

            var child1 = node.Child_1;
            var child2 = node.Child_2;

            if(node.IsLeaf)
            {
                Debug.Assert(child1 == null);
                Debug.Assert(child2 == null);
                Debug.Assert(node.Height == 0);
                return;
            }

            Debug.Assert(child1.Parent == node);
            Debug.Assert(child2.Parent == node);

            ValidateStructure(child1);
            ValidateStructure(child2);
        }

        public void ValidateMetrics(Node node)
        {
            if(node == null)
            {
                return;
            }

            var child1 = node.Child_1;
            var child2 = node.Child_2;

            if(node.IsLeaf)
            {
                Debug.Assert(child1 == null);
                Debug.Assert(child2 == null);
                Debug.Assert(node.Height == 0);
                return;
            }

            var height1 = child1.Height;
            var height2 = child2.Height;
            var height = 1 + Math.Max(height1, height2);
            Debug.Assert(node.Height == height);

            var rect = Rect.Union(child1.Rect, child2.Rect);

            Debug.Assert(rect.Min == node.Rect.Min);
            Debug.Assert(rect.Max == node.Rect.Max);

            ValidateMetrics(child1);
            ValidateMetrics(child2);
        }

        /// <summary>
        ///     Validate this tree. For testing.
        /// </summary>
        public void Validate()
        {
            ValidateStructure(_root);
            ValidateMetrics(_root);
            Debug.Assert(Height == ComputeHeight());
        }

        /// <summary>
        ///     Build an optimal tree. Very expensive. For testing.
        /// </summary>
        public void RebuildBottomUp()
        {
            var nodes = new List<Node>(_nodes.Count);
            var count = 0;

            // Build array of leaves. Free the rest.
            for(var i = 0; i < nodes.Count; ++i)
            {
                var node = _nodes[i];
                if(node.Height < 0)
                {
                    // free node in pool
                    continue;
                }

                if(node.IsLeaf)
                {
                    node.MakeOrphan();
                    nodes[count] = node;
                    ++count;
                }
                else
                {
                    FreeNode(node);
                }
            }

            while(count > 1)
            {
                var minCost = float.MaxValue;
                int iMin = -1, jMin = -1;
                for(var i = 0; i < count; ++i)
                {
                    var i_rect = nodes[i].Rect;

                    for(var j = i + 1; j < count; ++j)
                    {
                        var j_rect = nodes[j].Rect;
                        var b = Rect.Union(i_rect, j_rect);
                        var cost = b.Perimeter;
                        if(cost < minCost)
                        {
                            iMin = i;
                            jMin = j;
                            minCost = cost;
                        }
                    }
                }

                var child1 = nodes[iMin];
                var child2 = nodes[jMin];

                var parent = AllocateNode();
                parent.SetChild_1(child1);
                parent.SetChild_2(child2);

                nodes[jMin] = nodes[count - 1];
                nodes[iMin] = parent;
                --count;
            }

            _nodes.Clear();
            _nodes.AddRange(nodes);
            _root = nodes[0];

            Validate();
        }
        public Node GetProxy(Box box)
        {
            return _nodes.FirstOrDefault(p => p.Box == box);
        }
    }
}