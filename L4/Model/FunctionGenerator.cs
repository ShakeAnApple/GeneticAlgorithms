using System;
using System.Collections.Generic;

namespace L4.Model
{
    class FunctionGenerator
    {
        Random _r;
        int _minDepth;
        List<string> _usedTerminals;

        public FunctionGenerator()
        {
            _r = new Random();
            _minDepth = 3;
            _usedTerminals = new List<string>();
        }

        public INode GenerateFullInit(FunctionNodesFabric fabric, int maxDepth)
        {
            _usedTerminals.Clear();
            return this.GenerateFullInitImpl(fabric, maxDepth, 0);
        }

        public INode GenerateGrowthInit(FunctionNodesFabric fabric, int maxDepth)
        {
            _usedTerminals.Clear();
            return this.GenerateGrowthInitImpl(fabric, maxDepth, 0);
        }

        public INode GenerateFromStartNode(FunctionNodesFabric fabric, int maxDepth, INode startNode)
        {
            _usedTerminals.Clear();
            if (startNode.GetOpType() == OpType.Term)
            {
                return startNode;
            }

            var children = startNode.GetChildren();
            foreach (var child in children)
            {
                startNode.ReplaceChild(child, this.GenerateGrowthInitImpl(fabric, maxDepth, 0));
            }

            return startNode;
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

        private INode GenerateGrowthInitImpl(FunctionNodesFabric fabric, int maxDepth, int curDepth)
        {
            INode node = null;

            if (curDepth == maxDepth)
            {
                return fabric.GetRandomTerminalNode();
            }

            int nextType;
            if (curDepth < _minDepth && _minDepth <= maxDepth)
            {
                nextType = GetRandomIndex(2);
            }
            else
            {
                nextType = GetRandomIndex(3);
            }

            switch (nextType)
            {
                case 0:
                    node = fabric.GetRandomBinNode(
                    this.GenerateGrowthInitImpl(fabric, maxDepth, curDepth + 1),
                    this.GenerateGrowthInitImpl(fabric, maxDepth, curDepth + 1)
                    );
                    break;
                case 1:
                    node = fabric.GetRandomUnaryNode(
                    this.GenerateGrowthInitImpl(fabric, maxDepth, curDepth + 1));
                    break;
                case 2:
                    var isNextConst = _r.Next(100) <= 20;
                    if (isNextConst && _usedTerminals.Count == fabric.GetTerminalsCount())
                    {
                        node = fabric.GetRandomConstNode();
                    }
                    else if (_usedTerminals.Count < fabric.GetTerminalsCount())
                    {
                        var termNode = fabric.GetRandomTerminalNode(_usedTerminals);
                        _usedTerminals.Add(termNode.Name);
                        node = termNode;
                    }
                    else
                    {
                        node = fabric.GetRandomTerminalNode();
                    }
                    break;
                default:
                    break;
            }

            return node;
        }

        private INode GenerateFullInitImpl(FunctionNodesFabric fabric, int maxDepth, int curDepth)
        {
            INode node = null;

            if (curDepth == maxDepth)
            {
                var isNextConst = _r.Next(100) <= 20;
                if (isNextConst && _usedTerminals.Count == fabric.GetTerminalsCount())
                {
                    return fabric.GetRandomConstNode();
                }
                if (_usedTerminals.Count < fabric.GetTerminalsCount())
                {
                    var termNode = fabric.GetRandomTerminalNode(_usedTerminals);
                    _usedTerminals.Add(termNode.Name);
                    return termNode;
                }
                return fabric.GetRandomTerminalNode();                
            }


            var isNextUnary = _r.Next(100) <= 20;

            if (isNextUnary)
            {
                node = fabric.GetRandomUnaryNode(
                    this.GenerateFullInitImpl(fabric, maxDepth, curDepth + 1)
                    );
            }
            else
            {
                node = fabric.GetRandomBinNode(
                    this.GenerateFullInitImpl(fabric, maxDepth, curDepth + 1), 
                    this.GenerateFullInitImpl(fabric, maxDepth, curDepth + 1)
                    );
            }

            return node;
        }
    }

}
