using System;
using Mandarin.Common.Misc;

namespace Humper
{
    public partial class DynamicTree
    {
        public class Node
        {
            internal Node Child_1; // { get; private set; }
            internal Node Child_2; //{ get; private set; }
            internal int  Height; //{ get; private set; }


            internal int         Id;
            internal Node        Parent; // { get; private set; }
            internal Rect        Rect; //   { get; private set; }
            public   Box         Box;
            internal bool        IsLeaf => Child_1 == null;
            public   DynamicTree Tree; //  { get; set; }
            private int GetHeight()
            {
                if(IsLeaf) return 0;
                return 1 + Math.Max(Child_1?.Height ?? 0, Child_2?.Height ?? 0);
            }

            public void SetChild_1(Node node)
            {
                if(node != null)
                {
                    node.MakeOrphan();
                    node.Parent = this;
                }

                Child_1 = node;
                FixParents();
            }

            public void SetChild_2(Node node)
            {
                if(node != null)
                {
                    if(node.Parent != null) node.MakeOrphan();
                    if(node == Parent) throw new ArgumentException();
                    node.Parent = this;
                }

                Child_2 = node;
                FixParents();
            }

            public void MakeOrphan()
            {
                if(Parent == null) return;

                if(Parent.Child_1 == this)
                {
                    Parent.SetChild_1(null);
                }
                else if(Child_2 == this)
                {
                    Parent.SetChild_2(null);
                }

                Parent = null;
            }

            public void SetChild(Node oldChild, Node newChild)
            {
                if(Child_1 == oldChild)
                {
                    SetChild_1(newChild);
                }
                else if(Child_2 == oldChild)
                {
                    SetChild_2(newChild);
                }
                else
                {
                    throw new ArgumentException();
                }

                oldChild.Parent = null;
            }

            public Node GetSibling()
            {
                if(Parent == null) return null;
                if(Parent.Child_1 == this) return Parent.Child_2;
                if(Parent.Child_2 == this) return Parent.Child_1;
                throw new ArgumentException();
            }
            private void FixParents()
            {
                if(Child_1 != null && Child_2 != null)
                {
                    Rect = Rect.Union(Child_1.Rect, Child_2.Rect);
                }
                else if(Child_1 != null)
                {
                    Rect = Child_1.Rect;
                }
                else if(Child_2 != null)
                {
                    Rect = Child_2.Rect;
                }

                Height = GetHeight();

                if(Parent != null) Parent.FixParents();
            }
            public static void Swap(Node b, Node a, ref Node root)
            {

                //  O
                //  |
                //  A
                // / \
                //B   C

                var o = a.Parent;
                a.Parent = null;
                b.Parent = null;

                //Swap parents
                // a's old parent should point to b

                if(o != null)
                {
                    o.SetChild(a, b);
                }
                else
                {
                    b.MakeOrphan();
                    root = b;
                }

                b.SetChild_1(a);
            }

            public void SetRect(Rect rect)
            {
                if(!IsLeaf) throw new ArgumentException();
                Rect = rect;
            }
            public override string ToString()
            {
                return Id.ToString();
            }
        }
    }
}