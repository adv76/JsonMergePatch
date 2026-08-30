using BenchmarkDotNet.Attributes;

namespace Adv76.JsonMergePatch.Benchmarks;

[MemoryDiagnoser]
public class BasicBenchmarks
{
    private Class1 _object0;
    private Class1 _object1;
    
    private Class2 _object2;
    private Class2 _object3;

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
        
        _object2 = new()
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
            Branch0 = new Class2.BranchClass()
            {
                Int0 = 1,
                String0 = "Hello",
                Double0 = 1.5,
                Leaf0 = new Class2.LeafClass()
                {
                    Int0 = 1,
                    String0 = "Hello",
                    Double0 = 1.5,
                }
            }
        };
        
        _object3 = new()
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
            Branch0 = new Class2.BranchClass()
            {
                Int0 = 1,
                String0 = "Hello",
                Double0 = 1.5,
                Leaf0 = new Class2.LeafClass()
                {
                    Int0 = 1,
                    String0 = "Hello",
                    Double0 = 1.5,
                }
            }
        };
    }
    
    [Benchmark]
    public Class1 BasicParse()
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
    public Class1 BasicSafeParse()
    {
        var result = JsonMergePatcher.SafeApplyTo(ref _object1, """
        {
            "Int0": 42,
            "String1": "Sphinx of black quartz, judge my vow.",
            "Double2": 987.654
        }
        """);

        return _object1;
    }
    
    [Benchmark]
    public Class2 ComplexParse()
    {
        JsonMergePatcher.ApplyTo(ref _object2, """
        {
            "Int0": 42,
            "String1": "Sphinx of black quartz, judge my vow.",
            "Double2": 987.654,
            "Branch0": {
                "Int0": 42,
                "String0": "Sphinx of black quartz, judge my vow.",
                "Double0": 987.654,
                "Leaf0": {
                    "Int0": 42,
                    "String0": "Sphinx of black quartz, judge my vow.",
                    "Double0": 987.654
                }
            }
        }                    
        """);

        return _object2;
    }
    
    [Benchmark]
    public Class2 ComplexSafeParse()
    {
        var result = JsonMergePatcher.SafeApplyTo(ref _object3, """
        {
            "Int0": 42,
            "String1": "Sphinx of black quartz, judge my vow.",
            "Double2": 987.654,
            "Branch0": {
                "Int0": 42,
                "String0": "Sphinx of black quartz, judge my vow.",
                "Double0": 987.654,
                "Leaf0": {
                    "Int0": 42,
                    "String0": "Sphinx of black quartz, judge my vow.",
                    "Double0": 987.654
                }
            }
        }
        """);

        return _object3;
    }
}