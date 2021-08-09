using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using NReco.Linq;

namespace TreeToString {
    public class BinaryExpressionTreeParser : BinaryExpressionTreeUtilities {
        public BinaryExpressionTreeParser () { }
        public LambdaParser LambdaParser = new LambdaParser ();

        public ArrayList ToStringCollection (string treeAsString) {
            WritePsVerbose ("[Enter ToStringCollection]");
            BinaryExpression parsedExpression;
            try {
                treeAsString = LevelString (treeAsString);
            } catch (System.Exception) {
                throw new Exception ("[ToStringCollection] : Failed on LevelString");
            }
            try {
                parsedExpression = LambdaParser.Parse (treeAsString) as BinaryExpression;
            } catch (System.Exception) {
                throw new Exception ("[ToStringCollection] : Failed on Parsing");
            }
            WritePsVerbose ("[Exit ToStringCollection]");
            return ToStringCollection (parsedExpression);
        }
        public ArrayList ToStringCollection (BinaryExpression treeExpression) {
            WritePsVerbose ("[Enter ToStringCollection]");
            ArrayList outerList = new ArrayList ();
            ArrayList referenceList = new ArrayList ();
            // try {
            var buildList = BuildList (treeExpression, ref referenceList, ref outerList);
            // } catch (System.Exception ex) {
            //     throw new Exception ("Failed on BuildList");
            // }
            if (outerList.Count == 0) {
                WritePsVerbose ("[Exit ToStringCollection]");
                return buildList;
            }
            WritePsVerbose ("[Exit ToStringCollection]");
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