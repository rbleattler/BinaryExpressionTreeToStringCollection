using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using NReco.Linq;

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

    }
}