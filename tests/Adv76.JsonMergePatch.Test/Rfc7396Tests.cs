using System.Text.Json;
using System.Text.Json.Nodes;

namespace Adv76.JsonMergePatch.Test;

/// <summary>
/// All tests are derived from the RFC 7396 documentation
/// </summary>
/// <remarks>
/// https://datatracker.ietf.org/doc/html/rfc7396
/// </remarks>
[TestClass]
public sealed class Rfc7396Tests
{
    [TestMethod]
    public void TestMethod1()
    {
        var obj = new JsonObject { { "a", "b" } };

        var patch = new JsonObject { { "a", "c" } };

        var result = JsonMergePatcher.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonObject { { "a", "c" } };
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod2()
    {
        var obj = new JsonObject { { "a", "b" } };

        var patch = new JsonObject { { "b", "c" } };

        var result = JsonMergePatcher.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonObject { { "a", "b" }, { "b", "c" } };
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod3()
    {
        var obj = new JsonObject { { "a", "b" } };

        var patch = new JsonObject { { "a", null } };

        var result = JsonMergePatcher.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonObject();
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod4()
    {
        var obj = new JsonObject { { "a", "b" }, { "b", "c" } };

        var patch = new JsonObject { { "a", null } };

        var result = JsonMergePatcher.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonObject { { "b", "c" } };
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod5()
    {
        var obj = new JsonObject { { "a", new JsonArray([ "b" ])} };

        var patch = new JsonObject { { "a", "c" } };

        var result = JsonMergePatcher.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonObject { { "a", "c" } };
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod6()
    {
        var obj = new JsonObject { { "a", "c" } };

        var patch = new JsonObject { { "a", new JsonArray([ "b" ])} };

        var result = JsonMergePatcher.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonObject { { "a", new JsonArray([ "b" ]) } };
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod7()
    {
        var obj = new JsonObject { { "a", new JsonObject{ { "b", "c" } } } };

        var patch = new JsonObject { { "a", new JsonObject{ { "b", "d" }, { "c", null } } } };


        var result = JsonMergePatcher.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonObject { { "a", new JsonObject{ { "b", "d" } } } };
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod8()
    {
        var obj = new JsonObject { { "a", new JsonObject{ { "b", "c" } } } };

        var patch = new JsonObject { { "a", new JsonArray([ 1 ])} };

        var result = JsonMergePatcher.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonObject { { "a", new JsonArray([ 1 ])} };
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod9()
    {
        var obj = new JsonArray(["a", "b"]);

        var patch = new JsonArray(["c", "d"]);

        var result = JsonMergePatcher.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonArray(["c", "d"]);
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod10()
    {
        var obj = new JsonObject { { "a", "b" } };

        var patch = new JsonArray([ "c" ]);

        var result = JsonMergePatcher.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonArray([ "c" ]);
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod11()
    {
        var obj = new JsonObject { { "a", "foo" } };

        var patch = JsonValue.Create((string?)null); // TODO revisit

        var result = JsonMergePatcher.Merge(obj, patch);
        
        Assert.IsNull(result);

        var expected = JsonValue.Create((string?)null);
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod12()
    {
        var obj = new JsonObject { { "a", "foo" } };

        var patch = JsonValue.Create("bar");

        var result = JsonMergePatcher.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = JsonValue.Create("bar");
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod13()
    {
        var obj = new JsonObject { { "e", null } };

        var patch = new JsonObject { { "a", 1 } };

        var result = JsonMergePatcher.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonObject { { "e", null }, { "a", 1 } };
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod14()
    {
        var obj = new JsonArray([ 1, 2 ]);

        var patch = new JsonObject { { "a", "b" }, { "c", null } };

        var result = JsonMergePatcher.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonObject { { "a", "b" } };
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
    
    [TestMethod]
    public void TestMethod15()
    {
        var obj = new JsonObject();

        var patch = new JsonObject { { "a", new JsonObject{ { "bb", new JsonObject{ { "ccc", null } } } } } };

        var result = JsonMergePatcher.Merge(obj, patch);
        
        Assert.IsNotNull(result);

        var expected = new JsonObject { { "a", new JsonObject{ { "bb", new JsonObject() } } } };
        
        Assert.IsTrue(JsonNode.DeepEquals(expected, result));
    }
}