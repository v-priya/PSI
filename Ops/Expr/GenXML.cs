namespace PSI;
using System.Xml.Linq;

// An basic XML code generator, implemented using the Visitor pattern
public class ExprXMLGen : Visitor<XElement> {
   XElement? mElem;

   public override XElement Visit (NLiteral lit)
      => New ("Literal").Set ("Value", lit.Value.Text).Set ("Type", lit.Type);

   public override XElement Visit (NIdentifier ident)
      => New ("Ident").Set ("Name", ident.Name).Set ("Type", ident.Type);

   public override XElement Visit (NUnary unary) {
      var a = unary.Expr.Accept (this);
      mElem = New ("Unary").Set ("Op", unary.Op.Kind).Set ("Type", unary.Type);
      mElem.Add (a);
      return mElem;
   }

   public override XElement Visit (NBinary binary) {
      var left = binary.Left.Accept (this); var right = binary.Right.Accept (this);
      mElem = New ("Binary").Set ("Op", binary.Op.Kind).Set ("Type", binary.Type);
      mElem.Add (left); mElem.Add (right);
      return mElem;
   }

   public override XElement Visit (NFnCall func) {
      List<XElement> elems = new ();
      foreach (var p in func.Params) elems.Add (p.Accept (this));
      mElem = New ("Function").Set ("Name", func.Name.Text).Set ("Type", func.Type);
      mElem.Add (elems);
      return mElem;
   }

   public void SaveTo (string file) => File.WriteAllText (file, mElem?.ToString ());

   XElement New (string name)
      => new (name);
}

public static class XMLHelpers {
   public static XElement Set (this XElement elem, XName name, object value) {
      elem.SetAttributeValue (name, value);
      return elem;
   }
}