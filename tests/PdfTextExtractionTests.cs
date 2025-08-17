using UglyToad.PdfPig;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.Geometry;
using Xunit;

namespace Sample.PdfDataExtraction.Tests;

public class PdfTextExtractionTests
{
    private readonly string _testPdfPath;

    public PdfTextExtractionTests()
    {
        _testPdfPath = Path.Combine("TestData", "sample-invoice.pdf");
    }

    [Fact]
    public void ExtractAllText_ShouldReturnNonEmptyString()
    {
        // Arrange & Act
        var extractedText = ExtractAllText(_testPdfPath);

        // Assert
        Assert.NotNull(extractedText);
        Assert.NotEmpty(extractedText);
        Assert.True(extractedText.Length > 100, "Extracted text should be substantial");
    }

    [Fact]
    public void ExtractAllText_ShouldContainExpectedInvoiceContent()
    {
        // Arrange & Act
        var extractedText = ExtractAllText(_testPdfPath);

        // Assert
        Assert.Contains("Invoice", extractedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Total", extractedText, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(1)]
    public void GetPageText_ShouldReturnTextForValidPage(int pageNumber)
    {
        // Arrange & Act
        using var pdf = PdfDocument.Open(_testPdfPath);
        var page = pdf.GetPage(pageNumber);
        var pageText = page.Text;

        // Assert
        Assert.NotNull(pageText);
        Assert.NotEmpty(pageText);
    }

    [Fact]
    public void GetPageCount_ShouldReturnExpectedNumberOfPages()
    {
        // Arrange & Act
        using var pdf = PdfDocument.Open(_testPdfPath);
        var pageCount = pdf.NumberOfPages;

        // Assert
        Assert.True(pageCount > 0, "PDF should have at least one page");
    }

    [Fact]
    public void GetWords_ShouldReturnWordsWithBoundingBoxes()
    {
        // Arrange & Act
        using var pdf = PdfDocument.Open(_testPdfPath);
        var page = pdf.GetPage(1);
        var words = page.GetWords();

        // Assert
        Assert.NotEmpty(words);
        Assert.All(words, word =>
        {
            Assert.NotNull(word.Text);
            Assert.NotEmpty(word.Text.Trim());
            Assert.True(word.BoundingBox.Width > 0, "Word should have positive width");
            Assert.True(word.BoundingBox.Height > 0, "Word should have positive height");
        });
    }

    private static string ExtractAllText(string pdfPath)
    {
        string content = "";
        using var pdf = PdfDocument.Open(pdfPath);
        foreach (var page in pdf.GetPages())
        {
            content += page.Text;
        }
        return content;
    }
}
