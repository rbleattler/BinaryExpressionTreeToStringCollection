using System;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace StringOps {
    public class BinaryExpressionTreeUtilities {
        public string GetMemberSide (dynamic expression) {
            if (expression is BinaryExpression) {
                try {
                    return GetMemberSide (expression);
                } catch {
                    return "none";
                }
            } else {
                return "none";
            }
        }
        public string GetMemberSide (BinaryExpression expression) {
            string memberSide;
            Dictionary<string, bool> memberAccessMap = new Dictionary<string, bool> () {
                {
                "left",
                expression.Left.NodeType.ToString () == "MemberAccess"
                }, {
                "right",
                expression.Right.NodeType.ToString () == "MemberAccess"
                }
            };
            try {
                memberSide = memberAccessMap.Where (leaf => leaf.Value == true).First ().Key.ToString ();
            } catch {
                memberSide = null;
            }
            if (null == memberSide || memberSide.Length < 1) {
                memberSide = "none";
            }

            return memberSide;
        }
        public string LevelString (string inputString) {
            var chars = inputString.ToCharArray ();
            int leftOccurrences = Array.FindAll<char> (chars, x => x == '(').Count ();
            int rightOccurrences = Array.FindAll<char> (chars, x => x == ')').Count ();
            if (leftOccurrences != rightOccurrences) {
                if (leftOccurrences > rightOccurrences) {
                    Debug.WriteLine ("More Left");
                    inputString = inputString + ")";
                } else {
                    Debug.WriteLine ("More Right");
                    inputString = "(" + inputString;
                }
            }
            return inputString;
        }
        public bool ChildHasMember (dynamic expression) {
            if (expression is BinaryExpression) {
                return ChildHasMember (expression);
            } else {
                return false;
            }
        }
        public bool ChildHasMember (BinaryExpression expression) {
            ArrayList memberTypes = new ArrayList ();
            memberTypes.Add (expression.Left.NodeType);
            memberTypes.Add (expression.Right.NodeType);
            if (memberTypes.Contains (ExpressionType.MemberAccess)) {
                return true;
            } else {
                return false;
            }
        }

        protected ArrayList BuildList (BinaryExpression evalExpression, ref ArrayList referenceList, ref ArrayList mainList, bool isMemberless) {
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
            if (!memberAccessMap["left"] && !memberAccessMap["right"]) {
                // @ Experimenting
                if (!isMemberless) {
                    isMemberless = true;
                }
                //@
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
                            if (!innerList.Contains (entry)) {
                                innerList.Add (entry);
                            }
                        }
                        BuildList (process, ref innerList, ref mainList, true);
                    }
                } else {
                    if (!alternativeList.Contains (rootList[0])) {
                        alternativeList.Add (rootList[0]);
                    }
                    BuildList (process, ref alternativeList, ref mainList, true);
                }
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
                BuildList (nonMember, ref referenceList, ref mainList, false);
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
                try {
                    BuildList (nonMember, ref referenceList, ref mainList, false);

                } catch (System.Exception) { }
            }
            return mainList;
        }

    }
}