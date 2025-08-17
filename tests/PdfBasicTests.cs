using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Geometry;
using Xunit;

namespace Sample.PdfDataExtraction.Tests;

public class PdfBasicTests
{
    private readonly string _testPdfPath;

    public PdfBasicTests()
    {
        _testPdfPath = Path.Combine("TestData", "sample-invoice.pdf");
    }

    [Fact]
    public void OpenPdf_ShouldLoadSuccessfully()
    {
        // Arrange & Act
        using var pdf = PdfDocument.Open(_testPdfPath);

        // Assert
        Assert.NotNull(pdf);
        Assert.True(pdf.NumberOfPages > 0);
    }

    [Fact]
    public void GetFirstPage_ShouldReturnValidPage()
    {
        // Arrange & Act
        using var pdf = PdfDocument.Open(_testPdfPath);
        var page = pdf.GetPage(1);

        // Assert
        Assert.NotNull(page);
        Assert.True(page.Number > 0);
    }

    [Fact]
    public void ExtractPageText_ShouldReturnNonEmptyString()
    {
        // Arrange & Act
        using var pdf = PdfDocument.Open(_testPdfPath);
        var page = pdf.GetPage(1);
        var text = page.Text;

        // Assert
        Assert.NotNull(text);
        Assert.NotEmpty(text);
    }

    [Fact]
    public void GetWords_ShouldReturnWords()
    {
        // Arrange & Act
        using var pdf = PdfDocument.Open(_testPdfPath);
        var page = pdf.GetPage(1);
        var words = page.GetWords().ToList();

        // Assert
        Assert.NotEmpty(words);
        Assert.All(words, word => Assert.NotEmpty(word.Text));
    }
}
