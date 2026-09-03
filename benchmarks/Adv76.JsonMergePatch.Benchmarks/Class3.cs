namespace Adv76.JsonMergePatch.Benchmarks;

public class Class3
{
    public sbyte SByte0 { get; set; }
    public sbyte SByte1 { get; set; }
    public byte Byte0 { get; set; }
    public byte Byte1 { get; set; }
    public short Short0 { get; set; }
    public short Short1 { get; set; }
    public ushort UShort0 { get; set; }
    public ushort UShort1 { get; set; }
    public int Int0 { get; set; }
    public int Int1 { get; set; }
    public uint UInt0 { get; set; }
    public uint UInt1 { get; set; }
    public long Long0 { get; set; }
    public long Long1 { get; set; }
    public ulong ULong0 { get; set; }
    public ulong ULong1 { get; set; }
    public Int128 Int1280 { get; set; }
    public Int128 Int1281 { get; set; }
    public UInt128 UInt1280 { get; set; }
    public UInt128 UInt1281 { get; set; }
    public Half Half0 { get; set; }
    public Half Half1 { get; set; }
    public float Float0 { get; set; }
    public float Float1 { get; set; }
    public double Double0 { get; set; }
    public double Double1 { get; set; }
    public decimal Decimal0 { get; set; }
    public decimal Decimal1 { get; set; }
    public nint NInt0 { get; set; }
    public nint NInt1 { get; set; }
    public nuint NUInt0 { get; set; }
    public nuint NUInt1 { get; set; }
    public DateTime DateTime0 { get; set; }
    public DateOnly DateOnly0 { get; set; }
    public TimeOnly TimeOnly0 { get; set; }
    public DateTimeOffset DateTimeOffset0 { get; set; }
    public TimeSpan TimeSpan0 { get; set; }
    public Guid Guid0 { get; set; }
    public Guid Guid1 { get; set; }
    public string? String0 { get; set; }
    public string? String1 { get; set; }
    public string? String2 { get; set; }
    public Dictionary<string, int>? IntDict { get; set; }
    public Dictionary<string, string>? StringDict { get; set; }
    public Dictionary<string, DictValue>? ObjectDict { get; set; }
    public List<int>? IntList { get; set; }
    public List<Item>? ObjectList { get; set; }
    public IList<string>? StringIList { get; set; }
    public IList<Item>? ObjectIList { get; set; }
    public ICollection<double>? DoubleCollection { get; set; }
    public ICollection<Item>? ObjectCollection { get; set; }
    public Level1? DeepBranch { get; set; }
    public TwoLevelBranch? ShortBranch { get; set; }

    public static Class3 BuildForTesting()
    {
        return new Class3
        {
            SByte0 = 1,
            SByte1 = 2,
            Byte0 = 3,
            Byte1 = 4,
            Short0 = 5,
            Short1 = 6,
            UShort0 = 7,
            UShort1 = 8,
            Int0 = 1,
            Int1 = 2,
            UInt0 = 3,
            UInt1 = 4,
            Long0 = 5,
            Long1 = 6,
            ULong0 = 7,
            ULong1 = 8,
            Int1280 = 9,
            Int1281 = 10,
            UInt1280 = 11,
            UInt1281 = 12,
            Half0 = (Half)1.5,
            Half1 = (Half)2.5,
            Float0 = 1.5f,
            Float1 = 2.5f,
            Double0 = 1.5,
            Double1 = 2.5,
            Decimal0 = 1.5m,
            Decimal1 = 2.5m,
            NInt0 = 1,
            NInt1 = 2,
            NUInt0 = 3,
            NUInt1 = 4,
            DateTime0 = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            DateOnly0 = new DateOnly(2024, 1, 1),
            TimeOnly0 = new TimeOnly(12, 0, 0),
            DateTimeOffset0 = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero),
            TimeSpan0 = TimeSpan.FromHours(1),
            Guid0 = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid1 = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            String0 = "Hello",
            String1 = "World",
            String2 = "Unpatched",
            IntDict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 },
            StringDict = new Dictionary<string, string> { ["k1"] = "v1", ["k2"] = "v2" },
            ObjectDict = new Dictionary<string, DictValue>
            {
                ["item1"] = new DictValue { Int0 = 1, String0 = "Hello", Double0 = 1.5, DateTime0 = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc) },
            },
            IntList = new List<int> { 1, 2, 3 },
            ObjectList = new List<Item>
            {
                new() { Int0 = 1, String0 = "Hello", Double0 = 1.5 },
            },
            StringIList = new List<string> { "a", "b" },
            ObjectIList = new List<Item>
            {
                new() { Int0 = 2, String0 = "World", Double0 = 2.5 },
            },
            DoubleCollection = new List<double> { 1.5, 2.5 },
            ObjectCollection = new List<Item>
            {
                new() { Int0 = 3, String0 = "Unpatched", Double0 = 3.5 },
            },
            DeepBranch = new Level1
            {
                Int0 = 1,
                String0 = "Hello",
                Double0 = 1.5,
                Guid0 = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                DateTime0 = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                Level2 = new Level2
                {
                    Int0 = 1,
                    String0 = "Hello",
                    Double0 = 1.5,
                    Decimal0 = 1.5m,
                    Level3 = new Level3
                    {
                        Int0 = 1,
                        String0 = "Hello",
                        Double0 = 1.5,
                        Level4 = new Level4
                        {
                            Int0 = 1,
                            String0 = "Hello",
                            Double0 = 1.5,
                            Long0 = 100,
                            DateTime0 = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                        },
                    },
                },
            },
            ShortBranch = new TwoLevelBranch
            {
                Int0 = 1,
                String0 = "Hello",
                Double0 = 1.5,
                Leaf = new TwoLevelLeaf
                {
                    Int0 = 1,
                    String0 = "Hello",
                    Double0 = 1.5,
                },
            },
        };
    }

    public class Level1
    {
        public int Int0 { get; set; }
        public string? String0 { get; set; }
        public double Double0 { get; set; }
        public Guid Guid0 { get; set; }
        public DateTime DateTime0 { get; set; }
        public Level2? Level2 { get; set; }
    }

    public class Level2
    {
        public int Int0 { get; set; }
        public string? String0 { get; set; }
        public double Double0 { get; set; }
        public decimal Decimal0 { get; set; }
        public Level3? Level3 { get; set; }
    }

    public class Level3
    {
        public int Int0 { get; set; }
        public string? String0 { get; set; }
        public double Double0 { get; set; }
        public Level4? Level4 { get; set; }
    }

    public class Level4
    {
        public int Int0 { get; set; }
        public string? String0 { get; set; }
        public double Double0 { get; set; }
        public long Long0 { get; set; }
        public DateTime DateTime0 { get; set; }
    }

    public class TwoLevelBranch
    {
        public int Int0 { get; set; }
        public string? String0 { get; set; }
        public double Double0 { get; set; }
        public TwoLevelLeaf? Leaf { get; set; }
    }

    public class TwoLevelLeaf
    {
        public int Int0 { get; set; }
        public string? String0 { get; set; }
        public double Double0 { get; set; }
    }

    public class DictValue
    {
        public int Int0 { get; set; }
        public string? String0 { get; set; }
        public double Double0 { get; set; }
        public DateTime DateTime0 { get; set; }
    }

    public class Item
    {
        public int Int0 { get; set; }
        public string? String0 { get; set; }
        public double Double0 { get; set; }
    }
}
