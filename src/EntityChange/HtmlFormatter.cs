using System.Text;

namespace EntityChange;

/// <summary>
/// Format changes in HTML format.
/// </summary>
public class HtmlFormatter : IChangeFormatter
{
    /// <summary>
    /// Gets or sets the header template.
    /// </summary>
    /// <value>
    /// The header template.
    /// </value>
    public string HeaderTemplate { get; set; } = "<ul>";

    /// <summary>
    /// Gets or sets the footer template.
    /// </summary>
    /// <value>
    /// The footer template.
    /// </value>
    public string FooterTemplate { get; set; } = "</ul>";

    /// <summary>
    /// Gets or sets the operation add template.
    /// </summary>
    /// <value>
    /// The operation add template.
    /// </value>
    public string OperationAddTemplate { get; set; } = "<li>Added <span>{CurrentFormatted}</span> to <span>{DisplayName}</span></li>";

    /// <summary>
    /// Gets or sets the operation remove template.
    /// </summary>
    /// <value>
    /// The operation remove template.
    /// </value>
    public string OperationRemoveTemplate { get; set; } = "<li>Removed <span>{OriginalFormatted}</span> from <span>{DisplayName}</span></li>";

    /// <summary>
    /// Gets or sets the operation replace template.
    /// </summary>
    /// <value>
    /// The operation replace template.
    /// </value>
    public string OperationReplaceTemplate { get; set; } = "<li>Changed <span>{DisplayName}</span> from <span>{OriginalFormatted}</span> to <span>{CurrentFormatted}</span></li>";



    /// <summary>
    /// Create a readable change report.
    /// </summary>
    /// <param name="changes">The changes to format.</param>
    /// <returns>
    /// A string representing the <paramref name="changes" />.
    /// </returns>
    public string Format(IReadOnlyList<ChangeRecord> changes)
    {
        if (changes is null)
            throw new ArgumentNullException(nameof(changes));

        var builder = StringBuilderCache.Acquire();
        builder.AppendLine(HeaderTemplate ?? string.Empty);

        foreach (var change in changes)
        {
            string template;
            if (change.Operation == ChangeOperation.Add)
                template = OperationAddTemplate;
            else if (change.Operation == ChangeOperation.Remove)
                template = OperationRemoveTemplate;
            else
                template = OperationReplaceTemplate;

            var line = NameFormatter.FormatName(template, change);
            builder.AppendLine(line);
        }

        builder.AppendLine(FooterTemplate ?? string.Empty);

        return StringBuilderCache.ToString(builder);
    }
}
