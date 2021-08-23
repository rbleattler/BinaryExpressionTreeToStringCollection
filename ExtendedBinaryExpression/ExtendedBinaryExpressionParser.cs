using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TreeToString;
using TreeToString.PowerShellExtensions;

namespace TreeToString.ExtendedBinaryExpression {
    public class ExtendedBinaryExpressionParser {
        private PowerShellLogging PSLogger = new PowerShellLogging ();
        public ParsedNode Parse (BinaryExpression inputExpression) {
            ExtendedBinaryExpression extendedExpression = new ExtendedBinaryExpression (inputExpression);
            return Parse (extendedExpression);
        }
        public ParsedNode Parse (Expression inputExpression) {
            ExtendedBinaryExpression extendedExpression = new ExtendedBinaryExpression (inputExpression);
            return Parse (extendedExpression);
        }

        private ParsedNode ParseAnd (ExtendedBinaryExpression inputExpression) {
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::InputExpression.NodeType == AndAlso");
            ParsedNode parsedNode = new ParsedNode (inputExpression.NodeType);
            ExpressionType leftType = inputExpression.ParsedLeftNode.NodeType;
            ExpressionType rightType = inputExpression.ParsedRightNode.NodeType;
            bool isLeftMemberAccess = leftType == ExpressionType.MemberAccess;
            bool isRightMemberAccess = rightType == ExpressionType.MemberAccess;
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::LeftType == " + leftType);
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::RightType == " + rightType);
            // If both left and right nodes are MemberAccess types, add them both to the entries list. Return;
            if (isLeftMemberAccess && isRightMemberAccess) {
                parsedNode.AddEntry (inputExpression.ParsedLeftNode.Entries);
                parsedNode.AddEntry (inputExpression.ParsedRightNode.Entries);
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::End");
                return parsedNode;
            } else {
                if (isLeftMemberAccess && (!isRightMemberAccess)) {
                    //TODO: Left is Primary
                    //TODO: Right is Secondary
                }
                if (isRightMemberAccess && (!isLeftMemberAccess)) {
                    //TODO: Right is Primary
                    //TODO: Left is Secondary
                }
            }

            // If the left node is MemberAccess Type...
            if (isLeftMemberAccess) {
                // and the Right Node is an AND node...
                if (rightType == ExpressionType.AndAlso) {
                    parsedNode.AddEntry (inputExpression.ParsedLeftNode.Entries);
                    foreach (var entry in inputExpression.ParsedRightNode.Entries) {
                        parsedNode.AddEntry (entry);
                    }
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::End");
                    return parsedNode;
                }
                // and the Right Node is an OR node...
                if (rightType == ExpressionType.OrElse) {
                    // The desired effect here is that a list of possible combinations will be added to the entries list of the parsing node, 
                    // so if the left node is a MemberAccess node whose Expression.Name is LeftNode0 and the right node is an OrElse node which 
                    // contains two MemberAccess nodes RightNode0 and RightNode1, a nested list like the one below should be assigned to the parsing 
                    // node's Entries field. NOTE: The names in the representation below are meant to represent their respective objects.
                    // {
                    //  {RightNode0, LeftNode0}
                    //  {RightNode1, LeftNode0}
                    // }
                    foreach (var rightEntry in inputExpression.ParsedRightNode.Entries) {
                        List<object> newList = new List<object> ();
                        newList.Add (rightEntry);
                        foreach (var leftEntry in inputExpression.ParsedLeftNode.Entries) {
                            newList.Add (leftEntry);
                        }
                        parsedNode.AddEntry (newList);
                    }
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::End");
                    return parsedNode;
                }
            } else {
                // If the left node is NOT MemberAccess Type...
                //@ Since the right should never be of type MemberAccess when the left is NOT, we'll throw an error if that happens for now
                if (rightType == ExpressionType.MemberAccess) {
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::End");
                    throw new Exception ("Right member type was MemberAccess when Left Type was not. Handler not implemented.");
                }

                if (leftType == ExpressionType.AndAlso) {
                    // If the left node is an AND node... 
                    // And the right node is also an AND node... Since the Top Level Node is an AND node, we just concatenate the lists... 
                    if (rightType == ExpressionType.AndAlso) {
                        foreach (var entry in inputExpression.ParsedLeftNode.Entries) {
                            parsedNode.AddEntry (entry);
                        }
                        foreach (var entry in inputExpression.ParsedRightNode.Entries) {
                            parsedNode.AddEntry (entry);
                        }
                        PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::End");
                        return parsedNode;

                    }
                    // If the left node is an AND node... Since the Top Level Node is an AND node, we just concatenate the lists... 
                    if (rightType == ExpressionType.OrElse) {
                        List<object> baseLeftList = new List<object> ();
                        foreach (var leftEntry in inputExpression.ParsedLeftNode.Entries) {
                            baseLeftList.Add (leftEntry);
                        }
                        foreach (var rightEntry in inputExpression.ParsedRightNode.Entries) {
                            List<object> newList = new List<object> (baseLeftList);
                            newList.Add (rightEntry);
                            parsedNode.AddEntry (newList);
                        }
                        return parsedNode;
                    }
                }
                if (leftType == ExpressionType.OrElse) {
                    // If the left node is an OR node...

                    // And the right node is an AND node... Since the Top Level Node is an AND node, we just concatenate the lists... 
                    if (rightType == ExpressionType.AndAlso) {
                        List<object> baseRightList = new List<object> ();
                        foreach (var rightEntry in inputExpression.ParsedRightNode.Entries) {
                            baseRightList.Add (rightEntry);
                        }
                        foreach (var leftEntry in inputExpression.ParsedLeftNode.Entries) {
                            List<object> newList = new List<object> (baseRightList);
                            newList.Add (leftEntry);
                            parsedNode.AddEntry (newList);
                        }
                        PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::End");
                        return parsedNode;
                    }
                    if (rightType == ExpressionType.OrElse) {
                        foreach (var leftEntry in inputExpression.ParsedLeftNode.Entries) {
                            foreach (var rightEntry in inputExpression.ParsedRightNode.Entries) {
                                List<object> leftOutList = new List<object> ();
                                leftOutList.Add (leftEntry);
                                leftOutList.Add (rightEntry);
                                parsedNode.AddEntry (leftOutList);
                            }
                        }
                        PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::End");
                        return parsedNode;
                    }
                }
            }
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::End");
            return parsedNode;
        }

        private ParsedNode ParseOr (ExtendedBinaryExpression inputExpression) {
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::InputExpression.NodeType == OrElse");
            ParsedNode parsedNode = new ParsedNode (inputExpression.NodeType);
            ExpressionType leftType = inputExpression.ParsedLeftNode.NodeType;
            ExpressionType rightType = inputExpression.ParsedRightNode.NodeType;
            bool isLeftMemberAccess = leftType == ExpressionType.MemberAccess;
            bool isRightMemberAccess = rightType == ExpressionType.MemberAccess;
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::LeftType == " + leftType);
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::RightType == " + rightType);
            // If both left and right nodes are MemberAccess types, add them both to the entries list. Return;
            if (isLeftMemberAccess && isRightMemberAccess) {
                parsedNode.AddEntry (inputExpression.ParsedLeftNode.Entries);
                parsedNode.AddEntry (inputExpression.ParsedRightNode.Entries);
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::End");
                return parsedNode;
            } else {
                if (isLeftMemberAccess && (!isRightMemberAccess)) {
                    //TODO: Left is Primary
                    //TODO: Right is Secondary
                }
                if (isRightMemberAccess && (!isLeftMemberAccess)) {
                    //TODO: Right is Primary
                    //TODO: Left is Secondary
                }
            }

            // If the left node is MemberAccess Type...
            if (isLeftMemberAccess) {
                // and the Right Node is an AND node...
                if (rightType == ExpressionType.AndAlso) {
                    // Add the Left Entries (singular in this case), as well as the object that contains the Right Entries, because this should result in a list containing [0] the sole Left Entry, and [1] a list of the Right Entries 
                    parsedNode.AddEntry (inputExpression.ParsedLeftNode.Entries);
                    parsedNode.AddEntry (inputExpression.ParsedRightNode.Entries);
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::End");
                    return parsedNode;
                }
                // and the Right Node is an OR node...
                if (rightType == ExpressionType.OrElse) {
                    // Add the Left Entries (singular in this case), and each of the individual Right Entries, concatenating the lists as this is one extended OR statement
                    parsedNode.AddEntry (inputExpression.ParsedLeftNode.Entries);
                    foreach (var rightEntry in inputExpression.ParsedRightNode.Entries) {
                        parsedNode.AddEntry (rightEntry);
                    }
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::End");
                    return parsedNode;
                }
            } else {
                // If the left node is NOT MemberAccess Type...
                //@ Since the right should never be of type MemberAccess when the left is NOT, we'll throw an error if that happens for now
                if (rightType == ExpressionType.MemberAccess) {
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::End");
                    throw new Exception ("Right member type was MemberAccess when Left Type was not. Handler not implemented.");
                }

                if (leftType == ExpressionType.AndAlso) {
                    // If the left node is an AND node... 
                    // And the right node is also an AND node... Since the Top Level Node is an OR node, we just add both lists of entries to the list... 
                    if (rightType == ExpressionType.AndAlso) {
                        parsedNode.AddEntry (inputExpression.ParsedLeftNode.Entries);
                        parsedNode.AddEntry (inputExpression.ParsedRightNode.Entries);
                        PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::End");
                        return parsedNode;
                    }

                    // And the right node is an Or node... add the left AND node to the parsed node, as this is a single entry, and add each node in the right OR node to the parsed node as each entry will be a distint possibility
                    if (rightType == ExpressionType.OrElse) {
                        parsedNode.AddEntry (inputExpression.ParsedLeftNode.Entries);
                        foreach (var rightEntry in inputExpression.ParsedRightNode.Entries) {
                            parsedNode.AddEntry (rightEntry);
                        }
                        PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::End");
                        return parsedNode;
                    }
                }
                if (leftType == ExpressionType.OrElse) {
                    // If the left node is an OR node...

                    // And the right node is an AND node... Since the Top Level Node is an Or node, add the right Entries object to the list as an individual entry. Then add each left entry as an individual entry to the list
                    if (rightType == ExpressionType.AndAlso) {
                        parsedNode.AddEntry (inputExpression.ParsedRightNode.Entries);
                        foreach (var leftEntry in inputExpression.ParsedLeftNode.Entries) {
                            parsedNode.AddEntry (leftEntry);
                        }
                        PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::End");
                        return parsedNode;
                    }
                    // And the right node is an OR Node since the top level is an OR node, and both child nodes are OR nodes, the output list should be one list of entries in a greater OR statement
                    if (rightType == ExpressionType.OrElse) {
                        foreach (var leftEntry in inputExpression.ParsedLeftNode.Entries) {
                            parsedNode.AddEntry (leftEntry);
                        }
                        foreach (var rightEntry in inputExpression.ParsedRightNode.Entries) {
                            parsedNode.AddEntry (rightEntry);
                        }
                        PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::End");
                        return parsedNode;
                    }
                }
            }
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::End");
            return parsedNode;
        }

        public ParsedNode Parse (ExtendedBinaryExpression inputExpression) {
            //TODO: Apparently either *CAN* be memberaccess... so this needs to be restructured so that if *either* but *not both* are memberaccess it figures out which is the primary and secondary (where the primary is the node that is memberaccess, and the secondary is the one which is not... this should be relatively doable...
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::Parse]::Begin");
            // If the node is a MemberAccess node, we only need the value of that node... 
            if (inputExpression.NodeType == ExpressionType.MemberAccess) {
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::Parse]::InputExpression.NodeType == MemberAccess");
                var memberEntry = ((MemberExpression) inputExpression.Node).Expression.ToString ();
                ParsedNode parsedNode = new ParsedNode (inputExpression.NodeType, memberEntry);
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::Parse]::End");
                return parsedNode;
            }

            try {
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::Parse]::TryParseNodes::Begin");
                inputExpression.ParseNodes ();
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::Parse]::TryParseNodes::End");
            } catch {
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::Parse]::End");
                throw new Exception ("Unable to Parse Child Nodes...");
            }

            // Merge And
            if (inputExpression.NodeType == ExpressionType.AndAlso) {
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::Parse]::End");
                return ParseAnd (inputExpression);
            }

            // Merge Or 
            if (inputExpression.NodeType == ExpressionType.OrElse) {
                return ParseOr (inputExpression);
            }

            // Last Return Path
            PSLogger.WritePsVerbose ("Not Sure How We Got Here...");
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::Parse]::End");
            return new ParsedNode ();
        }
    }
}