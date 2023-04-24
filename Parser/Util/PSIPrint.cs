// ⓅⓈⒾ  ●  Pascal Language System  ●  Academy'23
// PSIPrint.cs ~ Prints a PSI syntax tree in Pascal format
// ─────────────────────────────────────────────────────────────────────────────
namespace PSI;

public class PSIPrint : Visitor<StringBuilder> {
   public override StringBuilder Visit (NProgram p) {
      Write ($"program {p.Name}; ");
      Visit (p.Block);
      return Write (".");
   }

   public override StringBuilder Visit (NBlock b) 
      => Visit (b.Decls, b.Body);

   public override StringBuilder Visit (NDeclarations d) {
      if (d.Vars.Length > 0) {
         NWrite ("var"); N++;
         foreach (var g in d.Vars.GroupBy (a => a.Type))
            NWrite ($"{g.Select (a => a.Name).ToCSV ()} : {g.Key};");
         N--;
      }
      Visit (d.Funcs);
      return S;
   }

   public override StringBuilder Visit (NVarDecl d)
      => NWrite ($"{d.Name} : {d.Type}");

   public override StringBuilder Visit (NCompoundStmt b) {
      NWrite ("begin"); N++;  Visit (b.Stmts); N--; return NWrite ("end"); 
   }

   public override StringBuilder Visit (NAssignStmt a) {
      NWrite ($"{a.Name} := "); a.Expr.Accept (this); return Write (";");
   }

   public override StringBuilder Visit (NWriteStmt w)
      => VisitN (w.NewLine ? "WriteLn" : "Write", nodes: w.Exprs);

   public override StringBuilder Visit (NLiteral t)
      => Write (t.Value.ToString ());

   public override StringBuilder Visit (NIdentifier d)
      => Write (d.Name.Text);

   public override StringBuilder Visit (NUnary u) {
      Write (u.Op.Text); return u.Expr.Accept (this);
   }

   public override StringBuilder Visit (NBinary b) {
      Write ("("); b.Left.Accept (this); Write ($" {b.Op.Text} ");
      b.Right.Accept (this); return Write (")");
   }

   public override StringBuilder Visit (NFnCall f)
      => VisitN (f.Name.Text, false, f.Params);

   public override StringBuilder Visit (NFnDecl f) {
      NWrite($"{(f.Func ? "function" : "procedure")} {f.Name.Text} (");
      Write (string.Join ("; ", f.Vars.GroupBy (a => a.Type)
         .Select (g => $"{g.Select (a => a.Name.Text).ToCSV ()} : {g.Key}")));
      Write ($"){(f.Func ? $": {f.Type}" : "")};");
      f.Block.Accept (this);
      return Write (";");
   }

   public override StringBuilder Visit (NReadStmt r)
      => NWrite ($"read ({r.Vars.ToCSV ()});");

   public override StringBuilder Visit (NCallStmt c)
      => VisitN (c.Name.Text, nodes: c.Args);

   public override StringBuilder Visit (NIfStmt f) {
      NWrite ($"if "); f.If.Accept (this);
      Write (" then "); N++; f.Then.Accept (this); N--;
      if (f.Else != null) { NWrite ("else "); N++; f.Else.Accept (this); N--; }
      return S;
   }

   public override StringBuilder Visit (NWhileStmt w) {
      NWrite ("while "); w.Cond.Accept (this);
      Write (" do "); w.Body.Accept (this);
      return Write (";");
   }

   public override StringBuilder Visit (NRepeatStmt r) {
      NWrite ("repeat");
      N++; Visit (r.Stmts); N--;
      NWrite ("until "); r.Cond.Accept (this);
      return Write (";");
   }

   public override StringBuilder Visit (NForStmt f) {
      NWrite ($"for {f.Var.Text} := ");
      f.Start.Accept (this);
      Write (f.Decrement ? " down to " : " to ");
      f.End.Accept (this);
      Write (" do"); N++;
      f.Body.Accept (this);
      N--;
      return S;
   }

   StringBuilder Visit (params Node[] nodes) {
      nodes.ForEach (a => a.Accept (this));
      return S;
   }

   // Helper to visit and write multiple parameters in format --> Name (arg1 [, arg2...argN]);
   StringBuilder VisitN (string name, bool newline = true, params Node[] nodes) {
      string s = $"{name} (";
      if (newline) NWrite (s); else Write (s);
      for (int i = 0; i < nodes.Length; i++) {
         if (i > 0) Write (", ");
         nodes[i].Accept (this);
      }
      return Write ($"){(newline ? ";" : "")}");
   }

   // Writes in a new line
   StringBuilder NWrite (string txt) 
      => Write ($"\n{new string (' ', N * 3)}{txt}");
   int N;   // Indent level

   // Continue writing on the same line
   StringBuilder Write (string txt) {
      Console.Write (txt);
      S.Append (txt);
      return S;
   }

   readonly StringBuilder S = new ();
}