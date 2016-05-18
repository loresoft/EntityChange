using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityChange.Extenstions;

namespace EntityChange
{
    /// <summary>
    /// Format changes in Markdown format.
    /// </summary>
    public class MarkdownFormatter : IChangeFormatter
    {
        /// <summary>
        /// Gets or sets the header template.
        /// </summary>
        /// <value>
        /// The header template.
        /// </value>
        public string HeaderTemplate { get; set; } = Environment.NewLine;

        /// <summary>
        /// Gets or sets the footer template.
        /// </summary>
        /// <value>
        /// The footer template.
        /// </value>
        public string FooterTemplate { get; set; } = Environment.NewLine;

        /// <summary>
        /// Gets or sets the operation add template.
        /// </summary>
        /// <value>
        /// The operation add template.
        /// </value>
        public string OperationAddTemplate { get; set; } = "* Added `{CurrentFormatted}` to `{DisplayName}`";

        /// <summary>
        /// Gets or sets the operation remove template.
        /// </summary>
        /// <value>
        /// The operation remove template.
        /// </value>
        public string OperationRemoveTemplate { get; set; } = "* Removed `{OriginalFormatted}` from `{DisplayName}`";

        /// <summary>
        /// Gets or sets the operation replace template.
        /// </summary>
        /// <value>
        /// The operation replace template.
        /// </value>
        public string OperationReplaceTemplate { get; set; } = "* Changed `{DisplayName}` from `{OriginalFormatted}` to `{CurrentFormatted}`";



        /// <summary>
        /// Create a readable change report.
        /// </summary>
        /// <param name="changes">The changes to format.</param>
        /// <returns>
        /// A string representing the <paramref name="changes" />.
        /// </returns>
        public string Format(IReadOnlyCollection<ChangeRecord> changes)
        {
            var builder = new StringBuilder();
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

                var line = NameFormatter.Format(template, change);
                builder.AppendLine(line);
            }

            builder.AppendLine(FooterTemplate ?? string.Empty);
            return builder.ToString();
        }
    }
}
