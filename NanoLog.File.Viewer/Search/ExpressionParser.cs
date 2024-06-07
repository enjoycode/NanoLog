using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Action = System.Action;

namespace NanoLog.File.Viewer;

public sealed class ExpressionParser : CSharpSyntaxVisitor<Expression?>
{
    static ExpressionParser()
    {
        var sdkPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        CoreLib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        MsCoreLib = MetadataReference.CreateFromFile(Path.Combine(sdkPath, "mscorlib.dll"));
        SystemLib = MetadataReference.CreateFromFile(Path.Combine(sdkPath, "System.dll"));
        SystemCoreLib = MetadataReference.CreateFromFile(Path.Combine(sdkPath, "System.Core.dll"));
        SystemRuntimeLib = MetadataReference.CreateFromFile(Path.Combine(sdkPath, "System.Runtime.dll"));
        NanoLogLib = MetadataReference.CreateFromFile(typeof(LogLevel).Assembly.Location);
        NanoLogFileLib = MetadataReference.CreateFromFile(typeof(LogDataParameter).Assembly.Location);
        ExpressionParameter = Expression.Parameter(typeof(LogDataParameter), "e");
    }

    private ExpressionParser(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
    }

    private readonly SemanticModel _semanticModel;

    private static readonly MetadataReference CoreLib;
    private static readonly MetadataReference MsCoreLib;
    private static readonly MetadataReference SystemLib;
    private static readonly MetadataReference SystemCoreLib;
    private static readonly MetadataReference SystemRuntimeLib;
    private static readonly MetadataReference NanoLogLib;
    private static readonly MetadataReference NanoLogFileLib;

    private static readonly ParameterExpression ExpressionParameter;

    private static readonly Dictionary<string, Type> KnownTypes = new()
    {
        { "bool", typeof(bool) },
        { "byte", typeof(byte) },
        { "sbyte", typeof(sbyte) },
        { "short", typeof(short) },
        { "ushort", typeof(ushort) },
        { "int", typeof(int) },
        { "uint", typeof(uint) },
        { "long", typeof(long) },
        { "ulong", typeof(ulong) },
        { "float", typeof(float) },
        { "double", typeof(double) },
        { "char", typeof(char) },
        { "string", typeof(string) },
        { "object", typeof(object) },
        { "NanoLog.LogLevel", typeof(LogLevel) },
    };

    public static Func<LogDataParameter, bool> ParseAndCompile(string expression)
    {
        var paraTypeName = typeof(LogDataParameter).FullName;
        var code =
            $"using System;using NanoLog;static class E{{static bool M({paraTypeName} e){{return {expression};}}}}";
        var body = ParseCode(code);
        if (body.Type != typeof(bool))
            body = Expression.Convert(body, typeof(bool));
        var lambda = Expression.Lambda<Func<LogDataParameter, bool>>(body, ExpressionParameter);
        return lambda.Compile();
    }

    /// <summary>
    /// 解析表达式字符串转换为Linq的表达式
    /// </summary>
    private static Expression ParseCode(string code)
    {
        var parseOptions = new CSharpParseOptions().WithLanguageVersion(LanguageVersion.CSharp11);
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithNullableContextOptions(NullableContextOptions.Enable);

        var tree = CSharpSyntaxTree.ParseText(code, parseOptions);
        var root = tree.GetCompilationUnitRoot();
        var compilation = CSharpCompilation.Create("Expression", options: compilationOptions)
            .AddReferences(CoreLib)
            .AddReferences(MsCoreLib)
            .AddReferences(SystemLib)
            .AddReferences(SystemCoreLib)
            .AddReferences(SystemRuntimeLib)
            .AddReferences(NanoLogLib)
            .AddReferences(NanoLogFileLib)
            .AddSyntaxTrees(tree);
        var semanticModel = compilation.GetSemanticModel(tree);
        //检查是否存在语义错误
        var diagnostics = semanticModel.GetDiagnostics();
        var errors = diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error);
        if (errors > 0)
            throw new Exception("表达式存在语义错误");

        var methodDecl = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        if (methodDecl.Body is { Statements.Count: > 1 })
            throw new NotImplementedException("Parse block body");

        if (methodDecl.ExpressionBody != null)
            throw new NotImplementedException("Parse expression body");

        var firstStatement = methodDecl.Body!.Statements.FirstOrDefault();
        if (firstStatement is not ReturnStatementSyntax returnNode)
            throw new Exception("表达式方法不是单行返回语句");

        var parser = new ExpressionParser(semanticModel);
        return parser.Visit(returnNode.Expression)!;
    }

    /// <summary>
    /// 根据类型字符串获取运行时类型
    /// </summary>
    private static Type ResolveType(string typeName)
    {
        if (KnownTypes.TryGetValue(typeName, out var sysType))
            return sysType;

        //通过反射获取类型
        var type = Type.GetType(typeName);
        if (type == null)
            throw new Exception($"Can't find type: {typeName} ");

        return type;
    }

    private static Type ResolveType(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.IsGenericType)
            throw new NotImplementedException("范型类型暂未实现");

        return ResolveType(typeSymbol.ToString()!);
    }

    private Type? GetConvertedType(SyntaxNode node)
    {
        var typeInfo = _semanticModel.GetTypeInfo(node);
        Type? convertedType = null;
        if (!SymbolEqualityComparer.Default.Equals(typeInfo.Type, typeInfo.ConvertedType))
            convertedType = ResolveType((INamedTypeSymbol)typeInfo.ConvertedType!);

        return convertedType;
    }

    public override Expression? VisitIdentifierName(IdentifierNameSyntax node)
    {
        var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
        if (symbol is IParameterSymbol { Name: "e" })
        {
            return ExpressionParameter;
        }

        return base.VisitIdentifierName(node);
    }

    public override Expression VisitLiteralExpression(LiteralExpressionSyntax node)
    {
        var convertedType = GetConvertedType(node);
        var res = Expression.Constant(node.Token.Value);
        return convertedType == null ? res : Expression.Convert(res, convertedType);
    }

    public override Expression VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
    {
        //特殊处理 eg: -1 转换为ConstantExpression
        if (node.Operand is LiteralExpressionSyntax literal && node.OperatorToken.IsKind(SyntaxKind.MinusToken))
        {
            var value = literal.Token.Value;
            // ReSharper disable once HeapView.BoxingAllocation
            value = value switch
            {
                sbyte b => -b,
                short s => -s,
                int i => -i,
                long l => -l,
                float f => -f,
                double d => -d,
                decimal dd => -dd,
                _ => throw new NotImplementedException()
            };

            var convertedType = GetConvertedType(node);
            var res = Expression.Constant(value);
            return convertedType == null ? res : Expression.Convert(res, convertedType);
        }

        throw new NotImplementedException();
    }

    public override Expression VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
        var owner = node.Expression.Accept(this);
        var convertedType = GetConvertedType(node);
        var isField = symbol is IFieldSymbol;
        var memberName = node.Name.Identifier.Text;
        var type = owner == null ? ResolveType(symbol!.ContainingType) : owner.Type;
        MemberInfo? memberInfo = isField ? type.GetField(memberName) : type.GetProperty(memberName);
        var res = Expression.MakeMemberAccess(owner, memberInfo!);
        return convertedType == null ? res : Expression.Convert(res, convertedType);
    }

    public override Expression VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        var left = node.Left.Accept(this)!;
        var right = node.Right.Accept(this)!;
        var op = GetBinaryOperator(node.OperatorToken);
        var convertedType = GetConvertedType(node);

        //Enum无法进行>=, > , <, <=比较
        if (op is ExpressionType.GreaterThan or ExpressionType.GreaterThanOrEqual
            or ExpressionType.LessThan or ExpressionType.LessThanOrEqual)
        {
            if (left.Type.IsEnum)
                left = Expression.Convert(left, typeof(int));
            if (right.Type.IsEnum)
                right = Expression.Convert(right, typeof(int));
        }

        var res = Expression.MakeBinary(op, left, right);
        return convertedType == null ? res : Expression.Convert(res, convertedType);
    }

    private static ExpressionType GetBinaryOperator(SyntaxToken token) => token.Kind() switch
    {
        SyntaxKind.PlusToken => ExpressionType.Add,
        SyntaxKind.MinusToken => ExpressionType.Subtract,
        SyntaxKind.AsteriskToken => ExpressionType.Multiply,
        SyntaxKind.SlashToken => ExpressionType.Divide,
        SyntaxKind.EqualsEqualsToken => ExpressionType.Equal,
        SyntaxKind.ExclamationEqualsToken => ExpressionType.NotEqual,
        SyntaxKind.GreaterThanToken => ExpressionType.GreaterThan,
        SyntaxKind.GreaterThanEqualsToken => ExpressionType.GreaterThanOrEqual,
        SyntaxKind.LessThanToken => ExpressionType.LessThan,
        SyntaxKind.LessThanEqualsToken => ExpressionType.LessThanOrEqual,
        _ => throw new NotImplementedException($"Binary Operator: {token}")
    };

    public override Expression VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        var convertedType = GetConvertedType(node);
        Expression[]? args = null;
        if (node.ArgumentList.Arguments.Count > 0)
        {
            args = new Expression[node.ArgumentList.Arguments.Count];
            for (var i = 0; i < args.Length; i++)
            {
                //TODO: check not supported ref and out
                args[i] = node.ArgumentList.Arguments[i].Expression.Accept(this)!;
            }
        }

        // eg: aa.Method(bb,cc)
        if (node.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var methodName = memberAccess.Name.Identifier.Text;
            var exp = memberAccess.Expression.Accept(this);
            MethodCallExpression res;
            if (exp == null)
            {
                var symbol = _semanticModel.GetSymbolInfo(memberAccess).Symbol;
                var type = ResolveType(symbol!.ContainingType);
                res = Expression.Call(type, methodName, null, args);
            }
            else
            {
                res = Expression.Call(exp, methodName, null, args);
            }

            return convertedType == null ? res : Expression.Convert(res, convertedType);
        }

        // eg: Equals(aa, bb)
        if (node.Expression is IdentifierNameSyntax identifierName)
        {
            var symbol = _semanticModel.GetSymbolInfo(identifierName).Symbol;
            var type = ResolveType(symbol!.ContainingType);
            var methodName = identifierName.Identifier.Text;
            var res = Expression.Call(type, methodName, null, args);
            return convertedType == null ? res : Expression.Convert(res, convertedType);
        }

        throw new NotImplementedException();
    }

    public override Expression VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        Expression[]? args = null;
        if (node.ArgumentList is { Arguments.Count: > 0 })
        {
            args = new Expression[node.ArgumentList.Arguments.Count];
            for (var i = 0; i < node.ArgumentList.Arguments.Count; i++)
            {
                args[i] = node.ArgumentList.Arguments[i].Expression.Accept(this)!;
            }
        }

        var ctorMethodSymbol = _semanticModel.GetSymbolInfo(node).Symbol;
        var typeSymbol = ctorMethodSymbol!.ContainingType!;
        var objectType = ResolveType(typeSymbol);
        var ctorInfo = objectType.GetConstructor(args == null
            ? []
            : args.Select(a => a.Type).ToArray());
        var convertedType = GetConvertedType(node);
        var res = Expression.New(ctorInfo!, args);
        return convertedType == null ? res : Expression.Convert(res, convertedType);
    }
}