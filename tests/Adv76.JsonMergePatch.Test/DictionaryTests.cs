using Adv76.JsonMergePatch.Test.TestClasses;

namespace Adv76.JsonMergePatch.Test;

[TestClass]
public sealed class DictionaryTests
{
    // Primitive dictionary simple update

    [TestMethod]
    public void Patch_PrimitiveDictionary_UpdateExistingKey_Succeeds_ApplyTo()
    {
        var obj = new PrimitiveDictionaryModel
        {
            Dictionary1 = { ["hello"] = 1, ["world"] = 2 }
        };
        var patch = """{"Dictionary1": {"hello": 5}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual(5, obj.Dictionary1["hello"]);
        Assert.AreEqual(2, obj.Dictionary1["world"]);
    }

    [TestMethod]
    public void Patch_PrimitiveDictionary_UpdateExistingKey_Succeeds_SafeApplyTo()
    {
        var obj = new PrimitiveDictionaryModel
        {
            Dictionary1 = { ["hello"] = 1, ["world"] = 2 }
        };
        var patch = """{"Dictionary1": {"hello": 5}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(5, obj.Dictionary1["hello"]);
        Assert.AreEqual(2, obj.Dictionary1["world"]);
    }

    [TestMethod]
    public void Patch_PrimitiveDictionary_AddNewKey_Succeeds_ApplyTo()
    {
        var obj = new PrimitiveDictionaryModel
        {
            Dictionary1 = { ["hello"] = 1 }
        };
        var patch = """{"Dictionary1": {"newKey": 99}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual(99, obj.Dictionary1["newKey"]);
        Assert.AreEqual(1, obj.Dictionary1["hello"]);
    }

    [TestMethod]
    public void Patch_PrimitiveDictionary_AddNewKey_Succeeds_SafeApplyTo()
    {
        var obj = new PrimitiveDictionaryModel
        {
            Dictionary1 = { ["hello"] = 1 }
        };
        var patch = """{"Dictionary1": {"newKey": 99}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(99, obj.Dictionary1["newKey"]);
    }

    // Int-key dictionary

    [TestMethod]
    public void Patch_IntKeyDictionary_Update_Succeeds_ApplyTo()
    {
        var obj = new PrimitiveDictionaryModel
        {
            Dictionary2 = { [1] = 12, [2] = 9 }
        };
        var patch = """{"Dictionary2": {"2": 42}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual(12, obj.Dictionary2[1]);
        Assert.AreEqual(42, obj.Dictionary2[2]);
    }

    [TestMethod]
    public void Patch_IntKeyDictionary_Update_Succeeds_SafeApplyTo()
    {
        var obj = new PrimitiveDictionaryModel
        {
            Dictionary2 = { [1] = 12, [2] = 9 }
        };
        var patch = """{"Dictionary2": {"2": 42}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(42, obj.Dictionary2[2]);
    }

    [TestMethod]
    public void Patch_IntKeyDictionary_InvalidKey_Fails_ApplyTo()
    {
        var obj = new PrimitiveDictionaryModel
        {
            Dictionary2 = { [1] = 12, [2] = 9 }
        };
        var patch = """{"Dictionary2": {"not-an-int": 42}}""";
        // Current implementation throws InvalidOperationException when dictionary key is null after failed conversion.
        Assert.Throws<InvalidOperationException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
        Assert.AreEqual(12, obj.Dictionary2[1]);
        Assert.AreEqual(9, obj.Dictionary2[2]);
    }

    [TestMethod]
    public void Patch_IntKeyDictionary_InvalidKey_Fails_SafeApplyTo()
    {
        var obj = new PrimitiveDictionaryModel
        {
            Dictionary2 = { [1] = 12, [2] = 9 }
        };
        var patch = """{"Dictionary2": {"not-an-int": 42}}""";
        // SafeApplyTo currently does not gracefully handle invalid int keys and propagates InvalidOperationException.
        Assert.Throws<InvalidOperationException>(() => JsonMergePatcher.SafeApplyTo(ref obj, patch));
        Assert.AreEqual(12, obj.Dictionary2[1]);
        Assert.AreEqual(9, obj.Dictionary2[2]);
    }

    // Object dictionary

    [TestMethod]
    public void Patch_ObjectDictionary_UpdateNestedProperty_Succeeds_ApplyTo()
    {
        var obj = new ObjectDictionaryModel
        {
            Dictionary1 =
            {
                ["hello"] = new SimpleModel { Int1 = 3, String1 = "hello" },
                ["world"] = new SimpleModel { Int1 = 4, String1 = "world" }
            }
        };
        var patch = """{"Dictionary1": {"hello": {"Int1": 8}}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual(8, obj.Dictionary1["hello"].Int1);
        Assert.AreEqual("hello", obj.Dictionary1["hello"].String1);
        Assert.AreEqual(4, obj.Dictionary1["world"].Int1);
    }

    [TestMethod]
    public void Patch_ObjectDictionary_UpdateNestedProperty_Succeeds_SafeApplyTo()
    {
        var obj = new ObjectDictionaryModel
        {
            Dictionary1 =
            {
                ["hello"] = new SimpleModel { Int1 = 3, String1 = "hello" },
                ["world"] = new SimpleModel { Int1 = 4, String1 = "world" }
            }
        };
        var patch = """{"Dictionary1": {"hello": {"Int1": 8}}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(8, obj.Dictionary1["hello"].Int1);
    }

    [TestMethod]
    public void Patch_ObjectDictionary_AddNewEntry_Succeeds_ApplyTo()
    {
        var obj = new ObjectDictionaryModel
        {
            Dictionary1 = { ["hello"] = new SimpleModel { Int1 = 3, String1 = "hello" } }
        };
        var patch = """{"Dictionary1": {"newKey": {"Int1": 5, "String1": "new"}}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.IsTrue(obj.Dictionary1.ContainsKey("newKey"));
        Assert.AreEqual(5, obj.Dictionary1["newKey"].Int1);
        Assert.AreEqual("new", obj.Dictionary1["newKey"].String1);
    }

    [TestMethod]
    public void Patch_ObjectDictionary_AddNewEntry_Succeeds_SafeApplyTo()
    {
        var obj = new ObjectDictionaryModel
        {
            Dictionary1 = { ["hello"] = new SimpleModel { Int1 = 3, String1 = "hello" } }
        };
        var patch = """{"Dictionary1": {"newKey": {"Int1": 5, "String1": "new"}}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(5, obj.Dictionary1["newKey"].Int1);
    }

    [TestMethod]
    public void Patch_ObjectDictionary_InvalidNestedType_Fails_ApplyTo()
    {
        var obj = new ObjectDictionaryModel
        {
            Dictionary1 = { ["hello"] = new SimpleModel { Int1 = 3, String1 = "hello" } }
        };
        var patch = """{"Dictionary1": {"hello": {"Int1": "bad"}}}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
        Assert.AreEqual(3, obj.Dictionary1["hello"].Int1);
    }

    [TestMethod]
    public void Patch_ObjectDictionary_InvalidNestedType_Fails_SafeApplyTo()
    {
        var obj = new ObjectDictionaryModel
        {
            Dictionary1 = { ["hello"] = new SimpleModel { Int1 = 3, String1 = "hello" } }
        };
        var patch = """{"Dictionary1": {"hello": {"Int1": "bad"}}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(3, obj.Dictionary1["hello"].Int1);
    }

    // Nested dictionary (dict of dict)

    [TestMethod]
    public void Patch_NestedDictionary_UpdateInnerValue_Succeeds_ApplyTo()
    {
        var obj = new NestedDictionaryModel
        {
            NestedDict = { ["outer"] = new Dictionary<string, int> { ["inner"] = 1, ["other"] = 2 } }
        };
        var patch = """{"NestedDict": {"outer": {"inner": 42}}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual(42, obj.NestedDict["outer"]["inner"]);
        Assert.AreEqual(2, obj.NestedDict["outer"]["other"]);
    }

    [TestMethod]
    public void Patch_NestedDictionary_UpdateInnerValue_Succeeds_SafeApplyTo()
    {
        var obj = new NestedDictionaryModel
        {
            NestedDict = { ["outer"] = new Dictionary<string, int> { ["inner"] = 1, ["other"] = 2 } }
        };
        var patch = """{"NestedDict": {"outer": {"inner": 42}}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(42, obj.NestedDict["outer"]["inner"]);
    }

    [TestMethod]
    public void Patch_NestedDictionary_AddNewOuterKey_Succeeds_ApplyTo()
    {
        var obj = new NestedDictionaryModel
        {
            NestedDict = { ["outer"] = new Dictionary<string, int> { ["inner"] = 1 } }
        };
        var patch = """{"NestedDict": {"newOuter": {"newInner": 7}}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual(7, obj.NestedDict["newOuter"]["newInner"]);
        Assert.AreEqual(1, obj.NestedDict["outer"]["inner"]);
    }

    [TestMethod]
    public void Patch_NestedDictionary_AddNewOuterKey_Succeeds_SafeApplyTo()
    {
        var obj = new NestedDictionaryModel
        {
            NestedDict = { ["outer"] = new Dictionary<string, int> { ["inner"] = 1 } }
        };
        var patch = """{"NestedDict": {"newOuter": {"newInner": 7}}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(7, obj.NestedDict["newOuter"]["newInner"]);
    }

    // Deep object dict (dict of dict of object)

    [TestMethod]
    public void Patch_DeepObjectDictionary_Update_Succeeds_ApplyTo()
    {
        var obj = new ComplexDictionaryModel
        {
            DeepObjectDict =
            {
                ["outer"] = new Dictionary<string, SimpleModel>
                {
                    ["inner"] = new SimpleModel { Int1 = 1, String1 = "a" }
                }
            }
        };
        var patch = """{"DeepObjectDict": {"outer": {"inner": {"String1": "b"}}}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual("b", obj.DeepObjectDict["outer"]["inner"].String1);
        Assert.AreEqual(1, obj.DeepObjectDict["outer"]["inner"].Int1);
    }

    [TestMethod]
    public void Patch_DeepObjectDictionary_Update_Succeeds_SafeApplyTo()
    {
        var obj = new ComplexDictionaryModel
        {
            DeepObjectDict =
            {
                ["outer"] = new Dictionary<string, SimpleModel>
                {
                    ["inner"] = new SimpleModel { Int1 = 1, String1 = "a" }
                }
            }
        };
        var patch = """{"DeepObjectDict": {"outer": {"inner": {"String1": "b"}}}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("b", obj.DeepObjectDict["outer"]["inner"].String1);
    }

    [TestMethod]
    public void Patch_DeepObjectDictionary_AddNewInner_Succeeds_ApplyTo()
    {
        var obj = new ComplexDictionaryModel
        {
            DeepObjectDict =
            {
                ["outer"] = new Dictionary<string, SimpleModel>
                {
                    ["inner"] = new SimpleModel { Int1 = 1, String1 = "a" }
                }
            }
        };
        var patch = """{"DeepObjectDict": {"outer": {"newInner": {"Int1": 99}}}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual(99, obj.DeepObjectDict["outer"]["newInner"].Int1);
        Assert.AreEqual(1, obj.DeepObjectDict["outer"]["inner"].Int1);
    }

    [TestMethod]
    public void Patch_DeepObjectDictionary_AddNewInner_Succeeds_SafeApplyTo()
    {
        var obj = new ComplexDictionaryModel
        {
            DeepObjectDict =
            {
                ["outer"] = new Dictionary<string, SimpleModel>
                {
                    ["inner"] = new SimpleModel { Int1 = 1, String1 = "a" }
                }
            }
        };
        var patch = """{"DeepObjectDict": {"outer": {"newInner": {"Int1": 99}}}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(99, obj.DeepObjectDict["outer"]["newInner"].Int1);
    }

    [TestMethod]
    public void Patch_NestedDictionary_InvalidInnerType_Fails_ApplyTo()
    {
        var obj = new NestedDictionaryModel
        {
            NestedDict = { ["outer"] = new Dictionary<string, int> { ["inner"] = 1 } }
        };
        var patch = """{"NestedDict": {"outer": {"inner": "not-an-int"}}}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
        Assert.AreEqual(1, obj.NestedDict["outer"]["inner"]);
    }

    [TestMethod]
    public void Patch_NestedDictionary_InvalidInnerType_Fails_SafeApplyTo()
    {
        var obj = new NestedDictionaryModel
        {
            NestedDict = { ["outer"] = new Dictionary<string, int> { ["inner"] = 1 } }
        };
        var patch = """{"NestedDict": {"outer": {"inner": "not-an-int"}}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(1, obj.NestedDict["outer"]["inner"]);
    }

    // Null dictionary handling - when dictionary is null, but model initializes with [], so we null the dictionary via direct assignment
    [TestMethod]
    public void Patch_NullDictionary_CreatesAndPatches_Succeeds_ApplyTo()
    {
        var obj = new PrimitiveDictionaryModel { Dictionary1 = null! };
        // Simulate null dictionary - patch should attempt to create via CreateObject
        var patch = """{"Dictionary1": {"hello": 5}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.IsNotNull(obj.Dictionary1);
        Assert.AreEqual(5, obj.Dictionary1["hello"]);
    }

    [TestMethod]
    public void Patch_NullDictionary_CreatesAndPatches_Succeeds_SafeApplyTo()
    {
        var obj = new PrimitiveDictionaryModel { Dictionary1 = null! };
        var patch = """{"Dictionary1": {"hello": 5}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(5, obj.Dictionary1["hello"]);
    }

    // Dictionary + object intermix failure atomic
    [TestMethod]
    public void Patch_DictionaryAndObject_MixedInvalidAtomic_Fails_SafeApplyTo()
    {
        var obj = new DictionaryWithObjectAndPrimitiveModel
        {
            ObjectDict = { ["k1"] = new SimpleModel { Int1 = 1, String1 = "a" } },
            PrimitiveDict = { ["p1"] = 10 }
        };
        var patch = """{"ObjectDict": {"k1": {"Int1": 2}}, "PrimitiveDict": {"p1": "bad"}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(1, obj.ObjectDict["k1"].Int1);
        Assert.AreEqual(10, obj.PrimitiveDict["p1"]);
    }

    [TestMethod]
    public void Patch_DictionaryAndObject_MixedInvalidAtomic_Fails_ApplyTo()
    {
        var obj = new DictionaryWithObjectAndPrimitiveModel
        {
            ObjectDict = { ["k1"] = new SimpleModel { Int1 = 1, String1 = "a" } },
            PrimitiveDict = { ["p1"] = 10 }
        };
        var patch = """{"ObjectDict": {"k1": {"Int1": 2}}, "PrimitiveDict": {"p1": "bad"}}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
        Assert.AreEqual(1, obj.ObjectDict["k1"].Int1);
        Assert.AreEqual(10, obj.PrimitiveDict["p1"]);
    }

    // RFC7396 Section 2: null value means removal. Dictionary<int> and Dictionary<object> null removal.
    [TestMethod]
    public void Patch_PrimitiveIntDictionary_NullRemovesKey_Succeeds_ApplyTo()
    {
        var obj = new PrimitiveDictionaryModel
        {
            Dictionary1 = { ["hello"] = 1, ["world"] = 2 }
        };
        var patch = """{"Dictionary1": {"hello": null}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        // RFC7396: if Value is null and Name exists in Target, remove the Name/Value pair
        Assert.IsFalse(obj.Dictionary1.ContainsKey("hello"), "RFC7396: null should remove key 'hello' from Dictionary<string,int>");
        Assert.IsTrue(obj.Dictionary1.ContainsKey("world"));
        Assert.HasCount(1, obj.Dictionary1);
        Assert.AreEqual(2, obj.Dictionary1["world"]);
    }

    [TestMethod]
    public void Patch_PrimitiveIntDictionary_NullRemovesKey_Succeeds_SafeApplyTo()
    {
        var obj = new PrimitiveDictionaryModel
        {
            Dictionary1 = { ["hello"] = 1, ["world"] = 2 }
        };
        var patch = """{"Dictionary1": {"hello": null}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.IsFalse(obj.Dictionary1.ContainsKey("hello"), "RFC7396: null should remove key 'hello'");
        Assert.HasCount(1, obj.Dictionary1);
    }

    [TestMethod]
    public void Patch_ObjectDictionary_NullRemovesKey_Succeeds_ApplyTo()
    {
        var obj = new ObjectDictionaryModel
        {
            Dictionary1 =
            {
                ["hello"] = new SimpleModel { Int1 = 3, String1 = "hello" },
                ["world"] = new SimpleModel { Int1 = 4, String1 = "world" }
            }
        };
        var patch = """{"Dictionary1": {"hello": null}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        // RFC7396: null should remove the entry, not replace with empty object
        Assert.IsFalse(obj.Dictionary1.ContainsKey("hello"), "RFC7396: null should remove key 'hello' from Dictionary<string,SimpleModel>");
        Assert.IsTrue(obj.Dictionary1.ContainsKey("world"));
        Assert.HasCount(1, obj.Dictionary1);
        Assert.AreEqual(4, obj.Dictionary1["world"].Int1);
    }

    [TestMethod]
    public void Patch_ObjectDictionary_NullRemovesKey_Succeeds_SafeApplyTo()
    {
        var obj = new ObjectDictionaryModel
        {
            Dictionary1 =
            {
                ["hello"] = new SimpleModel { Int1 = 3, String1 = "hello" },
                ["world"] = new SimpleModel { Int1 = 4, String1 = "world" }
            }
        };
        var patch = """{"Dictionary1": {"hello": null}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.IsFalse(obj.Dictionary1.ContainsKey("hello"), "RFC7396: null should remove key 'hello'");
        Assert.HasCount(1, obj.Dictionary1);
    }
}
