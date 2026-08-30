namespace Adv76.JsonMergePatch.Benchmarks;

public class Class2
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
    public BranchClass? Branch0 { get; set; }
    
    public class BranchClass
    {
        public int Int0 { get; set; }
        public string? String0 { get; set; }
        public double Double0 { get; set; }
        public LeafClass? Leaf0 { get; set; }
    }

    public class LeafClass
    {
        public int Int0 { get; set; }
        public string? String0 { get; set; }
        public double Double0 { get; set; }
    }
}