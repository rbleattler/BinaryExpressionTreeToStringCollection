using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TreeToString;

namespace TreeToString.ExtendedBinaryExpression {
    public class ExtendedBinaryExpressionParser {
        public ParsedNode Parse (BinaryExpression inputExpression) {
            ExtendedBinaryExpression extendedExpression = new ExtendedBinaryExpression (inputExpression);
            return Parse (extendedExpression);
        }
        public ParsedNode Parse (Expression inputExpression) {
            ExtendedBinaryExpression extendedExpression = new ExtendedBinaryExpression (inputExpression);
            return Parse (extendedExpression);
        }
        public ParsedNode Parse (ExtendedBinaryExpression inputExpression) {
            if (inputExpression.NodeType == ExpressionType.MemberAccess) {
                var memberEntry = ((MemberExpression) inputExpression.Node).Expression.ToString ();
                ParsedNode parsedNode = new ParsedNode (inputExpression.NodeType, memberEntry);
                return parsedNode;
            }

            // Merge And
            if (inputExpression.NodeType == ExpressionType.AndAlso) {
                ParsedNode parsedNode = new ParsedNode (inputExpression.NodeType);
                ExpressionType leftType = inputExpression.ParsedLeftNode.NodeType;
                ExpressionType rightType = inputExpression.ParsedRightNode.NodeType;
                bool isLeftMemberAccess = leftType == ExpressionType.MemberAccess;
                bool isRightMemberAccess = rightType == ExpressionType.MemberAccess;
                // If both left and right nodes are MemberAccess types, add them both to the entries list. Return;
                if (isLeftMemberAccess && isRightMemberAccess) {
                    parsedNode.AddEntry (inputExpression.ParsedLeftNode.Entries);
                    parsedNode.AddEntry (inputExpression.ParsedRightNode.Entries);
                    return parsedNode;
                }

                // If the left node is MemberAccess Type...
                if (isLeftMemberAccess) {
                    // and the Right Node is an AND node...
                    if (rightType == ExpressionType.AndAlso) {
                        parsedNode.AddEntry (inputExpression.ParsedLeftNode.Entries);
                        foreach (var entry in inputExpression.ParsedRightNode.Entries) {
                            parsedNode.AddEntry (entry);
                        }
                        return parsedNode;
                    }
                    // and the Right Node is an OR node...
                    if (rightType == ExpressionType.OrElse) {
                        // The desired effect here is that a list of possible combinations will be added to the entries list of the parsing node, 
                        // so if the left node is a MemberAccess node whose Expression.Name is LeftNode0 and the right node is an OrElse node which 
                        // contains two MemberAccess nodes RightNode0 and RightNode1, a nested list like the one below should be assigned to the parsing 
                        // node's Entries field. NOTE: The names in the representation below are meant to represent their respective objects.
                        // {
                        //  {RightNode1, LeftNode0}
                        //  {LeftNode1, LeftNode0}
                        // }
                        foreach (var rightEntry in inputExpression.ParsedRightNode.Entries) {
                            List<object> newList = new List<object> ();
                            newList.Add (rightEntry);
                            foreach (var leftEntry in inputExpression.ParsedLeftNode.Entries) {
                                newList.Add (leftEntry);
                            }
                            parsedNode.AddEntry (newList);
                        }
                    }
                } else {
                    // If the left node is NOT MemberAccess Type...
                    if (leftType == ExpressionType.AndAlso) {
                        // If the left node is an AND node...

                    }
                    if (leftType == ExpressionType.OrElse) {
                        // If the left node is an OR node...

                    }

                    if (rightType == ExpressionType.MemberAccess) {
                        //TODO: ??
                    }
                    if (rightType == ExpressionType.AndAlso) {
                        //TODO: ??
                    }
                    if (rightType == ExpressionType.OrElse) {
                        //TODO: ??
                    }
                }

            }

            // Merge Or 
            if (inputExpression.NodeType == ExpressionType.OrElse) {

            }

            // Last Return Path
            return new ParsedNode ();
        }
    }
}