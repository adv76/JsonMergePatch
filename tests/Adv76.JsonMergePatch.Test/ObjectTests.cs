using System.Text.Json;
using Adv76.JsonMergePatch.Test.TestClasses;

namespace Adv76.JsonMergePatch.Test;

[TestClass]
public sealed class ObjectTests
{
    // Nested object patch

    [TestMethod]
    public void Patch_NestedObject_SingleProperty_Succeeds_ApplyTo()
    {
        var obj = new NestedModel
        {
            Inner = new SimpleModel { Int1 = 3, String1 = "world" },
            String1 = "hello"
        };
        var patch = """{"Inner": {"Int1": 1}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual(1, obj.Inner?.Int1);
        Assert.AreEqual("world", obj.Inner?.String1);
        Assert.AreEqual("hello", obj.String1);
    }

    [TestMethod]
    public void Patch_NestedObject_SingleProperty_Succeeds_SafeApplyTo()
    {
        var obj = new NestedModel
        {
            Inner = new SimpleModel { Int1 = 3, String1 = "world" },
            String1 = "hello"
        };
        var patch = """{"Inner": {"Int1": 1}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(1, obj.Inner?.Int1);
        Assert.AreEqual("world", obj.Inner?.String1);
    }

    // Nested null auto-creation

    [TestMethod]
    public void Patch_NestedObject_NullCreatesNew_Succeeds_ApplyTo()
    {
        var obj = new NestedModel { Inner = null, String1 = "hello" };
        var patch = """{"Inner": {"Int1": 1}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.IsNotNull(obj.Inner);
        Assert.AreEqual(1, obj.Inner?.Int1);
    }

    [TestMethod]
    public void Patch_NestedObject_NullCreatesNew_Succeeds_SafeApplyTo()
    {
        var obj = new NestedModel { Inner = null, String1 = "hello" };
        var patch = """{"Inner": {"Int1": 1}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(1, obj.Inner?.Int1);
    }

    // Deep double nested

    [TestMethod]
    public void Patch_DoubleNested_Succeeds_ApplyTo()
    {
        var obj = new DoubleNestedModel
        {
            Nested = new NestedModel { Inner = new SimpleModel { Int1 = 5, String1 = "a" }, String1 = "b" },
            Name = "root"
        };
        var patch = """{"Nested": {"Inner": {"String1": "updated"}}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual("updated", obj.Nested?.Inner?.String1);
        Assert.AreEqual(5, obj.Nested?.Inner?.Int1);
    }

    [TestMethod]
    public void Patch_DoubleNested_Succeeds_SafeApplyTo()
    {
        var obj = new DoubleNestedModel
        {
            Nested = new NestedModel { Inner = new SimpleModel { Int1 = 5, String1 = "a" }, String1 = "b" },
            Name = "root"
        };
        var patch = """{"Nested": {"Inner": {"String1": "updated"}}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("updated", obj.Nested?.Inner?.String1);
    }

    // Nested with invalid type should fail atomically

    [TestMethod]
    public void Patch_NestedObject_InvalidInnerType_Fails_ApplyTo()
    {
        var obj = new NestedModel
        {
            Inner = new SimpleModel { Int1 = 3, String1 = "world" },
            String1 = "hello"
        };
        var patch = """{"Inner": {"Int1": "oops"}}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
        Assert.AreEqual(3, obj.Inner?.Int1);
    }

    [TestMethod]
    public void Patch_NestedObject_InvalidInnerType_Fails_SafeApplyTo()
    {
        var obj = new NestedModel
        {
            Inner = new SimpleModel { Int1 = 3, String1 = "world" },
            String1 = "hello"
        };
        var patch = """{"Inner": {"Int1": "oops"}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(3, obj.Inner?.Int1);
        Assert.AreEqual("world", obj.Inner?.String1);
    }

    // Unknown property inside nested object

    [TestMethod]
    public void Patch_NestedObject_UnknownInnerProperty_Fails_ApplyTo()
    {
        var obj = new NestedModel
        {
            Inner = new SimpleModel { Int1 = 3, String1 = "world" },
            String1 = "hello"
        };
        var patch = """{"Inner": {"Unknown": 1}}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
    }

    [TestMethod]
    public void Patch_NestedObject_UnknownInnerProperty_Fails_SafeApplyTo()
    {
        var obj = new NestedModel
        {
            Inner = new SimpleModel { Int1 = 3, String1 = "world" },
            String1 = "hello"
        };
        var patch = """{"Inner": {"Unknown": 1}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.IsTrue(result.Errors.ContainsKey("Inner.Unknown"));
        Assert.AreEqual(3, obj.Inner?.Int1);
    }

    // Parent with child that has no parameterless ctor - creation should fail when null

    [TestMethod]
    public void Patch_ChildWithNoCtor_WhenNull_Fails_ApplyTo()
    {
        var obj = new ParentWithNoCtorChild { Child = null, Name = "parent" };
        var patch = """{"Child": {"Value": "new"}}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
        Assert.IsNull(obj.Child);
    }

    [TestMethod]
    public void Patch_ChildWithNoCtor_WhenNull_Fails_SafeApplyTo()
    {
        var obj = new ParentWithNoCtorChild { Child = null, Name = "parent" };
        var patch = """{"Child": {"Value": "new"}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.IsNull(obj.Child);
        // error path includes property name
        Assert.IsTrue(result.Errors.Any(kvp => kvp.Key.Contains("Child")));
    }

    [TestMethod]
    public void Patch_ChildWithNoCtor_WhenExists_Succeeds_ApplyTo()
    {
        var obj = new ParentWithNoCtorChild { Child = new ChildWithNoParameterlessCtor("old"), Name = "parent" };
        var patch = """{"Child": {"Value": "new"}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual("new", obj.Child?.Value);
    }

    [TestMethod]
    public void Patch_ChildWithNoCtor_WhenExists_Succeeds_SafeApplyTo()
    {
        var obj = new ParentWithNoCtorChild { Child = new ChildWithNoParameterlessCtor("old"), Name = "parent" };
        var patch = """{"Child": {"Value": "new"}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("new", obj.Child?.Value);
    }

    // Custom converter tests

    [TestMethod]
    public void Patch_CustomConverter_Succeeds_ApplyTo()
    {
        var obj = new TimeSpanModel { TimeSpan1 = new TimeSpan(2, 3, 0) };
        var patch = """{"TimeSpan1": {"hr": 4, "min": 5}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual(4, obj.TimeSpan1.Hours);
        Assert.AreEqual(5, obj.TimeSpan1.Minutes);
    }

    [TestMethod]
    public void Patch_CustomConverter_Succeeds_SafeApplyTo()
    {
        var obj = new TimeSpanModel { TimeSpan1 = new TimeSpan(2, 3, 0) };
        var patch = """{"TimeSpan1": {"hr": 4, "min": 5}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(4, obj.TimeSpan1.Hours);
        Assert.AreEqual(5, obj.TimeSpan1.Minutes);
    }

    [TestMethod]
    public void Patch_CustomConverter_InvalidValue_Fails_SafeApplyTo()
    {
        var obj = new TimeSpanModel { TimeSpan1 = new TimeSpan(2, 3, 0) };
        var patch = """{"TimeSpan1": "not-an-object"}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(2, obj.TimeSpan1.Hours);
        Assert.AreEqual(3, obj.TimeSpan1.Minutes);
    }

    [TestMethod]
    public void Patch_CustomConverter_InvalidValue_Fails_ApplyTo()
    {
        var obj = new TimeSpanModel { TimeSpan1 = new TimeSpan(2, 3, 0) };
        var patch = """{"TimeSpan1": "not-an-object"}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
        Assert.AreEqual(2, obj.TimeSpan1.Hours);
    }

    // Wrapper model with dictionary + nested object

    [TestMethod]
    public void Patch_Wrapper_NestedObjectAndDictionary_Succeeds_ApplyTo()
    {
        var obj = new WrapperModel
        {
            Inner = new SimpleModel { Int1 = 1, String1 = "a" },
            Map = { ["k1"] = new SimpleModel { Int1 = 10, String1 = "x" } }
        };
        var patch = """{"Inner": {"Int1": 9}, "Map": {"k1": {"String1": "y"}}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual(9, obj.Inner?.Int1);
        Assert.AreEqual("y", obj.Map["k1"].String1);
        Assert.AreEqual(10, obj.Map["k1"].Int1);
    }

    [TestMethod]
    public void Patch_Wrapper_NestedObjectAndDictionary_Succeeds_SafeApplyTo()
    {
        var obj = new WrapperModel
        {
            Inner = new SimpleModel { Int1 = 1, String1 = "a" },
            Map = { ["k1"] = new SimpleModel { Int1 = 10, String1 = "x" } }
        };
        var patch = """{"Inner": {"Int1": 9}, "Map": {"k1": {"String1": "y"}}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(9, obj.Inner?.Int1);
        Assert.AreEqual("y", obj.Map["k1"].String1);
    }
}
