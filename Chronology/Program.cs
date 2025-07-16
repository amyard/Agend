// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");

(string doc1, string doc2) = Helper.PdfDocumentHelper.ExtractFullTextFromDocument("text/report.pdf");

// var doc3 = Helper.AsposePdfExtractor.ExtractContentByParagraph("text/report.pdf");
var doc4 = Helper.AsposePdfExtractor.ExtractPdfContentByParagraphDS_v1("text/report.pdf");
// var doc5 = Helper.AsposePdfExtractor.ExtractPdfContentByParagraph_V2("text/report.pdf");
// var doc6 = Helper.AsposePdfExtractor.ExtractPdfContentByParagraph_V3("text/report.pdf");

// var doc5 = Helper.AsposePdfExtractor.ExtractPdfContentByParagraphAdvanced("text/report.pdf");

var lol = 1;
