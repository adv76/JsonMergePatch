namespace Adv76.JsonMergePatch.Test.TestClasses;

internal class Level3
{
    public string? Value { get; set; }
    public int Number { get; set; }
}

internal class Level2
{
    public Level3? Level3 { get; set; }
    public Dictionary<string, int> Scores { get; set; } = [];
    public string? Tag { get; set; }
}

internal class Level1
{
    public Level2? Level2 { get; set; }
    public string? Name { get; set; }
}

internal class ComplexRoot
{
    public Level1? Level1 { get; set; }
    public Dictionary<string, Level2> Registry { get; set; } = [];
    public Dictionary<string, Dictionary<string, SimpleModel>> DeepDictionary { get; set; } = [];
    public string? Title { get; set; }
}

internal class Employee
{
    public string? Name { get; set; }
    public int Age { get; set; }
}

internal class Department
{
    public string? DeptName { get; set; }
    public Dictionary<string, Employee> Employees { get; set; } = [];
    public Employee? Manager { get; set; }
}

internal class Company
{
    public string? Name { get; set; }
    public Dictionary<string, Department> Departments { get; set; } = [];
    public Employee? Ceo { get; set; }
}
