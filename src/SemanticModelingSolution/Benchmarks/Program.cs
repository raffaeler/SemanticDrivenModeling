using System;

using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var s = new Serialization();
            //var s = new Deserialization();
            //s.Initialize();
            //s.SemanticOrderToOnlineOrder();
            //s.SemanticOnlineOrderToOrder();

            var p = new Program();
            p.RunBenchmark();
        }

        private int RunBenchmark()
        {
            Summary summary;
            //summary = BenchmarkRunner.Run<Serialization>();
            summary = BenchmarkRunner.Run<Deserialization>();
            return 0;
        }
    }
}
