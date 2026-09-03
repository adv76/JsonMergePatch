using System.Text.Json;
using Adv76.JsonMergePatch.Test.TestClasses;

namespace Adv76.JsonMergePatch.Test;

[TestClass]
public sealed class SimplePropertyTests
{
    // Success: patch single int property

    [TestMethod]
    public void Patch_SingleIntProperty_Succeeds_ApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patch = """{"Int1": 1}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual(1, obj.Int1);
        Assert.AreEqual("hello", obj.String1);
    }

    [TestMethod]
    public void Patch_SingleIntProperty_Succeeds_SafeApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patch = """{"Int1": 1}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(1, obj.Int1);
        Assert.AreEqual("hello", obj.String1);
    }

    // Success: patch string to null

    [TestMethod]
    public void Patch_StringProperty_ToNull_Succeeds_ApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patch = """{"String1": null}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual(3, obj.Int1);
        Assert.IsNull(obj.String1);
    }

    [TestMethod]
    public void Patch_StringProperty_ToNull_Succeeds_SafeApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patch = """{"String1": null}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(3, obj.Int1);
        Assert.IsNull(obj.String1);
    }

    // Success: patch multiple primitive properties

    [TestMethod]
    public void Patch_MultiplePrimitiveProperties_Succeeds_ApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patch = """{"Int1": 42, "String1": "world"}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual(42, obj.Int1);
        Assert.AreEqual("world", obj.String1);
    }

    [TestMethod]
    public void Patch_MultiplePrimitiveProperties_Succeeds_SafeApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patch = """{"Int1": 42, "String1": "world"}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(42, obj.Int1);
        Assert.AreEqual("world", obj.String1);
    }

    // Failure: patch int property with string value (type mismatch) should fail atomically

    [TestMethod]
    public void Patch_IntProperty_WithStringValue_Fails_ApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patch = """{"Int1": "not-an-int"}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
        // Atomic: original values unchanged
        Assert.AreEqual(3, obj.Int1);
        Assert.AreEqual("hello", obj.String1);
    }

    [TestMethod]
    public void Patch_IntProperty_WithStringValue_Fails_SafeApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patch = """{"Int1": "not-an-int"}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.IsNotEmpty(result.Errors);
        // Atomic: original values unchanged
        Assert.AreEqual(3, obj.Int1);
        Assert.AreEqual("hello", obj.String1);
    }

    // Failure: unknown property

    [TestMethod]
    public void Patch_UnknownProperty_Fails_ApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patch = """{"UnknownProp": 123}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
        Assert.AreEqual(3, obj.Int1);
        Assert.AreEqual("hello", obj.String1);
    }

    [TestMethod]
    public void Patch_UnknownProperty_Fails_SafeApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patch = """{"UnknownProp": 123}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.IsTrue(result.Errors.ContainsKey("UnknownProp"));
        Assert.AreEqual(3, obj.Int1);
        Assert.AreEqual("hello", obj.String1);
    }

    // Case sensitivity: default options are case-sensitive, Web options are case-insensitive

    [TestMethod]
    public void Patch_CaseSensitivePropertyName_Fails_WhenCaseMismatched_ApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patch = """{"int1": 99}"""; // lower-case
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
        Assert.AreEqual(3, obj.Int1);
    }

    [TestMethod]
    public void Patch_CaseSensitivePropertyName_Fails_WhenCaseMismatched_SafeApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patch = """{"int1": 99}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(3, obj.Int1);
    }

    [TestMethod]
    public void Patch_CaseInsensitive_WithWebOptions_Succeeds_ApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patch = """{"int1": 99}""";
        var options = new JsonMergeOptions { JsonSerializerOptions = JsonSerializerOptions.Web };
        JsonMergePatcher.ApplyTo(ref obj, patch, options);
        Assert.AreEqual(99, obj.Int1);
    }

    [TestMethod]
    public void Patch_CaseInsensitive_WithWebOptions_Succeeds_SafeApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patch = """{"int1": 99}""";
        var options = new JsonMergeOptions { JsonSerializerOptions = JsonSerializerOptions.Web };
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch, options);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(99, obj.Int1);
    }

    // Byte array overload

    [TestMethod]
    public void Patch_UsingByteArrayOverload_Succeeds_ApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patchBytes = """{"Int1": 7}"""u8.ToArray();
        JsonMergePatcher.ApplyTo(ref obj, patchBytes);
        Assert.AreEqual(7, obj.Int1);
    }

    [TestMethod]
    public void Patch_UsingByteArrayOverload_Succeeds_SafeApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patchBytes = """{"Int1": 7}"""u8.ToArray();
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patchBytes);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(7, obj.Int1);
    }

    // Atomicity: one valid and one invalid property should fail and not pollute

    [TestMethod]
    public void Patch_MixedValidAndInvalid_DoesNotPollute_ApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        // Int1 valid, unknown prop invalid to test atomicity
        var patch2 = """{"Int1": 1, "Unknown": "x"}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch2));
        // Even though Int1 was valid, it should not be applied because SafeApplyTo is atomic and ApplyTo delegates to it.
        // However we need to verify that after exception, obj not mutated.
        Assert.AreEqual(3, obj.Int1);
    }

    [TestMethod]
    public void Patch_MixedValidAndInvalid_DoesNotPollute_SafeApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patch = """{"Int1": 1, "Unknown": "x"}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(3, obj.Int1);
        Assert.AreEqual("hello", obj.String1);
    }

    [TestMethod]
    public void Patch_InvalidValueAndValidValue_Atomic_SafeApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patch = """{"Int1": 1, "String1": 1}"""; // String1 expects string?, but 1 is number
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(3, obj.Int1);
        Assert.AreEqual("hello", obj.String1);
    }

    [TestMethod]
    public void Patch_InvalidValueAndValidValue_Atomic_ApplyTo()
    {
        var obj = new SimpleModel { Int1 = 3, String1 = "hello" };
        var patch = """{"Int1": 1, "String1": 1}"""; // String1 expects string?, but 1 is number
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
        Assert.AreEqual(3, obj.Int1);
        Assert.AreEqual("hello", obj.String1);
    }

    // Read-only property (no setter) should fail

    [TestMethod]
    public void Patch_ReadOnlyProperty_Fails_ApplyTo()
    {
        var obj = new ReadOnlyModel { WritableProp = "ok" };
        var patch = """{"ReadOnlyProp": "new"}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
        Assert.AreEqual("initial", obj.ReadOnlyProp);
        Assert.AreEqual("ok", obj.WritableProp);
    }

    [TestMethod]
    public void Patch_ReadOnlyProperty_Fails_SafeApplyTo()
    {
        var obj = new ReadOnlyModel { WritableProp = "ok" };
        var patch = """{"ReadOnlyProp": "new"}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("initial", obj.ReadOnlyProp);
        Assert.AreEqual("ok", obj.WritableProp);
    }

    [TestMethod]
    public void Patch_WritableProperty_Succeeds_ReadOnlyModel_ApplyTo()
    {
        var obj = new ReadOnlyModel { WritableProp = "ok" };
        var patch = """{"WritableProp": "updated"}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual("updated", obj.WritableProp);
    }

    [TestMethod]
    public void Patch_WritableProperty_Succeeds_ReadOnlyModel_SafeApplyTo()
    {
        var obj = new ReadOnlyModel { WritableProp = "ok" };
        var patch = """{"WritableProp": "updated"}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("updated", obj.WritableProp);
    }

    // Primitive root tests: string and array

    [TestMethod]
    public void Patch_PrimitiveStringRoot_Succeeds_ApplyTo()
    {
        var obj = "hello";
        var patch = "\"world\"";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual("world", obj);
    }

    [TestMethod]
    public void Patch_PrimitiveStringRoot_Succeeds_SafeApplyTo()
    {
        var obj = "hello";
        var patch = "\"world\"";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("world", obj);
    }

    [TestMethod]
    public void Patch_PrimitiveStringRoot_WithInvalidType_Fails_ApplyTo()
    {
        var obj = "hello";
        var patch = "1";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
        Assert.AreEqual("hello", obj);
    }

    [TestMethod]
    public void Patch_PrimitiveStringRoot_WithInvalidType_Fails_SafeApplyTo()
    {
        var obj = "hello";
        var patch = "1";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.IsTrue(result.Errors.ContainsKey("~"));
        Assert.AreEqual("hello", obj);
    }

    [TestMethod]
    public void Patch_IntArrayRoot_Succeeds_ApplyTo()
    {
        int[] obj = [1, 2];
        var patch = "[3, 4]";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual(3, obj[0]);
        Assert.AreEqual(4, obj[1]);
    }

    [TestMethod]
    public void Patch_IntArrayRoot_Succeeds_SafeApplyTo()
    {
        int[] obj = [1, 2];
        var patch = "[3, 4]";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(3, obj[0]);
        Assert.AreEqual(4, obj[1]);
    }

    [TestMethod]
    public void Patch_IntArrayRoot_WithInvalidJson_Fails_SafeApplyTo()
    {
        int[] obj = [1, 2];
        var patch = "\"not-an-array\"";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(1, obj[0]);
    }

    [TestMethod]
    public void Patch_IntArrayRoot_WithInvalidJson_Fails_ApplyTo()
    {
        int[] obj = [1, 2];
        var patch = "\"not-an-array\"";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
        Assert.AreEqual(1, obj[0]);
    }
}
