using BenchmarkDotNet.Attributes;

namespace Adv76.JsonMergePatch.Benchmarks;

public class BasicBenchmarks
{
    private Class1 _object0;
    private Class1 _object1;

    public BasicBenchmarks()
    {
        _object0 = new()
        {
            Int0 = 1,
            Int1 = 2,
            Int2 = 3,
            String0 = "Hello",
            String1 = "World",
            String2 = null,
            Double0 = 1.5,
            Double1 = 2.5,
            Double2 = 3.5,
        };

        _object1 = new()
        {
            Int0 = 1,
            Int1 = 2,
            Int2 = 3,
            String0 = "Hello",
            String1 = "World",
            String2 = null,
            Double0 = 1.5,
            Double1 = 2.5,
            Double2 = 3.5,
        };
    }
    
    [Benchmark]
    public Class1 Parse()
    {
        JsonMergePatcher.ApplyTo(ref _object0, """
        {
           "Int0": 42,
           "String1": "Sphinx of black quartz, judge my vow.",
           "Double2": 987.654
        }                    
        """);

        return _object0;
    }
    
    [Benchmark]
    public Class1 SafeParse()
    {
        var result = JsonMergePatcher.SafeApplyTo(ref _object0, """
        {
            "Int0": 42,
            "String1": "Sphinx of black quartz, judge my vow.",
            "Double2": 987.654
        }
        """);

        return _object0;
    }
}