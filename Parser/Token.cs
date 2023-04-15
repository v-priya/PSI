namespace PSI;
using static Token.E;
using static System.Console;

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
      Out (hdr);
      Out (new string ('\u2501', hdr.Length));
      int ln = 0, start = 4, err = Column - 1 + start;
      for (int i = 3; i > 0; i--) {
         if ((ln = Line - i) < 0) continue;
         Out (Source.Lines[ln], ln + 1);
      }
      ForegroundColor = ConsoleColor.Yellow;
      Out (new string (' ', err) + '^');
      int mid = Text.Length / 2 - (Text.Length % 2 == 0 ? 1 : 0);
      int left = err - mid;
      CursorLeft = left > start ? left : start;
      Out (Text);
      ResetColor ();
      for (int i = 0; i < 2; i++) {
         if ((ln = Line + i) == Source.Lines.Length) break;
         Out (Source.Lines[ln], ln + 1);
      }

      void Out (string msg, int line = 0) {
         OutputEncoding = Encoding.Unicode;
         if (line > 0) WriteLine ($"{line, 3}\u2502{msg}");
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
