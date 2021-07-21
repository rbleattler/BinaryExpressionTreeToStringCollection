using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace StringOps {
    public class ExpressionTree {
        public enum OperationTypes { Not, And, Or }

        public static Dictionary<string, OperationTypes> OperationTypeMap = new Dictionary<string, OperationTypes> { { "NOT", OperationTypes.Not },
            { "!", OperationTypes.Not },
            { "AND", OperationTypes.And },
            { "&&", OperationTypes.And },
            { "OR", OperationTypes.Or },
            { "||", OperationTypes.Or }
        };

        public class Group {
            public OperationTypes OperationType;
            public Group Parent;

            public List<Group> Groups = new List<Group> ();
            public List<Identifier> Identifiers = new List<Identifier> ();
        }

        public class Identifier {
            public OperationTypes OperationType;
            public Group Parent;

            public int Id;
        }

        public Group Root;
    }

    public class ValueVisitor : Visitor<bool> {
        public bool Visit (OrNode node) {
            return node.Childs.Aggregate (false, (current, child) => current | child.Accept (this));
        }

        public bool Visit (NotNode node) {
            return !node.Childs[0].Accept (this);
        }

        public bool Visit (AndNode node) {
            return node.Childs.Aggregate (true, (current, child) => current & child.Accept (this));
        }

        public bool Visit (IdNode node) {
            return node.Value;
        }
    }

    public interface Visitor<T> {
        T Visit (OrNode node);
        T Visit (NotNode node);
        T Visit (AndNode node);
        T Visit (IdNode node);
    }
    public abstract class ANode {
        public List<ANode> Childs { get; }
        public abstract T Accept<T> (Visitor<T> visitor);
    }
    public class OrNode : ANode {
        public override T Accept<T> (Visitor<T> visitor) {
            return visitor.Visit (this);
        }
    }
    public class NotNode : ANode {
        public override T Accept<T> (Visitor<T> visitor) {
            return visitor.Visit (this);
        }
    }

    public class AndNode : ANode {
        public override T Accept<T> (Visitor<T> visitor) {
            return visitor.Visit (this);
        }
    }

    public class IdNode : ANode {
        public bool Value;
        public override T Accept<T> (Visitor<T> visitor) {
            return visitor.Visit (this);
        }
    }

    public class StackOverflow_6915554 {
        [DataContract]
        [KnownType (typeof (LeafExpression))]
        [KnownType (typeof (BinaryExpression))]
        public class Expression { }

        [DataContract]
        public class LeafExpression : Expression {
            [DataMember]
            public string Name;
        }

        [DataContract]
        public class BinaryExpression : Expression {
            [DataMember]
            public BinaryOperator Operator;
            [DataMember]
            public Expression Left;
            [DataMember]
            public Expression Right;
        }

        public enum BinaryOperator {
            And,
            Or,
        }

        public static void Test () {
            List<Expression> expressions = new List<Expression> ();
            expressions.Add (new BinaryExpression {
                Left = new BinaryExpression {
                        Left = new LeafExpression { Name = "A" },
                            Operator = BinaryOperator.And,
                            Right = new LeafExpression { Name = "B" },
                    },
                    Operator = BinaryOperator.Or,
                    Right = new BinaryExpression {
                        Left = new LeafExpression { Name = "C" },
                            Operator = BinaryOperator.And,
                            Right = new LeafExpression { Name = "D" },
                    }
            });

            expressions.Add (new BinaryExpression {
                Left = new LeafExpression { Name = "E" },
                    Operator = BinaryOperator.Or,
                    Right = new LeafExpression { Name = "F" }
            });

            expressions.Add (new LeafExpression { Name = "G" });

            XmlWriterSettings ws = new XmlWriterSettings {
                Indent = true,
                IndentChars = "  ",
                Encoding = new UTF8Encoding (false),
                OmitXmlDeclaration = true,
            };

            MemoryStream ms = new MemoryStream ();
            XmlWriter w = XmlWriter.Create (ms, ws);

            DataContractSerializer dcs = new DataContractSerializer (typeof (List<Expression>));
            dcs.WriteObject (w, expressions);
            w.Flush ();

            Console.WriteLine (Encoding.UTF8.GetString (ms.ToArray ()));
        }
    }

}