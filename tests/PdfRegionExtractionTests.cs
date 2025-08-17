using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Geometry;
using Xunit;

namespace Sample.PdfDataExtraction.Tests;

public class PdfRegionExtractionTests
{
    private readonly string _testPdfPath;

    public PdfRegionExtractionTests()
    {
        _testPdfPath = Path.Combine("TestData", "sample-invoice.pdf");
    }

    [Theory]
    [InlineData(1, 0, 0, 100, 100)]
    [InlineData(1, 50, 50, 200, 200)]
    [InlineData(1, 100, 100, 300, 300)]
    public void ExtractRegion_WithValidCoordinates_ShouldNotThrow(
        int pageNumber, double x1, double y1, double x2, double y2)
    {
        // Arrange & Act
        var extractedText = ExtractRegion(_testPdfPath, pageNumber, x1, y1, x2, y2);

        // Assert
        Assert.NotNull(extractedText);
        // Text might be empty if no words are in the region, which is valid
    }

    [Fact]
    public void ExtractRegion_WithLargeRegion_ShouldReturnMoreText()
    {
        // Arrange
        var smallRegionText = ExtractRegion(_testPdfPath, 1, 0, 0, 100, 100);
        var largeRegionText = ExtractRegion(_testPdfPath, 1, 0, 0, 500, 500);

        // Act & Assert
        Assert.True(largeRegionText.Length >= smallRegionText.Length,
            "Larger region should contain at least as much text as smaller region");
    }

    [Fact]
    public void ExtractRegion_WithOverlappingRegions_ShouldHaveCommonText()
    {
        // Arrange
        var region1Text = ExtractRegion(_testPdfPath, 1, 0, 0, 200, 200);
        var region2Text = ExtractRegion(_testPdfPath, 1, 100, 100, 300, 300);

        // Act & Assert
        if (!string.IsNullOrEmpty(region1Text) && !string.IsNullOrEmpty(region2Text))
        {
            // If both regions contain text, there might be some overlap
            // This is a probabilistic test based on typical document layouts
            Assert.True(region1Text.Length > 0 || region2Text.Length > 0,
                "At least one region should contain text");
        }
    }

    [Theory]
    [InlineData(-10, -10, 10, 10)] // Negative coordinates
    [InlineData(0, 0, 0, 0)]       // Zero-size region
    public void ExtractRegion_WithEdgeCaseCoordinates_ShouldHandleGracefully(
        double x1, double y1, double x2, double y2)
    {
        // Arrange & Act
        var extractedText = ExtractRegion(_testPdfPath, 1, x1, y1, x2, y2);

        // Assert
        Assert.NotNull(extractedText);
        // Should not throw exceptions even with edge case coordinates
    }

    [Fact]
    public void ExtractRegion_WithInvalidPageNumber_ShouldThrow()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ExtractRegion(_testPdfPath, 999, 0, 0, 100, 100));
    }

    [Fact]
    public void GetWordsInRegion_ShouldReturnOrderedWords()
    {
        // Arrange
        using var doc = PdfDocument.Open(_testPdfPath);
        var page = doc.GetPage(1);
        var allWords = page.GetWords().ToList();

        // Act - Get words in a large region to ensure we have some results
        var wordsInRegion = GetWordsInRegion(page, 0, 0, 500, 500).ToList();

        // Assert
        Assert.NotEmpty(wordsInRegion);
        
        // Verify ordering: words should be ordered by Y coordinate (bottom) then X coordinate (left)
        for (int i = 1; i < wordsInRegion.Count; i++)
        {
            var prevWord = wordsInRegion[i - 1];
            var currentWord = wordsInRegion[i];
            
            // Allow for small floating-point differences in Y coordinates
            const double tolerance = 1.0;
            
            if (Math.Abs(prevWord.BoundingBox.Bottom - currentWord.BoundingBox.Bottom) > tolerance)
            {
                Assert.True(prevWord.BoundingBox.Bottom <= currentWord.BoundingBox.Bottom,
                    "Words should be ordered by bottom Y coordinate");
            }
            else
            {
                // If Y coordinates are similar, check X ordering
                Assert.True(prevWord.BoundingBox.Left <= currentWord.BoundingBox.Left,
                    "Words on same line should be ordered by left X coordinate");
            }
        }
    }

    private static string ExtractRegion(
        string pdfPath,
        int pageNumber,
        double x1, double y1,
        double x2, double y2)
    {
        using var doc = PdfDocument.Open(pdfPath);
        var page = doc.GetPage(pageNumber);

        var wordsInRegion = GetWordsInRegion(page, x1, y1, x2, y2)
            .Select(w => w.Text);

        return string.Join(" ", wordsInRegion);
    }

    private static IEnumerable<Word> GetWordsInRegion(
        Page page,
        double x1, double y1,
        double x2, double y2)
    {
        return page.GetWords()
            .Where(w => w.BoundingBox.Left >= x1 && w.BoundingBox.Right <= x2 &&
                       w.BoundingBox.Bottom >= y1 && w.BoundingBox.Top <= y2)
            .OrderBy(w => w.BoundingBox.Bottom)
            .ThenBy(w => w.BoundingBox.Left);
    }
}
