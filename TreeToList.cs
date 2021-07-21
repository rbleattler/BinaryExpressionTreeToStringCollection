using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using NReco.Linq;

namespace StringOps {
    public class BinaryExpressionTreeParser : BinaryExpressionTreeUtilities {
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

        public ArrayList BuildList (BinaryExpression evalExpression, ref ArrayList referenceList, ref ArrayList mainList) {
            return BuildList (evalExpression, ref referenceList, ref mainList, false);
        }
        private ArrayList BuildList (BinaryExpression evalExpression, ref ArrayList referenceList, ref ArrayList mainList, bool isMemberless) {
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
                ArrayList memberLessList = new ArrayList ();
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
                            if (!clonedList.Contains (entry)) {
                                clonedList.Add (entry);
                            }
                        }
                    }
                    if (!clonedList.Contains (leftString)) {
                        clonedList.Add (leftString);
                    }
                    if (!isMemberless) {
                        if (!mainList.Contains (clonedList)) {
                            mainList.Add (clonedList);
                        }
                    } else {
                        if (!memberLessList.Contains (clonedList)) {
                            memberLessList.Add (clonedList);
                        }
                    }
                    if (!referenceList.Contains (rightString)) {
                        referenceList.Add (rightString);
                    }
                }
                if (!isMemberless) {
                    if (!mainList.Contains (referenceList)) {
                        mainList.Add (referenceList);
                    }
                    return mainList;
                } else {
                    if (!memberLessList.Contains (referenceList)) {
                        memberLessList.Add (referenceList);
                    }
                    return memberLessList;
                }
            }

            //TODO: In this situation it is reasonable to assume that both sides are OR statements, and the current node is an AND statement. We need to build a list for both sides, where both new lists are built from the existing reference list if it exists. 
            //TODO: ?? find the side that has a member, if both, it doesnt matter which ?? -- new func? GetMemberSide?
            //TODO: ?? Maybe for this edge case.. a new function is needed to walk the or statements? Or... why can't we just use the existing or logic? I'll re-explore this
            if (!memberAccessMap["left"] && !memberAccessMap["right"]) {
                BinaryExpression root;
                BinaryExpression process;
                ArrayList rootList = new ArrayList ();
                ArrayList processList = new ArrayList ();
                ArrayList primaryList = new ArrayList ();
                ArrayList alternativeList = new ArrayList ();
                if (referenceList.Count > 0) {
                    foreach (var entry in referenceList) {
                        if (!primaryList.Contains (entry)) {
                            primaryList.Add (entry);

                        }
                        if (!alternativeList.Contains (entry)) {
                            alternativeList.Add (entry);
                        }
                    }
                }

                var leftCheck = ChildHasMember (evalExpression.Left);
                var rightCheck = ChildHasMember (evalExpression.Right);
                var memberSide = leftCheck ? "left" : (rightCheck ? "right" : null);
                if (memberSide == "left") {
                    root = evalExpression.Left as BinaryExpression;
                    process = evalExpression.Right as BinaryExpression;

                } else if (memberSide == "right") {
                    root = evalExpression.Right as BinaryExpression;
                    process = evalExpression.Left as BinaryExpression;
                } else {
                    throw new Exception ("no memberside" + evalExpression.ToString ());
                }
                rootList = BuildList (root, ref primaryList, ref mainList, true);
                if (rootList.Count > 1) {
                    foreach (ArrayList list in rootList) {
                        ArrayList innerList = new ArrayList (alternativeList);
                        foreach (string entry in list) {
                            // if (!alternativeList.Contains (entry)) {
                            //     alternativeList.Add (entry);
                            // }
                            if (!innerList.Contains (entry)) {
                                innerList.Add (entry);
                            }
                        }
                        BuildList (process, ref innerList, ref mainList, true);
                        // BuildList (process, ref alternativeList, ref mainList, true);
                    }
                } else {
                    if (!alternativeList.Contains (rootList[0])) {
                        alternativeList.Add (rootList[0]);
                    }
                    BuildList (process, ref alternativeList, ref mainList, true);
                }

                // try {
                //     subList = BuildList (evalExpression.Left as BinaryExpression, ref rightClonedList, ref mainList);
                //     foreach (ArrayList list in subList) {
                //         ArrayList innerSubList = list;
                //         int listIndex = subList.IndexOf (list);
                //         BuildList (evalExpression.Left as BinaryExpression, ref innerSubList, ref mainList);
                //     }
                // } catch (System.Exception) { }
                // try {
                //     BuildList (evalExpression.Right as BinaryExpression, ref rightClonedList, ref mainList);
                // } catch (System.Exception) { }

                //throw new Exception ("no member/nonMember could be found...");
                return null;
            }

            if (memberAccessMap["left"]) {
                member = evalExpression.Left as MemberExpression;
                nonMember = evalExpression.Right;
            } else if (memberAccessMap["right"]) {
                member = evalExpression.Right as MemberExpression;
                nonMember = evalExpression.Left;
            } else {
                throw new Exception ();
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