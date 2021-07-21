using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using NReco.Linq;

namespace StringOps {
    public class BinaryExpressionTreeParser {
        private LambdaParser LambdaParser = new LambdaParser ();
        public ArrayList ToStringCollection (string treeAsString) {
            return ToStringCollection (LambdaParser.Parse (treeAsString) as BinaryExpression);
        }
        public ArrayList ToStringCollection (BinaryExpression treeExpression) {
            ArrayList outerList = new ArrayList ();
            ArrayList referenceList = new ArrayList ();

            BuildList (treeExpression, ref referenceList, ref outerList);

            return outerList;
        }

        private ArrayList BuildList (BinaryExpression evalExpression, ref ArrayList referenceList, ref ArrayList mainList) {
            Dictionary<string, bool> memberAccessMap = new Dictionary<string, bool> () {
                {
                "left",
                evalExpression.Left.NodeType.ToString () == "MemberAccess"
                }, {
                "right",
                evalExpression.Right.NodeType.ToString () == "MemberAccess"
                }
            };
            MemberExpression member;
            string memberString;
            dynamic nonMember;
            // If both left/right are MemberAccess
            // -- add both to reference list and return the list

            if (memberAccessMap["left"] && memberAccessMap["right"]) {
                var leftMember = evalExpression.Left as MemberExpression;
                var rightMember = evalExpression.Right as MemberExpression;
                var leftString = leftMember.Expression.ToString ();
                var rightString = rightMember.Expression.ToString ();
                if (evalExpression.NodeType == ExpressionType.AndAlso) {
                    if (!referenceList.Contains (leftString)) {
                        referenceList.Add (leftString);
                    }
                    if (!referenceList.Contains (rightString)) {
                        referenceList.Add (rightString);
                    }
                }
                if (evalExpression.NodeType == ExpressionType.OrElse) {
                    ArrayList clonedList = new ArrayList ();
                    if (referenceList.Count > 0) {
                        foreach (var entry in referenceList) {
                            clonedList.Add (entry);
                        }
                    }
                    if (!clonedList.Contains (leftString)) {
                        clonedList.Add (leftString);
                    }
                    mainList.Add (clonedList);
                    if (!referenceList.Contains (rightString)) {
                        referenceList.Add (rightString);
                    }
                }
                mainList.Add (referenceList);
                return mainList;
            }

            if (memberAccessMap["left"]) {
                member = evalExpression.Left as MemberExpression;
                nonMember = evalExpression.Right;
            } else if (memberAccessMap["right"]) {
                member = evalExpression.Right as MemberExpression;
                nonMember = evalExpression.Left;
            } else {
                throw new Exception ("no member/nonMember could be found...");
            }
            memberString = member.Expression.ToString ();

            if (null == member) {
                throw new Exception ("variable 'member' was null");
            }
            if (null == nonMember) {
                throw new Exception ("variable 'nonMember' was null");

            }

            // If is AndAlso add Member to innerlist
            // -- Continue in Process Node

            if (evalExpression.NodeType == ExpressionType.AndAlso) {
                if (!referenceList.Contains (memberString)) {
                    referenceList.Add (memberString);
                }
                BuildList (nonMember, ref referenceList, ref mainList);
            }

            // If is OrElse 
            // -- Clone existing list
            //    -- Add member to Cloned List 
            //    -- Add Cloned List to mainList
            // -- Continue in Process Node, pass existing list

            if (evalExpression.NodeType == ExpressionType.OrElse) {
                ArrayList clonedList = new ArrayList ();
                if (referenceList.Count > 0) {
                    foreach (var entry in referenceList) {
                        clonedList.Add (entry);
                    }
                }
                if (!clonedList.Contains (memberString)) {
                    clonedList.Add (memberString);
                }
                mainList.Add (clonedList);
                // return BuildList (nonMember, ref referenceList, ref mainList);
                try {
                    BuildList (nonMember, ref referenceList, ref mainList);
                } catch (System.Exception) { }
            }

            return mainList;
        }
        public dynamic Test (string StringToTest) {
            try {
                return ToStringCollection (StringToTest);
            } catch (System.Exception ex) {
                return ex;
            }
        }

    }
}