// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


var watch = System.Diagnostics.Stopwatch.StartNew();

Console.WriteLine(Puzzle_23.Part_1.Execute());

// the code that you want to measure comes here
watch.Stop();
var elapsedMs = watch.ElapsedMilliseconds;

Console.WriteLine("Execution in: " + elapsedMs + "ms");