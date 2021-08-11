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

    public class ListUtilities : PowerShellLogging {
        public ListUtilities () { }

        // Merges Outer and inner lists, in such that each entry or list in each list will be treated as if they are in an OR statement together -- (IE: list0[0] || list0[1]). These lists will then combined -- each entry in the first list will be paired with each entry in the second list and added to the new output list. 
        //TODO: 
        // ? Create a new reference parameter to store the node type of the CHILD node in process
        // @ Get the node type from the variable of the selected node 
        // if neither left nor right are MemberAccess
        //  if THIS node is AndAlso
        //      process both nodes, store in vars
        //      create a new list PROCESSLIST
        //      pick a processed child
        //          If child is AndAlso
        //              for each entry in the list
        //              create a new list containing it and 
        //          If child is OrElse
        //  if THIS node is OrElse

        public void AddToList (dynamic item, ref ArrayList list) {
            WritePsVerbose ("[Enter AddToList]");
            if (!list.Contains (item)) {
                WritePsDebug ("[AddToList] : Item Added");
                list.Add (item);
            } else {
                WritePsDebug ("[AddToList] : Item Already In List");
            }
            WritePsVerbose ("[Exit AddToList]");
        }
        public void AddToList (dynamic item, ref List<dynamic> list) {
            WritePsVerbose ("[Enter AddToList]");
            if (!list.Contains (item)) {
                WritePsDebug ("[AddToList] : Item Added");
                list.Add (item);
            } else {
                WritePsDebug ("[AddToList] : Item Already In List");
            }
            WritePsVerbose ("[Exit AddToList]");
        }

        public void AddToList (dynamic[] item, ref ArrayList list) {
            WritePsVerbose ("[Enter AddToList]");
            foreach (var innerItem in item) {
                if (!list.Contains (innerItem)) {
                    WritePsDebug ("[AddToList] : Item Added");
                    list.Add (innerItem);
                } else {
                    WritePsDebug ("[AddToList] : Item Already In List");
                }
            }
            WritePsVerbose ("[Exit AddToList]");
        }
        public void AddToList (dynamic[] item, ref List<dynamic> list) {
            WritePsVerbose ("[Enter AddToList]");
            foreach (var innerItem in item) {
                if (!list.Contains (innerItem)) {
                    WritePsDebug ("[AddToList] : Item Added");
                    list.Add (innerItem);
                } else {
                    WritePsDebug ("[AddToList] : Item Already In List");
                }
            }
            WritePsVerbose ("[Exit AddToList]");
        }

        public dynamic ProcessNode (dynamic node) {
            ArrayList primaryList = new ArrayList ();
            ProcessNode (node, ref primaryList);
            return primaryList;
        }
        public dynamic ProcessNode (dynamic node, ref ArrayList primaryList) {
            ArrayList referenceList = new ArrayList ();
            return ProcessNode (node, ref primaryList, ref referenceList);
        }
        public dynamic ProcessNode (dynamic node, ref ArrayList referenceList, ref ArrayList primaryList) {
            WritePsDebug ("Enter ProcessNode");
            if (node is MemberExpression) {
                String nodeString = node.Expression.ToString ();
                WritePsDebug (nodeString + " is MemberExpression");
                return nodeString;
                // WritePsDebug ("Adding to primaryList...");
                // primaryList.Add(node.Expression.ToString ());
            }
            ExpressionType nodeExpressionType = node.NodeType;
            // ArrayList innerList = new ArrayList ();
            // ArrayList nestedList = new ArrayList ();
            dynamic leftNode = node.Left;
            dynamic rightNode = node.Right;

            WritePsDebug ("Process Left Node");
            dynamic leftProcessedNode = ProcessNode (leftNode, ref referenceList, ref primaryList);

            WritePsDebug ("Process Right Node");
            dynamic rightProcessedNode = ProcessNode (rightNode, ref referenceList, ref primaryList);

            // if the sidenodes aren't null, they must be strings. They should be processed based on the NodeType now
            if (null != leftProcessedNode) {
                if (nodeExpressionType is ExpressionType.AndAlso) {
                    WritePsDebug ("is AndAlso");
                    referenceList.Add (leftProcessedNode);
                }
            }

            if (null != rightProcessedNode) {
                if (nodeExpressionType is ExpressionType.AndAlso) {
                    WritePsDebug ("is AndAlso");
                    referenceList.Add (rightProcessedNode);
                }
            }

            // TODO: When the node is processed, 

            // if (leftProcessedNode is String) {
            //     referenceList.Add (leftProcessedNode);
            // }

            // if (rightProcessedNode is String) {
            //     referenceList.Add (rightProcessedNode);
            // }
            // if (leftProcessedNode is String && rightProcessedNode is String) {
            //     return referenceList;
            // }

            // if (nodeExpressionType is ExpressionType.AndAlso) {
            //     WritePsDebug ("is AndAlso");

            // }
            // if (nodeExpressionType is ExpressionType.OrElse) {
            //     WritePsDebug ("is OrElse");
            //     WritePsDebug ("nestedList.Add (ProcessNode (leftNode));");
            //     var leftProcessNode = ProcessNode (leftNode);
            //     if (leftProcessNode is String) {
            //         if (!referenceList.Contains (leftProcessNode)) {
            //             referenceList.Add (leftProcessNode);
            //         }
            //     } else {
            //         if (!nestedList.Contains (leftProcessNode)) {
            //             nestedList.Add (leftProcessNode);
            //         }
            //     }
            //     WritePsDebug ("nestedList.Add (ProcessNode (rightNode));");
            //     var rightProcessNode = ProcessNode (leftNode);
            //     if (rightProcessNode is String) {
            //         if (!referenceList.Contains (rightProcessNode)) {
            //             referenceList.Add (rightProcessNode);
            //         }
            //     } else {
            //         if (!nestedList.Contains (rightProcessNode)) {
            //             nestedList.Add (rightProcessNode);
            //         }
            //     }
            //     if (!referenceList.Contains (nestedList)) {
            //         referenceList.Add (nestedList);
            //     }
            //     // if (nestedList[0] is ArrayList) {
            //     //     WritePsDebug ("create mergedList");
            //     //     var mergedList = OrMerge (nestedList[0] as ArrayList, nestedList[1]);
            //     //     WritePsDebug ("Add mergedList to referenceList");
            //     //     referenceList.Add (mergedList);
            //     // } else if (nestedList[1] is ArrayList) {
            //     //     WritePsDebug ("create mergedList");
            //     //     var mergedList = OrMerge (nestedList[1] as ArrayList, nestedList[0]);
            //     //     WritePsDebug ("Add mergedList to referenceList");
            //     //     referenceList.Add (mergedList);
            //     // } else {
            //     //     WritePsDebug ("Add nestedList to referenceList");
            //     //     referenceList.Add (nestedList);
            //     // }
            // }
            // WritePsDebug ("Exiting ProcessNode...");
            return null;
        }

        // Merges the Inner Lists, but keeps the Two Lists as individual entries in the new master list

        // public ArrayList OrMerge (ArrayList mergeEach, dynamic mergeWith) {
        //     WritePsVerbose ("[Enter OrMerge]");
        //     ArrayList newList = new ArrayList ();
        //     foreach (var option in mergeEach) {
        //         ArrayList newInnerList = new ArrayList ();
        //         AddToList (option, ref newInnerList);
        //         AddToList (mergeWith, ref newInnerList);
        //         AddToList (newInnerList, ref newList);
        //     }
        //     WritePsVerbose ("[Exit OrMerge]");
        //     return newList;
        // }
        public ArrayList OrMerge (dynamic left, dynamic right, ExpressionType rightExpressionType) {
            WritePsVerbose ("[Enter OrMerge (dynamic, dynamic, ExpressionType)]");
            WritePsVerbose ("[OrMerge] ExpressionType : " + rightExpressionType.ToString ());
            WritePsVerbose ("[OrMerge] right Count : " + (right as ArrayList).Count.ToString ());
            ArrayList outList = new ArrayList ();
            if (rightExpressionType == ExpressionType.OrElse) {
                // ArrayList newList = new ArrayList ();
                foreach (var leftItem in left) {
                    foreach (var rightItem in right) {
                        var newItem = OrMerge (leftItem, rightItem);
                        foreach (var item in newItem) {
                            AddToList (item, ref outList);
                        }
                    }
                }
                WritePsVerbose ("[Exit OrMerge]");
                return outList;
            }
            if (rightExpressionType == ExpressionType.AndAlso) {
                if (left is ArrayList && right is ArrayList) {
                    left = left as ArrayList;
                    right = right as ArrayList;
                }
                AddToList (left, ref outList);
                AddToList (right, ref outList);
            }
            WritePsVerbose ("[Exit OrMerge]");
            return outList;
        }
        public ArrayList OrMerge (dynamic left, dynamic right) {
            WritePsVerbose ("[Enter OrMerge (dynamic, dynamic)]");
            ArrayList newList = new ArrayList ();
            if (left is ArrayList) {
                foreach (var option in left) {
                    ArrayList newInnerList = new ArrayList ();
                    AddToList (option, ref newInnerList);
                    AddToList (right, ref newInnerList);
                    AddToList (newInnerList, ref newList);
                }
            } else if (right is ArrayList) {
                foreach (var option in right) {
                    ArrayList newInnerList = new ArrayList ();
                    AddToList (option, ref newInnerList);
                    AddToList (left, ref newInnerList);
                    AddToList (newInnerList, ref newList);
                }
            } else {
                AddToList (left, ref newList);
                AddToList (right, ref newList);
            }
            WritePsVerbose ("[Exit OrMerge]");
            return newList;
        }

        public ArrayList AndMerge (ArrayList rootList, ArrayList processList) {
            WritePsVerbose ("[Enter AndMerge (ArrayList, ArrayList)]");
            ArrayList mainList = new ArrayList ();
            foreach (dynamic rootItem in rootList) {
                ArrayList newBaseList = new ArrayList ();
                if (rootItem is ArrayList) {
                    if (rootItem.Count > 1) {
                        foreach (var item in rootItem) {
                            AddToList (item, ref newBaseList);
                        }
                    } else {
                        AddToList (rootItem[0], ref newBaseList);
                    }
                } else {
                    AddToList (rootItem, ref newBaseList);
                }
                foreach (dynamic processItem in processList) {
                    ArrayList newList = new ArrayList (newBaseList);
                    if (processItem is ArrayList) {
                        if (processItem.Count > 1) {
                            foreach (var item in processItem) {
                                AddToList (item, ref newList);
                            }
                        } else {
                            AddToList (processItem[0], ref newList);
                        }
                    } else {
                        AddToList (processItem, ref newList);
                    }
                    AddToList (newList, ref mainList);
                }
            }
            WritePsVerbose ("[Exit AndMerge]");
            return mainList;
        }
        //TODO: Maybe make all of these use the one with the processMergeType?
        public ArrayList AndMerge (ArrayList rootList, ArrayList processList, ExpressionType processMergeType) {
            WritePsVerbose ("[Enter AndMerge (ArrayList, ArrayList, ExpressionType)]");
            ArrayList mainList = new ArrayList ();
            foreach (dynamic rootItem in rootList) {
                ArrayList newBaseList = new ArrayList ();
                if (rootItem is ArrayList) {
                    if (rootItem.Count > 1) {
                        foreach (var item in rootItem) {
                            AddToList (item, ref newBaseList);
                        }
                    } else {
                        AddToList (rootItem[0], ref newBaseList);
                    }
                } else {
                    AddToList (rootItem, ref newBaseList);
                }
                if (processMergeType == ExpressionType.AndAlso) {
                    foreach (dynamic processItem in processList) {
                        if (processItem is ArrayList) {
                            if (processItem.Count > 1) {
                                foreach (var item in processItem) {
                                    AddToList (item, ref newBaseList);
                                }
                            } else {
                                AddToList (processItem[0], ref newBaseList);
                            }
                        } else {
                            AddToList (processItem, ref newBaseList);
                        }
                        AddToList (newBaseList, ref mainList);
                    }
                }
                if (processMergeType == ExpressionType.OrElse) {
                    foreach (dynamic processItem in processList) {
                        ArrayList newList = new ArrayList (newBaseList);
                        if (processItem is ArrayList) {
                            // TODO : I think we need to do an OrMerge for each entry?
                            if (processItem.Count > 1) {
                                foreach (var item in processItem) {
                                    AddToList (item, ref newList);
                                }
                            } else {
                                AddToList (processItem[0], ref newList);
                            }
                        } else {
                            AddToList (processItem, ref newList);
                        }
                        AddToList (newList, ref mainList);
                    }
                }
            }
            WritePsVerbose ("[Exit AndMerge]");
            return mainList;
        }

        public ArrayList AndMerge (ArrayList mergeEach, object mergeWith) {
            WritePsVerbose ("[Enter AndMerge (ArrayList, Object)]");
            ArrayList newList = new ArrayList ();
            foreach (var item in mergeEach) {
                ArrayList newInnerList = new ArrayList ();
                if (item is ArrayList) {
                    foreach (var entry in item as ArrayList) {
                        AddToList (entry, ref newInnerList);
                    }
                } else {
                    AddToList (item, ref newInnerList);
                }
                AddToList (mergeWith, ref newInnerList);
                AddToList (newInnerList, ref newList);
            }
            WritePsVerbose ("[Exit AndMerge]");
            return newList;
        }

        public ArrayList Merge (dynamic left, dynamic right, ExpressionType expressionType) {
            return Merge (left, right, expressionType, expressionType);
        }
        public ArrayList Merge (dynamic left, dynamic right, ExpressionType expressionType, ExpressionType rightExpressionType) {
            WritePsVerbose ("[Enter Merge]");
            var leftObject = left is ArrayList ? left as ArrayList : left;
            var rightObject = right is ArrayList ? right as ArrayList : right;
            var checkTypes = new ArrayList ();
            AddToList (leftObject is ArrayList, ref checkTypes);
            AddToList (rightObject is ArrayList, ref checkTypes);

            if (!checkTypes.Contains (false)) {
                WritePsDebug ("All Lists");
                return MergeLists (leftObject, rightObject, expressionType, rightExpressionType);
            }
            if (checkTypes.Contains (false) && checkTypes.Contains (true)) {
                WritePsDebug ("Some Lists");
                ArrayList arrayObject;
                dynamic nonArrayObject;
                if (leftObject is ArrayList) {
                    arrayObject = leftObject;
                    nonArrayObject = rightObject;
                } else {
                    arrayObject = rightObject;
                    nonArrayObject = leftObject;
                }
                if (expressionType == ExpressionType.AndAlso) {
                    ArrayList outList;
                    try {
                        outList = AndMerge (arrayObject, nonArrayObject, rightExpressionType);
                    } catch (System.Exception ex) {
                        WritePsVerbose ("ERROR CALLING WITH RIGHTEXPRESSIONTYPE : " + ex.Message);
                        outList = AndMerge (arrayObject, nonArrayObject);
                    }
                    WritePsVerbose ("[Exit Merge]");
                    return outList;
                }
                if (expressionType == ExpressionType.OrElse) {
                    ArrayList outList;
                    try {
                        // If the statement is nested OR statements, we need to merge the lists into one larger list... which OrMerge doesn't do by default... apparently
                        if (expressionType == rightExpressionType) {
                            rightExpressionType = ExpressionType.AndAlso;
                        }
                        outList = OrMerge (arrayObject, nonArrayObject, rightExpressionType);
                    } catch (System.Exception ex) {
                        WritePsVerbose ("ERROR CALLING WITH RIGHTEXPRESSIONTYPE : " + ex.Message);
                        outList = OrMerge (arrayObject, nonArrayObject);
                    }
                    WritePsVerbose ("[Exit Merge]");
                    return outList;
                }
            }
            if (expressionType == ExpressionType.AndAlso) {
                WritePsDebug ("No Lists - AndAlso");
                ArrayList outList;
                try {
                    outList = AndMerge (leftObject, rightObject, rightExpressionType);
                } catch (System.Exception ex) {
                    WritePsVerbose ("ERROR CALLING WITH RIGHTEXPRESSIONTYPE : " + ex.Message);

                    outList = AndMerge (leftObject, rightObject);
                }
                WritePsVerbose ("[Exit Merge]");

                return outList;
            }
            if (expressionType == ExpressionType.OrElse) {
                WritePsDebug ("No Lists - OrElse");
                ArrayList outList;
                try {
                    outList = OrMerge (leftObject, rightObject);
                } catch (System.Exception ex) {
                    WritePsVerbose ("ERROR CALLING WITH RIGHTEXPRESSIONTYPE : " + ex.Message);
                    outList = OrMerge (leftObject, rightObject, rightExpressionType);
                }
                WritePsVerbose ("[Exit Merge]");
                return outList;
            }
            WritePsVerbose ("[Exit Merge]");
            return null;
        }

        // For merging a list's nested lists
        public ArrayList MergeNestedLists (ArrayList inputList, ExpressionType expressionType) {
            WritePsVerbose ("[Enter MergeNestedLists]");
            //region OrElse
            if (expressionType == ExpressionType.OrElse) {
                WritePsVerbose ("[MergeNestedLists] : expressionType : OrElse");
                WritePsVerbose ("[MergeNestedLists] : inputList Count : " + inputList.Count.ToString ());
                if (inputList.Count == 2) {
                    return OrMerge (inputList[0], inputList[1], expressionType);
                } else {
                    ArrayList outList = new ArrayList ();
                    for (var i = 1; i < inputList.Count; i++) {
                        AddToList (OrMerge (inputList[0], inputList[i]), ref outList);
                    }
                    WritePsVerbose ("[Exit MergeNestedLists]");
                    return outList;
                }
            }
            //endregion OrElse
            //region AndAlso
            if (expressionType == ExpressionType.AndAlso) {
                WritePsVerbose ("[MergeNestedLists] : expressionType : AndAlso");
                WritePsVerbose ("[MergeNestedLists] : inputList Count : " + inputList.Count.ToString ());
                if (inputList.Count == 2) {
                    return AndMerge ((ArrayList) inputList[0], (ArrayList) inputList[1], expressionType);
                } else {
                    ArrayList outList = new ArrayList ();
                    for (var i = 1; i < inputList.Count; i++) {
                        AddToList (AndMerge ((ArrayList) inputList[0], (ArrayList) inputList[i]), ref outList);
                    }
                    WritePsVerbose ("[Exit MergeNestedLists]");
                    return outList;
                }
            }
            //endregion AndAlso
            return null;
        }
        public ArrayList MergeLists (ArrayList rootList, ArrayList processList, ExpressionType expressionType) {
            return MergeLists (rootList, processList, expressionType, expressionType);
        }
        public ArrayList MergeLists (ArrayList rootList, ArrayList processList, ExpressionType expressionType, ExpressionType processExpressionType) {
            WritePsVerbose ("[Enter MergeLists]");
            switch (expressionType) {
                case ExpressionType.AndAlso:
                    // if (processExpressionType == ExpressionType.OrElse) {
                    //     return; 
                    // }
                    WritePsVerbose ("[Exit MergeLists]");
                    return AndMerge (rootList, processList, processExpressionType);
                case ExpressionType.OrElse:
                    WritePsVerbose ("[Exit MergeLists]");
                    return OrMerge (rootList, processList, processExpressionType);
                    // return null;
                default:
                    throw new Exception ("[MergeLists] : Unsupported Expression Type");
            }
        }
    }
}