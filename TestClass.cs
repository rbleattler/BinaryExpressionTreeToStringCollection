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
    public class TestClass : ListUtilities {

        //TODO: remove?
        public string GetNodeType (dynamic node) {
            if (node is Expression) {
                if (node is BinaryExpression) {
                    return "BinaryExpression";
                }
                if (node is MemberExpression) {
                    return "MemberExpression";
                }
            }
            return null;
        }

        public ArrayList MergeExpressions (dynamic left, dynamic right, ExpressionType parentType) {
            dynamic processLeft; // = Process (left);
            dynamic processRight; // = Process (right);
            processLeft = Process (left);
            processRight = Process (right);

            if (null != processLeft && null != processRight) {
                WritePsVerbose ("Merging left and right | " + parentType);
                ArrayList merged = Merge (processLeft, processRight, parentType, right.NodeType);
                // ArrayList merged = Merge (leftList, rightList, parentType);
                if (merged.Count > 1) {
                    WritePsDebug ("Return Merged");
                    return merged;
                }
                WritePsDebug ("Return Merged[0]");
                return merged[0] as ArrayList;
            } else {
                throw new Exception ("Not all processes were not null...");
            }
            // return null;
        }
        public dynamic Process (dynamic processItem) {
            if (processItem is ArrayList) {
                return processItem;
            }
            if (processItem is MemberExpression) {
                return (processItem as MemberExpression).Expression.ToString ();
            }
            if (processItem is BinaryExpression) {
                processItem = processItem as BinaryExpression;
                return MergeExpressions (
                    processItem.Left,
                    processItem.Right,
                    processItem.NodeType
                );
            }
            return null;
        }
    }
}