using System;
using System.IO;

namespace DeterministicTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var tester = new DeterministicTester();

            {
                var hash = tester.TestTumbler(3000, 400);
                Console.WriteLine(hash.Hash);
                File.WriteAllText("tumbler.txt", hash.Data);
            }
        }
    }
}