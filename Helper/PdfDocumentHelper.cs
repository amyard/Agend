using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace Helper;

public static class PdfDocumentHelper
{
    public static (string, string) ExtractFullTextFromDocument(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("PDF file not found.");
            return (string.Empty, string.Empty);
        }
        
        StringBuilder fullContentText = new();
        List<PageContentInfo> fullContentTextDataList = [];

        using PdfReader pdfReader = new(filePath);
        using PdfDocument pdfDocument = new(pdfReader);
        PdfAcroForm acroForm = PdfAcroForm.GetAcroForm(pdfDocument, false);
        IDictionary<string, PdfFormField>? formFields = acroForm?.GetAllFormFields();

        for (int page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
        {
            StringBuilder tempContent = new();
            PdfPage pdfPage = pdfDocument.GetPage(page);

            // Extract page text - LocationTextExtractionStrategy return same result
            string currentText = PdfTextExtractor.GetTextFromPage(pdfPage, new SimpleTextExtractionStrategy());

            // validate content. extra newLineCharacter after text extraction can cause problem in semantic search
            currentText = currentText.Trim().Replace("\r\n", " ").Replace("\n", " ").Replace(Environment.NewLine, " ");
            tempContent.AppendLine(currentText);

            // extract form data
            if (acroForm != null)
            {
                IList<PdfAnnotation> annotations = pdfPage.GetAnnotations();

                if (annotations.Any(annotation => annotation.GetSubtype().Equals(PdfName.Widget)))
                {
                    // Filter and process form fields on the current page
                    List<KeyValuePair<string, PdfFormField>> fieldsToProcess = formFields
                        .Where(field => field.Value.GetWidgets().FirstOrDefault()?.GetPage() == pdfPage)
                        .ToList();

                    foreach (KeyValuePair<string, PdfFormField> field in fieldsToProcess)
                    {
                        string fieldName = field.Key;
                        string fieldValue = field.Value.GetValueAsString();
                        tempContent.AppendLine($"Field: {fieldName}, Value: {fieldValue}");

                        // Remove processed fields after appending
                        formFields.Remove(field.Key);
                    }
                }
            }

            fullContentTextDataList.Add(new (page, tempContent.ToString()));
            fullContentText.AppendLine(tempContent.ToString());
        }

        // we need encoder to handle with different languages
        string content = JsonSerializer.Serialize(fullContentTextDataList, new JsonSerializerOptions
        {
            // Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            // Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            IncludeFields = true
        });

        return (fullContentText.ToString(), content);
    }
}

internal class PageContentInfo
{
    public PageContentInfo(int pageNumber, string content)
    {
        PageNumber = pageNumber;
        Content = content;
    }
    
    public int PageNumber { get; set; }
    public string Content { get; set; } = string.Empty;
}
