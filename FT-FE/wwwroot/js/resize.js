window.initializeResizeHandler = () => {
    //console.log("Initializing resize handler...");

    const resizeHandle = document.getElementById('resizeHandle');
    const leftPanel = document.getElementById('leftFormPanel');
    const rightPanel = document.getElementById('right-tree-panel');
    const container = document.querySelector('.main-container');

    // Null checks for DOM elements
    if (!resizeHandle || !leftPanel || !rightPanel || !container) {
        console.error("Resize handler failed: One or more elements not found.", {
            resizeHandle: !!resizeHandle,
            leftPanel: !!leftPanel,
            rightPanel: !!rightPanel,
            container: !!container
        });
        return false; // Return false to indicate failure
    }

    let isResizing = false;

    const onMouseDown = (e) => {
        isResizing = true;
        document.body.style.userSelect = 'none';
        //console.log("Resize started");
    };

    const onMouseMove = (e) => {
        if (!isResizing) return;

        const containerWidth = container.offsetWidth;
        const leftPanelRect = leftPanel.getBoundingClientRect();
        let newLeftWidth = e.clientX - leftPanelRect.left;

        // Enforce minimum and maximum widths
        newLeftWidth = Math.max(250, Math.min(newLeftWidth, containerWidth - 250));

        leftPanel.style.flexBasis = `${newLeftWidth}px`;
        leftPanel.style.maxWidth = `${newLeftWidth}px`;
        rightPanel.style.flexBasis = `calc(100% - ${newLeftWidth}px - 5px)`; // 5px for resize handle
        //console.log(`Resizing: Left panel width = ${newLeftWidth}px`);
    };

    const onMouseUp = () => {
        isResizing = false;
        document.body.style.userSelect = '';
        //console.log("Resize ended");
    };

    // Attach event listeners
    resizeHandle.addEventListener('mousedown', onMouseDown);
    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', onMouseUp);

    // Provide a cleanup function
    window.cleanupResizeHandler = () => {
        resizeHandle.removeEventListener('mousedown', onMouseDown);
        document.removeEventListener('mousemove', onMouseMove);
        document.removeEventListener('mouseup', onMouseUp);
        //console.log("Resize handler cleaned up");
    };

    console.log("Resize handler initialized successfully");
    return true; // Return true to indicate success
};