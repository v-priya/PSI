namespace PSI;
using System.Xml.Linq;

// An basic XML code generator, implemented using the Visitor pattern
public class ExprXMLGen : Visitor<XElement> {
   public override XElement Visit (NLiteral lit)
      => New ("Literal").Set ("Value", lit.Value.Text).Set ("Type", lit.Type);

   public override XElement Visit (NIdentifier ident)
      => New ("Ident").Set ("Name", ident.Name).Set ("Type", ident.Type);

   public override XElement Visit (NUnary unary)
      => New ("Unary").Set ("Op", unary.Op.Kind).Set ("Type", unary.Type).AddE (unary.Expr.Accept (this));

   public override XElement Visit (NBinary binary) {
      var left = binary.Left.Accept (this); var right = binary.Right.Accept (this);
      var b = New ("Binary").Set ("Op", binary.Op.Kind).Set ("Type", binary.Type);
      b.AddE (left).AddE (right);
      return b;
   }

   public override XElement Visit (NFnCall func) {
      List<XElement> elems = new ();
      foreach (var p in func.Params) elems.Add (p.Accept (this));
      return New ("Function").Set ("Name", func.Name.Text).Set ("Type", func.Type).AddE (elems.ToArray ());
   }

   XElement New (string name)
      => new (name);
}

public static class XMLHelpers {
   public static XElement Set (this XElement elem, XName name, object value) {
      elem.SetAttributeValue (name, value);
      return elem;
   }

   public static XElement AddE (this XElement elem, params XElement[] child) {
      elem.Add (child);
      return elem;
   }
}