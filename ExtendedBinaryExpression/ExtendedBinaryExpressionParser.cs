using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NReco.Linq;
using TreeToString;
using TreeToString.PowerShellExtensions;

namespace TreeToString.ExtendedBinaryExpression {
    public class ExtendedBinaryExpressionParser {
        private PowerShellLogging PSLogger = new PowerShellLogging ();
        public LambdaParser LambdaParser = new LambdaParser ();
        public string LevelString (string inputString) {
            var chars = inputString.ToCharArray ();
            int leftOccurrences = Array.FindAll<char> (chars, x => x == '(').Count ();
            int rightOccurrences = Array.FindAll<char> (chars, x => x == ')').Count ();
            if (leftOccurrences != rightOccurrences) {
                if (leftOccurrences > rightOccurrences) {
                    PSLogger.WritePsDebug ("More Left");
                    inputString = inputString + ")";
                } else {
                    PSLogger.WritePsDebug ("More Right");
                    inputString = "(" + inputString;
                }
            }
            return inputString;
        }
        public ParsedNode Parse (BinaryExpression inputExpression) {
            ExtendedBinaryExpression extendedExpression = new ExtendedBinaryExpression (inputExpression);
            return Parse (extendedExpression);
        }

        public List<dynamic> ToStringEntries (ParsedNode parsedNode, ExpressionType expressionType) {
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ToStringEntries]::Begin");

            if (expressionType == ExpressionType.AndAlso) {
                if (
                    (parsedNode.RawExpression.ParsedLeftNode.NodeType == ExpressionType.MemberAccess) &&
                    (parsedNode.RawExpression.ParsedRightNode.NodeType == ExpressionType.MemberAccess)
                ) {
                    string stringEntry = String.Format ("{0}, {1}", parsedNode.Entries[0], parsedNode.Entries[1]);
                    parsedNode.StringEntries.Add (stringEntry);
                }
                // TODO: LeftMa & RightAnd
                // TODO: LeftMa & RightOr
                // TODO: RightMa & LeftAnd
                // TODO: RightMa & LeftOr
            }
            if (expressionType == ExpressionType.OrElse) {
                // TODO: LeftMa & RightMa
                // TODO: LeftMa & RightAnd
                // TODO: LeftMa & RightOr
                // TODO: RightMa & LeftAnd
                // TODO: RightMa & LeftOr
            }

            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ToStringEntries]::End");
            return parsedNode.StringEntries;
        }

        public ParsedNode Parse (Expression inputExpression) {
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::Parse]::Begin");
            if (inputExpression.NodeType == ExpressionType.Parameter) {
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::Parse]::ConvertParameterExpressionToBinaryExpression");
                string newString = String.Format ("{0} || {0}", (inputExpression as ParameterExpression).Name);
                // This will generate a Binary Expression... when the expression being presented is a single entry...
                inputExpression = new LambdaParser ().Parse (newString);
            }
            ExtendedBinaryExpression extendedExpression = new ExtendedBinaryExpression (inputExpression);
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::Parse]::End");
            return Parse (extendedExpression);
        }

        private ParsedNode ParseAnd (ExtendedBinaryExpression inputExpression) {
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::InputExpression.NodeType == AndAlso");
            ParsedNode parsedNode = new ParsedNode (inputExpression, inputExpression.NodeType);
            ExpressionType leftType = inputExpression.ParsedLeftNode.NodeType;
            ExpressionType rightType = inputExpression.ParsedRightNode.NodeType;
            bool isLeftMemberAccess = leftType == ExpressionType.MemberAccess;
            bool isRightMemberAccess = rightType == ExpressionType.MemberAccess;
            ParsedNode primaryParsedNode = null;
            ParsedNode secondaryParsedNode = null;
            ExpressionType primaryType = ExpressionType.Negate; // can't be null
            ExpressionType secondaryType = ExpressionType.Negate; // can't be null

            // If both Primary and Secondary Nodes are MemberAccess types, add them both to the entries list. Return;
            if (isLeftMemberAccess && isRightMemberAccess) {
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::BothMemberAccess");
                parsedNode.AddEntry (inputExpression.ParsedLeftNode.Entries);
                parsedNode.AddEntry (inputExpression.ParsedRightNode.Entries);
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::End");
                return parsedNode;
            } else {
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::NotBothMemberAccess");
                if (isLeftMemberAccess && (!isRightMemberAccess)) {
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::LeftIsMemberAccess");
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::LeftIsPrimary");
                    // Left is Primary
                    // Right is Secondary
                    primaryParsedNode = inputExpression.ParsedLeftNode;
                    secondaryParsedNode = inputExpression.ParsedRightNode;
                    primaryType = leftType;
                    secondaryType = rightType;
                }
                if (isRightMemberAccess && (!isLeftMemberAccess)) {
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::RightIsMemberAccess");
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::RightIsPrimary");
                    // Right is Primary
                    // Left is Secondary
                    primaryParsedNode = inputExpression.ParsedRightNode;
                    secondaryParsedNode = inputExpression.ParsedLeftNode;
                    primaryType = rightType;
                    secondaryType = leftType;
                }
                if ((!isLeftMemberAccess) && (!isRightMemberAccess)) {
                    primaryParsedNode = inputExpression.ParsedLeftNode;
                    secondaryParsedNode = inputExpression.ParsedRightNode;
                    primaryType = leftType;
                    secondaryType = rightType;
                }
            }
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::PrimaryType == " + primaryType);
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::SecondaryType == " + secondaryType);

            // If the Primary Node is MemberAccess Type...
            if (primaryType == ExpressionType.MemberAccess) {
                // and the Secondary Node is an AND node...
                if (secondaryType == ExpressionType.AndAlso) {
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::SecondaryIsAndAlso");
                    parsedNode.AddEntry (primaryParsedNode.Entries);
                    foreach (var secondaryEntry in secondaryParsedNode.Entries) {
                        parsedNode.AddEntry (secondaryEntry);
                    }
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::End");
                    return parsedNode;
                }
                // and the Secondary Node is an OR node...
                if (secondaryType == ExpressionType.OrElse) {
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::SecondaryIsOrElse");
                    // The desired effect here is that a list of possible combinations will be added to the entries list of the parsing node, 
                    // so if the Primary Node is a MemberAccess node whose Expression.Name is PrimaryNode0 and the Secondary Node is an OrElse node which 
                    // contains two MemberAccess nodes RightNode0 and RightNode1, a nested list like the one below should be assigned to the parsing 
                    // node's Entries field. NOTE: The names in the representation below are meant to represent their respective objects.
                    // {
                    //  {RightNode0, PrimaryNode0}
                    //  {RightNode1, PrimaryNode0}
                    // }
                    foreach (var secondaryEntry in secondaryParsedNode.Entries) {
                        List<object> newList = new List<object> ();
                        newList.Add (secondaryEntry);
                        foreach (var primaryEntry in primaryParsedNode.Entries) {
                            newList.Add (primaryEntry);
                        }
                        parsedNode.AddEntry (newList);
                    }
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::End");
                    return parsedNode;
                }
            }

            if (primaryType == ExpressionType.AndAlso) {
                // If the Primary Node is an AND node...
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::PrimaryIsAndAlso");
                // And the Secondary Node is also an AND node... Since the Top Level Node is an AND node, we just concatenate the lists... 
                if (secondaryType == ExpressionType.AndAlso) {
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::SecondaryIsAndAlso");
                    foreach (var primaryEntry in primaryParsedNode.Entries) {
                        parsedNode.AddEntry (primaryEntry);
                    }
                    foreach (var secondaryEntry in secondaryParsedNode.Entries) {
                        parsedNode.AddEntry (secondaryEntry);
                    }
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::End");
                    return parsedNode;

                }
                // If the Primary Node is an AND node... Since the Top Level Node is an AND node, we just concatenate the lists... 
                if (secondaryType == ExpressionType.OrElse) {
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::SecondaryIsOrElse");
                    List<object> basePrimaryList = new List<object> ();
                    foreach (var primaryEntry in primaryParsedNode.Entries) {
                        basePrimaryList.Add (primaryEntry);
                    }
                    foreach (var secondaryEntry in secondaryParsedNode.Entries) {
                        List<object> newList = new List<object> (basePrimaryList);
                        newList.Add (secondaryEntry);
                        parsedNode.AddEntry (newList);
                    }
                    return parsedNode;
                }
            }
            if (primaryType == ExpressionType.OrElse) {
                // If the Primary Node is an OR node...
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::PrimaryIsOrElse");
                // And the Secondary Node is an AND node... Since the Top Level Node is an AND node, we just concatenate the lists... 
                if (secondaryType == ExpressionType.AndAlso) {
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::SecondaryIsAndAlso");
                    List<object> baseRightList = new List<object> ();
                    foreach (var secondaryEntry in secondaryParsedNode.Entries) {
                        baseRightList.Add (secondaryEntry);
                    }
                    foreach (var primaryEntry in primaryParsedNode.Entries) {
                        List<object> newList = new List<object> (baseRightList);
                        newList.Add (primaryEntry);
                        parsedNode.AddEntry (newList);
                    }
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::End");
                    return parsedNode;
                }
                if (secondaryType == ExpressionType.OrElse) {
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::SecondaryIsOrElse");
                    foreach (var primaryEntry in primaryParsedNode.Entries) {
                        foreach (var secondaryEntry in secondaryParsedNode.Entries) {
                            List<object> primaryOutList = new List<object> ();
                            primaryOutList.Add (primaryEntry);
                            primaryOutList.Add (secondaryEntry);
                            parsedNode.AddEntry (primaryOutList);
                        }
                    }
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::End");
                    return parsedNode;
                }
            }

            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::ReachedUnexpectedEnd");
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::End");
            return parsedNode;
        }

        private ParsedNode ParseOr (ExtendedBinaryExpression inputExpression) {
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::InputExpression.NodeType == OrElse");
            ParsedNode parsedNode = new ParsedNode (inputExpression, inputExpression.NodeType);
            ExpressionType leftType = inputExpression.ParsedLeftNode.NodeType;
            ExpressionType rightType = inputExpression.ParsedRightNode.NodeType;
            bool isLeftMemberAccess = leftType == ExpressionType.MemberAccess;
            bool isRightMemberAccess = rightType == ExpressionType.MemberAccess;
            ParsedNode primaryParsedNode = null;
            ParsedNode secondaryParsedNode = null;
            ExpressionType primaryType = ExpressionType.Negate; // can't be null
            ExpressionType secondaryType = ExpressionType.Negate; // can't be null

            // If both Primary and Secondary Nodes are MemberAccess types, add them both to the entries list. Return;
            if (isLeftMemberAccess && isRightMemberAccess) {
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::BothMemberAccess");
                parsedNode.AddEntry (inputExpression.ParsedLeftNode.Entries);
                parsedNode.AddEntry (inputExpression.ParsedRightNode.Entries);
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::End");
                return parsedNode;
            } else {
                if (isLeftMemberAccess && (!isRightMemberAccess)) {
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::LeftIsMemberAccess");
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::LeftIsPrimary");
                    // Left is Primary
                    // Right is Secondary
                    primaryParsedNode = inputExpression.ParsedLeftNode;
                    secondaryParsedNode = inputExpression.ParsedRightNode;
                    primaryType = leftType;
                    secondaryType = rightType;
                }
                if (isRightMemberAccess && (!isLeftMemberAccess)) {
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::RightIsMemberAccess");
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::RightIsPrimary");
                    // Right is Primary
                    // Left is Secondary
                    primaryParsedNode = inputExpression.ParsedRightNode;
                    secondaryParsedNode = inputExpression.ParsedLeftNode;
                    primaryType = rightType;
                    secondaryType = leftType;
                }
                if ((!isLeftMemberAccess) && (!isRightMemberAccess)) {
                    primaryParsedNode = inputExpression.ParsedLeftNode;
                    secondaryParsedNode = inputExpression.ParsedRightNode;
                    primaryType = leftType;
                    secondaryType = rightType;
                }
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::FinishedFindingMemberAccessTypes");
            }
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::PrimaryType == " + primaryType);
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::SecondaryType == " + secondaryType);

            // If the Primary Node is MemberAccess Type...
            if (primaryType == ExpressionType.MemberAccess) {
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::PrimaryIsMemberAccess");
                // and the Secondary Node is an AND node...
                if (secondaryType == ExpressionType.AndAlso) {
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::SecondaryIsAndAlso");
                    // Add the primary entries (singular in this case), as well as the object that contains the secondary entries, because this should result in a list containing [0] the sole primary entry, and [1] a list of the secondary entries 
                    parsedNode.AddEntry (primaryParsedNode.Entries);
                    parsedNode.AddEntry (secondaryParsedNode.Entries);
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::End");
                    return parsedNode;
                }
                // and the Secondary Node is an OR node...
                if (secondaryType == ExpressionType.OrElse) {
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::SecondaryIsOrElse");
                    // Add the primary entries (singular in this case), and each of the individual secondary entries, concatenating the lists as this is one extended OR statement
                    parsedNode.AddEntry (primaryParsedNode.Entries);
                    foreach (var secondaryEntry in secondaryParsedNode.Entries) {
                        parsedNode.AddEntry (secondaryEntry);
                    }
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::End");
                    return parsedNode;
                }
                // If the Primary Node is NOT MemberAccess Type...
            }
            if (primaryType == ExpressionType.AndAlso) {
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::PrimaryIsAndAlso");
                // If the Primary Node is an AND node... 
                // And the Secondary Node is also an AND node... Since the Top Level Node is an OR node, we just add both lists of entries to the list... 
                if (secondaryType == ExpressionType.AndAlso) {
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::SecondaryIsAndAlso");
                    parsedNode.AddEntry (primaryParsedNode.Entries);
                    parsedNode.AddEntry (secondaryParsedNode.Entries);
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::End");
                    return parsedNode;
                }

                // And the Secondary Node is an Or node... add the primary AND node to the parsed node, as this is a single entry, and add each node in the secondary OR node to the parsed node as each entry will be a distint possibility
                if (secondaryType == ExpressionType.OrElse) {
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::SecondaryIsOrElse");
                    parsedNode.AddEntry (primaryParsedNode.Entries);
                    foreach (var secondaryEntry in secondaryParsedNode.Entries) {
                        parsedNode.AddEntry (secondaryEntry);
                    }
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::End");
                    return parsedNode;
                }
            }
            if (primaryType == ExpressionType.OrElse) {
                // If the Primary Node is an OR node...
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::PrimaryIsOrElse");
                // And the Secondary Node is an AND node... Since the Top Level Node is an Or node, add the secondary entries object to the list as an individual entry. Then add each primary entry as an individual entry to the list
                if (secondaryType == ExpressionType.AndAlso) {
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::SecondaryIsAndAlso");
                    parsedNode.AddEntry (secondaryParsedNode.Entries);
                    foreach (var primaryEntry in primaryParsedNode.Entries) {
                        parsedNode.AddEntry (primaryEntry);
                    }
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::End");
                    return parsedNode;
                }
                // And the Secondary Node is an OR Node since the top level is an OR node, and both child nodes are OR nodes, the output list should be one list of entries in a greater OR statement
                if (secondaryType == ExpressionType.OrElse) {
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseAnd]::SecondaryIsOrElse");
                    foreach (var primaryEntry in primaryParsedNode.Entries) {
                        parsedNode.AddEntry (primaryEntry);
                    }
                    foreach (var secondaryEntry in secondaryParsedNode.Entries) {
                        parsedNode.AddEntry (secondaryEntry);
                    }
                    PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::End");
                    return parsedNode;
                }
            }
            PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::ParseOr]::ReachedUnexpectedEnd");
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
                ParsedNode parsedNode = new ParsedNode (inputExpression, inputExpression.NodeType, memberEntry);
                PSLogger.WritePsDebug ("[ExtendedBinaryExpressionParser::Parse]::End");
                parsedNode.StringEntries.Add (parsedNode.Entries[0].ToString ());
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