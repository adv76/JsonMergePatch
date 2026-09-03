namespace Adv76.JsonMergePatch.Benchmarks;

public class Class1
{
    public int Int0 { get; set; }
    public int Int1 { get; set; }
    public int Int2 { get; set; }
    public string? String0 { get; set; }
    public string? String1 { get; set; }
    public string? String2 { get; set; }
    public double Double0 { get; set; }
    public double Double1 { get; set; }
    public double Double2 { get; set; }

    public static Class1 BuildForTesting()
    {
        return new()
        {
            Int0 = 1,
            Int1 = 2,
            Int2 = 3,
            String0 = "Hello",
            String1 = "World",
            String2 = null,
            Double0 = 1.5,
            Double1 = 2.5,
            Double2 = 3.5,
        };
    }
}