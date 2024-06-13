using NanoLog;

namespace ConsoleDemo;

public class Person : ILogValue
{
    public string Name { get; set; } = null!;
    public DateTime Birthday { get; set; }
    public string Phone { get; set; } = null!;
    
    bool ILogValue.IsScalar => false;

    public void AppendMembers(ref LogMessageWriter writer, string name)
    {
        writer.AppendString(nameof(Name), Name);
        writer.AppendDateTime(nameof(Birthday), Birthday, "yyyy/MM/dd");
        writer.AppendString(nameof(Phone), Phone);
    }
}