// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


var watch = System.Diagnostics.Stopwatch.StartNew();

Console.WriteLine(Puzzle_13.Part_2.Execute());

// the code that you want to measure comes here
watch.Stop();
var elapsedMs = watch.ElapsedMilliseconds;

Console.WriteLine("Execution in: " + elapsedMs + "ms");