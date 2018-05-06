using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L4.Model
{
    public class Function
    {
        private INode _root;
        private int _depth;
        private Random _r;

        public INode ExprRoot { get { return _root; } }

        public Function(INode root)
        {
            _r = new Random();
            _root = root;
        }

        public double Calculate(Dictionary<string, double> termValues)
        {
            _root.SetTerminals(termValues);
            return _root.GetValue();
        }

        public bool CanReplace(INode oldNode, INode newNode, int maxDepth)
        {
            var oldNodeDepth = oldNode.GetMaxDepth(0);
            var newNodeDepth = newNode.GetMaxDepth(0);
           
            if (oldNodeDepth >= newNodeDepth)
            {
                return true;
            }

            var tempFunc = this.Clone();

            INode tempParentNode = tempFunc.GetParent(oldNode);

            if (tempParentNode == null)
            {
                Debug.Print("tried to replace root node");
                return false;
            }

            //if (newNode.Flatten(n => n.GetChildren()).Contains(parentNode))
            //    Console.WriteLine();

            tempParentNode.ReplaceChild(oldNode, newNode);
            var newDepth = tempFunc.ExprRoot.GetMaxDepth(0);

            return newDepth <= maxDepth;
        }

        public void ReplaceNode(INode oldNode, INode newNode)
        {
            INode parentNode = GetParent(oldNode);

            parentNode.ReplaceChild(oldNode, newNode);
        }

        private INode GetParent(INode child)
        {
            INode parentNode = null;
            var nextNodes = new Queue<INode>();
            nextNodes.Enqueue(_root);
            while (nextNodes.Any() && parentNode == null)
            {
                var cur = nextNodes.Dequeue();
                var children = cur.GetChildren();

                foreach (var c in children)
                {
                    if (c.Equals(child))
                    {
                        parentNode = cur;
                    }
                    else
                    {
                        nextNodes.Enqueue(c);
                    }
                }
            }

            return parentNode;
        }

        public INode GetRandomOp()
        {
            var nodesCount = _root.GetNodesCount();
            var nodeNum = _r.Next(nodesCount) + 1;

            var nextNodes = new Queue<INode>();
            nextNodes.Enqueue(_root);

            INode resNode = null;
            var counter = 0;
            while (counter < nodeNum && nextNodes.Any())
            {
                var cur = nextNodes.Dequeue();
                var children = cur.GetChildren();
                counter += children.Count();
                foreach (var c in children)
                {
                    resNode = c;
                    nextNodes.Enqueue(c);
                }

            }
            return resNode;
        }

        public INode GetRandomOp(OpType type)
        {
            var nodes = new List<INode>();

            var nextNodes = new Queue<INode>();
            nextNodes.Enqueue(_root);

            while (nextNodes.Any())
            {
                var cur = nextNodes.Dequeue();
                var children = cur.GetChildren();
                foreach (var c in children)
                {
                    if (c.GetOpType() == type)
                    {
                        nodes.Add(c);
                    }
                    nextNodes.Enqueue(c);
                }
            }

            if (nodes.Count == 0)
            {
                return null;
            }

            var nodesCount = nodes.Count;
            var nodeNum = _r.Next(nodesCount);
            return nodes[nodeNum];
        }

        public Function Clone()
        {
            return new Function(_root.Clone());
        }

        public override string ToString()
        {
            return _root.ToString();
        }
    }


}
