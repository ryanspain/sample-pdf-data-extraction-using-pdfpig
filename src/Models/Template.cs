using Sample.PdfDataExtraction.Models;

public class Template
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<DocumentSelection> DocumentSelections { get; set; } = new();
}