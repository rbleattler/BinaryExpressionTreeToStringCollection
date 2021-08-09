using System;
using System.Collections;
using System.Linq.Expressions;
using TreeToString;

namespace TreeToString {
    public class EnhancedBinaryExpression {

        public BinaryExpression ParentNode;
        public BinaryExpression Node;
        public ExpressionType LeftNodeType;
        public ExpressionType RightNodeType;

        public EnhancedBinaryExpression (BinaryExpression fromExpression, BinaryExpression parentNode) {
            this.Node = fromExpression;
            this.ParentNode = parentNode;
            this.LeftNodeType = fromExpression.Left.NodeType;
            this.RightNodeType = fromExpression.Right.NodeType;
        }
        public EnhancedBinaryExpression (BinaryExpression fromExpression) {
            this.Node = fromExpression;
            this.LeftNodeType = fromExpression.Left.NodeType;
            this.RightNodeType = fromExpression.Right.NodeType;
        }

        public ArrayList ToList () {
            // Convert the expression to a list
            return null;
        }
    }

}