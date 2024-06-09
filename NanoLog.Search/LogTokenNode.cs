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

    public byte ToByte() => _tokenType == TokenType.Byte
        ? (byte)_value!
        : throw new InvalidCastException($"Can't cast {_tokenType} to DateTime");

    public char ToChar() => _tokenType == TokenType.Char
        ? (char)_value!
        : throw new InvalidCastException($"Can't cast {_tokenType} to DateTime");

    public int ToInt() => _tokenType switch
    {
        TokenType.Byte => (byte)_value!,
        TokenType.Char => (char)_value!,
        TokenType.Short => (short)_value!,
        TokenType.UShort => (ushort)_value!,
        TokenType.Int => (int)_value!,
        TokenType.UInt => (int)(uint)_value!,
        TokenType.Long => (int)(long)_value!,
        TokenType.ULong => (int)(ulong)_value!,
        TokenType.Float => (int)(float)_value!,
        TokenType.Double => (int)(double)_value!,
        TokenType.Decimal => (int)(decimal)_value!,
        _ => throw new InvalidCastException($"Can't cast {_tokenType} to Int32")
    };

    public double ToDouble() => _tokenType switch
    {
        TokenType.Byte => (byte)_value!,
        TokenType.Char => (char)_value!,
        TokenType.Short => (short)_value!,
        TokenType.UShort => (ushort)_value!,
        TokenType.Int => (int)_value!,
        TokenType.UInt => (uint)_value!,
        TokenType.Long => (long)_value!,
        TokenType.ULong => (ulong)_value!,
        TokenType.Float => (float)_value!,
        TokenType.Double => (double)_value!,
        TokenType.Decimal => (double)(decimal)_value!,
        _ => throw new InvalidCastException($"Can't cast {_tokenType} to Double")
    };

    public DateTime ToDateTime() => _tokenType == TokenType.DateTime
        ? (DateTime)_value!
        : throw new InvalidCastException($"Can't cast {_tokenType} to DateTime");

    public Guid ToGuid() => _tokenType == TokenType.Guid
        ? (Guid)_value!
        : throw new InvalidCastException($"Can't cast {_tokenType} to Guid");
    
    public string ToStringValue() => _tokenType is TokenType.String1 or TokenType.String2 or TokenType.String4
        ? (string)_value!
        : throw new InvalidCastException($"Can't cast {_tokenType} to String");

    #region ====隐式转换(仅用于简化表达式字符串)====

    public static implicit operator byte(LogTokenNode node) => node.ToByte();
    public static implicit operator char(LogTokenNode node) => node.ToChar();
    public static implicit operator int(LogTokenNode node) => node.ToInt();
    public static implicit operator double(LogTokenNode node) => node.ToDouble();
    public static implicit operator DateTime(LogTokenNode node) => node.ToDateTime();
    public static implicit operator Guid(LogTokenNode node) => node.ToGuid();

    #endregion
}

public sealed class NoneLogValueException(string message) : Exception(message);