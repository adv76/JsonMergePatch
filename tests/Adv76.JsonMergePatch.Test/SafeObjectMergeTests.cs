using System.Text.Json;
using System.Text.Json.Serialization;

namespace Adv76.JsonMergePatch.Test;

[TestClass]
public sealed class SafeObjectMergeTests
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

    private class Class3()
    {
        [JsonMergeConverterBehavior(Behavior = JsonMergeConverterBehavior.UseCustomConverter)]
        [JsonConverter(typeof(Class3.TimeSpanJsonConverter))]
        public TimeSpan TimeSpan1 { get; set; }

        public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
        {
            public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                reader.Read();
                var p1 = reader.GetString();
                reader.Read();
                var v1 = reader.GetInt32();
                reader.Read();
                var p2 = reader.GetString();
                reader.Read();
                var v2 = reader.GetInt32();
                reader.Read();
                return new TimeSpan(p1 == "hr" ? v1 : v2, p1 == "min" ? v1 : v2, 0);
            }

            public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("hr");
                writer.WriteNumberValue(value.Hours);
                writer.WritePropertyName("min");
                writer.WriteNumberValue(value.Minutes);
                writer.WriteEndObject();
            }
        }
    }
    
    private class Class4()
    {   
        [JsonMergePropertySecurity(Policy = JsonMergeSecurityPolicy.AllowPatching)]
        public string? AllowedString { get; set; }
        
        [JsonMergePropertySecurity(Policy = JsonMergeSecurityPolicy.BlockPatching)]
        public string? BlockedString { get; set; }
        
        [JsonMergePropertySecurity(Policy = JsonMergeSecurityPolicy.SkipSilently)]
        public string? IgnoredString { get; set; }
        
        public string? String { get; set; }
    }
    
    private class Class5()
    {
        public Dictionary<string, int> Dictionary1 { get; set; } = [];
        public Dictionary<int, int> Dictionary2 { get; set; } = [];
    }
    
    private class Class6()
    {
        public Dictionary<string, Class1> Dictionary1 { get; set; } = [];
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
        
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        
        Assert.IsTrue(result.Succeeded);
        
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
        
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        
        Assert.IsTrue(result.Succeeded);
        
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
        
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        
        Assert.IsTrue(result.Succeeded);
        
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
        
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        
        Assert.IsTrue(result.Succeeded);
        
        Assert.AreEqual(1, obj.Class1?.Int1);
    }
    
    [TestMethod]
    public void TestMethod5()
    {
        var obj = "hello";

        var patch = "\"world\"";
        
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        
        Assert.IsTrue(result.Succeeded);
        
        Assert.AreEqual("world", obj);
    }
    
    [TestMethod]
    public void TrySetStringToIntTest()
    {
        var obj = "hello";

        var patch = "1";
        
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("~", result.Errors.Keys.First());
        
        Assert.AreEqual("hello", obj);
    }
    
    [TestMethod]
    public void TestMethod6()
    {
        int[] obj = [1, 2];

        var patch = "[3, 4]";
        
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        
        Assert.IsTrue(result.Succeeded);
        
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
        
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch, new JsonMergeOptions() { JsonSerializerOptions = JsonSerializerOptions.Web });
        
        Assert.IsTrue(result.Succeeded);
        
        Assert.AreEqual(1, obj.Int1);
        Assert.AreEqual("hello", obj.String1);
    }

    [TestMethod]
    public void CustomObjectConverterTest()
    {
        var obj = new Class3()
        {
            TimeSpan1 = new TimeSpan(2, 3, 0)
        };

        var patch = "{\"TimeSpan1\": {\"hr\": 4,\"min\": 5}}";
        
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        
        Assert.IsTrue(result.Succeeded);
        
        Assert.AreEqual(4, obj.TimeSpan1.Hours);
        Assert.AreEqual(5, obj.TimeSpan1.Minutes);
    }

    [TestMethod]
    public void TestAllowPropertyUpdate()
    {
        var obj = new Class4()
        {
            AllowedString = "Hello"
        };

        var patch = "{\"AllowedString\": \"World\"}";
        
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch, JsonMergeOptions.Strict);
        
        Assert.IsTrue(result.Succeeded);
        
        Assert.AreEqual("World", obj.AllowedString);
    }
    
    [TestMethod]
    public void TestBlockPropertyUpdate()
    {
        var obj = new Class4()
        {
            BlockedString = "Hello"
        };

        var patch = "{\"BlockedString\": \"World\"}";
        
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch, JsonMergeOptions.Strict);
        
        Assert.IsFalse(result.Succeeded);
        
        Assert.AreEqual("Hello", obj.BlockedString);
    }
    
    [TestMethod]
    public void TestSkipPropertyUpdate()
    {
        var obj = new Class4()
        {
            IgnoredString = "Hello"
        };

        var patch = "{\"IgnoredString\": \"World\"}";
        
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch, JsonMergeOptions.Strict);
        
        Assert.IsTrue(result.Succeeded);
        
        Assert.AreEqual("Hello", obj.IgnoredString);
    }
    
    [TestMethod]
    public void TestDefaultStrictUpdate()
    {
        var obj = new Class4()
        {
            String = "Hello"
        };

        var patch = "{\"String\": \"World\"}";
        
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch, JsonMergeOptions.Strict);
        
        Assert.IsFalse(result.Succeeded);
        
        Assert.AreEqual("Hello", obj.String);
    }
    
    [TestMethod]
    public void TestDefaultLooseUpdate()
    {
        var obj = new Class4()
        {
            String = "Hello"
        };

        var patch = "{\"String\": \"World\"}";
        
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch, JsonMergeOptions.Default);
        
        Assert.IsTrue(result.Succeeded);
        
        Assert.AreEqual("World", obj.String);
    }
    
    [TestMethod]
    public void TestUpdatesDontPollute()
    {
        var obj = new Class1()
        {
            Int1 = 3,
            String1 = "hello"
        };

        var patch = "{\"Int1\": 1,\"String1\": 1}";
        
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);
        
        Assert.IsFalse(result.Succeeded);
        
        Assert.AreEqual(3, obj.Int1);
        Assert.AreEqual("hello", obj.String1);
    }
    
    [TestMethod]
    public void SimpleDictionaryTest()
    {
        var obj = new Class5()
        {
            Dictionary1 =
            {
                ["hello"] = 1,
                ["world"] = 2
            }
        };

        var patch = "{\"Dictionary1\": {\"hello\": 5}}";
        
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);

        Assert.IsTrue(result.Succeeded);
        
        Assert.AreEqual(5, obj.Dictionary1["hello"]);
        Assert.AreEqual(2, obj.Dictionary1["world"]);
    }
    
    [TestMethod]
    public void IntDictionaryTest()
    {
        var obj = new Class5()
        {
            Dictionary2 =
            {
                [1] = 12,
                [2] = 9
            }
        };

        var patch = "{\"Dictionary2\": {2: 42}}";
        
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);

        Assert.IsTrue(result.Succeeded);
        
        Assert.AreEqual(12, obj.Dictionary2[1]);
        Assert.AreEqual(42, obj.Dictionary2[2]);
    }
    
    [TestMethod]
    public void ObjectDictionaryTest()
    {
        var obj = new Class6()
        {
            Dictionary1 =
            {
                ["hello"] = new Class1()
                {
                    Int1 = 3,
                    String1 = "hello"
                },
                ["world"] = new Class1()
                {
                    Int1 = 4,
                    String1 = "world"
                }
            }
        };

        var patch = "{\"Dictionary1\": {\"hello\": {\"Int1\":8}}}";
        
        var result = JsonMergePatcher.SafeApplyTo(ref obj, patch);

        Assert.IsTrue(result.Succeeded);
        
        Assert.AreEqual(8, obj.Dictionary1["hello"].Int1);
    }
}