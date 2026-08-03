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

        var patch = new JsonMergePatch<Class1>("{\"Int1\": 1}");
        
        patch.ApplyTo(ref obj);
        
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

        var patch = new JsonMergePatch<Class1>("{\"String1\": null}");
        
        patch.ApplyTo(ref obj);
        
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

        var patch = new JsonMergePatch<Class2>("{\"Class1\": { \"Int1\": 1 }}");
        
        patch.ApplyTo(ref obj);
        
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

        var patch = new JsonMergePatch<Class2>("{\"Class1\": { \"Int1\": 1 }}");
        
        patch.ApplyTo(ref obj);
        
        Assert.AreEqual(1, obj.Class1?.Int1);
    }
    
    [TestMethod]
    public void TestMethod5()
    {
        var obj = "hello";

        var patch = new JsonMergePatch<string>("\"world\"");
        
        patch.ApplyTo(ref obj);
        
        Assert.AreEqual("world", obj);
    }
    
    [TestMethod]
    public void TestMethod6()
    {
        int[] obj = [1, 2];

        var patch = new JsonMergePatch<int[]>("[3, 4]");
        
        patch.ApplyTo(ref obj);
        
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

        var patch = new JsonMergePatch<Class1>("{\"int1\": 1}", JsonSerializerOptions.Web);
        
        patch.ApplyTo(ref obj);
        
        Assert.AreEqual(1, obj.Int1);
        Assert.AreEqual("hello", obj.String1);
    }
}