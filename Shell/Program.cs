using PSI;

static class Start {
   static void Main () {
      try {
         string[] files = Directory.GetFiles ("P:/Shell/Demo", "*.pas");
         foreach (var f in files) {
            string hdr = $"(* {Path.GetFileName (f)} *)";
            Console.WriteLine (hdr); Console.WriteLine ($"(* {new string ('-', hdr.Length - 6)} *)");
            NProgram? node = new Parser (new Tokenizer (f)).Parse ();
            node.Accept (new PSIPrint ());
            Console.WriteLine ("\n");
         }
      } catch (ParseException pe) {
         Console.WriteLine ();
         pe.Print ();
      } catch (Exception e) {
         Console.WriteLine ();
         Console.WriteLine (e);
      }
   }
}