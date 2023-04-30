program CTest;
const
   pi = 3.14;
   message = "Hello";

function Test (const c, d : string;) : string;
begin
   Test := c + " " + d;
end;
begin
   write (message, ". pi = ", pi);
end.