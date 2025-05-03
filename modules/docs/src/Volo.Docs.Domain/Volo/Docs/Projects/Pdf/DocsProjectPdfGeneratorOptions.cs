using System;
using Scriban.Syntax;

namespace Volo.Docs.Projects.Pdf;

public class DocsProjectPdfGeneratorOptions
{
    public const string StylePlaceholder = "{{style-placeholder}}";
    public const string ContentPlaceholder = "{{content-placeholder}}";

    /// <summary>
    /// The HTML layout for the PDF document.
    /// </summary>
    public string HtmlLayout { get; set; }

    /// <summary>
    /// The HTML style for the PDF document.
    /// </summary>
    public string HtmlStyle { get; set; }
    
    /// <summary>
    /// The base URL for the PDF document.
    /// Used for resolving relative links and images.
    /// </summary>
    public string BaseUrl { get; set; }
    
    /// <summary>
    /// The path to the index page of the documentation.
    /// </summary>
    public string IndexPagePath { get; set; }
    
    /// <summary>
    /// PDF file cache expiration time.
    /// Default value is 24 hours.
    /// </summary>
    public TimeSpan PdfFileCacheExpiration { get; set; } = TimeSpan.FromHours(24);
    
    /// <summary>
    /// The function to calculate the PDF file name.
    /// Default is "{project.ShortName}-{version}-{languageCode}.pdf".
    /// </summary>
    public Func<Project, string, string, string> CalculatePdfFileName { get; set; }
    
    /// <summary>
    /// The function to calculate the PDF file title.
    /// </summary>
    public Func<Project, string> CalculatePdfFileTitle { get; set; }
    
    public DocsProjectPdfGeneratorOptions()
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

        HtmlStyle = @"
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
        
        CalculatePdfFileName = (project, version, languageCode) => $"{project.ShortName}-{version}-{languageCode}.pdf";
    }
}