namespace PSI;
using static Token.E;
using static System.Console;
using static System.Math;

// Represents a PSI language Token
public class Token {
   public Token (Tokenizer source, E kind, string text, int line, int column) 
      => (Source, Kind, Text, Line, Column) = (source, kind, text, line, column);
   public Tokenizer Source { get; }
   public E Kind { get; }
   public string Text { get; }
   public int Line { get; }
   public int Column { get; }

   // The various types of token
   public enum E {
      // Keywords
      PROGRAM, VAR, IF, THEN, WHILE, ELSE, FOR, TO, DOWNTO,
      DO, BEGIN, END, PRINT, TYPE, NOT, OR, AND, MOD, _ENDKEYWORDS,
      // Operators
      ADD, SUB, MUL, DIV, NEQ, LEQ, GEQ, EQ, LT, GT, ASSIGN, 
      _ENDOPERATORS,
      // Punctuation
      SEMI, PERIOD, COMMA, OPEN, CLOSE, COLON, 
      _ENDPUNCTUATION,
      // Others
      IDENT, INTEGER, REAL, BOOLEAN, STRING, CHAR, EOF, ERROR
   }

   // Print a Token
   public override string ToString () => Kind switch {
      EOF or ERROR => Kind.ToString (),
      < _ENDKEYWORDS => $"\u00ab{Kind.ToString ().ToLower ()}\u00bb",
      STRING => $"\"{Text}\"",
      CHAR => $"'{Text}'",
      _ => Text,
   };

   // Utility function used to echo an error to the console
   public void PrintError () {
      if (Kind != ERROR) throw new Exception ("PrintError called on a non-error token");
      string hdr = $"File: {Source.FileName}";
      OutputEncoding = Encoding.Unicode;
      const int margin = 4;
      Out (hdr);
      Out (new string ('\u2501', hdr.Length));
      int lnstart = Min (Line, Max (Line - 2, 1)), lnend = Max (Line, Min (Line + 2, Source.Lines.Length));
      int err = Column + margin;
      for (int i = lnstart; i <= lnend; i++) {
         Out (Source.Lines[i - 1], i);
         if (i == Line) {
            ForegroundColor = ConsoleColor.Yellow;
            Out (new string (' ', err) + '^');
            int mid = Text.Length / 2 - (Text.Length % 2 == 0 ? 1 : 0);
            Out (new string (' ', Max (err - mid, margin)) + Text);
            ResetColor ();
         }
      }

      void Out (string msg, int line = 0) {
         if (line > 0) WriteLine ($"{line, margin}\u2502{msg}");
         else WriteLine (msg);
      }
   }

   // Helper used by the parser (maps operator sequences to E values)
   public static List<(E Kind, string Text)> Match = new () {
      (NEQ, "<>"), (LEQ, "<="), (GEQ, ">="), (ASSIGN, ":="), (ADD, "+"),
      (SUB, "-"), (MUL, "*"), (DIV, "/"), (EQ, "="), (LT, "<"),
      (LEQ, "<="), (GT, ">"), (SEMI, ";"), (PERIOD, "."), (COMMA, ","),
      (OPEN, "("), (CLOSE, ")"), (COLON, ":")
   };
}
