namespace NanoLog.File.Viewer;

public sealed class LogTokenNode
{
    /// <summary>
    /// For LogValue
    /// </summary>
    /// <param name="parent"></param>
    internal LogTokenNode(LogTokenNode? parent)
    {
        Parent = parent;
        _tokenType = TokenType.LogValue;
        _value = new Dictionary<string, LogTokenNode>();
    }

    /// <summary>
    /// For None LogValue
    /// </summary>
    internal LogTokenNode(LogTokenNode? parent, TokenType tokenType, object? value)
    {
        if (tokenType is TokenType.End or TokenType.LogValue or TokenType.LogValueEndMembers)
            throw new ArgumentException($"Not supported TokenType: {tokenType}");

        Parent = parent;
        _tokenType = tokenType;
        _value = value;
    }

    internal readonly LogTokenNode? Parent;
    private readonly TokenType _tokenType;
    private readonly object? _value;

    private Dictionary<string, LogTokenNode> Children => (Dictionary<string, LogTokenNode>)_value!;

    public LogTokenNode this[string name]
    {
        get
        {
            if (_tokenType != TokenType.LogValue) //考虑返回EmptyTokenNode
                throw new NoneLogValueException("Can't get child for none LogValue");
            if (!Children!.TryGetValue(name, out var child))
                throw new Exception("Child not exists");
            return child;
        }
    }

    //因可能存在相同名称(值可能不同 eg: LogMessage=$"{DateTime.Now}, {DateTime.Now)")
    internal void TryAddChild(string name, LogTokenNode value)
    {
        if (_tokenType != TokenType.LogValue)
            throw new NoneLogValueException("Can't add child to none LogValue");
        Children!.TryAdd(name, value);
    }

    public bool IsNull => _tokenType == TokenType.Null || _value == null;
    public bool IsNotNull => _tokenType != TokenType.Null && _value != null;
    public object? Value => _value;

    public int ToInt() => _tokenType switch
    {
        TokenType.Byte => (byte)_value!,
        TokenType.Char => (char)_value!,
        TokenType.Short => (short)_value!,
        TokenType.UShort => (ushort)_value!,
        TokenType.Int => (int)_value!,
        TokenType.Long => (int)(long)_value!,
        TokenType.Float => (int)(float)_value!,
        TokenType.Double => (int)(double)_value!,
        TokenType.Decimal => (int)(decimal)_value!,
        _ => throw new InvalidCastException($"Can't cast {_tokenType} to Int32")
    };

    public DateTime ToDateTime() => _tokenType != TokenType.DateTime
        ? throw new InvalidCastException($"Can't cast {_tokenType} to DateTime")
        : (DateTime)_value!;

    public static implicit operator int(LogTokenNode node) => node.ToInt();
    public static implicit operator DateTime(LogTokenNode node) => node.ToDateTime();
}

public sealed class NoneLogValueException(string message) : Exception(message);