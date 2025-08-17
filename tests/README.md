# PDF data extraction unit tests

This unit test project serves as a testing playground for exploring `PdfPig` NuGet packages features and understanding its API. This project does not test the web application found under `/src`, but rather focuses on demonstrating and validating various PDF data extraction techniques using `PdfPig`.

## Test categories

### 1. Basic PDF operations (`PdfBasicTests.cs`)

Tests fundamental PDF operations to ensure the library is working correctly:

- Opening PDF documents
- Accessing page information
- Basic text extraction
- Word-level content retrieval

### 2. Text extraction tests (`PdfTextExtractionTests.cs`)

Focuses on various text extraction methods:

- **Full document text extraction**: Extracting all text content from PDFs
- **Page-specific extraction**: Getting text from individual pages
- **Content validation**: Verifying extracted text contains expected content
- **Word-level extraction**: Accessing individual words with bounding box information

### 3. Block-based extraction (`PdfBlockExtractionTests.cs`)

Demonstrates document layout analysis using text blocks:

- **Text block detection**: Using `DocstrumBoundingBoxes` for layout analysis
- **Block validation**: Ensuring blocks have valid dimensions and content
- **Region filtering**: Finding blocks within specific coordinate boundaries
- **Layout structure analysis**: Understanding document organization

### 4. Region-based extraction (`PdfRegionExtractionTests.cs`)

Tests coordinate-based text extraction techniques:

- **Coordinate-based filtering**: Extracting text from specific page regions
- **Boundary validation**: Testing edge cases with different coordinate ranges
- **Word ordering**: Ensuring proper text sequence within regions
- **Overlapping region analysis**: Testing behavior with intersecting areas

### 5. Advanced structure analysis (`PdfStructureAnalysisTests.cs`)

Explores more advanced PDF analysis capabilities:

- **Page dimension analysis**: - Understanding document layout and size
- **Font information extraction**: Analyzing typefaces, sizes, and formatting
- **Text distribution patterns**: Measuring content spread across pages
- **Header detection**: Identifying document structure through font analysis
- **Text alignment detection**: Understanding document formatting
- **Content density analysis**: Measuring text coverage on pages

## Test data

The tests use `sample-invoice.pdf` located in the `TestData` folder. This file is automatically copied to the output directory during build to ensure tests can access the sample document.

## Running the tests

Use your preferred IDE, or run the following commands in a terminal.

```bash
# Navigate to this test project directory
cd tests

# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "ClassName=PdfBasicTests"
```
