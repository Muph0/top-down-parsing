using System;

namespace TopDownParser {
    class Program {
        static void Main(string[] args) {

            Grammar g = new Grammar();
            var S = g.CreateNonTerminal();

            S += "a" & S | "";

            g.Parse("aaa");

            //Console.WriteLine("Enter text:");
            //string str = Console.ReadLine();
        }
    }


}
