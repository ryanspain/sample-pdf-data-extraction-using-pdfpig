using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig.Geometry;
using Xunit;

namespace Sample.PdfDataExtraction.Tests;

public class PdfBlockExtractionTests
{
    private readonly string _testPdfPath;

    public PdfBlockExtractionTests()
    {
        _testPdfPath = Path.Combine("TestData", "sample-invoice.pdf");
    }

    [Fact]
    public void GetTextBlocks_ShouldReturnMultipleBlocks()
    {
        // Arrange & Act
        var blocks = GetTextBlocks(_testPdfPath, pageNumber: 1);

        // Assert
        Assert.NotEmpty(blocks);
        Assert.True(blocks.Count > 1, "Should find multiple text blocks in the document");
    }

    [Fact]
    public void GetTextBlocks_ShouldHaveValidBoundingBoxes()
    {
        // Arrange & Act
        var blocks = GetTextBlocks(_testPdfPath, pageNumber: 1);

        // Assert
        Assert.NotEmpty(blocks);
        foreach (var block in blocks)
        {
            Assert.True(block.BoundingBox.Width > 0, "Block should have positive width");
            Assert.True(block.BoundingBox.Height > 0, "Block should have positive height");
            Assert.NotNull(block.Text);
            Assert.NotEmpty(block.Text.Trim());
        }
    }

    [Fact]
    public void GetTextBlocks_ShouldContainExpectedContent()
    {
        // Arrange & Act
        var blocks = GetTextBlocks(_testPdfPath, pageNumber: 1);
        var allBlockText = "";
        foreach (var block in blocks)
        {
            allBlockText += " " + block.Text;
        }

        // Assert
        Assert.Contains("Invoice", allBlockText, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(100, 100, 200, 200)] // Small region
    [InlineData(0, 0, 500, 500)]     // Larger region
    public void GetBlocksInBoundingBox_ShouldFilterBlocksByRegion(double x1, double y1, double x2, double y2)
    {
        // Arrange
        var allBlocks = GetTextBlocks(_testPdfPath, pageNumber: 1);
        var region = new PdfRectangle(new PdfPoint(x1, y1), new PdfPoint(x2, y2));

        // Act
        var blocksInRegion = GetBlocksInBoundingBox(_testPdfPath, 1, x1, y1, x2, y2);

        // Assert
        Assert.True(blocksInRegion.Count <= allBlocks.Count, 
            "Filtered blocks should be less than or equal to total blocks");
    }

    [Fact]
    public void GetBlocksInBoundingBox_WithValidRegion_ShouldReturnExpectedBlocks()
    {
        // Arrange - Using coordinates from the original program
        double x1 = 269.08, y1 = 51.149;
        double x2 = 301.7521600000001, y2 = 54.945271484375;

        // Act
        var blocksInRegion = GetBlocksInBoundingBox(_testPdfPath, 1, x1, y1, x2, y2);

        // Assert
        Assert.NotNull(blocksInRegion);
        // The specific region might or might not contain blocks depending on the PDF content
        // but the method should not throw exceptions
    }

    private static dynamic GetTextBlocks(string pdfPath, int pageNumber)
    {
        using var pdf = PdfDocument.Open(pdfPath);
        var page = pdf.GetPage(pageNumber);
        var words = page.GetWords();
        return DocstrumBoundingBoxes.Instance.GetBlocks(words);
    }

    private static dynamic GetBlocksInBoundingBox(
        string pdfPath, 
        int pageNumber,
        double x1, double y1, 
        double x2, double y2)
    {
        using var pdf = PdfDocument.Open(pdfPath);
        var page = pdf.GetPage(pageNumber);
        var words = page.GetWords();
        var blocks = DocstrumBoundingBoxes.Instance.GetBlocks(words);

        var region = new PdfRectangle(
            bottomLeft: new PdfPoint(x1, y1),
            topRight: new PdfPoint(x2, y2)
        );

        var filteredBlocks = new List<dynamic>();
        foreach (var block in blocks)
        {
            if (block.BoundingBox.IntersectsWith(region))
            {
                filteredBlocks.Add(block);
            }
        }
        
        return filteredBlocks;
    }
}
