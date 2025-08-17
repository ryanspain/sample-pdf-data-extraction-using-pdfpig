using System.Drawing;
using Sample.PdfDataExtraction.Models;

namespace Sample.PdfDataExtraction.Extensions;

/// <summary>
/// Extension methods for converting between PDF and Canvas coordinate systems.
/// 
/// PDF Coordinates: Origin (0,0) at bottom-left, Y increases upward
/// Canvas Coordinates: Origin (0,0) at top-left, Y increases downward
/// </summary>
public static class CoordinateExtensions
{
    /// <summary>
    /// Converts a Y coordinate from Canvas to PDF coordinate system
    /// </summary>
    /// <param name="canvasY">Y coordinate in canvas system (top-left origin)</param>
    /// <param name="pdfHeight">Total height of the PDF/canvas</param>
    /// <returns>Y coordinate in PDF system (bottom-left origin)</returns>
    private static int ToPdfY(this int canvasY, double pdfHeight)
    {
        return (int)pdfHeight - canvasY;
    }

    /// <summary>
    /// Converts a Y coordinate from PDF to Canvas coordinate system
    /// </summary>
    /// <param name="pdfY">Y coordinate in PDF system (bottom-left origin)</param>
    /// <param name="pdfHeight">Total height of the PDF/canvas</param>
    /// <returns>Y coordinate in canvas system (top-left origin)</returns>
    private static int ToCanvasY(this int pdfY, double pdfHeight)
    {
        return (int)pdfHeight - pdfY;
    }

    // TODO: Add doc comment
    public static PdfSelection ToPdfSelectionCoordinates(
        this CanvasSelection selection,
        int pdfHeight
    )
    {
        return new PdfSelection
        {
            BottomLeftX = selection.X,
            BottomLeftY = selection.Y.ToPdfY(pdfHeight),
            TopRightX = selection.X + (int)selection.Width,
            TopRightY = (selection.Y + (int)selection.Height).ToPdfY(pdfHeight)
        };
    }
    
    /// <summary>
    /// Creates a PdfSelection from canvas coordinates (during mouse interaction)
    /// </summary>
    /// <param name="canvas">Start and end X and Y coordinates in canvas</param>
    /// <param name="pdfHeight">Total height of the PDF/canvas</param>
    /// <returns>PdfSelection with PDF coordinates</returns>
    public static PdfSelection ToPdfSelectionCoordinates(
        this (Point start, Point end) canvas,
        int pdfHeight
    )
    {
        // Calculate min/max in canvas coordinates
        var minX = Math.Min(canvas.start.X, canvas.end.X);
        var maxX = Math.Max(canvas.start.X, canvas.end.X);
        var minY = Math.Min(canvas.start.X, canvas.end.Y);
        var maxY = Math.Max(canvas.start.X, canvas.end.Y);

        return new PdfSelection
        {
            BottomLeftX = minX,
            BottomLeftY = maxY.ToPdfY(pdfHeight),
            TopRightX = maxX,
            TopRightY = minY.ToPdfY(pdfHeight)
        };
    }

    /// <summary>
    /// Converts a PdfSelection from PDF coordinates to canvas drawing parameters
    /// </summary>
    /// <param name="pdfSelection">PdfSelection with PDF coordinates</param>
    /// <param name="pdfHeight">Total height of the PDF/canvas</param>
    /// <returns>Canvas coordinates for drawing (x, y, width, height)</returns>
    public static CanvasSelection ToCanvasSelectionCoordinates(
        this PdfSelection pdfSelection,
        int pdfHeight
    )
    {
        // The Y-axis is inverted between PDF and Canvas
        var topLeftX = pdfSelection.BottomLeftX;
        var topLeftY = pdfSelection.TopRightY.ToCanvasY(pdfHeight);
        
        // Height and width are the same in both systems
        var width = pdfSelection.TopRightX - pdfSelection.BottomLeftX;
        var height = pdfSelection.TopRightY - pdfSelection.BottomLeftY;

        return new CanvasSelection
        {
            X = topLeftX,
            Y = topLeftY,
            Width = width,
            Height = height
        };
    }
    
    /// <summary>
    /// Converts a selection represented as a tuple of points to CanvasSelection
    /// </summary>
    /// <param name="selection">Start and end X and Y coordinates in canvas</param>
    /// <returns></returns>
    public static CanvasSelection AsCanvasSelectionCoordinates(this (Point start, Point end) selection)
    {
        return new CanvasSelection
        {
            X = Math.Min(selection.start.X, selection.end.X),
            Y = Math.Min(selection.start.Y, selection.end.Y),
            Width = Math.Abs(selection.start.X - selection.end.X),
            Height = Math.Abs(selection.start.Y - selection.end.Y)
        };
    }
}