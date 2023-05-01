// ⓅⓈⒾ  ●  Pascal Language System  ●  Academy'23
// TypeAnalyze.cs ~ Type checking, type coercion
// ─────────────────────────────────────────────────────────────────────────────
namespace PSI;

using System.Xml.Linq;
using static NType;
using static Token.E;

public class TypeAnalyze : Visitor<NType> {
   public TypeAnalyze () {
      mSymbols = SymTable.Root;
   }
   SymTable mSymbols;

   #region Declarations ------------------------------------
   public override NType Visit (NProgram p) 
      => Visit (p.Block);
   
   public override NType Visit (NBlock b) {
      mSymbols = new SymTable { Parent = mSymbols };
      Visit (b.Declarations); Visit (b.Body);
      mSymbols = mSymbols.Parent;
      return Void;
   }

   public override NType Visit (NDeclarations d) {
      Visit (d.Consts); Visit (d.Vars); return Visit (d.Funcs);
   }

   public override NType Visit (NConstDecl c) {
      if (!Has (c.Name)) mSymbols.Consts.Add (c);
      else throw new ParseException (c.Name, $"'{c.Name.Text}' already defined.");
      if (c.Expr != null) {
         c.Expr.Accept (this);
         c.Expr = AddTypeCast (c.Name, c.Expr, c.Expr.Type);
         c.Type = c.Expr.Type;
      }
      return c.Type;
   }

   public override NType Visit (NVarDecl d) {
      if (!Has (d.Name)) mSymbols.Vars.Add (d);
      else throw new ParseException (d.Name, $"'{d.Name.Text}' already defined.");
      return d.Type;
   }

   public override NType Visit (NFnDecl f) {
      if (!Has (f)) mSymbols.Funcs.Add (f);
      Visit (f.Params); f.Body?.Accept (this);
      return f.Return;
   }
   #endregion

   #region Statements --------------------------------------
   public override NType Visit (NCompoundStmt b)
      => Visit (b.Stmts);

   public override NType Visit (NAssignStmt a) {
      if (!Has (a.Name, false))
         throw new ParseException (a.Name, "Unknown variable");
      var t = Check (a.Name);
      a.Expr.Accept (this);
      a.Expr = AddTypeCast (a.Name, a.Expr, t);
      return t;
   }
   
   NExpr AddTypeCast (Token token, NExpr source, NType target) {
      if (source.Type == target) return source;
      bool valid = (source.Type, target) switch {
         (Int, Real) or (Char, Int) or (Char, String) => true,
         _ => false
      };
      if (!valid) throw new ParseException (token, $"Expected {target}, found {source.Type}");
      return new NTypeCast (source) { Type = target };
   }

   public override NType Visit (NWriteStmt w)
      => Visit (w.Exprs);

   public override NType Visit (NIfStmt f) {
      f.Condition.Accept (this);
      f.IfPart.Accept (this); f.ElsePart?.Accept (this);
      return Void;
   }

   public override NType Visit (NForStmt f) {
      f.Start.Accept (this); f.End.Accept (this); f.Body.Accept (this);
      return Void;
   }

   public override NType Visit (NReadStmt r) => Void;

   public override NType Visit (NWhileStmt w) {
      w.Condition.Accept (this); w.Body.Accept (this);
      return Void; 
   }

   public override NType Visit (NRepeatStmt r) {
      Visit (r.Stmts); r.Condition.Accept (this);
      return Void;
   }

   public override NType Visit (NCallStmt c) 
      => Check (c.Name, c.Params);
   #endregion

   #region Expression --------------------------------------
   public override NType Visit (NLiteral t) {
      t.Type = t.Value.Kind switch {
         L_INTEGER => Int, L_REAL => Real, L_BOOLEAN => Bool, L_STRING => String,
         L_CHAR => Char, _ => Error,
      };
      return t.Type;
   }

   public override NType Visit (NUnary u) 
      => u.Expr.Accept (this);

   public override NType Visit (NBinary bin) {
      NType a = bin.Left.Accept (this), b = bin.Right.Accept (this);
      bin.Type = (bin.Op.Kind, a, b) switch {
         (ADD or SUB or MUL or DIV, Int or Real, Int or Real) when a == b => a,
         (ADD or SUB or MUL or DIV, Int or Real, Int or Real) => Real,
         (MOD, Int, Int) => Int,
         (ADD, String, _) => String, 
         (ADD, _, String) => String,
         (LT or LEQ or GT or GEQ, Int or Real, Int or Real) => Bool,
         (LT or LEQ or GT or GEQ, Int or Real or String or Char, Int or Real or String or Char) when a == b => Bool,
         (EQ or NEQ, _, _) when a == b => Bool,
         (EQ or NEQ, Int or Real, Int or Real) => Bool,
         (AND or OR, Int or Bool, Int or Bool) when a == b => a,
         _ => Error,
      };
      if (bin.Type == Error)
         throw new ParseException (bin.Op, "Invalid operands");
      var (acast, bcast) = (bin.Op.Kind, a, b) switch {
         (_, Int, Real) => (Real, Void),
         (_, Real, Int) => (Void, Real), 
         (_, String, not String) => (Void, String),
         (_, not String, String) => (String, Void),
         _ => (Void, Void)
      };
      if (acast != Void) bin.Left = new NTypeCast (bin.Left) { Type = acast };
      if (bcast != Void) bin.Right = new NTypeCast (bin.Right) { Type = bcast };
      return bin.Type;
   }

   public override NType Visit (NIdentifier d)
      => d.Type = Check (d.Name);

   public override NType Visit (NFnCall f)
      => f.Type = Check (f.Name, f.Params);

   public override NType Visit (NTypeCast c) {
      c.Expr.Accept (this); return c.Type;
   }
   #endregion

   NType Visit (IEnumerable<Node> nodes) {
      foreach (var node in nodes) node.Accept (this);
      return NType.Void;
   }

   #region Helpers -----------------------------------------
   bool Has (NFnDecl f) {
      var name = f.Name.Text; var decls = f.Body?.Declarations;
      if (Has (f.Name)) Err (f.Name);
      var par = f.Params.FirstOrDefault (a => a.Name.Text.EqualsIC (name));
      if (par != null) Err (par.Name);
      var con = decls?.Consts.FirstOrDefault (a => a.Name.Text.EqualsIC (name));
      if (con != null) Err (con.Name);
      var var = decls?.Vars.FirstOrDefault (a => a.Name.Text.EqualsIC (name));
      if (var != null) Err (var.Name);
      return false;

      void Err (Token t) => throw new ParseException (t, $"'{name}' already defined.");
   }

   bool Has (Token Name, bool local = true)
      => mSymbols.Find (Name.Text, local) != null;

   NType Check (Token Name) {
      return mSymbols.Find (Name.Text) switch {
         NVarDecl v => v.Type,
         NConstDecl c => c.Type,
         NFnCall f => f.Type,
         NFnDecl f => f.Return,
         _ => throw new ParseException (Name, "Unknown variable")
      };
   }
   NType Check (Token Name, NExpr[] Params) {
      if (mSymbols.Find (Name.Text) is not NFnDecl fd) return Unknown;
      if (fd.Params.Length != Params.Length)
         throw new ParseException (Name, $"Parameter count mismatch");
      for (int i = 0; i < fd.Params.Length; i++) {
         var (p, fp) = (Params[i], fd.Params[i]);
         p.Accept (this);
         p = AddTypeCast (Name, p, fp.Type);
         if (p.Type == fp.Type) continue;
      }
      return fd.Return;
   }
   #endregion
}
