using System.Text.Json;

namespace Adv76.JsonMergePatch.Test;

[TestClass]
public sealed class ObjectMergeTests
{
    private class Class1()
    {   
        public int Int1 { get; set; }
        public string? String1 { get; set; }
    }
    
    private class Class2()
    {   
        public Class1? Class1 { get; set; }
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

        var patch = "{\"Int1\": 1}";
        
        JsonMergePatcher.ApplyTo(ref obj, patch);
        
        Assert.AreEqual(1, obj.Int1);
        Assert.AreEqual("hello", obj.String1);
    }
    
    [TestMethod]
    public void TestMethod2()
    {
        var obj = new Class1()
        {
            Int1 = 3,
            String1 = "hello"
        };

        var patch = "{\"String1\": null}";
        
        JsonMergePatcher.ApplyTo(ref obj, patch);
        
        Assert.AreEqual(3, obj.Int1);
        Assert.IsNull(obj.String1);
    }
    
    [TestMethod]
    public void TestMethod3()
    {
        var obj = new Class2()
        {
            Class1 = new Class1()
            {
                Int1 = 3, 
                String1 = "world"
            },
            String1 = "hello"
        };

        var patch = "{\"Class1\": { \"Int1\": 1 }}";
        
        JsonMergePatcher.ApplyTo(ref obj, patch);
        
        Assert.AreEqual(1, obj.Class1?.Int1);
        Assert.AreEqual("world", obj.Class1?.String1);
    }
    
    [TestMethod]
    public void TestMethod4()
    {
        var obj = new Class2()
        {
            Class1 = null,
            String1 = "hello"
        };

        var patch = "{\"Class1\": { \"Int1\": 1 }}";
        
        JsonMergePatcher.ApplyTo(ref obj, patch);
        
        Assert.AreEqual(1, obj.Class1?.Int1);
    }
    
    [TestMethod]
    public void TestMethod5()
    {
        var obj = "hello";

        var patch = "\"world\"";
        
        JsonMergePatcher.ApplyTo(ref obj, patch);
        
        Assert.AreEqual("world", obj);
    }
    
    [TestMethod]
    public void TestMethod6()
    {
        int[] obj = [1, 2];

        var patch = "[3, 4]";
        
        JsonMergePatcher.ApplyTo(ref obj, patch);
        
        Assert.AreEqual(3, obj[0]);
        Assert.AreEqual(4, obj[1]);
    }
    
    [TestMethod]
    public void TestMethod7()
    {
        var obj = new Class1()
        {
            Int1 = 3,
            String1 = "hello"
        };

        var patch = "{\"int1\": 1}";
        
        JsonMergePatcher.ApplyTo(ref obj, patch, JsonSerializerOptions.Web);
        
        Assert.AreEqual(1, obj.Int1);
        Assert.AreEqual("hello", obj.String1);
    }
}