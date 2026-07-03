using System.Text.Json.Nodes;

namespace Adv76.JsonMergePatch.Test;

[TestClass]
public sealed class ObjectMergeTests
{
    private class Class1()
    {   
        public int Int1 { get; set; }
        public string? String1 { get; set; }
    }
    
    [TestMethod]
    public void TestMethod1()
    {
        var obj = new Class1()
        {
            Int1 = 3,
            String1 = "hello"
        };

        var patch = new JsonObject { { "Int1", 1 } };

        var result = obj.MergePatch(patch);
        
        Assert.IsNotNull(result);
        
        Assert.AreEqual(1, result.Int1);
        Assert.AreEqual("hello", result.String1);
    }
    
    [TestMethod]
    public void TestMethod2()
    {
        var obj = new Class1()
        {
            Int1 = 3,
            String1 = "hello"
        };

        var patch = new JsonObject { { "Int1", 1 }, {"String1", null} };

        var result = obj.MergePatch(patch);
        
        Assert.IsNotNull(result);
        
        Assert.AreEqual(1, result.Int1);
        Assert.IsNull(result.String1);
    }
}