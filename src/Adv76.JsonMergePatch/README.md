# Adv76.JsonMergePatch

A library for patching .NET objects via JSON Merge Patch

Suppose you have `Class1` below:

    public class Class1
    {
        public int Int0 { get; set; }
        public string? String0 { get; set; }
        public double Double0 { get; set; }
    }

And suppose you have this instance:

    var obj = new Class1()
    {
        Int0 = 1,
        String0 = "Hello World",
        Double0 = 3.5,
    };

A simple example of patching this object:

    var result = JsonMergePatcher.SafeApplyTo(ref obj, """
    {
        "Int0": 42,
        "String0": "Sphinx of black quartz, judge my vow."
    }
    """);

If the patch succeeded, `result.Succeeded` will equal `true` and `obj` will have the `"Int0"` and `"String0"` properties updated to the corresponding values.
