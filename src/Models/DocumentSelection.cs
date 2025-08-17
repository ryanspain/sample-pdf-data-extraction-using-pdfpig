using System.ComponentModel.DataAnnotations;

namespace Sample.PdfDataExtraction.Models;

public record DocumentSelection
{
    public string Id { get; set; } = string.Empty;
    [Required] public string Name { get; set; } = string.Empty;
    public CanvasSelection CanvasSelection { get; set; } = new();
    public PdfSelection PdfSelection { get; set; } = new();
}