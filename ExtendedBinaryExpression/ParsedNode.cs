using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TreeToString;

namespace TreeToString.ExtendedBinaryExpression {
    public class ParsedNode {
        private ListUtilities listUtilities = new ListUtilities ();
        public ExpressionType NodeType;
        public ExtendedBinaryExpression RawExpression;
        public List<dynamic> Entries = new List<dynamic> ();
        public List<dynamic> StringEntries = new List<dynamic> (); //TODO: StringEntries

        public ParsedNode () { }

        // public void UpdateStringEntries (ExtendedBinaryExpression rawNode) {
        //     throw new NotImplementedException ();
        // }
        public void AddEntry (dynamic entry) {
            try {
                listUtilities.AddToList (entry, ref this.Entries);
            } catch (System.Exception) { }
        }
        public ParsedNode (ExpressionType nodeType, dynamic[] entries) {
            this.NodeType = nodeType;
            listUtilities.AddToList (entries, ref this.Entries);
        }
        public ParsedNode (ExpressionType nodeType, dynamic entry) {
            this.NodeType = nodeType;
            listUtilities.AddToList (entry, ref this.Entries);
        }
        public ParsedNode (ExpressionType nodeType) {
            this.NodeType = nodeType;
        }
        public ParsedNode (ExtendedBinaryExpression rawExpression, ExpressionType nodeType, dynamic[] entries) {
            this.RawExpression = rawExpression;
            this.NodeType = nodeType;
            listUtilities.AddToList (entries, ref this.Entries);
        }
        public ParsedNode (ExtendedBinaryExpression rawExpression, ExpressionType nodeType, dynamic entry) {
            this.RawExpression = rawExpression;
            this.NodeType = nodeType;
            listUtilities.AddToList (entry, ref this.Entries);
        }
        public ParsedNode (ExtendedBinaryExpression rawExpression, ExpressionType nodeType) {
            this.RawExpression = rawExpression;
            this.NodeType = nodeType;
        }

    }
}