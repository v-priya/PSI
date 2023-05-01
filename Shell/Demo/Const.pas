program CTest;
const
   one = sin (90);
   message = "Hello";

function Test (const c, d : string;) : string;
begin
   Test := c + " " + d;
end;
begin
   write (Test (message, "World!"), ". one = ", one);
end.