using System;
using System.Text;

namespace EntityChange.Generators;

internal sealed class SourceBuilder
{
    private readonly StringBuilder _sb = new();
    private int _indent;

    public void AppendLine(string line = "")
    {
        if (string.IsNullOrEmpty(line))
        {
            _sb.AppendLine();
            return;
        }

        _sb.Append(' ', _indent * 4);
        _sb.AppendLine(line);
    }

    public void Append(string text)
    {
        _sb.Append(' ', _indent * 4);
        _sb.Append(text);
    }

    public void OpenBrace()
    {
        AppendLine("{");
        _indent++;
    }

    public void CloseBrace(string suffix = "")
    {
        _indent--;
        AppendLine("}" + suffix);
    }

    public IDisposable Block(string header = "")
    {
        if (!string.IsNullOrEmpty(header))
            AppendLine(header);
        OpenBrace();
        return new BlockScope(this);
    }

    public void Indent() => _indent++;
    public void Unindent() => _indent--;

    public override string ToString() => _sb.ToString();

    private sealed class BlockScope : IDisposable
    {
        private readonly SourceBuilder _builder;
        public BlockScope(SourceBuilder builder) => _builder = builder;
        public void Dispose() => _builder.CloseBrace();
    }
}
