using Adv76.JsonMergePatch.Test.TestClasses;

namespace Adv76.JsonMergePatch.Test;

[TestClass]
public sealed class SecurityPolicyTests
{
    [TestMethod]
    public void Patch_Security_AllowedProperty_Succeeds_ApplyTo()
    {
        var obj = new SecurityPolicyModel { AllowedString = "Hello" };
        var patch = """{"AllowedString": "World"}""";
        JsonMergePatcher.ApplyTo(ref obj, patch, JsonMergeOptions.Strict);
        Assert.AreEqual("World", obj.AllowedString);
    }

    [TestMethod]
    public void Patch_Security_AllowedProperty_Succeeds_SafeApplyTo()
    {
        var obj = new SecurityPolicyModel { AllowedString = "Hello" };
        var patch = """{"AllowedString": "World"}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch, JsonMergeOptions.Strict);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("World", obj.AllowedString);
    }

    [TestMethod]
    public void Patch_Security_BlockedProperty_Fails_ApplyTo()
    {
        var obj = new SecurityPolicyModel { BlockedString = "Hello" };
        var patch = """{"BlockedString": "World"}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch, JsonMergeOptions.Strict));
        Assert.AreEqual("Hello", obj.BlockedString);
    }

    [TestMethod]
    public void Patch_Security_BlockedProperty_Fails_SafeApplyTo()
    {
        var obj = new SecurityPolicyModel { BlockedString = "Hello" };
        var patch = """{"BlockedString": "World"}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch, JsonMergeOptions.Strict);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("Hello", obj.BlockedString);
    }

    [TestMethod]
    public void Patch_Security_SkipSilently_SucceedsButIgnored_ApplyTo()
    {
        var obj = new SecurityPolicyModel { IgnoredString = "Hello" };
        var patch = """{"IgnoredString": "World"}""";
        JsonMergePatcher.ApplyTo(ref obj, patch, JsonMergeOptions.Strict);
        Assert.AreEqual("Hello", obj.IgnoredString);
    }

    [TestMethod]
    public void Patch_Security_SkipSilently_SucceedsButIgnored_SafeApplyTo()
    {
        var obj = new SecurityPolicyModel { IgnoredString = "Hello" };
        var patch = """{"IgnoredString": "World"}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch, JsonMergeOptions.Strict);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("Hello", obj.IgnoredString);
    }

    [TestMethod]
    public void Patch_Security_UnannotatedBlockedInStrict_Fails_ApplyTo()
    {
        var obj = new SecurityPolicyModel { String = "Hello" };
        var patch = """{"String": "World"}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch, JsonMergeOptions.Strict));
        Assert.AreEqual("Hello", obj.String);
    }

    [TestMethod]
    public void Patch_Security_UnannotatedBlockedInStrict_Fails_SafeApplyTo()
    {
        var obj = new SecurityPolicyModel { String = "Hello" };
        var patch = """{"String": "World"}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch, JsonMergeOptions.Strict);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("Hello", obj.String);
    }

    [TestMethod]
    public void Patch_Security_UnannotatedAllowedInDefault_Succeeds_ApplyTo()
    {
        var obj = new SecurityPolicyModel { String = "Hello" };
        var patch = """{"String": "World"}""";
        JsonMergePatcher.ApplyTo(ref obj, patch, JsonMergeOptions.Default);
        Assert.AreEqual("World", obj.String);
    }

    [TestMethod]
    public void Patch_Security_UnannotatedAllowedInDefault_Succeeds_SafeApplyTo()
    {
        var obj = new SecurityPolicyModel { String = "Hello" };
        var patch = """{"String": "World"}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch, JsonMergeOptions.Default);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("World", obj.String);
    }

    [TestMethod]
    public void Patch_Security_MixedAllowedAndBlocked_AtomicFails_ApplyTo()
    {
        var obj = new SecurityPolicyModel { AllowedString = "A", BlockedString = "B", String = "C" };
        var patch = """{"AllowedString": "AA", "BlockedString": "BB"}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch, JsonMergeOptions.Strict));
        Assert.AreEqual("A", obj.AllowedString);
        Assert.AreEqual("B", obj.BlockedString);
    }

    [TestMethod]
    public void Patch_Security_MixedAllowedAndBlocked_AtomicFails_SafeApplyTo()
    {
        var obj = new SecurityPolicyModel { AllowedString = "A", BlockedString = "B", String = "C" };
        var patch = """{"AllowedString": "AA", "BlockedString": "BB"}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch, JsonMergeOptions.Strict);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("A", obj.AllowedString);
        Assert.AreEqual("B", obj.BlockedString);
    }

    [TestMethod]
    public void Patch_Security_SkipSilently_WithBlocked_ShouldStillFail_ApplyTo()
    {
        var obj = new SecurityPolicyModel { IgnoredString = "I", BlockedString = "B" };
        var patch = """{"IgnoredString": "II", "BlockedString": "BB"}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch, JsonMergeOptions.Strict));
        Assert.AreEqual("I", obj.IgnoredString);
        Assert.AreEqual("B", obj.BlockedString);
    }

    [TestMethod]
    public void Patch_Security_SkipSilently_WithBlocked_ShouldStillFail_SafeApplyTo()
    {
        var obj = new SecurityPolicyModel { IgnoredString = "I", BlockedString = "B" };
        var patch = """{"IgnoredString": "II", "BlockedString": "BB"}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch, JsonMergeOptions.Strict);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("I", obj.IgnoredString);
    }
}
