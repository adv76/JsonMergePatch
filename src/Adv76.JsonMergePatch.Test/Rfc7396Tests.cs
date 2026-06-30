using System.Text.Json;
using System.Text.Json.Nodes;

namespace Adv76.JsonMergePatch.Test;

[TestClass]
public sealed class Rfc7396Tests
{
    [TestMethod]
    public void TestMethod1()
    {
        var obj = new JsonObject { { "a", "b" } };

        var patch = new JsonObject { { "a", "c" } };

        var result = JsonMergePatch.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonObject { { "a", "c" } };
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod2()
    {
        var obj = new JsonObject { { "a", "b" } };

        var patch = new JsonObject { { "b", "c" } };

        var result = JsonMergePatch.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonObject { { "a", "b" }, { "b", "c" } };
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod3()
    {
        var obj = new JsonObject { { "a", "b" } };

        var patch = new JsonObject { { "a", null } };

        var result = JsonMergePatch.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonObject();
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod4()
    {
        var obj = new JsonObject { { "a", "b" }, { "b", "c" } };

        var patch = new JsonObject { { "a", null } };

        var result = JsonMergePatch.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonObject { { "b", "c" } };
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod5()
    {
        var obj = new JsonObject { { "a", new JsonArray([ "b" ])} };

        var patch = new JsonObject { { "a", "c" } };

        var result = JsonMergePatch.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonObject { { "a", "c" } };
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod6()
    {
        var obj = new JsonObject { { "a", "c" } };

        var patch = new JsonObject { { "a", new JsonArray([ "b" ])} };

        var result = JsonMergePatch.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonObject { { "a", new JsonArray([ "b" ]) } };
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
}