using Adv76.JsonMergePatch.Benchmarks;
using BenchmarkDotNet.Running;

var summary = BenchmarkRunner.Run<BasicBenchmarks>();