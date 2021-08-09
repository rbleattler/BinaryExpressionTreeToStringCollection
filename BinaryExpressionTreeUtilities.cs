using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using TreeToString;

namespace TreeToString {
    public class BinaryExpressionTreeUtilities : PowerShellLogging {
        ListUtilities listUtilities = new ListUtilities ();
        // TODO: Can I figure out the case to report at the beginning of processing? 
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
        public ArrayList BuildAllMembers (BinaryExpression evalExpression, ref ArrayList referenceList, ref ArrayList mainList, bool isMemberless) {
            WritePsVerbose ("[Enter BuildAllMembers]");
            SetPsVariable ("evalExpression", evalExpression);
            ArrayList memberLessList = new ArrayList ();
            var leftMember = evalExpression.Left as MemberExpression;
            var rightMember = evalExpression.Right as MemberExpression;
            var leftString = leftMember.Expression.ToString ();
            var rightString = rightMember.Expression.ToString ();
            if (evalExpression.NodeType == ExpressionType.AndAlso) {
                listUtilities.AddToList (leftString, ref referenceList);
                listUtilities.AddToList (rightString, ref referenceList);
            }
            if (evalExpression.NodeType == ExpressionType.OrElse) {
                ArrayList clonedList = new ArrayList ();
                if (referenceList.Count > 0) {
                    foreach (var entry in referenceList) {
                        listUtilities.AddToList (entry, ref clonedList);
                    }
                }
                listUtilities.AddToList (leftString, ref clonedList);
                if (!isMemberless) {
                    listUtilities.AddToList (clonedList, ref mainList);
                } else {
                    listUtilities.AddToList (clonedList, ref memberLessList);
                }
                listUtilities.AddToList (rightString, ref referenceList);
            }
            if (!isMemberless) {
                WritePsDebug ("[BuildAllMembers] : Not MemberLess");
                listUtilities.AddToList (referenceList, ref mainList);
                SetPsVariable ("referenceList", referenceList);
                SetPsVariable ("mainList", mainList);
                WritePsDebug ("[BuildAllMembers] : Return mainList");
                WritePsDebug ("[Exit BuildAllMembers]");
                referenceList = new ArrayList ();
                return mainList;
            } else {
                WritePsDebug ("[BuildAllMembers] : Is MemberLess");
                listUtilities.AddToList (referenceList, ref memberLessList);
                SetPsVariable ("memberLessList", memberLessList);
                WritePsDebug ("[BuildAllMembers] : Return memberLessList");
                WritePsDebug ("[Exit BuildAllMembers]");
                referenceList = new ArrayList ();
                return memberLessList;
            }
        }
        public ArrayList BuildMemberLess (BinaryExpression evalExpression, ref ArrayList referenceList, ref ArrayList mainList, bool isMemberless) {
            WritePsVerbose ("[Enter BuildMemberLess]");
            if (evalExpression.NodeType == ExpressionType.AndAlso) {
                WritePsVerbose ("[BuildMemberLess] : AndAlso -- Will Return");
                ListUtilities listUtilities = new ListUtilities ();
                ArrayList leftMainList = new ArrayList (mainList);
                ArrayList leftRefList = new ArrayList ();
                // ArrayList leftRefList = new ArrayList (referenceList);
                ArrayList rightMainList = new ArrayList (mainList);
                // ArrayList rightRefList = new ArrayList (referenceList);
                ArrayList rightRefList = new ArrayList ();
                ArrayList processedLeft = BuildList (evalExpression.Left as BinaryExpression, ref leftRefList, ref leftMainList, false);
                ArrayList processedRight = BuildList (evalExpression.Right as BinaryExpression, ref rightRefList, ref rightMainList, false);
                ArrayList outList = listUtilities.MergeLists (leftMainList, rightMainList, ExpressionType.AndAlso);
                SetPsVariable ("leftMainList", leftMainList);
                SetPsVariable ("leftRefList", leftRefList);
                SetPsVariable ("rightMainList", rightMainList);
                SetPsVariable ("rightRefList", rightRefList);
                SetPsVariable ("processedLeft", processedLeft);
                SetPsVariable ("processedRight", processedRight);
                SetPsVariable ("outList", outList);
                WritePsVerbose ("[Exit BuildMemberLess]");
                return outList;
            }
            WritePsVerbose ("OrElse -- Will Continue!");
            // @ Experimenting
            if (!isMemberless) {
                WritePsDebug ("[BuildMemberLess] : Memberless set to false, but contains no members...");
                isMemberless = true;
                WritePsDebug ("[BuildMemberLess] : Set Memberless to true...");
            }
            //@
            BinaryExpression root;
            BinaryExpression process;
            ArrayList rootList = new ArrayList ();
            ArrayList processList = new ArrayList (); //? Wat?
            ArrayList primaryList = new ArrayList ();
            ArrayList alternativeList = new ArrayList ();
            if (referenceList.Count > 0) {
                WritePsDebug ("[BuildMemberLess] : referenceList count > 0");
                foreach (var entry in referenceList) {
                    WritePsDebug ("iterate through referenceList");
                    listUtilities.AddToList (entry, ref primaryList);
                    listUtilities.AddToList (entry, ref alternativeList);
                }
            }
            WritePsDebug ("Checking which child has member");
            var leftCheck = ChildHasMember (evalExpression.Left);
            var rightCheck = ChildHasMember (evalExpression.Right);
            WritePsDebug ("Done checking which child has member");
            var memberSide = leftCheck ? "left" : (rightCheck ? "right" : null);
            if (memberSide == "left") {
                WritePsDebug ("[BuildMemberLess] : MemberSide = left");
                root = evalExpression.Left as BinaryExpression;
                process = evalExpression.Right as BinaryExpression;

            } else if (memberSide == "right") {
                WritePsDebug ("[BuildMemberLess] : MemberSide = right");
                root = evalExpression.Right as BinaryExpression;
                process = evalExpression.Left as BinaryExpression;
            } else {
                throw new Exception ("[BuildMemberLess] : no memberside" + evalExpression.ToString ());
            }
            rootList = BuildList (root, ref primaryList, ref mainList, true);
            if (rootList.Count > 1) {
                WritePsDebug ("rootList count > 1");
                WritePsDebug ("Begin iterating through rootList");
                foreach (ArrayList list in rootList) {
                    ArrayList innerList = new ArrayList (alternativeList);
                    foreach (string entry in list) {
                        listUtilities.AddToList (entry, ref innerList);
                    }
                    WritePsDebug ("try BuildList from innerList");
                    BuildList (process, ref innerList, ref mainList, true);
                    WritePsDebug ("BuildList from innerList : success");
                }
                WritePsDebug ("Finish iterating through rootList");
            } else {
                WritePsDebug ("[BuildMemberLess] : rootList count < 1");
                if (!alternativeList.Contains (rootList[0])) {
                    alternativeList.Add (rootList[0]);
                }
                WritePsDebug ("[BuildMemberLess] : try BuildList from alternativeList");
                if (process.Left.NodeType != ExpressionType.MemberAccess && process.Right.NodeType != ExpressionType.MemberAccess) {
                    BuildList (process, ref alternativeList, ref mainList, true);
                } else {
                    BuildList (process, ref alternativeList, ref mainList, false);
                }
                WritePsDebug ("[BuildMemberLess] : BuildList from alternativeList : success");
            }
            WritePsVerbose ("[BuildMemberLess] : [Exit BuildMemberLess]");
            return null;
        }
        protected ArrayList BuildList (BinaryExpression evalExpression, ref ArrayList referenceList, ref ArrayList mainList, bool isMemberless) {
            WritePsVerbose ("[Enter BuildList]");
            WritePsDebug ("[BuildList] : Begin BuildList" + evalExpression.ToString ());
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
                WritePsVerbose ("[BuildList] : All Members!");
                WritePsDebug ("[BuildList] : Return BuildAllMembers");
                WritePsVerbose ("[Exit BuildList]");
                return BuildAllMembers (evalExpression, ref referenceList, ref mainList, isMemberless);
            }

            if (!memberAccessMap["left"] && !memberAccessMap["right"]) {
                WritePsVerbose ("[BuildList] : No Members!");
                if (evalExpression.NodeType == ExpressionType.AndAlso) {
                    WritePsDebug ("[BuildList] : Return BuildMemberless");
                    WritePsVerbose ("[Exit BuildList]");
                    if (referenceList.Count < 1) {
                        return BuildMemberLess (evalExpression, ref referenceList, ref mainList, isMemberless);
                    } else {
                        ArrayList newList = BuildMemberLess (evalExpression, ref referenceList, ref mainList, isMemberless);
                        SetPsVariable ("memberLessNewList", newList);
                        ArrayList mergedList = listUtilities.MergeLists (referenceList, newList, evalExpression.NodeType);
                        SetPsVariable ("memberLessMergedList", mergedList);
                        return mergedList;
                    }
                }
                WritePsDebug ("[BuildList] : Build Memberless (not return)");
                BuildMemberLess (evalExpression, ref referenceList, ref mainList, isMemberless);
            }

            if (memberAccessMap["left"]) {
                WritePsVerbose ("[BuildList] : Left Member!");
                member = evalExpression.Left as MemberExpression;
                nonMember = evalExpression.Right;
            } else if (memberAccessMap["right"]) {
                WritePsVerbose ("[BuildList] : Right Member!");
                member = evalExpression.Right as MemberExpression;
                nonMember = evalExpression.Left;
            } else {
                WritePsDebug ("[BuildList] : No memberAccessMap");
                throw new Exception ();
            }

            WritePsDebug ("[BuildList] : Try Set memberString");
            memberString = member.Expression.ToString ();
            WritePsDebug ("[BuildList] : Set memberString : success");

            if (null == member) {
                throw new Exception ("[BuildList] : variable 'member' was null");
            }
            if (null == nonMember) {
                throw new Exception ("[BuildList] : variable 'nonMember' was null");

            }

            // If is AndAlso add Member to innerlist
            // -- Continue in Process Node

            if (evalExpression.NodeType == ExpressionType.AndAlso) {
                WritePsVerbose ("[BuildList] : NodeType = AndAlso");
                if (!referenceList.Contains (memberString)) {
                    WritePsDebug ("[BuildList] : referenceList not Contains " + memberString);
                    WritePsDebug ("[BuildList] : Try Add memberString to referenceList");
                    listUtilities.AddToList (memberString, ref referenceList);
                    WritePsDebug ("[BuildList] : Add memberString to referenceList : success");
                }
                try {
                    WritePsDebug ("[BuildList] : try Buildlist with nonMember and referenceList (not memberLess)");
                    BuildList (nonMember, ref referenceList, ref mainList, false); // This adds the nonMember's left member to the reference list
                    WritePsDebug ("[BuildList] : Buildlist with nonMember and referenceList (not memberLess) : success");
                } catch (System.Exception) {
                    throw new Exception ("[BuildList] : Failed to continue in Process Node");
                }
            }

            // If is OrElse 
            // -- Clone existing list
            //    -- Add member to Cloned List 
            //    -- Add Cloned List to mainList
            // -- Continue in Process Node, pass existing list

            if (evalExpression.NodeType == ExpressionType.OrElse) {
                WritePsVerbose ("[BuildList] : NodeType = OrElse");
                ArrayList clonedList = new ArrayList ();
                if (referenceList.Count > 0) {
                    WritePsDebug ("[BuildList] : referenceList count > 0");
                    WritePsDebug ("[BuildList] : Begin iterate through referenceList");
                    foreach (var entry in referenceList) {
                        listUtilities.AddToList (entry, ref clonedList);
                    }
                    WritePsDebug ("[BuildList] : End iterate through referenceList");
                }
                listUtilities.AddToList (memberString, ref clonedList);
                listUtilities.AddToList (clonedList, ref mainList);
                try {
                    WritePsDebug ("[BuildList] : try BuildList with nonMember and referenceList");
                    BuildList (nonMember, ref referenceList, ref mainList, false);
                    WritePsDebug ("[BuildList] : BuildList with nonMember and referenceList : Success");
                } catch (System.Exception) {
                    throw new Exception ("[BuildList] : Error in Final Block before return running BuildList");
                }
            }
            WritePsDebug ("[BuildList] : End BuildList" + evalExpression.ToString ());
            try {
                WritePsDebug ("[BuildList] : try return MainList");
                WritePsVerbose ("[Exit BuildList]");
                return mainList;
            } catch (System.Exception) {
                throw new Exception ("[BuildList] : Return Failure");
            }
            // return mainList;
        }

    }
}