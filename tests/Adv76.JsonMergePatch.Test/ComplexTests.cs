using Adv76.JsonMergePatch.Test.TestClasses;

namespace Adv76.JsonMergePatch.Test;

[TestClass]
public sealed class ComplexTests
{
    // ComplexRoot with Level1->Level2->Level3 + dictionary

    [TestMethod]
    public void Patch_ComplexRoot_DeepNestedObject_Succeeds_ApplyTo()
    {
        var obj = new ComplexRoot
        {
            Title = "original",
            Level1 = new Level1
            {
                Name = "L1",
                Level2 = new Level2
                {
                    Tag = "L2",
                    Level3 = new Level3 { Value = "old", Number = 1 },
                    Scores = { ["a"] = 10 }
                }
            },
            Registry = { ["key1"] = new Level2 { Tag = "reg1", Level3 = new Level3 { Value = "v1", Number = 5 } } },
            DeepDictionary =
            {
                ["outer"] = new Dictionary<string, SimpleModel> { ["inner"] = new SimpleModel { Int1 = 1, String1 = "x" } }
            }
        };

        var patch = """{"Level1": {"Level2": {"Level3": {"Value": "new"}}}, "Title": "updated"}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual("new", obj.Level1?.Level2?.Level3?.Value);
        Assert.AreEqual(1, obj.Level1?.Level2?.Level3?.Number);
        Assert.AreEqual("updated", obj.Title);
        Assert.AreEqual("L1", obj.Level1?.Name);
    }

    [TestMethod]
    public void Patch_ComplexRoot_DeepNestedObject_Succeeds_SafeApplyTo()
    {
        var obj = new ComplexRoot
        {
            Title = "original",
            Level1 = new Level1
            {
                Name = "L1",
                Level2 = new Level2
                {
                    Tag = "L2",
                    Level3 = new Level3 { Value = "old", Number = 1 },
                    Scores = { ["a"] = 10 }
                }
            },
            Registry = { ["key1"] = new Level2 { Tag = "reg1", Level3 = new Level3 { Value = "v1", Number = 5 } } },
            DeepDictionary =
            {
                ["outer"] = new Dictionary<string, SimpleModel> { ["inner"] = new SimpleModel { Int1 = 1, String1 = "x" } }
            }
        };

        var patch = """{"Level1": {"Level2": {"Level3": {"Value": "new"}}}, "Title": "updated"}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("new", obj.Level1?.Level2?.Level3?.Value);
        Assert.AreEqual("updated", obj.Title);
    }

    [TestMethod]
    public void Patch_ComplexRoot_NullIntermediateCreates_Succeeds_ApplyTo()
    {
        var obj = new ComplexRoot { Level1 = null, Title = "t" };
        var patch = """{"Level1": {"Name": "created", "Level2": {"Tag": "L2", "Level3": {"Value": "deep"}}}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.IsNotNull(obj.Level1);
        Assert.AreEqual("created", obj.Level1?.Name);
        Assert.AreEqual("deep", obj.Level1?.Level2?.Level3?.Value);
    }

    [TestMethod]
    public void Patch_ComplexRoot_NullIntermediateCreates_Succeeds_SafeApplyTo()
    {
        var obj = new ComplexRoot { Level1 = null, Title = "t" };
        var patch = """{"Level1": {"Name": "created", "Level2": {"Tag": "L2", "Level3": {"Value": "deep"}}}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("created", obj.Level1?.Name);
        Assert.AreEqual("deep", obj.Level1?.Level2?.Level3?.Value);
    }

    [TestMethod]
    public void Patch_ComplexRoot_MultipleNestedDictionaries_Succeeds_ApplyTo()
    {
        var obj = new ComplexRoot
        {
            Registry =
            {
                ["k1"] = new Level2 { Tag = "t1", Scores = { ["s1"] = 1 }, Level3 = new Level3 { Value = "v1", Number = 10 } },
                ["k2"] = new Level2 { Tag = "t2", Scores = { ["s2"] = 2 } }
            },
            DeepDictionary =
            {
                ["outer1"] = new Dictionary<string, SimpleModel>
                {
                    ["inner1"] = new SimpleModel { Int1 = 1, String1 = "a" },
                    ["inner2"] = new SimpleModel { Int1 = 2, String1 = "b" }
                }
            }
        };

        var patch = """
        {
            "Registry": {
                "k1": {"Tag": "updated", "Scores": {"s1": 99, "newScore": 42}},
                "k3": {"Tag": "newEntry", "Level3": {"Value": "newVal"}}
            },
            "DeepDictionary": {
                "outer1": {"inner1": {"Int1": 999}},
                "outer2": {"newInner": {"String1": "fresh"}}
            }
        }
        """;

        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual("updated", obj.Registry["k1"].Tag);
        Assert.AreEqual(99, obj.Registry["k1"].Scores["s1"]);
        Assert.AreEqual(42, obj.Registry["k1"].Scores["newScore"]);
        Assert.AreEqual("newEntry", obj.Registry["k3"].Tag);
        Assert.AreEqual("newVal", obj.Registry["k3"].Level3?.Value);
        Assert.AreEqual(999, obj.DeepDictionary["outer1"]["inner1"].Int1);
        Assert.AreEqual("b", obj.DeepDictionary["outer1"]["inner2"].String1);
        Assert.AreEqual("fresh", obj.DeepDictionary["outer2"]["newInner"].String1);
    }

    [TestMethod]
    public void Patch_ComplexRoot_MultipleNestedDictionaries_Succeeds_SafeApplyTo()
    {
        var obj = new ComplexRoot
        {
            Registry =
            {
                ["k1"] = new Level2 { Tag = "t1", Scores = { ["s1"] = 1 }, Level3 = new Level3 { Value = "v1", Number = 10 } },
                ["k2"] = new Level2 { Tag = "t2", Scores = { ["s2"] = 2 } }
            },
            DeepDictionary =
            {
                ["outer1"] = new Dictionary<string, SimpleModel>
                {
                    ["inner1"] = new SimpleModel { Int1 = 1, String1 = "a" }
                }
            }
        };

        var patch = """
        {
            "Registry": {
                "k1": {"Tag": "updated", "Scores": {"s1": 99}},
                "k3": {"Tag": "newEntry"}
            },
            "DeepDictionary": {
                "outer1": {"inner1": {"Int1": 999}},
                "outer2": {"newInner": {"String1": "fresh"}}
            }
        }
        """;

        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("updated", obj.Registry["k1"].Tag);
        Assert.AreEqual(999, obj.DeepDictionary["outer1"]["inner1"].Int1);
        Assert.AreEqual("fresh", obj.DeepDictionary["outer2"]["newInner"].String1);
    }

    // Company -> Department -> Employee model

    [TestMethod]
    public void Patch_Company_DeepHierarchy_Succeeds_ApplyTo()
    {
        var obj = new Company
        {
            Name = "Acme",
            Ceo = new Employee { Name = "Alice", Age = 50 },
            Departments =
            {
                ["eng"] = new Department
                {
                    DeptName = "Engineering",
                    Manager = new Employee { Name = "Bob", Age = 40 },
                    Employees =
                    {
                        ["e1"] = new Employee { Name = "Carol", Age = 30 },
                        ["e2"] = new Employee { Name = "Dave", Age = 35 }
                    }
                }
            }
        };

        var patch = """
        {
            "Departments": {
                "eng": {
                    "Employees": {
                        "e1": {"Age": 31},
                        "e3": {"Name": "Eve", "Age": 28}
                    },
                    "Manager": {"Name": "Bobby"}
                }
            },
            "Ceo": {"Age": 51}
        }
        """;

        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual(31, obj.Departments["eng"].Employees["e1"].Age);
        Assert.AreEqual("Eve", obj.Departments["eng"].Employees["e3"].Name);
        Assert.AreEqual(28, obj.Departments["eng"].Employees["e3"].Age);
        Assert.AreEqual("Bobby", obj.Departments["eng"].Manager?.Name);
        Assert.AreEqual(40, obj.Departments["eng"].Manager?.Age); // unchanged
        Assert.AreEqual(51, obj.Ceo?.Age);
        Assert.AreEqual("Acme", obj.Name); // unchanged
    }

    [TestMethod]
    public void Patch_Company_DeepHierarchy_Succeeds_SafeApplyTo()
    {
        var obj = new Company
        {
            Name = "Acme",
            Ceo = new Employee { Name = "Alice", Age = 50 },
            Departments =
            {
                ["eng"] = new Department
                {
                    DeptName = "Engineering",
                    Manager = new Employee { Name = "Bob", Age = 40 },
                    Employees =
                    {
                        ["e1"] = new Employee { Name = "Carol", Age = 30 },
                        ["e2"] = new Employee { Name = "Dave", Age = 35 }
                    }
                }
            }
        };

        var patch = """
        {
            "Departments": {
                "eng": {
                    "Employees": {
                        "e1": {"Age": 31},
                        "e3": {"Name": "Eve", "Age": 28}
                    },
                    "Manager": {"Name": "Bobby"}
                }
            },
            "Ceo": {"Age": 51}
        }
        """;

        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(31, obj.Departments["eng"].Employees["e1"].Age);
        Assert.AreEqual("Eve", obj.Departments["eng"].Employees["e3"].Name);
        Assert.AreEqual("Bobby", obj.Departments["eng"].Manager?.Name);
    }

    [TestMethod]
    public void Patch_Company_AddNewDepartment_Succeeds_ApplyTo()
    {
        var obj = new Company { Name = "Acme" };
        var patch = """
        {
            "Departments": {
                "hr": {"DeptName": "HR", "Employees": {"h1": {"Name": "Helen", "Age": 29}}}
            }
        }
        """;
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.IsTrue(obj.Departments.ContainsKey("hr"));
        Assert.AreEqual("HR", obj.Departments["hr"].DeptName);
        Assert.AreEqual("Helen", obj.Departments["hr"].Employees["h1"].Name);
    }

    [TestMethod]
    public void Patch_Company_AddNewDepartment_Succeeds_SafeApplyTo()
    {
        var obj = new Company { Name = "Acme" };
        var patch = """
        {
            "Departments": {
                "hr": {"DeptName": "HR", "Employees": {"h1": {"Name": "Helen", "Age": 29}}}
            }
        }
        """;
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("Helen", obj.Departments["hr"].Employees["h1"].Name);
    }

    // Triple nested dictionary

    [TestMethod]
    public void Patch_TripleNestedDictionary_Succeeds_ApplyTo()
    {
        var obj = new MultiDictModel
        {
            TripleNested =
            {
                ["a"] = new Dictionary<string, Dictionary<string, int>>
                {
                    ["b"] = new Dictionary<string, int> { ["c"] = 1, ["d"] = 2 }
                }
            }
        };
        var patch = """{"TripleNested": {"a": {"b": {"c": 99}}}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual(99, obj.TripleNested["a"]["b"]["c"]);
        Assert.AreEqual(2, obj.TripleNested["a"]["b"]["d"]);
    }

    [TestMethod]
    public void Patch_TripleNestedDictionary_Succeeds_SafeApplyTo()
    {
        var obj = new MultiDictModel
        {
            TripleNested =
            {
                ["a"] = new Dictionary<string, Dictionary<string, int>>
                {
                    ["b"] = new Dictionary<string, int> { ["c"] = 1, ["d"] = 2 }
                }
            }
        };
        var patch = """{"TripleNested": {"a": {"b": {"c": 99}}}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(99, obj.TripleNested["a"]["b"]["c"]);
    }

    [TestMethod]
    public void Patch_TripleNestedDictionary_AddDeepNewKeys_Succeeds_ApplyTo()
    {
        var obj = new MultiDictModel
        {
            TripleNested =
            {
                ["a"] = new Dictionary<string, Dictionary<string, int>>
                {
                    ["b"] = new Dictionary<string, int> { ["c"] = 1 }
                }
            }
        };
        var patch = """{"TripleNested": {"a": {"b": {"newKey": 42}}, "newA": {"newB": {"newC": 7}}}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.AreEqual(42, obj.TripleNested["a"]["b"]["newKey"]);
        Assert.AreEqual(7, obj.TripleNested["newA"]["newB"]["newC"]);
    }

    [TestMethod]
    public void Patch_TripleNestedDictionary_AddDeepNewKeys_Succeeds_SafeApplyTo()
    {
        var obj = new MultiDictModel
        {
            TripleNested =
            {
                ["a"] = new Dictionary<string, Dictionary<string, int>>
                {
                    ["b"] = new Dictionary<string, int> { ["c"] = 1 }
                }
            }
        };
        var patch = """{"TripleNested": {"a": {"b": {"newKey": 42}}, "newA": {"newB": {"newC": 7}}}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(42, obj.TripleNested["a"]["b"]["newKey"]);
        Assert.AreEqual(7, obj.TripleNested["newA"]["newB"]["newC"]);
    }

    // Failure: deep invalid should be atomic across all levels

    [TestMethod]
    public void Patch_ComplexRoot_DeepInvalidType_FailsAtomic_ApplyTo()
    {
        var obj = new ComplexRoot
        {
            Level1 = new Level1
            {
                Name = "L1",
                Level2 = new Level2 { Level3 = new Level3 { Value = "old", Number = 1 }, Scores = { ["a"] = 10 } }
            },
            Title = "t"
        };
        var patch = """{"Level1": {"Level2": {"Level3": {"Number": "bad"}}}, "Title": "updated"}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
        Assert.AreEqual("old", obj.Level1?.Level2?.Level3?.Value);
        Assert.AreEqual(1, obj.Level1?.Level2?.Level3?.Number);
        Assert.AreEqual("t", obj.Title); // should not pollute
    }

    [TestMethod]
    public void Patch_ComplexRoot_DeepInvalidType_FailsAtomic_SafeApplyTo()
    {
        var obj = new ComplexRoot
        {
            Level1 = new Level1
            {
                Name = "L1",
                Level2 = new Level2 { Level3 = new Level3 { Value = "old", Number = 1 }, Scores = { ["a"] = 10 } }
            },
            Title = "t"
        };
        var patch = """{"Level1": {"Level2": {"Level3": {"Number": "bad"}}}, "Title": "updated"}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("old", obj.Level1?.Level2?.Level3?.Value);
        Assert.AreEqual(1, obj.Level1?.Level2?.Level3?.Number);
        Assert.AreEqual("t", obj.Title);
    }

    [TestMethod]
    public void Patch_Company_DeepInvalidAtomic_Fails_ApplyTo()
    {
        var obj = new Company
        {
            Name = "Acme",
            Departments =
            {
                ["eng"] = new Department
                {
                    DeptName = "Engineering",
                    Employees = { ["e1"] = new Employee { Name = "Carol", Age = 30 } }
                }
            }
        };
        var patch = """{"Departments": {"eng": {"Employees": {"e1": {"Age": "not-a-number"}}}}}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
        Assert.AreEqual(30, obj.Departments["eng"].Employees["e1"].Age);
    }

    [TestMethod]
    public void Patch_Company_DeepInvalidAtomic_Fails_SafeApplyTo()
    {
        var obj = new Company
        {
            Name = "Acme",
            Departments =
            {
                ["eng"] = new Department
                {
                    DeptName = "Engineering",
                    Employees = { ["e1"] = new Employee { Name = "Carol", Age = 30 } }
                }
            }
        };
        var patch = """{"Departments": {"eng": {"Employees": {"e1": {"Age": "not-a-number"}}}}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(30, obj.Departments["eng"].Employees["e1"].Age);
    }

    [TestMethod]
    public void Patch_TripleNested_InvalidDeepType_Fails_ApplyTo()
    {
        var obj = new MultiDictModel
        {
            TripleNested = { ["a"] = new Dictionary<string, Dictionary<string, int>> { ["b"] = new Dictionary<string, int> { ["c"] = 1 } } }
        };
        var patch = """{"TripleNested": {"a": {"b": {"c": "bad"}}}}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
        Assert.AreEqual(1, obj.TripleNested["a"]["b"]["c"]);
    }

    [TestMethod]
    public void Patch_TripleNested_InvalidDeepType_Fails_SafeApplyTo()
    {
        var obj = new MultiDictModel
        {
            TripleNested = { ["a"] = new Dictionary<string, Dictionary<string, int>> { ["b"] = new Dictionary<string, int> { ["c"] = 1 } } }
        };
        var patch = """{"TripleNested": {"a": {"b": {"c": "bad"}}}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(1, obj.TripleNested["a"]["b"]["c"]);
    }

    // Multiple errors in deep patch should all be reported

    [TestMethod]
    public void Patch_Complex_MultipleInvalidErrors_Fails_SafeApplyTo()
    {
        var obj = new ComplexRoot
        {
            Level1 = new Level1 { Name = "L1", Level2 = new Level2 { Level3 = new Level3 { Value = "old", Number = 1 } } },
            Registry = { ["k1"] = new Level2 { Tag = "t1" } }
        };
        var patch = """{"Level1": {"Level2": {"Level3": {"Number": "bad"}}}, "Registry": {"k1": {"Scores": {"s1": "bad2"}}}, "UnknownProp": 123}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsFalse(result.Succeeded);
        Assert.IsGreaterThanOrEqualTo(1, result.Errors.Count);
        // Atomic: no changes
        Assert.AreEqual(1, obj.Level1?.Level2?.Level3?.Number);
    }

    [TestMethod]
    public void Patch_Complex_MultipleInvalidErrors_Fails_ApplyTo()
    {
        var obj = new ComplexRoot
        {
            Level1 = new Level1 { Name = "L1", Level2 = new Level2 { Level3 = new Level3 { Value = "old", Number = 1 } } },
            Registry = { ["k1"] = new Level2 { Tag = "t1" } }
        };
        var patch = """{"Level1": {"Level2": {"Level3": {"Number": "bad"}}}, "Registry": {"k1": {"Scores": {"s1": "bad2"}}}, "UnknownProp": 123}""";
        Assert.Throws<JsonMergePatchException>(() => JsonMergePatcher.ApplyTo(ref obj, patch));
        Assert.AreEqual(1, obj.Level1?.Level2?.Level3?.Number);
    }

    // Null deep dictionary creation

    [TestMethod]
    public void Patch_DeepDictionary_NullOuterCreates_Succeeds_ApplyTo()
    {
        var obj = new ComplexRoot { DeepDictionary = null!, Title = "t" };
        var patch = """{"DeepDictionary": {"outer": {"inner": {"Int1": 5}}}}""";
        JsonMergePatcher.ApplyTo(ref obj, patch);
        Assert.IsNotNull(obj.DeepDictionary);
        Assert.AreEqual(5, obj.DeepDictionary["outer"]["inner"].Int1);
    }

    [TestMethod]
    public void Patch_DeepDictionary_NullOuterCreates_Succeeds_SafeApplyTo()
    {
        var obj = new ComplexRoot { DeepDictionary = null!, Title = "t" };
        var patch = """{"DeepDictionary": {"outer": {"inner": {"Int1": 5}}}}""";
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(5, obj.DeepDictionary["outer"]["inner"].Int1);
    }
}
