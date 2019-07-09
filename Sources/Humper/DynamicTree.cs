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

using Humper.Base;

namespace Humper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;


    /// <summary>
    /// A dynamic tree arranges data in a binary tree to accelerate
    /// queries such as volume queries and ray casts. Leafs are proxies
    /// with an AABB. In the tree we expand the proxy AABB by Settings.b2_fatAABBFactor
    /// so that the proxy AABB is bigger than the client object. This allows the client
    /// object to move by small amounts without triggering a tree update.
    /// Nodes are pooled and relocatable, so we use node indices rather than pointers.
    /// </summary>
    public class DynamicTree: IBroadPhase
    {
        public class Node
        {
            /// <summary>
            /// Enlarged AABB
            /// </summary>
            internal Rect Rect;

            internal Node Child_1;
            internal Node Child_2;

            internal int    Height;
            internal Node   Parent;
            internal object UserData;
            internal int    Id;

            internal bool IsLeaf()
            {
                return Child_1 == DynamicTree.NullNode;
            }
        }

        internal const   Node        NullNode = null;
        private readonly List<Node>  _nodes;
        private readonly Stack<Node> _queryStack   = new Stack<Node>();
        private readonly Stack<Node> _raycastStack = new Stack<Node>();
        private          Node        _root;
        private          Vector2     AABBExtension  = Vector2.Zero;
        private          Vector2     AABBMultiplier = Vector2.One;
        private Rect _bounds;

        /// <summary>
        /// Constructing the tree initializes the node pool.
        /// </summary>
        public DynamicTree()
        {
            _root = NullNode;
            _nodes = new List<Node>();
        }

        /// <summary>
        /// Compute the height of the binary tree in O(N) time. Should not be called often.
        /// </summary>
        public int Height
        {
            get
            {
                if(_root == NullNode)
                {
                    return 0;
                }

                return _root.Height;
            }
        }

        /// <summary>
        /// Get the ratio of the sum of the node areas to the root area.
        /// </summary>
        public float AreaRatio
        {
            get
            {
                if(_root == NullNode)
                {
                    return 0.0f;
                }

                float rootArea = _root.Rect.Perimeter;

                float totalArea = 0.0f;
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
        /// Get the maximum balance of an node in the tree. The balance is the difference
        /// in height of the two children of a node.
        /// </summary>
        public int MaxBalance
        {
            get
            {
                int maxBalance = 0;
                foreach(var node in _nodes)
                {
                    if(node.Height <= 1)
                    {
                        continue;
                    }

                    Debug.Assert(node.IsLeaf() == false);

                    var child1 = node.Child_1;
                    var child2 = node.Child_2;
                    int balance = Math.Abs(child2.Height - child1.Height);
                    maxBalance = Math.Max(maxBalance, balance);
                }

                return maxBalance;
            }
        }

        /// <summary>
        /// Create a proxy in the tree as a leaf node. We return the index
        /// of the node instead of a pointer so that we can grow
        /// the node pool.
        /// </summary>
        /// <param name="rect">The AABB.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Index of the created proxy</returns>
        public Node AddProxy(Rect rect, object userData)
        {
            var node = AllocateNode();

            // Fatten the AABB.
            node.Rect = rect.Expand(AABBExtension);
            node.UserData = userData;
            node.Height = 0;

            InsertLeaf(node);

            return node;
        }

        /// <summary>
        /// Destroy a proxy. This asserts if the id is invalid.
        /// </summary>
        public bool RemoveProxy(Node node)
        {
            Debug.Assert(node.IsLeaf());

            RemoveLeaf(node);
            return FreeNode(node);
        }

        /// <summary>
        /// Move a proxy with a swepted AABB. If the proxy has moved outside of its fattened AABB,
        /// then the proxy is removed from the tree and re-inserted. Otherwise
        /// the function returns immediately.
        /// </summary>
        /// <param name="node">The proxy id.</param>
        /// <param name="rect">The AABB.</param>
        /// <param name="displacement">The displacement.</param>
        /// <returns>true if the proxy was re-inserted.</returns>
        public bool MoveProxy(Node node, Rect rect, Vector2 displacement)
        {
            Debug.Assert(node.IsLeaf());

            if(node.Rect.Contains(rect))
            {
                return false;
            }

            RemoveLeaf(node);

            // Extend AABB.
            Rect newRect = rect.Expand(AABBExtension);
            // Predict AABB displacement.
            newRect = newRect.Offset(AABBMultiplier * displacement);
            node.Rect = newRect;

            InsertLeaf(node);
            return true;
        }

        /// <summary>
        /// Query an AABB for overlapping proxies. The callback class
        /// is called for each proxy that overlaps the supplied AABB.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="rect">The AABB.</param>
        public void Query(Func<Node, bool> callback, Rect rect)
        {
            _queryStack.Clear();
            _queryStack.Push(_root);

            while(_queryStack.Count > 0)
            {
                var node = _queryStack.Pop();
                if(node == NullNode)
                {
                    continue;
                }

                if(node.Rect.Intersects(rect))
                {
                    if(node.IsLeaf())
                    {
                        bool proceed = callback(node);
                        if(proceed == false)
                        {
                            return;
                        }
                    }
                    else
                    {
                        _queryStack.Push(node.Child_1);
                        _queryStack.Push(node.Child_2);
                    }
                }
            }
        }

        /// <summary>
        /// Ray-cast against the proxies in the tree. This relies on the callback
        /// to perform a exact ray-cast in the case were the proxy contains a Shape.
        /// The callback also performs the any collision filtering. This has performance
        /// roughly equal to k * log(n), where k is the number of collisions and n is the
        /// number of proxies in the tree.
        /// </summary>
        /// <param name="callback">A callback class that is called for each proxy that is hit by the ray.</param>
        /// <param name="input">The ray-cast input data. The ray extends from p1 to p1 + maxFraction * (p2 - p1).</param>
        public void RayCast(Func<RayCastInput, Node, float> callback, RayCastInput input)
        {
            Vector2 p1 = input.From;
            Vector2 p2 = input.To;
            Vector2 r = p2 - p1;
            Debug.Assert(r.LengthSquared() > 0.0f);
            r.Normalize();

            // v is perpendicular to the segment.
            Vector2 absV = Vector2.Abs(new Vector2(-r.Y, r.X)); //Velcro: Inlined the 'v' variable

            // Separating axis for segment (Gino, p80).
            // |dot(v, p1 - c)| > dot(|v|, h)

            float maxFraction = input.MaxFraction;

            // Build a bounding box for the segment.
            Rect segmentRect = new Rect();
            {
                Vector2 t = p1 + maxFraction * (p2 - p1);
                segmentRect.Min = Vector2.Min(p1, t);
                segmentRect.Max = Vector2.Max(p1, t);
            }

            _raycastStack.Clear();
            _raycastStack.Push(_root);

            while(_raycastStack.Count > 0)
            {
                var node = _raycastStack.Pop();
                if(node == NullNode)
                {
                    continue;
                }

                if(node.Rect.Intersects(segmentRect) == false)
                {
                    continue;
                }

                // Separating axis for segment (Gino, p80).
                // |dot(v, p1 - c)| > dot(|v|, h)
                Vector2 c = node.Rect.Center;
                Vector2 h = node.Rect.Extents;
                float separation = Math.Abs(Vector2.Dot(new Vector2(-r.Y, r.X), p1 - c)) - Vector2.Dot(absV, h);
                if(separation > 0.0f)
                {
                    continue;
                }

                if(node.IsLeaf())
                {
                    RayCastInput subInput;
                    subInput.From = input.From;
                    subInput.To = input.To;
                    subInput.MaxFraction = maxFraction;

                    float value = callback(subInput, node);

                    if(value == 0.0f)
                    {
                        // the client has terminated the raycast.
                        return;
                    }

                    if(value > 0.0f)
                    {
                        // Update segment bounding box.
                        maxFraction = value;
                        Vector2 t = p1 + maxFraction * (p2 - p1);
                        segmentRect.Min = Vector2.Min(p1, t);
                        segmentRect.Max = Vector2.Max(p1, t);
                    }
                }
                else
                {
                    _raycastStack.Push(node.Child_1);
                    _raycastStack.Push(node.Child_2);
                }
            }
        }

        private Node AllocateNode()
        {
            var node = new Node();
            _nodes.Add(node);
            node.Id = _nodes.Count;
            return node;
        }

        private bool FreeNode(Node node)
        {
            return _nodes.Remove(node);
        }

        private void InsertLeaf(Node leaf)
        {
            if(_root == NullNode)
            {
                _root = leaf;
                _root.Parent = NullNode;
                return;
            }

            // Find the best sibling for this node
            Rect leafRect = leaf.Rect;
            var current = _root;
            while(current.IsLeaf() == false)
            {
                var child1 = current.Child_1;
                var child2 = current.Child_2;

                float area = current.Rect.Perimeter;

                Rect combinedRect = Rect.Union(current.Rect, leafRect);
                float combinedArea = combinedRect.Perimeter;

                // Cost of creating a new parent for this node and the new leaf
                float cost = 2.0f * combinedArea;

                // Minimum cost of pushing the leaf further down the tree
                float inheritanceCost = 2.0f * (combinedArea - area);

                // Cost of descending into child1
                float cost1;
                if(child1.IsLeaf())
                {
                    Rect rect = Rect.Union(leafRect, child1.Rect);
                    cost1 = rect.Perimeter + inheritanceCost;
                }
                else
                {
                    Rect rect = Rect.Union(leafRect, child1.Rect);
                    float oldArea = child1.Rect.Perimeter;
                    float newArea = rect.Perimeter;
                    cost1 = (newArea - oldArea) + inheritanceCost;
                }

                // Cost of descending into child2
                float cost2;
                if(child2.IsLeaf())
                {
                    Rect rect = Rect.Union(leafRect, child2.Rect);
                    cost2 = rect.Perimeter + inheritanceCost;
                }
                else
                {
                    Rect rect = Rect.Union(leafRect, child2.Rect);
                    float oldArea = child2.Rect.Perimeter;
                    float newArea = rect.Perimeter;
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
            }

            var sibling = current;

            // Create a new parent.
            var oldParent = sibling.Parent;
            var newParent = AllocateNode();
            newParent.Parent = oldParent;
            newParent.Rect = Rect.Union(leafRect, sibling.Rect);
            newParent.Height = sibling.Height + 1;

            if(oldParent != NullNode)
            {
                // The sibling was not the root.
                if(oldParent.Child_1 == sibling)
                {
                    oldParent.Child_1 = newParent;
                }
                else
                {
                    oldParent.Child_2 = newParent;
                }

                newParent.Child_1 = sibling;
                newParent.Child_2 = leaf;
                sibling.Parent = newParent;
                leaf.Parent = newParent;
            }
            else
            {
                // The sibling was the root.
                newParent.Child_1 = sibling;
                newParent.Child_2 = leaf;
                sibling.Parent = newParent;
                leaf.Parent = newParent;
                _root = newParent;
            }

            // Walk back up the tree fixing heights and AABBs
            current = leaf.Parent;
            while(current != NullNode)
            {
                current = Balance(current);

                var child1 = current.Child_1;
                var child2 = current.Child_2;

                Debug.Assert(child1 != NullNode);
                Debug.Assert(child2 != NullNode);

                current.Height = 1 + Math.Max(child1.Height, child2.Height);
                current.Rect = Rect.Union(child1.Rect, child2.Rect);

                current = current.Parent;
            }

            //Validate();
        }

        private void RemoveLeaf(Node leaf)
        {
            if(leaf == _root)
            {
                _root = NullNode;
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

            if(grandParent != NullNode)
            {
                // Destroy parent and connect sibling to grandParent.
                if(grandParent.Child_1 == parent)
                {
                    grandParent.Child_1 = sibling;
                }
                else
                {
                    grandParent.Child_2 = sibling;
                }
                sibling.Parent = grandParent;
                FreeNode(parent);

                // Adjust ancestor bounds.
                var current = grandParent;
                while(current != NullNode)
                {
                    current = Balance(current);

                    var child1 = current.Child_1;
                    var child2 = current.Child_2;

                    current.Rect = Rect.Union(child1.Rect, child2.Rect);
                    current.Height = 1 + Math.Max(child1.Height, child2.Height);

                    current = current.Parent;
                }
            }
            else
            {
                _root = sibling;
                sibling.Parent = NullNode;
                FreeNode(parent);
            }

            //Validate();
        }

        /// <summary>
        /// Perform a left or right rotation if node A is imbalanced.
        /// </summary>
        /// <param name="iA"></param>
        /// <returns>the new root index.</returns>
        private Node Balance(Node A)
        {
            if(A.IsLeaf() || A.Height < 2)
            {
                return A;
            }

            var B = A.Child_1;
            var C = A.Child_2;

            int balance = C.Height - B.Height;

            // Rotate C up
            if(balance > 1)
            {
                var F = C.Child_1;
                var G = C.Child_2;

                // Swap A and C
                C.Child_1 = A;
                C.Parent = A.Parent;
                A.Parent = C;

                // A's old parent should point to C
                if(C.Parent != NullNode)
                {
                    if(C.Parent.Child_1 == A)
                    {
                        C.Parent.Child_1 = C;
                    }
                    else
                    {
                        C.Parent.Child_2 = C;
                    }
                }
                else
                {
                    _root = C;
                }

                // Rotate
                if(F.Height > G.Height)
                {
                    C.Child_2 = F;
                    A.Child_2 = G;
                    G.Parent = A;
                    A.Rect = Rect.Union(B.Rect, G.Rect);
                    C.Rect = Rect.Union(A.Rect, F.Rect);

                    A.Height = 1 + Math.Max(B.Height, G.Height);
                    C.Height = 1 + Math.Max(A.Height, F.Height);
                }
                else
                {
                    C.Child_2 = G;
                    A.Child_2 = F;
                    F.Parent = A;
                    A.Rect = Rect.Union(B.Rect, F.Rect);
                    C.Rect = Rect.Union(A.Rect, G.Rect);

                    A.Height = 1 + Math.Max(B.Height, F.Height);
                    C.Height = 1 + Math.Max(A.Height, G.Height);
                }

                return C;
            }

            // Rotate B up
            if(balance < -1)
            {
                var D = B.Child_1;
                var E = B.Child_2;

                // Swap A and B
                B.Child_1 = A;
                B.Parent = A.Parent;
                A.Parent = B;

                // A's old parent should point to B
                if(B.Parent != NullNode)
                {
                    if(B.Parent.Child_1 == A)
                    {
                        B.Parent.Child_1 = B;
                    }
                    else
                    {
                        Debug.Assert(B.Parent.Child_2 == A);
                        B.Parent.Child_2 = B;
                    }
                }
                else
                {
                    _root = B;
                }

                // Rotate
                if(D.Height > E.Height)
                {
                    B.Child_2 = D;
                    A.Child_1 = E;
                    E.Parent = A;
                    A.Rect = Rect.Union(C.Rect, E.Rect);
                    B.Rect = Rect.Union(A.Rect, D.Rect);

                    A.Height = 1 + Math.Max(C.Height, E.Height);
                    B.Height = 1 + Math.Max(A.Height, D.Height);
                }
                else
                {
                    B.Child_2 = E;
                    A.Child_1 = D;
                    D.Parent = A;
                    A.Rect = Rect.Union(C.Rect, D.Rect);
                    B.Rect = Rect.Union(A.Rect, E.Rect);

                    A.Height = 1 + Math.Max(C.Height, D.Height);
                    B.Height = 1 + Math.Max(A.Height, E.Height);
                }

                return B;
            }

            return A;
        }

        /// <summary>
        /// Compute the height of a sub-tree.
        /// </summary>
        /// <param name="nodeId">The node id to use as parent.</param>
        /// <returns>The height of the tree.</returns>
        public int ComputeHeight(Node node)
        {
            if(node.IsLeaf())
            {
                return 0;
            }

            int height1 = ComputeHeight(node.Child_1);
            int height2 = ComputeHeight(node.Child_2);
            return 1 + Math.Max(height1, height2);
        }

        /// <summary>
        /// Compute the height of the entire tree.
        /// </summary>
        /// <returns>The height of the tree.</returns>
        public int ComputeHeight()
        {
            int height = ComputeHeight(_root);
            return height;
        }

        public void ValidateStructure(Node node)
        {
            if(node == NullNode)
            {
                return;
            }

            if(node == _root)
            {
                Debug.Assert(node.Parent == NullNode);
            }

            var child1 = node.Child_1;
            var child2 = node.Child_2;

            if(node.IsLeaf())
            {
                Debug.Assert(child1 == NullNode);
                Debug.Assert(child2 == NullNode);
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
            if(node == NullNode)
            {
                return;
            }

            var child1 = node.Child_1;
            var child2 = node.Child_2;

            if(node.IsLeaf())
            {
                Debug.Assert(child1 == NullNode);
                Debug.Assert(child2 == NullNode);
                Debug.Assert(node.Height == 0);
                return;
            }

            int height1 = child1.Height;
            int height2 = child2.Height;
            int height = 1 + Math.Max(height1, height2);
            Debug.Assert(node.Height == height);

            Rect rect = Rect.Union(child1.Rect, child2.Rect);

            Debug.Assert(rect.Min == node.Rect.Min);
            Debug.Assert(rect.Max == node.Rect.Max);

            ValidateMetrics(child1);
            ValidateMetrics(child2);
        }

        /// <summary>
        /// Validate this tree. For testing.
        /// </summary>
        public void Validate()
        {
            ValidateStructure(_root);
            ValidateMetrics(_root);
            Debug.Assert(Height == ComputeHeight());
        }

        /// <summary>
        /// Build an optimal tree. Very expensive. For testing.
        /// </summary>
        public void RebuildBottomUp()
        {
            var nodes = new List<Node>(_nodes.Count);
            int count = 0;

            // Build array of leaves. Free the rest.
            for(int i = 0; i < nodes.Count; ++i)
            {
                var node = _nodes[i];
                if(node.Height < 0)
                {
                    // free node in pool
                    continue;
                }

                if(node.IsLeaf())
                {
                    node.Parent = NullNode;
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
                float minCost = float.MaxValue;
                int iMin = -1, jMin = -1;
                for(int i = 0; i < count; ++i)
                {
                    Rect i_rect = nodes[i].Rect;

                    for(int j = i + 1; j < count; ++j)
                    {
                        Rect j_rect = nodes[j].Rect;
                        Rect b = Rect.Union(i_rect, j_rect);
                        float cost = b.Perimeter;
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
                parent.Child_1 = child1;
                parent.Child_2 = child2;
                parent.Height = 1 + Math.Max(child1.Height, child2.Height);
                parent.Rect = Rect.Union(child1.Rect, child2.Rect);
                parent.Parent = NullNode;

                child1.Parent = parent;
                child2.Parent = parent;

                nodes[jMin] = nodes[count - 1];
                nodes[iMin] = parent;
                --count;
            }

            _nodes.Clear();
            _nodes.AddRange(nodes);
            _root = nodes[0];

            Validate();
        }

        /// <summary>
        /// Shift the origin of the nodes
        /// </summary>
        /// <param name="newOrigin">The displacement to use.</param>
        public void ShiftOrigin(Vector2 newOrigin)
        {
            // Build array of leaves. Free the rest.
            foreach(Node node in _nodes)
            {
                node.Rect.Min -= newOrigin;
                node.Rect.Max -= newOrigin;
            }
        }
        public object GetUserData(int proxyId)
        {
            return _nodes[proxyId - 1].UserData;
        }

        public Node Get(int id)
        {
            return _nodes[id - 1];
        }
        void IBroadPhase.Add(Box box)
        {
            var node = AddProxy(box.Bounds, box);
            box.BroadPhaseData = node;
        }
        IEnumerable<Box> IBroadPhase.QueryBoxes(Rect area)
        {
            List<Box> result = new List<Box>();
            Query(p =>
            {
                result.Add((Box)p.UserData);
                return true;
            }, area);

            return result;
        }
        Rect IBroadPhase.Bounds => _bounds;
        bool IBroadPhase.Remove(Box box)
        {
            return RemoveProxy((Node)box.BroadPhaseData);
        }
        void IBroadPhase.Update(Box box, Rect @from)
        {
            MoveProxy((Node)box.BroadPhaseData, @from, box.Bounds.Location - from.Location);
        }
        void IBroadPhase.DrawDebug(Rect area, Action<Rect, float> drawCell, Action<Box> drawBox, Action<string, int, int, float> drawString)
        {
            Query(p =>
            {
                drawCell(p.Rect, 0.25f);
                return true;
            }, area);
        }
    }
}