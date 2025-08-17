let pdfDoc = null;
let pageNum = 1;
let pageRendering = false;
let pageNumPending = null;
let scale = 1.0; // 100% zoom
let canvas = null;
let ctx = null;
let overlayCanvas = null;
let overlayCtx = null;

// Configure PDF.js worker
pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';

window.initializePdfViewer = async function(pdfUrl) {
    try {
        canvas = document.getElementById('pdfCanvas');
        ctx = canvas.getContext('2d');
        overlayCanvas = document.getElementById('overlayCanvas');
        overlayCtx = overlayCanvas.getContext('2d');
        
        // Load the PDF
        const loadingTask = pdfjsLib.getDocument(pdfUrl);
        pdfDoc = await loadingTask.promise;
        
        // Render the first page
        await renderPage(pageNum);
        
        console.log('PDF loaded successfully');
    } catch (error) {
        console.error('Error loading PDF:', error);
    }
};

async function renderPage(num) {
    pageRendering = true;
    
    try {
        const page = await pdfDoc.getPage(num);
        const viewport = page.getViewport({ scale: scale });
        
        // Set canvas dimensions
        canvas.height = viewport.height;
        canvas.width = viewport.width;
        overlayCanvas.height = viewport.height;
        overlayCanvas.width = viewport.width;
        
        // Update container size to fit content exactly (no scrollbars)
        const container = canvas.parentElement;
        container.style.width = viewport.width + 'px';
        container.style.height = viewport.height + 'px';
        
        // Render PDF page into canvas context
        const renderContext = {
            canvasContext: ctx,
            viewport: viewport
        };
        
        await page.render(renderContext).promise;
        pageRendering = false;
        
        if (pageNumPending !== null) {
            renderPage(pageNumPending);
            pageNumPending = null;
        }
    } catch (error) {
        console.error('Error rendering page:', error);
        pageRendering = false;
    }
}

window.getCanvasRect = function(canvasId) {
    const canvas = document.getElementById(canvasId);
    const rect = canvas.getBoundingClientRect();
    return {
        left: rect.left,
        top: rect.top,
        right: rect.right,
        bottom: rect.bottom,
        width: rect.width,
        height: rect.height
    };
};

window.drawBoundingBox = function(selection) {
    if (!overlayCtx) return;
    
    // Clear previous current drawing (but preserve saved boxes)
    overlayCtx.clearRect(0, 0, overlayCanvas.width, overlayCanvas.height);
    
    // Redraw all saved boxes first
    redrawSavedBoxes();
    
    // Draw current bounding box
    overlayCtx.strokeStyle = '#007bff';
    overlayCtx.lineWidth = 2;
    overlayCtx.setLineDash([5, 5]);
    overlayCtx.strokeRect(selection.x, selection.y, selection.width, selection.height);
    
    // Draw fill with low opacity
    overlayCtx.fillStyle = 'rgba(0, 123, 255, 0.1)';
    overlayCtx.fillRect(selection.x, selection.y, selection.width, selection.height);
};

window.drawSavedBoundingBox = function(selection, name) {
    if (!overlayCtx) return;
    
    // Draw bounding box with solid line
    overlayCtx.strokeStyle = '#28a745';
    overlayCtx.lineWidth = 2;
    overlayCtx.setLineDash([]);
    overlayCtx.strokeRect(selection.x, selection.y, selection.width, selection.height);
    
    // Draw fill with low opacity
    overlayCtx.fillStyle = 'rgba(40, 167, 69, 0.1)';
    overlayCtx.fillRect(selection.x, selection.y, selection.width, selection.height);
    
    // Draw name label
    if (name) {
        overlayCtx.fillStyle = '#28a745';
        overlayCtx.font = '12px Arial';
        overlayCtx.fillRect(selection.x, selection.y - 20, overlayCtx.measureText(name).width + 8, 20);
        overlayCtx.fillStyle = 'white';
        overlayCtx.fillText(name, selection.x + 4, selection.y - 6);
    }
};

window.highlightBoundingBox = function(selection, name) {
    if (!overlayCtx) return;
    
    // Clear and redraw all saved boxes
    overlayCtx.clearRect(0, 0, overlayCanvas.width, overlayCanvas.height);
    redrawSavedBoxes();
    
    // Draw highlight box with different style
    overlayCtx.strokeStyle = '#ffc107';
    overlayCtx.lineWidth = 3;
    overlayCtx.setLineDash([10, 5]);
    overlayCtx.strokeRect(selection.x, selection.y, selection.width, selection.height);
    
    // Draw highlight fill
    overlayCtx.fillStyle = 'rgba(255, 193, 7, 0.2)';
    overlayCtx.fillRect(selection.x, selection.y, selection.width, selection.height);

    // Draw name label
    if (name) {
        overlayCtx.fillStyle = '#ffc107';
        overlayCtx.font = '12px Arial';
        overlayCtx.fillRect(selection.x, selection.y - 20, overlayCtx.measureText(name).width + 8, 20);
        overlayCtx.fillStyle = 'white';
        overlayCtx.fillText(name, selection.x + 4, selection.y - 6);
    }
};

window.clearBoundingBoxes = function() {
    if (!overlayCtx) return;
    overlayCtx.clearRect(0, 0, overlayCanvas.width, overlayCanvas.height);
};

// Store saved boxes for redrawing
let savedBoxes = [];

window.storeSavedBoxes = function(boxes) {
    savedBoxes = boxes;
};

function redrawSavedBoxes() {
    // This function is called internally to redraw saved boxes
    // The actual box data comes from the C# side through drawSavedBoundingBox calls
}

// Click detection on overlay canvas for selecting boxes
overlayCanvas.addEventListener('click', function(e) {
    if (!savedBoxes.length) return;
    
    const rect = overlayCanvas.getBoundingClientRect();
    const clickX = e.clientX - rect.left;
    const clickY = e.clientY - rect.top;
    const pdfHeight = rect.height;
    
    // Check if click is within any saved bounding box
    for (let i = savedBoxes.length - 1; i >= 0; i--) {
        const box = savedBoxes[i];
        const canvasY1 = pdfHeight - box.topRightY;
        const canvasY2 = pdfHeight - box.bottomLeftY;
        
        if (clickX >= box.bottomLeftX && clickX <= box.topRightX &&
            clickY >= canvasY1 && clickY <= canvasY2) {
            // Notify Blazor component about box selection
            DotNet.invokeMethodAsync('web', 'OnBoundingBoxClicked', i);
            break;
        }
    }
});

// Navigation functions (for future use)
window.goToPrevPage = function() {
    if (pageNum <= 1) return;
    pageNum--;
    queueRenderPage(pageNum);
};

window.goToNextPage = function() {
    if (pageNum >= pdfDoc.numPages) return;
    pageNum++;
    queueRenderPage(pageNum);
};

function queueRenderPage(num) {
    if (pageRendering) {
        pageNumPending = num;
    } else {
        renderPage(num);
    }
}
