using BenchmarkDotNet.Attributes;

namespace Adv76.JsonMergePatch.Benchmarks;

[MemoryDiagnoser]
public class Benchmarks
{
    private Class1 _object0;
    private Class1 _object1;
    
    private Class2 _object2;
    private Class2 _object3;

    private Class3 _object4;
    private Class3 _object5;

    public Benchmarks()
    {
        _object0 = Class1.BuildForTesting();
        _object1 = Class1.BuildForTesting();

        _object2 = Class2.BuildForTesting();
        _object3 = Class2.BuildForTesting();

        _object4 = Class3.BuildForTesting();
        _object5 = Class3.BuildForTesting();
    }

    [Benchmark]
    public Class1 BasicApply()
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
    public Class1 BasicSafeApply()
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
    public Class2 ComplexApply()
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
    public Class2 ComplexSafeApply()
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

    [Benchmark]
    public Class3 ExtremeApply()
    {
        JsonMergePatcher.ApplyTo(ref _object4, """
        {
            "SByte0": 42,
            "Byte0": 200,
            "Byte1": 201,
            "Short0": 1234,
            "Short1": 1235,
            "UShort0": 4321,
            "Int0": 42,
            "Int1": 43,
            "UInt0": 42000,
            "Long0": 1234567890123,
            "Long1": 1234567890124,
            "ULong0": 9876543210987,
            "Int1280": 1234567890123456789,
            "UInt1280": 9876543210987654321,
            "Half0": 3.5,
            "Float0": 3.14,
            "Float1": 6.28,
            "Double0": 987.654,
            "Decimal0": 12345.6789,
            "DateTime0": "2024-05-01T12:34:56Z",
            "DateOnly0": "2024-05-02",
            "TimeOnly0": "13:45:30",
            "DateTimeOffset0": "2024-05-01T12:34:56+02:00",
            "Guid0": "33333333-3333-3333-3333-333333333333",
            "String0": "Sphinx of black quartz, judge my vow.",
            "String1": "Pack my box with five dozen liquor jugs.",
            "IntDict": { "a": 100, "c": 300 },
            "StringDict": { "k1": "patched", "k3": "new" },
            "ObjectDict": {
                "item1": { "Int0": 42, "String0": "Sphinx of black quartz, judge my vow.", "Double0": 987.654, "DateTime0": "2024-05-01T12:34:56Z" },
                "item2": { "Int0": 7, "String0": "How vexingly quick daft zebras jump!", "Double0": 1.25, "DateTime0": "2024-06-01T00:00:00Z" }
            },
            "IntList": [10, 20, 30, 40],
            "ObjectList": [{ "Int0": 42, "String0": "Sphinx of black quartz, judge my vow.", "Double0": 987.654 }],
            "StringIList": ["patched1", "patched2"],
            "ObjectIList": [{ "Int0": 99, "String0": "The five boxing wizards jump quickly.", "Double0": 3.25 }],
            "DoubleCollection": [9.87, 6.54],
            "DeepBranch": {
                "Int0": 42,
                "String0": "Sphinx of black quartz, judge my vow.",
                "Double0": 987.654,
                "Guid0": "33333333-3333-3333-3333-333333333333",
                "DateTime0": "2024-05-01T12:34:56Z",
                "Level2": {
                    "Int0": 42,
                    "String0": "Sphinx of black quartz, judge my vow.",
                    "Double0": 987.654,
                    "Decimal0": 9999.9999,
                    "Level3": {
                        "Int0": 42,
                        "String0": "Sphinx of black quartz, judge my vow.",
                        "Double0": 987.654,
                        "Level4": {
                            "Int0": 42,
                            "String0": "Sphinx of black quartz, judge my vow.",
                            "Double0": 987.654,
                            "DateTime0": "2024-05-01T12:34:56Z"
                        }
                    }
                }
            },
            "ShortBranch": {
                "Int0": 42,
                "String0": "Sphinx of black quartz, judge my vow.",
                "Double0": 987.654,
                "Leaf": {
                    "Int0": 42,
                    "String0": "Sphinx of black quartz, judge my vow.",
                    "Double0": 987.654
                }
            }
        }
        """);

        return _object4;
    }

    [Benchmark]
    public Class3 ExtremeSafeApply()
    {
        var result = JsonMergePatcher.SafeApplyTo(ref _object5, """
        {
            "SByte0": 42,
            "Byte0": 200,
            "Byte1": 201,
            "Short0": 1234,
            "Short1": 1235,
            "UShort0": 4321,
            "Int0": 42,
            "Int1": 43,
            "UInt0": 42000,
            "Long0": 1234567890123,
            "Long1": 1234567890124,
            "ULong0": 9876543210987,
            "Int1280": 1234567890123456789,
            "UInt1280": 9876543210987654321,
            "Half0": 3.5,
            "Float0": 3.14,
            "Float1": 6.28,
            "Double0": 987.654,
            "Decimal0": 12345.6789,
            "DateTime0": "2024-05-01T12:34:56Z",
            "DateOnly0": "2024-05-02",
            "TimeOnly0": "13:45:30",
            "DateTimeOffset0": "2024-05-01T12:34:56+02:00",
            "Guid0": "33333333-3333-3333-3333-333333333333",
            "String0": "Sphinx of black quartz, judge my vow.",
            "String1": "Pack my box with five dozen liquor jugs.",
            "IntDict": { "a": 100, "c": 300 },
            "StringDict": { "k1": "patched", "k3": "new" },
            "ObjectDict": {
                "item1": { "Int0": 42, "String0": "Sphinx of black quartz, judge my vow.", "Double0": 987.654, "DateTime0": "2024-05-01T12:34:56Z" },
                "item2": { "Int0": 7, "String0": "How vexingly quick daft zebras jump!", "Double0": 1.25, "DateTime0": "2024-06-01T00:00:00Z" }
            },
            "IntList": [10, 20, 30, 40],
            "ObjectList": [{ "Int0": 42, "String0": "Sphinx of black quartz, judge my vow.", "Double0": 987.654 }],
            "StringIList": ["patched1", "patched2"],
            "ObjectIList": [{ "Int0": 99, "String0": "The five boxing wizards jump quickly.", "Double0": 3.25 }],
            "DoubleCollection": [9.87, 6.54],
            "DeepBranch": {
                "Int0": 42,
                "String0": "Sphinx of black quartz, judge my vow.",
                "Double0": 987.654,
                "Guid0": "33333333-3333-3333-3333-333333333333",
                "DateTime0": "2024-05-01T12:34:56Z",
                "Level2": {
                    "Int0": 42,
                    "String0": "Sphinx of black quartz, judge my vow.",
                    "Double0": 987.654,
                    "Decimal0": 9999.9999,
                    "Level3": {
                        "Int0": 42,
                        "String0": "Sphinx of black quartz, judge my vow.",
                        "Double0": 987.654,
                        "Level4": {
                            "Int0": 42,
                            "String0": "Sphinx of black quartz, judge my vow.",
                            "Double0": 987.654,
                            "DateTime0": "2024-05-01T12:34:56Z"
                        }
                    }
                }
            },
            "ShortBranch": {
                "Int0": 42,
                "String0": "Sphinx of black quartz, judge my vow.",
                "Double0": 987.654,
                "Leaf": {
                    "Int0": 42,
                    "String0": "Sphinx of black quartz, judge my vow.",
                    "Double0": 987.654
                }
            }
        }
        """);

        return _object5;
    }
}
