function renderFamilyTree(nodes, edges, spouseEdges, dotnetHelper, treeName) {
    const familyTree = document.getElementById("familyTree");
    if (!familyTree) {
        console.error("Element #familyTree not found");
        return;
    }

    // Clear any existing SVG
    d3.select("#familyTree").selectAll("*").remove();

    if (!nodes || nodes.length === 0) {
        console.log("No nodes to render");
        return;
    }

    // Set dimensions with better sizing
    const width = familyTree.clientWidth || 1000;
    const height = familyTree.clientHeight || 800;

    // Create SVG with a gradient background
    const svg = d3.select("#familyTree")
        .append("svg")
        .attr("width", width)
        .attr("height", height)
        .attr("viewBox", [0, 0, width, height]);

    // Add a subtle gradient background
    const defs = svg.append("defs");
    const gradient = defs.append("linearGradient")
        .attr("id", "background-gradient")
        .attr("x1", "0%")
        .attr("y1", "0%")
        .attr("x2", "0%")
        .attr("y2", "100%");

    gradient.append("stop")
        .attr("offset", "0%")
        .attr("stop-color", "#f8f9fa");

    gradient.append("stop")
        .attr("offset", "100%")
        .attr("stop-color", "#e9ecef");

    // Add the background rectangle
    svg.append("rect")
        .attr("width", width)
        .attr("height", height)
        .attr("fill", "url(#background-gradient)");

    // Create container group for zoom functionality
    const g = svg.append("g");

    // Debug: Log input data
    console.log("Received Nodes:", nodes);
    console.log("Received Parent Edges:", edges);
    console.log("Received Spouse Edges:", spouseEdges);

    // Build a graph representation
    const graphNodes = nodes.map(node => ({ ...node }));
    const graphEdges = [...edges, ...spouseEdges]; // Combine edges for layout

    // Create a map of nodes for easy lookup
    const nodeMap = new Map(nodes.map(node => [node.id, node]));

    // Build adjacency list for the graph
    const childrenMap = new Map();
    const parentMap = new Map();
    edges.forEach(edge => {
        if (!childrenMap.has(edge.from)) childrenMap.set(edge.from, []);
        childrenMap.get(edge.from).push(edge.to);
        if (!parentMap.has(edge.to)) parentMap.set(edge.to, []);
        parentMap.get(edge.to).push(edge.from);
    });

    // Find roots (nodes with no parents in the parent-child hierarchy)
    const roots = nodes.filter(node => !parentMap.has(node.id)).map(node => node.id);

    // Assign levels to nodes using a topological sort
    const levels = new Map();
    const queue = [...roots];
    let currentLevel = 0;
    const visited = new Set();

    while (queue.length > 0) {
        const levelSize = queue.length;
        for (let i = 0; i < levelSize; i++) {
            const nodeId = queue.shift();
            if (visited.has(nodeId)) continue;
            visited.add(nodeId);
            levels.set(nodeId, currentLevel);
            const children = childrenMap.get(nodeId) || [];
            queue.push(...children);
        }
        currentLevel++;
    }

    // Assign x and y positions based on levels with improved spacing
    const nodesByLevel = Array.from(levels.entries()).reduce((acc, [id, level]) => {
        if (!acc[level]) acc[level] = [];
        acc[level].push(id);
        return acc;
    }, []);

    // IMPROVEMENT: Reduced vertical spacing between generations
    const levelHeight = 120; // Reduced from 180 to make tree more compact
    const nodePositions = new Map();

    nodesByLevel.forEach((nodeIds, level) => {
        const spacing = width / (nodeIds.length + 1);
        nodeIds.forEach((id, index) => {
            nodePositions.set(id, {
                x: (index + 1) * spacing,
                y: level * levelHeight + 70 // Reduced from 100 to make tree more compact
            });
        });
    });

    // First pass: Identify all child nodes for each marriage
    const marriageChildren = new Map();
    edges.forEach(edge => {
        // If the source is a marriage node, add the target as its child
        const sourceNode = nodeMap.get(edge.from);
        if (sourceNode && sourceNode.type === "marriage") {
            if (!marriageChildren.has(edge.from)) {
                marriageChildren.set(edge.from, []);
            }
            marriageChildren.get(edge.from).push(edge.to);
        }
    });

    // Position marriage nodes between spouses and respect hierarchy
    const marriageNodes = nodes.filter(node => node.type === "marriage");
    marriageNodes.forEach(marriage => {
        const spouses = spouseEdges
            .filter(e => e.to === marriage.id)
            .map(e => e.from);

        const children = marriageChildren.get(marriage.id) || [];
        const marriagePos = nodePositions.get(marriage.id);

        if (spouses.length === 2) {
            const spouse1Pos = nodePositions.get(spouses[0]);
            const spouse2Pos = nodePositions.get(spouses[1]);

            if (spouse1Pos && spouse2Pos && marriagePos) {
                // Calculate average position of children if any
                let childrenCenterX = marriagePos.x;
                if (children.length > 0) {
                    const childPositions = children
                        .map(childId => nodePositions.get(childId))
                        .filter(pos => pos); // Remove undefined positions

                    if (childPositions.length > 0) {
                        childrenCenterX = childPositions.reduce((sum, pos) => sum + pos.x, 0) / childPositions.length;

                        // Adjust marriage position to be centered above its children
                        marriagePos.x = childrenCenterX;
                    }
                }

                // Place spouses side by side centered above children
                const spouseDistance = 100; // Reduced from 100 to make spouses closer
                spouse1Pos.x = marriagePos.x - spouseDistance / 2;
                spouse2Pos.x = marriagePos.x + spouseDistance / 2;
                spouse1Pos.y = marriagePos.y - 40; // Reduced vertical distance
                spouse2Pos.y = marriagePos.y - 40; // Reduced vertical distance
            }
        }
    });

    // IMPROVEMENT: Create shorter curved paths for edges
    function createPath(sourcePos, targetPos, isSpouse = false) {
        if (!sourcePos || !targetPos) return null;

        if (isSpouse) {
            // Straight line for spouse connections
            return `M${sourcePos.x},${sourcePos.y} L${targetPos.x},${targetPos.y}`;
        } else {
            // Shorter curved path for parent-child connections
            const midY = sourcePos.y + (targetPos.y - sourcePos.y) * 0.4; // Reduced curve point
            return `M${sourcePos.x},${sourcePos.y} 
                    C${sourcePos.x},${midY} 
                     ${targetPos.x},${midY} 
                     ${targetPos.x},${targetPos.y}`;
        }
    }

    // Render parent-child links with curved paths, hide if isLayout is true
    g.selectAll(".parent-child-link")
        .data(edges)
        .enter()
        .append("path")
        .attr("class", "parent-child-link")
        .attr("d", d => {
            const sourcePos = nodePositions.get(d.from);
            const targetPos = nodePositions.get(d.to);
            return createPath(sourcePos, targetPos);
        })
        .attr("fill", "none")
        .attr("stroke", d => d.isLayoutOnly ? "#f8f9fa" : "#6c757d") // Match background color if isLayout is true
        .attr("stroke-width", 2)
        .attr("stroke-dasharray", d => {
            return "0";
        });

    // Render spouse links, hide if isLayout is true
    g.selectAll(".spouse-link")
        .data(spouseEdges)
        .enter()
        .append("path")
        .attr("class", "spouse-link")
        .attr("d", d => {
            const sourcePos = nodePositions.get(d.from);
            const targetPos = nodePositions.get(d.to);
            return createPath(sourcePos, targetPos, true);
        })
        .attr("fill", "none")
        .attr("stroke", d => d.isLayout ? "#f8f9fa" : "#e83e8c") // Match background color if isLayout is true
        .attr("stroke-width", 2);

    // Add decorative elements
    // Marriage symbol - small heart at marriage nodes
    g.selectAll(".marriage-symbol")
        .data(marriageNodes)
        .enter()
        .append("path")
        .attr("class", "marriage-symbol")
        .attr("d", d => {
            const pos = nodePositions.get(d.id);
            if (!pos) return null;
            const heartSize = 10; // Reduced from 12
            const x = pos.x;
            const y = pos.y;
            return `M ${x} ${y - heartSize / 4} 
                    C ${x} ${y - heartSize / 2}, ${x - heartSize / 2} ${y - heartSize}, ${x - heartSize / 4} ${y - heartSize / 4} 
                    L ${x} ${y + heartSize / 4} 
                    L ${x + heartSize / 4} ${y - heartSize / 4} 
                    C ${x + heartSize / 2} ${y - heartSize}, ${x} ${y - heartSize / 2}, ${x} ${y - heartSize / 4}`;
        })
        .attr("fill", "#e83e8c")
        .attr("stroke", "none");

    // Render person nodes with nicer styling
    const node = g.selectAll(".node")
        .data(nodes.filter(d => d.type === "person"))
        .enter()
        .append("g")
        .attr("class", "node")
        .attr("transform", d => {
            const pos = nodePositions.get(d.id);
            return pos ? `translate(${pos.x},${pos.y})` : "";
        })
        .style("cursor", "pointer")
        .on("click", (event, d) => {
            dotnetHelper.invokeMethodAsync("OnNodeClick", parseInt(d.id));
        })
        .on("mouseover", function () {
            d3.select(this).select("circle")
                .transition()
                .duration(300)
                .attr("r", 32);
        })
        .on("mouseout", function () {
            d3.select(this).select("circle")
                .transition()
                .duration(300)
                .attr("r", 28);
        });

    // Add shadow filter for nodes
    defs.append("filter")
        .attr("id", "drop-shadow")
        .attr("height", "130%")
        .append("feDropShadow")
        .attr("dx", 0)
        .attr("dy", 3)
        .attr("stdDeviation", 3)
        .attr("flood-color", "rgba(0,0,0,0.3)");

    // Person node backgrounds - slightly smaller circle with gradient fill
    node.append("circle")
        .attr("r", 28) // Reduced from 32
        .attr("fill", d => {
            const gradientId = `person-gradient-${d.id}`;

            const personGradient = defs.append("radialGradient")
                .attr("id", gradientId)
                .attr("cx", "30%")
                .attr("cy", "30%")
                .attr("r", "70%");

            if (d.gender === "male") {
                personGradient.append("stop")
                    .attr("offset", "0%")
                    .attr("stop-color", "#4dabf7");
                personGradient.append("stop")
                    .attr("offset", "100%")
                    .attr("stop-color", "#1971c2");
            }
            else {
                personGradient.append("stop")
                    .attr("offset", "0%")
                    .attr("stop-color", "#f783ac");
                personGradient.append("stop")
                    .attr("offset", "100%")
                    .attr("stop-color", "#c2255c");
            }

            return `url(#${gradientId})`;
        })
        .attr("stroke", "#fff")
        .attr("stroke-width", 2)
        .attr("filter", "url(#drop-shadow)");

    // Add person icons based on gender
    node.append("text")
        .attr("text-anchor", "middle")
        .attr("dominant-baseline", "central")
        .attr("fill", "white")
        .attr("font-family", "FontAwesome")
        .attr("font-size", "16px") // Reduced from 20px
        .text(d => d.gender === "male" ? "\uf183" : "\uf182");

    // Add name labels with better styling
    node.append("text")
        .attr("dy", 45) // Reduced from 50
        .attr("text-anchor", "middle")
        .attr("font-family", "Arial, sans-serif") /////////////////////////////////////////////
        .attr("font-size", "18px") // Reduced from 14px
        .attr("font-weight", "bold")
        .attr("fill", "#343a40")
        .text(d => d.label);

    // Add birth-death years if available
    node.append("text")
        .attr("dy", 60) // Reduced from 70
        .attr("text-anchor", "middle")
        .attr("font-family", "Arial, sans-serif")  ////////////////////////////////
        .attr("font-size", "14px") // Reduced from 12px
        .attr("fill", "#343a40")
        .text(d => {
            if (d.birthYear) {
                const birth = new Date(d.birthYear).toISOString().split('T')[0];
                const death = d.deathYear ? new Date(d.deathYear).toISOString().split('T')[0] : '';
                return `(${birth}${death ? ` - ${death}` : ''})`;
            }
            return '';
        });

    // Add controls - zoom buttons
    const zoomControl = svg.append("g")
        .attr("class", "zoom-control")
        .attr("transform", `translate(${width - 100}, 30)`);

    // Zoom in button
    zoomControl.append("rect")
        .attr("x", 0)
        .attr("y", 0)
        .attr("width", 30)
        .attr("height", 30)
        .attr("rx", 5)
        .attr("ry", 5)
        .attr("fill", "#fff")
        .attr("stroke", "#dee2e6")
        .style("cursor", "pointer")
        .on("click", () => {
            zoom.scaleBy(svg.transition().duration(750), 1.3);
        });

    zoomControl.append("text")
        .attr("x", 15)
        .attr("y", 20)
        .attr("text-anchor", "middle")
        .attr("font-size", "20px")
        .attr("fill", "#495057")
        .style("pointer-events", "none")
        .text("+");

    // Zoom out button
    zoomControl.append("rect")
        .attr("x", 40)
        .attr("y", 0)
        .attr("width", 30)
        .attr("height", 30)
        .attr("rx", 5)
        .attr("ry", 5)
        .attr("fill", "#fff")
        .attr("stroke", "#dee2e6")
        .style("cursor", "pointer")
        .on("click", () => {
            zoom.scaleBy(svg.transition().duration(750), 0.7);
        });

    zoomControl.append("text")
        .attr("x", 55)
        .attr("y", 20)
        .attr("text-anchor", "middle")
        .attr("font-size", "20px")
        .attr("fill", "#495057")
        .style("pointer-events", "none")
        .text("−");

    // Add title to the family tree
    svg.append("text")
        .attr("x", width / 2)
        .attr("y", 30)
        .attr("text-anchor", "middle")
        .attr("font-family", "Arial, sans-serif") ///////////////////////////////
        .attr("font-size", "24px")
        .attr("font-weight", "bold")
        .attr("fill", "#0E4158")
        .text(treeName);

    // Improve zoom functionality
    const zoom = d3.zoom()
        .scaleExtent([0.1, 3])
        .on("zoom", (event) => {
            g.attr("transform", event.transform);
        });

    svg.call(zoom);

    // Center the view initially with a better fit
    fitTreeToView(svg, g, zoom, width, height);
}

function clearFamilyTree() {
    const familyTree = document.getElementById("familyTree");
    if (familyTree) {
        d3.select("#familyTree").selectAll("*").remove();
    }
}

function fitTreeToView(svg, g, zoom, width, height) {
    if (!g || g.empty()) return;

    // Get bounds of all elements
    const bounds = g.node().getBBox();

    // Calculate scale to fit everything with padding
    const padding = 50;
    const scale = Math.min(
        width / (bounds.width + padding * 2),
        height / (bounds.height + padding * 2)
    );

    // Apply scale but keep it reasonable
    const finalScale = Math.min(scale, 0.9);

    // Center the tree in view
    const centerX = width / 2 - (bounds.x + bounds.width / 2) * finalScale;
    const centerY = height / 2 - (bounds.y + bounds.height / 2) * finalScale;

    // Create and apply transform
    const transform = d3.zoomIdentity
        .translate(centerX, centerY)
        .scale(finalScale);

    svg.transition()
        .duration(750)
        .call(zoom.transform, transform);
}

function fitFamilyTreeToView() {
    const familyTree = document.getElementById("familyTree");
    if (!familyTree) return;

    const svg = d3.select("#familyTree svg");
    if (svg.empty()) return;

    const g = svg.select("g");
    if (g.empty()) return;

    const width = familyTree.clientWidth;
    const height = familyTree.clientHeight;

    // Get bounds of all elements
    const bounds = g.node().getBBox();

    // Calculate scale to fit everything with less padding (more compact view)
    const padding = 40; // Reduced from 50
    const scale = Math.min(
        width / (bounds.width + 2 * padding),
        height / (bounds.height + 2 * padding)
    );

    // Limit maximum scale to prevent overly large trees
    const finalScale = Math.min(scale, 0.9);

    // Center the view
    const centerX = width / 2 - (bounds.x + bounds.width / 2) * finalScale;
    const centerY = height / 2 - (bounds.y + bounds.height / 2) * finalScale;

    // Apply the transform
    const transform = d3.zoomIdentity
        .translate(centerX, centerY)
        .scale(finalScale);

    // Get the zoom behavior from the SVG
    const zoom = d3.zoom().on("zoom", (event) => {
        g.attr("transform", event.transform);
    });

    svg.call(zoom).transition().duration(750).call(zoom.transform, transform);
}