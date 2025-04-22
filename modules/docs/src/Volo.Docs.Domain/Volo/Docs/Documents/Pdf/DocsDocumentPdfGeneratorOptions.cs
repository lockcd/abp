using System;
using Volo.Docs.Projects;

namespace Volo.Docs.Documents.Pdf;

public class DocsDocumentPdfGeneratorOptions
{
    public const string StylePlaceholder = "{{style-placeholder}}";
    public const string ContentPlaceholder = "{{content-placeholder}}";

    /// <summary>
    /// The HTML layout for the PDF document.
    /// </summary>
    public string HtmlLayout { get; set; }
    public string HtmlStyles { get; set; }
    public string BaseUrl { get; set; }
    public string CoverPagePath { get; set; }
    public TimeSpan PdfFileCacheExpiration { get; set; } = TimeSpan.FromDays(1);
    public Func<Project, string, string, string> CalculatePdfFileName { get; set; } = (project, version, languageCode) =>
    {
        return $"{project.ShortName}_{version}_{languageCode}.pdf";
    };
    
    public DocsDocumentPdfGeneratorOptions()
    {
        HtmlLayout = $@"
        <!DOCTYPE html>
            <head>
                <meta charset='utf-8' />
                <style>
                    {StylePlaceholder}
                </style>
            </head>
            <body>
                 {ContentPlaceholder}
            </body>
        </html>";

        HtmlStyles = @"
        body { margin: 20px; line-height: 1.6; font-family: Arial, sans-serif;}
        a { text-decoration: none; }
        .page {
            page-break-after: always;
            margin-bottom: 30px;
            padding: 15px;
        }
        img {
            max-width:100%;
            height: auto;
            display: block;
        }
        code, pre {
            background: #f8fafc;
            white-space: pre-wrap;
            word-wrap: break-word;
        }
        pre code { padding: 0; border: none; }
        code {
            padding: 2px 6px;
            border-radius: 4px;
            border: 1px solid #e2e8f0;
            font-family: Consolas, monospace;
        }
        pre {
            position: relative;
            padding: 16px;
            border-radius: 8px;
            border: 1px solid #e2e8f0;
        }
        blockquote {
            border-left: 4px solid #e2e8f0;
            margin: 20px 0;
            padding: 10px 20px;
            background: #f8fafc;
        }";
    }
}