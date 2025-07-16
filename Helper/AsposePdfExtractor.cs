using System.Text;
using Aspose.Pdf;
using Aspose.Pdf.Text;
using Helper.Models;

namespace Helper;

public class AsposePdfExtractor
{
    public static List<PdfPageContent> ExtractContentByParagraph(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"PDF file not found: {filePath}");
        }

        var result = new List<PdfPageContent>();

        // Load the PDF document
        using (var document = new Document(filePath))
        {
            // Process each page
            for (int pageIndex = 1; pageIndex <= document.Pages.Count; pageIndex++)
            {
                var page = document.Pages[pageIndex];
                var pageContent = new PdfPageContent
                {
                    PageNumber = pageIndex
                };

                // Create TextAbsorber object to extract text
                var textAbsorber = new TextAbsorber();
                
                // Accept the absorber for the page
                page.Accept(textAbsorber);

                // Get the extracted text
                string pageText = textAbsorber.Text;
                
                // Split the text into paragraphs
                // This uses double newlines as paragraph separators, which is a common approach
                // but may need adjustment based on your specific PDF structure
                string[] paragraphs = pageText.Split(
                    new[] { "\r\n\r\n", "\n\n" },
                    StringSplitOptions.RemoveEmptyEntries
                );
                
                // Clean up each paragraph (remove excessive whitespace)
                foreach (var paragraph in paragraphs)
                {
                    string cleanedParagraph = CleanupParagraphText2(paragraph);
                    if (!string.IsNullOrWhiteSpace(cleanedParagraph))
                    {
                        pageContent.Content.Add(cleanedParagraph);
                    }
                }

                result.Add(pageContent);
            }
        }

        return result;
    }
    
    private static string CleanupParagraphText2(string text)
    {
        // Replace multiple spaces with a single space
        string cleaned = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
            
        // Trim whitespace from the beginning and end
        cleaned = cleaned.Trim();
            
        return cleaned;
    }
    
    /// <summary>
    /// Good solution . Extract all as it should be. Make full copy of page.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static List<PdfPageContent> ExtractPdfContentByParagraphDS_v1(string filePath)
    {
        var result = new List<PdfPageContent>();
    
        // Load the PDF document
        Document pdfDocument = new Document(filePath);
        
        // Process each page
        for (int i = 1; i <= pdfDocument.Pages.Count; i++)
        {
            List<string> pageContent = [];
            // var pageContent = new PdfPageContent { PageNumber = i, Content = []};
        
            // Get the page
            Page page = pdfDocument.Pages[i];
            
            // Create TextAbsorber to extract text
            TextAbsorber textAbsorber = new TextAbsorber();
        
            // Extract text from the page
            textAbsorber.Visit(page);
        
            // Split the text into paragraphs (assuming paragraphs are separated by double newlines)
            string extractedText = textAbsorber.Text;
            string[] paragraphs = extractedText.Split(new[] { "\n\r" }, StringSplitOptions.RemoveEmptyEntries);
            
            // it will extract full page
            // string[] paragraphs = extractedText.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        
            // Clean up each paragraph and add to the result
            foreach (string paragraph in paragraphs)
            {
                string cleanedParagraph = CleanupParagraphText(paragraph);
                if (!string.IsNullOrEmpty(cleanedParagraph))
                {
                    pageContent.Add(cleanedParagraph);
                }
            }
        
            result.Add(new PdfPageContent { PageNumber = i, Content = pageContent});
        }
    
        return result;
    }
    
    private static string CleanupParagraphText(string text)
    {
        // Replace newlines with spaces
        string cleaned = text.Replace("\r\n", " ").Replace("\n", " ");
    
        // Replace multiple spaces with a single space
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", " ");
    
        // Trim whitespace from the beginning and end
        cleaned = cleaned.Trim();
    
        return cleaned;
    }

    
    
    /// <summary>
    /// It stores each word as unique
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static List<PdfPageContent> ExtractPdfContentByParagraphAdvanced(string filePath)
    {
        var result = new List<PdfPageContent>();
    
        // Load the PDF document
        Document pdfDocument = new Document(filePath);
    
        // Process each page
        for (int i = 1; i <= pdfDocument.Pages.Count; i++)
        {
            var pageContent = new PdfPageContent { PageNumber = i };
            Page page = pdfDocument.Pages[i];
        
            // Use TextFragmentAbsorber to get more detailed text information
            TextFragmentAbsorber textFragmentAbsorber = new TextFragmentAbsorber();
            page.Accept(textFragmentAbsorber);
        
            // Collect text fragments
            TextFragmentCollection textFragmentCollection = textFragmentAbsorber.TextFragments;
        
            // Group fragments into paragraphs based on their positions
            List<TextFragment> currentParagraph = new List<TextFragment>();
            double lastBottom = 0;
        
            foreach (TextFragment textFragment in textFragmentCollection)
            {
                // If this fragment is significantly lower than the last one, it's probably a new paragraph
                if (textFragment.Position.YIndent < lastBottom - 10) // 10 is a threshold
                {
                    if (currentParagraph.Count > 0)
                    {
                        pageContent.Content.Add(string.Join(" ", currentParagraph.Select(tf => tf.Text)));
                        currentParagraph.Clear();
                    }
                }
            
                currentParagraph.Add(textFragment);
                lastBottom = textFragment.Position.YIndent + textFragment.Rectangle.Height;
            }
        
            // Add the last paragraph if any
            if (currentParagraph.Count > 0)
            {
                pageContent.Content.Add(string.Join(" ", currentParagraph.Select(tf => tf.Text)));
            }
        
            result.Add(pageContent);
        }
    
        return result;
    }
    
    /// <summary>
    /// Updated version of v1
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static List<PdfPageContent> ExtractPdfContentByParagraph_V2(string filePath)
    {
        var result = new List<PdfPageContent>();
        
        // Load the PDF document
        Document pdfDocument = new Document(filePath);
        
        // Process each page
        for (int i = 1; i <= pdfDocument.Pages.Count; i++)
        {
            var pageContent = new PdfPageContent { PageNumber = i };
            Page page = pdfDocument.Pages[i];
            
            // Use TextFragmentAbsorber for precise text extraction
            TextFragmentAbsorber absorber = new TextFragmentAbsorber();
            page.Accept(absorber);
            
            // Get all text fragments
            TextFragmentCollection textFragments = absorber.TextFragments;
            
            if (textFragments.Count == 0)
            {
                result.Add(pageContent);
                continue;
            }
            
            // Order fragments by their vertical position (top to bottom)
            var orderedFragments = textFragments
                .OrderBy(f => f.Position.YIndent)
                .ThenBy(f => f.Position.XIndent)
                .ToList();
            
            // Group fragments into paragraphs
            List<TextFragment> currentParagraphFragments = new List<TextFragment>();
            double lastFragmentBottom = orderedFragments.First().Position.YIndent;
            
            foreach (TextFragment fragment in orderedFragments)
            {
                // Calculate vertical distance from previous fragment
                double verticalGap = lastFragmentBottom - fragment.Position.YIndent;
                
                // If significant vertical gap, treat as new paragraph
                if (verticalGap > fragment.TextState.FontSize * 1.5) // 1.5x font size as threshold
                {
                    if (currentParagraphFragments.Any())
                    {
                        // Combine fragments into paragraph text
                        string paragraphText = CombineFragments(currentParagraphFragments);
                        pageContent.Content.Add(paragraphText);
                        currentParagraphFragments.Clear();
                    }
                }
                
                currentParagraphFragments.Add(fragment);
                lastFragmentBottom = fragment.Position.YIndent + fragment.Rectangle.Height;
            }
            
            // Add the last paragraph if any fragments remain
            if (currentParagraphFragments.Any())
            {
                string paragraphText = CombineFragments(currentParagraphFragments);
                pageContent.Content.Add(paragraphText);
            }
            
            result.Add(pageContent);
        }
        
        return result;
    }

    private static string CombineFragments(List<TextFragment> fragments)
    {
        // Order fragments left-to-right, top-to-bottom within the paragraph
        var ordered = fragments
            .OrderBy(f => f.Position.YIndent)
            .ThenBy(f => f.Position.XIndent)
            .ToList();
        
        // Combine with appropriate spacing
        string result = string.Empty;
        TextFragment previousFragment = null;
        
        foreach (var fragment in ordered)
        {
            if (previousFragment != null)
            {
                // Calculate horizontal gap between fragments
                double horizontalGap = fragment.Position.XIndent - 
                                     (previousFragment.Position.XIndent + previousFragment.Rectangle.Width);
                
                // If significant gap, add space (might be tab or column layout)
                if (horizontalGap > previousFragment.TextState.FontSize * 0.5)
                {
                    result += " ";
                }
            }
            
            result += fragment.Text;
            previousFragment = fragment;
        }
        
        return result.Trim();
    }
    
    /// <summary>
    /// V3
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static List<PdfPageContent> ExtractPdfContentByParagraph_V3(string filePath)
    {
        var result = new List<PdfPageContent>();
        
        // Load the PDF document
        Document pdfDocument = new Document(filePath);
        
        // Process each page
        for (int i = 1; i <= pdfDocument.Pages.Count; i++)
        {
            var pageContent = new PdfPageContent { PageNumber = i };
            Page page = pdfDocument.Pages[i];
            
            // Use TextFragmentAbsorber for precise text extraction
            TextFragmentAbsorber absorber = new TextFragmentAbsorber();
            page.Accept(absorber);
            
            // Get all text fragments
            TextFragmentCollection textFragments = absorber.TextFragments;
            
            if (textFragments.Count == 0)
            {
                result.Add(pageContent);
                continue;
            }
            
            // Order fragments by their position
            var orderedFragments = textFragments
                .OrderBy(f => f.Position.YIndent)
                .ThenBy(f => f.Position.XIndent)
                .ToList();
            
            // Process fragments into clean paragraphs
            StringBuilder currentParagraph = new StringBuilder();
            double lastFragmentBottom = orderedFragments.First().Position.YIndent;
            
            foreach (TextFragment fragment in orderedFragments)
            {
                // Calculate vertical distance from previous fragment
                double verticalGap = lastFragmentBottom - fragment.Position.YIndent;
                
                // If significant vertical gap, finalize current paragraph
                if (verticalGap > fragment.TextState.FontSize * 1.5 && currentParagraph.Length > 0)
                {
                    pageContent.Content.Add(CleanText(currentParagraph.ToString()));
                    currentParagraph.Clear();
                }
                
                // Handle horizontal spacing
                if (currentParagraph.Length > 0 && !ShouldAddSpace(currentParagraph, fragment.Text))
                {
                    currentParagraph.Append(" ");
                }
                
                currentParagraph.Append(fragment.Text);
                lastFragmentBottom = fragment.Position.YIndent + fragment.Rectangle.Height;
            }
            
            // Add the last paragraph if it exists
            if (currentParagraph.Length > 0)
            {
                pageContent.Content.Add(CleanText(currentParagraph.ToString()));
            }
            
            result.Add(pageContent);
        }
        
        return result;
    }

    private static string CleanText(string text)
    {
        // Replace all whitespace (including newlines) with single spaces
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
        
        // Trim leading/trailing whitespace
        return text.Trim();
    }

    private static bool ShouldAddSpace(StringBuilder currentParagraph, string nextText)
    {
        if (currentParagraph.Length == 0) return false;
        if (string.IsNullOrWhiteSpace(nextText)) return false;
        
        char lastChar = currentParagraph[currentParagraph.Length - 1];
        char firstChar = nextText[0];
        
        // Don't add space after certain punctuation
        if (lastChar == '-' || lastChar == '(') return false;
        
        // Don't add space before certain punctuation
        if (firstChar == '.' || firstChar == ',' || firstChar == '!' || 
            firstChar == '?' || firstChar == ')' || firstChar == ':') return false;
        
        return true;
    }
}
