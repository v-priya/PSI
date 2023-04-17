using PSI;
using PSI.Ops;
using System.Diagnostics;

static class Start {
   static void Main () {
      Test1 ();      // Test ExprEval and ExprILGen
      Test2 ();      // Test ExprTyper and ExprGrapher
      Test3 ();      // Type checks on various expressions
      Test4 ();      // Tokenizer - printout of invalid token
      Test5 ();      // Test Functions
   }

   // Test ExprEval and ExprILGen
   static void Test1 () {
      string expr = "(3 + 2) * 4 - 17 * -five * (two + 1 + 4 + 5)";
      var node = Parse (expr);
      PrintHeader (expr);
      Dictionary<string, int> vars = new () { ["five"] = 5, ["two"] = 2 };
      Eval (node, vars);
      GenIL (node);
      Console.Write ("\nPress any key..."); Console.ReadKey (true);
   }

   // Test type-assignment, graph generation, XML generation
   static void Test2 () {
      string expr = "(pi + 3.5) + 2 <= 1 <> \"Hello\" + two > true + \"World\"";
      var node = Parse (expr);
      PrintHeader (expr);
      Dictionary<string, NType> types = new () { ["pi"] = NType.Real, ["two"] = NType.Int };
      GenType (node, types);
      GenGraph (expr, node);
      GenXML (node);
      Console.Write ("\nPress any key..."); Console.ReadKey (true);
   }

   // Type checks of various expressions
   static void Test3 () {
      Console.WriteLine ("-----------------");
      string[] mTypeCheck = new[] {
         "12", "13.5", "true", "\"hello!\"", "'A'", "-12", "-13.5", "five", "pi", "-two",
         "12 + 3", "13.5 - 3.5", "3 * 4", "13.3 / 3.5", "13 / 3.5", "3.5 * 2", "\"A\" + \"B\"",
         "'A' + 'B'", "\"A\" + true", "13.5 + \"B\"", "3 < 2", "3.5 < 2", "\"ABC\" < \"DEF\"",
         "'a' < 'b'", "true < false", "true = true", "3.5 = 2", "\"ABC\" = \"abc\"", "3 = true",
         "3 and 4", "true and false", "2 and not 7", "12 mod 3"
      };
      Dictionary<string, NType> types = new () { ["five"] = NType.Int, ["pi"] = NType.Real, ["two"] = NType.Int };
      foreach (var s in mTypeCheck) {
         var node = new Parser (new Tokenizer (s)).Parse ();
         var type = node.Accept (new ExprTyper (types));
         Console.WriteLine ($"{s,20} : {type}");
      }
      Console.Write ("\nPress any key..."); Console.ReadKey (true);
   }

   // Tokenizer test of valid and invalid programs
   static void Test4 () {
      Console.WriteLine ("-----------------");
      Console.WriteLine ("Valid program");
      var tokenizer = new Tokenizer (Prog0);
      int line = 1;
      for (; ; ) {
         var token = tokenizer.Next ();
         if (token.Kind == Token.E.EOF) break;
         while (line < token.Line) { Console.WriteLine (); line++; }
         Console.Write ($"{token}  ");
      }
      Console.WriteLine (); Console.WriteLine ();

      Console.WriteLine ("Testing invalid program:");
      var prog1 = Prog0.Replace ("prod * i;", "prod * i?");
      tokenizer = new Tokenizer (prog1);
      for (; ; ) {
         var token = tokenizer.Next ();
         if (token.Kind == Token.E.ERROR) { token.PrintError (); break; }
         if (token.Kind == Token.E.EOF) break;
      }
      Console.WriteLine ();
      Console.Write ("\nPress any key..."); Console.ReadKey (true);
   }
   static string Prog0 = """
      program Expr;
      var
        i, fib: integer;

      function Fibo (n: integer) : integer;
      var 
        i, prod: integer;
      begin 
        prod := 1;
        for i := 1 to n do begin
          prod := prod * i;
        end
        Fibo := prod;
      end;

      begin
        for i := 1 to 10 do begin
          fib := Fibo (i);
          WriteLn ("Fibo(", i, ") = ", fib);
        end
      end.
      """;

   static void Test5 () {
      string expr = "12.0 + pi + sin (3.5) + atan2 (12, 13.5) + length (\"hello\") + random ()";
      var node = Parse (expr);
      PrintHeader (expr);
      Dictionary<string, NType> types = new () { ["pi"] = NType.Real, ["sin"] = NType.Real,
         ["atan2"] = NType.Real, ["length"] = NType.Int, ["random"] = NType.Int };
      GenType (node, types);
      GenGraph (expr, node);
      GenXML (node);
      GenIL (node);
      Console.Write ("\nPress any key..."); Console.ReadKey (true);
   }

   // Helpers
   static NExpr Parse (string expr) {
      return new Parser (new Tokenizer (expr)).Parse ();
   }

   static void PrintHeader (string expr) {
      Console.WriteLine ("-----------------");
      Console.WriteLine ($"Expression = {expr}");
   }

   static void Eval (NExpr node, Dictionary<string, int> vars) {
      int value = node.Accept (new ExprEvaluator (vars));
      Console.WriteLine ($"Value = {value}");
   }

   static void GenType (NExpr node, Dictionary<string, NType> types) {
      NType type = node.Accept (new ExprTyper (types));
      Console.WriteLine ($"Type = {type}");
   }

   static void GenIL (NExpr node) {
      var il = node.Accept (new ExprILGen ());
      Console.WriteLine ($"\nIL Code = \n{il}");
   }

   static void GenGraph (string expr, NExpr node) {
      var graph = new ExprGrapher (expr);
      node.Accept (graph);
      Directory.CreateDirectory ("c:/etc");
      graph.SaveTo ("c:/etc/test.html");
      var pi = new ProcessStartInfo ("c:/etc/test.html") { UseShellExecute = true };
      Process.Start (pi);
   }

   static void GenXML (NExpr node) {
      var xml = new ExprXMLGen ();
      var elem = node.Accept (xml);
      File.WriteAllText ("c:/etc/test.xml", elem.ToString ());
      var pi = new ProcessStartInfo ("c:/etc/test.xml") { UseShellExecute = true };
      Process.Start (pi);
   }
}