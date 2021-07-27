using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using NReco.Linq;

namespace StringOps {
    public class BinaryExpressionTreeParser : BinaryExpressionTreeUtilities {
        public BinaryExpressionTreeParser () { }
        public LambdaParser LambdaParser = new LambdaParser ();

        public string LevelString (string inputString) {
            var chars = inputString.ToCharArray ();
            int leftOccurrences = Array.FindAll<char> (chars, x => x == '(').Count (); 
            int rightOccurrences = Array.FindAll<char> (chars, x => x == ')').Count ();
            if (leftOccurrences != rightOccurrences) {
                if (leftOccurrences > rightOccurrences) {
                    Debug.WriteLine("More Left");
                    inputString = inputString + ")";
                } else {
                    Debug.WriteLine("More Right");
                    inputString = "(" + inputString;
                }
            }
            return inputString;
        }

        public ArrayList ToStringCollection (string treeAsString) {
            treeAsString = LevelString(treeAsString);
            BinaryExpression parsedExpression = LambdaParser.Parse (treeAsString) as BinaryExpression;
            return ToStringCollection (parsedExpression);
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
        public dynamic Test (string StringToTest) {
            try {
                return ToStringCollection (StringToTest);
            } catch (System.Exception ex) {
                return ex;
            }
        }

    }
}