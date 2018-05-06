using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L4.Model
{
    class FunctionNodesFabric
    {
        Func<INode, INode, INode>[] _binNodesCtors;
        Func<INode, INode>[] _unNodesCtors;

        string[] _terminals;
        Random _r;

        public FunctionNodesFabric(string[] terminals)
        {
            _r = new Random();

            _binNodesCtors = new Func<INode, INode, INode>[] {
              (l,r) => new PlusNode(l,r),
              (l,r) => new MinusNode(l,r),
              (l,r) => new MulNode(l,r),
              (l,r) => new DivNode(l,r),
              (l,r) => new PowNode(l,r)
            };

            _unNodesCtors = new Func<INode, INode>[] {
                (c) => new SinNode(c),
                (c) => new CosNode(c),
                (c) => new AbsNode(c),
                (c) => new ExpNode(c)
            };

            _terminals = terminals;
        }

        public INode GetRandomBinNode(INode left, INode right)
        {
            var index = GetRandomIndex(_binNodesCtors.Length);

            return _binNodesCtors[index](left, right);                        
        }

        private int GetRandomIndex(int length)
        {
            var intervals = new List<Range>();
            var intervalStep = 100 / length;
            int c = 0;
            for (int i = 0; i < length; i++)
            {
                intervals.Add(new Range(c, c += intervalStep));
            }

            var next = _r.Next(c);
            for (int i = 0; i < intervals.Count; i++)
            {
                if (intervals[i].start <= next && intervals[i].end > next)
                {
                    return i;
                }
            }
            return -1;
        }

        public INode GetRandomUnaryNode(INode child)
        {
            var index = GetRandomIndex(_unNodesCtors.Length);
            return _unNodesCtors[index](child);
        }

        public TermNode GetRandomTerminalNode(List<string> usedTerminals)
        {
            var possibleTerminals = _terminals.Where(t => !usedTerminals.Any(ut => ut == t)).ToList();
            var index = GetRandomIndex(possibleTerminals.Count);

            return new TermNode(possibleTerminals[index], 0);
        }

        public TermNode GetRandomTerminalNode()
        {
            var index = GetRandomIndex(_terminals.Count());

            return new TermNode(_terminals[index], 0);
        }

        public INode GetRandomConstNode()
        {
            var value = (((double)_r.Next(39000) - 20000) / 1000) + 1;
            return new ConstNode(value);
        }

        public int GetTerminalsCount()
        {
            return _terminals.Count();
        }
    }
}
