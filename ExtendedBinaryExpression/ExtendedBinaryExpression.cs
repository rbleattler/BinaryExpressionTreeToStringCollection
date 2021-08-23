using System;
using System.Collections;
using System.Linq.Expressions;
using TreeToString;
using TreeToString.PowerShellExtensions;

namespace TreeToString.ExtendedBinaryExpression {
    public class ExtendedBinaryExpression {
        private PowerShellLogging PSLogger = new PowerShellLogging ();

        public ExtendedBinaryExpressionParser ExpressionParser = new ExtendedBinaryExpressionParser ();
        public BinaryExpression ParentNode;
        public Expression Node;
        public ExpressionType NodeType;
        public ParsedNode ParsedLeftNode = new ParsedNode ();
        public ParsedNode ParsedRightNode = new ParsedNode ();

        public ExtendedBinaryExpression (BinaryExpression fromExpression, BinaryExpression parentNode) {
            this.Node = fromExpression;
            this.ParentNode = parentNode;
            this.NodeType = fromExpression.NodeType;
            // TODO: Assume that if a ParentNode is provided this is NOT the topmost node. 
        }

        public ExtendedBinaryExpression (BinaryExpression fromExpression) {
            this.Node = fromExpression;
            this.NodeType = fromExpression.NodeType;
            // TODO: Assume that if a ParentNode is not provided this is the *topmost* node. 
        }
        public ExtendedBinaryExpression (Expression fromExpression) {
            this.Node = fromExpression;
            this.NodeType = fromExpression.NodeType;
            // TODO: Assume that if a ParentNode is not provided this is the *topmost* node. 
        }

        public void ParseNodes () {
            PSLogger.WritePsDebug ("[ExtendedBinaryExpression::ParseNodes]::Begin");
            PSLogger.WritePsDebug ("[ExtendedBinaryExpression::ParseNodes]::ParseNode::Left");
            this.ParsedLeftNode = ExpressionParser.Parse (((BinaryExpression) this.Node).Left);
            PSLogger.WritePsDebug ("[ExtendedBinaryExpression::ParseNodes]::ParseNode::Right");
            this.ParsedRightNode = ExpressionParser.Parse (((BinaryExpression) this.Node).Right);
            PSLogger.WritePsDebug ("[ExtendedBinaryExpression::ParseNodes]::End");
        }

        public ParsedNode ToParsedNode () {
            ExtendedBinaryExpression thisNode = new ExtendedBinaryExpression (this.Node);
            return ExpressionParser.Parse (thisNode);
        }
    }
}