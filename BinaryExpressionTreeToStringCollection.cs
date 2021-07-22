using System.Collections;
using System.Linq.Expressions;
using NReco.Linq;

namespace StringOps
{
    public class BinaryExpressionTreeParser : BinaryExpressionTreeUtilities {
            public BinaryExpressionTreeParser () { }
            public LambdaParser LambdaParser = new LambdaParser ();

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
            public dynamic Test (string StringToTest) {
                try {
                    return ToStringCollection (StringToTest);
                } catch (System.Exception ex) {
                    return ex;
                }
            }

        }
    }