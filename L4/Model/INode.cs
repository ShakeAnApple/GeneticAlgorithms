using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace L4.Model
{
    public interface INode
    {
        List<INode> GetChildren();
        Double GetValue();
        int GetMaxDepth(int c);
        int GetNodesCount();
        OpType GetOpType();
        void ReplaceChild(INode oldChild, INode newChild);
        void SetTerminals(Dictionary<string, double> termValues);
        INode Clone();
        bool Equals(INode other);
    }

    public enum OpType
    {
        None = 0,
        Binary = 1,
        Unary = 2,
        Term = 3,
        Const = 4
    }

    abstract class BinaryNode : INode
    {
        public INode Left { get; protected set; }
        public INode Right { get; protected set; }

        public BinaryNode(INode left, INode right)
        {
            Left = left;
            Right = right;
        }

        public List<INode> GetChildren()
        {
            return new List<INode> { Left, Right };
        }

        public void SetTerminals(Dictionary<string, double> termValues)
        {
            Left.SetTerminals(termValues);
            Right.SetTerminals(termValues);
        }

        public int GetMaxDepth(int c)
        {
            c++;
            int a = 0;
            if (c > 500)
            {
                a = 1;
            }
            var lDepth = Left.GetMaxDepth(c);
            var rDepth = Right.GetMaxDepth(c);

            return (lDepth > rDepth ? lDepth : rDepth) + 1;
        }

        public int GetNodesCount()
        {
            return Left.GetNodesCount() + Right.GetNodesCount() + 1;
        }

        OpType INode.GetOpType()
        {
            return OpType.Binary;
        }

        public void ReplaceChild(INode oldChild, INode newChild)
        {
            if (Left.Equals(oldChild))
            {
                Left = newChild.Clone();
            }
            else if (Right.Equals(oldChild))
            {
                Right = newChild.Clone();
            }
        }

        protected abstract double GetValueImpl(double left, double right);

        public double GetValue()
        {
            var leftVal = Left.GetValue();
            var rightVal = Right.GetValue();

            if (Double.IsNaN(leftVal) | Double.IsNaN(rightVal))
            {
                return Double.NaN;
            }

            return GetValueImpl(leftVal, rightVal);
        }

        public abstract INode Clone();

        public bool Equals(INode other)
        {
            if (this.GetType().IsEquivalentTo(other.GetType()))
            {
                var otherBinaryNode = (BinaryNode)other;
                return this.Left.Equals(otherBinaryNode.Left)
                    && this.Right.Equals(otherBinaryNode.Right);
            }
            return false;
        }
    }

    abstract class UnaryNode : INode
    {
        public INode Child { get; protected set; }

        public UnaryNode(INode child)
        {
            Child = child;
        }

        public List<INode> GetChildren()
        {
            return new List<INode> { Child };
        }

        public void SetTerminals(Dictionary<string, double> termValues)
        {
            Child.SetTerminals(termValues);
        }

        public int GetMaxDepth(int c)
        {
            c++;
            if (c > 10000)
            {
                int a = 1;
            }
            return Child.GetMaxDepth(c) + 1;
        }

        public int GetNodesCount()
        {
            return Child.GetNodesCount() + 1;
        }

        OpType INode.GetOpType()
        {
            return OpType.Unary;
        }

        public void ReplaceChild(INode oldChild, INode newChild)
        {
            if (Child.Equals(oldChild))
            {
                Child = newChild.Clone();
            }
            else
            {
                throw new Exception("wtf");
            }
        }

        public abstract double GetValueImpl(double val);

        public double GetValue()
        {
            var childVal = Child.GetValue();
            if (Double.IsNaN(childVal))
            {
                return Double.NaN;
            }

            return GetValueImpl(childVal);
        }

        public abstract INode Clone();

        public bool Equals(INode other)
        {
            if (this.GetType().IsEquivalentTo(other.GetType()))
            {
                var otherUnaryNode = (UnaryNode)other;
                return this.Child.Equals(otherUnaryNode.Child);
            }
            return false;
        }
    }


    class TermNode : INode
    {
        public String Name { get; private set; }
        public Double Value { get; set; }

        public TermNode(String name, Double value)
        {
            Name = name;
            Value = value;
        }

        public List<INode> GetChildren()
        {
            return new List<INode>();
        }

        public override string ToString()
        {
            return string.Format("{0}", this.Name);
        }

        public double GetValue()
        {
            return Value;
        }

        public void SetTerminals(Dictionary<string, double> termValues)
        {
            Value = termValues[Name];
        }

        public int GetMaxDepth(int c)
        {
            c++;
            if (c > 10000)
            {
                throw new Exception();
            }
            return 0;
        }

        public int GetNodesCount()
        {
            return 1;
        }

        OpType INode.GetOpType()
        {
            return OpType.Term;
        }

        public void ReplaceChild(INode oldChild, INode newChild) { }

        public INode Clone()
        {
            return new TermNode(this.Name, this.Value);
        }

        public bool Equals(INode other)
        {
            if (other is TermNode)
            {
                var otherTermNode = (TermNode)other;
                    return this.Name == otherTermNode.Name;
            }
            return false;
        }
    }

    class ConstNode : INode
    {
        public Double Value { get; set; }

        public ConstNode(Double value)
        {
            Value = value;
        }

        public List<INode> GetChildren()
        {
            return new List<INode>();
        }

        public override string ToString()
        {
            return string.Format("{0}", this.Value);
        }

        public double GetValue()
        {
            return Value;
        }

        public void SetTerminals(Dictionary<string, double> termValues)
        {
        }

        public int GetMaxDepth(int c)
        {
            c++;
            if (c > 10000)
            {
                throw new Exception();
            }
            return 0;
        }

        public int GetNodesCount()
        {
            return 1;
        }

        OpType INode.GetOpType()
        {
            return OpType.Term;
        }

        public void ReplaceChild(INode oldChild, INode newChild) { }

        public INode Clone()
        {
            return new ConstNode(this.Value);
        }

        public bool Equals(INode other)
        {
            if (other is ConstNode)
            {
                var otherConstNode = (ConstNode)other;
                return this.Value == otherConstNode.Value;
            }
            return false;
        }
    }

    class PlusNode : BinaryNode
    {
        public PlusNode(INode left, INode right) : base(left, right)
        {
        }

        public override INode Clone()
        {
            return new PlusNode(Left.Clone(), Right.Clone());
        }

        public override string ToString()
        {
            return string.Format("({0} + {1})", this.Left.ToString(), this.Right.ToString());
        }

        protected override double GetValueImpl(double left, double right)
        {
            return left + right;
        }
    }

    class MinusNode : BinaryNode
    {
        public MinusNode(INode left, INode right) : base(left, right)
        {
        }

        public override INode Clone()
        {
            return new MinusNode(Left.Clone(), Right.Clone());
        }

        public override string ToString()
        {
            return string.Format("({0} - {1})", this.Left.ToString(), this.Right.ToString());
        }

        protected override double GetValueImpl(double left, double right)
        {
            return left - right;
        }
    }

    class MulNode : BinaryNode
    {
        public MulNode(INode left, INode right) : base(left, right)
        {
        }

        public override INode Clone()
        {
            return new MulNode(Left.Clone(), Right.Clone());
        }

        public override string ToString()
        {
            return string.Format("({0} * {1})", this.Left.ToString(), this.Right.ToString());
        }

        protected override double GetValueImpl(double left, double right)
        {
            return left * right;
        }
    }

    class DivNode : BinaryNode
    {
        public DivNode(INode left, INode right) : base(left, right)
        {
        }

        public override INode Clone()
        {
            return new DivNode(Left.Clone(), Right.Clone());
        }

        public override string ToString()
        {
            return string.Format("({0} / {1})", this.Left.ToString(), this.Right.ToString());
        }

        protected override double GetValueImpl(double left, double right)
        {
            if (right == 0)
            {
                return Double.NaN;
            }
            return (double)(left / right);
        }
    }

    class AbsNode : UnaryNode
    {
        public AbsNode(INode child) : base(child)
        {
        }

        public override INode Clone()
        {
            return new AbsNode(Child.Clone());
        }

        public override double GetValueImpl(double val)
        {
            return Math.Abs(val);
        }

        public override string ToString()
        {
            return string.Format("(abs({0}))", this.Child.ToString());
        }
    }

    class SinNode : UnaryNode
    {
        public SinNode(INode child) : base(child)
        {
        }

        public override INode Clone()
        {
            return new SinNode(Child.Clone());
        }

        public override double GetValueImpl(double val)
        {
            return Math.Sin(val);
        }

        public override string ToString()
        {
            return string.Format("(sin({0}))", this.Child.ToString());
        }
    }

    class CosNode : UnaryNode
    {
        public CosNode(INode child) : base(child)
        {
        }

        public override INode Clone()
        {
            return new CosNode(Child.Clone());
        }

        public override double GetValueImpl(double val)
        {
            return Math.Cos(val);
        }

        public override string ToString()
        {
            return string.Format("(cos({0}))", this.Child.ToString());
        }
    }

    class ExpNode : UnaryNode
    {
        public ExpNode(INode child) : base(child)
        {
        }

        public override INode Clone()
        {
            return new ExpNode(Child.Clone());
        }

        public override string ToString()
        {
            return String.Format("(e)^({0})", Child.ToString());
        }

        public override double GetValueImpl(double val)
        {
            return Math.Exp(val);
        }
    }

    class PowNode : BinaryNode
    {
        public PowNode(INode left, INode right) : base(left, right)
        {
        }

        public override INode Clone()
        {
            return new PowNode(Left.Clone(), Right.Clone());
        }

        public override string ToString()
        {
            return String.Format("({0})^({1})", Left.ToString(), Right.ToString());
        }

        protected override double GetValueImpl(double left, double right)
        {
            if (right % 2 == 0 && left < 0)
            {
                return Double.NaN;
            }

            return Math.Pow(left, right);
        }
    }
}
