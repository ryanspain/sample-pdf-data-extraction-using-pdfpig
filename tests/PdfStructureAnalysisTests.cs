using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Geometry;
using Xunit;

namespace Sample.PdfDataExtraction.Tests;

public class PdfStructureAnalysisTests
{
    private readonly string _testPdfPath;

    public PdfStructureAnalysisTests()
    {
        _testPdfPath = Path.Combine("TestData", "sample-invoice.pdf");
    }

    [Fact]
    public void AnalyzePdfStructure_ShouldExtractBasicMetadata()
    {
        // Arrange & Act
        using var pdf = PdfDocument.Open(_testPdfPath);
        var info = pdf.Information;

        // Assert
        Assert.NotNull(info);
        // PDF metadata might be null/empty, but the object should exist
    }

    [Fact]
    public void AnalyzePageDimensions_ShouldReturnValidDimensions()
    {
        // Arrange & Act
        using var pdf = PdfDocument.Open(_testPdfPath);
        var page = pdf.GetPage(1);
        var mediaBox = page.MediaBox;

        // Assert
        // MediaBox is a PdfRectangle, so we calculate width and height from its bounds
        var width = mediaBox.Bounds.Right - mediaBox.Bounds.Left;
        var height = mediaBox.Bounds.Top - mediaBox.Bounds.Bottom;

        Assert.True(width > 0, "Page width should be positive");
        Assert.True(height > 0, "Page height should be positive");
        Assert.True(width > 100, "Page width should be reasonable (> 100 points)");
        Assert.True(height > 100, "Page height should be reasonable (> 100 points)");
    }

    [Fact]
    public void AnalyzeWordDistribution_ShouldShowReasonableSpread()
    {
        // Arrange & Act
        using var pdf = PdfDocument.Open(_testPdfPath);
        var page = pdf.GetPage(1);
        var words = page.GetWords().ToList();

        // Assert
        Assert.NotEmpty(words);

        // Analyze word distribution across the page
        var minX = words.Min(w => w.BoundingBox.Left);
        var maxX = words.Max(w => w.BoundingBox.Right);
        var minY = words.Min(w => w.BoundingBox.Bottom);
        var maxY = words.Max(w => w.BoundingBox.Top);

        var pageWidth = maxX - minX;
        var pageHeight = maxY - minY;

        Assert.True(pageWidth > 0, "Words should span some width on the page");
        Assert.True(pageHeight > 0, "Words should span some height on the page");
    }

    [Fact]
    public void AnalyzeFontInformation_ShouldExtractFontDetails()
    {
        // Arrange & Act
        using var pdf = PdfDocument.Open(_testPdfPath);
        var page = pdf.GetPage(1);
        var letters = page.Letters.ToList();

        // Assert
        Assert.NotEmpty(letters);

        // Check that font information is available
        var fontsUsed = letters.Select(l => l.FontName).Distinct().ToList();
        Assert.NotEmpty(fontsUsed);

        // Verify font sizes are reasonable
        var fontSizes = letters.Select(l => l.FontSize).Distinct().ToList();
        Assert.NotEmpty(fontSizes);
        Assert.All(fontSizes, size => Assert.True(size > 0, "Font size should be positive"));
    }

    [Theory]
    [InlineData(6)] // Very small font
    [InlineData(12)] // Standard font
    [InlineData(18)] // Large font
    public void FindTextByFontSize_ShouldFilterCorrectly(double targetFontSize)
    {
        // Arrange
        using var pdf = PdfDocument.Open(_testPdfPath);
        var page = pdf.GetPage(1);
        var letters = page.Letters.ToList();

        // Act
        var lettersWithTargetSize = letters
            .Where(l => Math.Abs(l.FontSize - targetFontSize) < 1.0) // Allow 1 point tolerance
            .ToList();

        // Assert
        Assert.All(lettersWithTargetSize, letter =>
            Assert.True(Math.Abs(letter.FontSize - targetFontSize) < 1.0,
                $"Letter font size {letter.FontSize} should be close to target {targetFontSize}"));
    }

    [Fact]
    public void ExtractLargestTextElements_ShouldIdentifyHeaders()
    {
        // Arrange
        using var pdf = PdfDocument.Open(_testPdfPath);
        var page = pdf.GetPage(1);
        var words = page.GetWords().ToList();

        // Act - Find words with largest font sizes (likely headers)
        if (words.Any())
        {
            var maxFontSize = words.Max(w => w.Letters.Max(l => l.FontSize));
            var headerWords = words
                .Where(w => w.Letters.Any(l => Math.Abs(l.FontSize - maxFontSize) < 0.5))
                .ToList();

            // Assert
            Assert.NotEmpty(headerWords);

            // Header words should typically be in the upper portion of the page
            var pageHeight = page.MediaBox.Bounds.Top - page.MediaBox.Bounds.Bottom;
            var avgHeaderY = headerWords.Average(w => w.BoundingBox.Bottom);

            // This is a heuristic test - headers are often in the upper half of the page
            Assert.True(avgHeaderY > pageHeight * 0.3,
                "Header text should typically appear in upper portion of page");
        }
    }

    [Fact]
    public void DetectTextAlignment_ShouldIdentifyAlignmentPatterns()
    {
        // Arrange
        using var pdf = PdfDocument.Open(_testPdfPath);
        var page = pdf.GetPage(1);
        var words = page.GetWords().ToList();

        // Act - Group words by approximate Y coordinate (same line)
        var lineGroups = words
            .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 0)) // Round to nearest point
            .Where(g => g.Count() > 1) // Only consider lines with multiple words
            .ToList();

        // Assert
        Assert.NotEmpty(lineGroups);

        foreach (var lineGroup in lineGroups.Take(5)) // Test first 5 lines
        {
            var wordsInLine = lineGroup.OrderBy(w => w.BoundingBox.Left).ToList();

            // Check that words in the same line have similar Y coordinates
            var yCoordinates = wordsInLine.Select(w => w.BoundingBox.Bottom).ToList();
            var maxYDifference = yCoordinates.Max() - yCoordinates.Min();

            Assert.True(maxYDifference < 5.0,
                "Words on the same line should have similar Y coordinates");
        }
    }

    [Fact]
    public void MeasureTextDensity_ShouldCalculateReasonableDensity()
    {
        // Arrange
        using var pdf = PdfDocument.Open(_testPdfPath);
        var page = pdf.GetPage(1);
        var words = page.GetWords().ToList();
        var pageWidth = page.MediaBox.Bounds.Right - page.MediaBox.Bounds.Left;
        var pageHeight = page.MediaBox.Bounds.Top - page.MediaBox.Bounds.Bottom;
        var pageArea = pageWidth * pageHeight;

        // Act
        var totalTextArea = words.Sum(w => w.BoundingBox.Width * w.BoundingBox.Height);
        var textDensity = totalTextArea / pageArea;

        // Assert
        Assert.True(textDensity > 0, "Text density should be positive");
        Assert.True(textDensity < 1.0, "Text density should be less than 100%");
        Assert.True(textDensity < 0.5, "Text density should be reasonable for a typical document");
    }
}