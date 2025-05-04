// ==UserScript==
// @name         Claude Project Knowledge Exporter
// @namespace    http://tampermonkey.net/
// @version      1.3
// @description  Export project knowledge files from Claude's Project Knowledge section
// @author       You
// @match        https://*.claude.ai/*
// @match        https://claude.ai/*
// @match        https://*.anthropic.com/*
// @match        https://claude.anthropic.com/*
// @icon         https://www.google.com/s2/favicons?sz=64&domain=claude.ai
// @grant        GM_registerMenuCommand
// @grant        GM_download
// @grant        GM_addStyle
// @grant        GM_log
// @run-at       document-start
// ==/UserScript==

(function() {
    'use strict';

    // Configuration
    // No hardcoded files - we'll detect them dynamically from the Project Knowledge section
    
    // Debug mode - set to true to enable console logging
    const DEBUG = true;
    
    // Script settings
    const MAX_INIT_ATTEMPTS = 20; // Maximum number of initialization attempts
    const INIT_RETRY_DELAY = 1000; // Delay between initialization attempts (ms)
    let initAttempts = 0; // Counter for initialization attempts
    
    // Settings for automatic scrolling
    const SCROLL_STEP = 300; // pixels to scroll each time
    const SCROLL_DELAY = 500; // milliseconds between scrolls
    const MAX_SCROLL_ATTEMPTS = 10; // maximum number of scroll attempts
    
    // CSS styles for our UI elements
    GM_addStyle(`
        .claude-exporter-btn {
            position: fixed;
            right: 20px;
            bottom: 20px;
            background-color: #5436DA;
            color: white;
            border: none;
            border-radius: 8px;
            padding: 10px 18px;
            font-size: 16px;
            font-weight: bold;
            cursor: pointer;
            z-index: 9999;
            box-shadow: 0 4px 10px rgba(84, 54, 218, 0.3);
            display: flex;
            align-items: center;
            gap: 8px;
            animation: pulse-light 2s infinite;
        }
        @keyframes pulse-light {
            0% { box-shadow: 0 4px 10px rgba(84, 54, 218, 0.3); }
            50% { box-shadow: 0 4px 20px rgba(84, 54, 218, 0.6); }
            100% { box-shadow: 0 4px 10px rgba(84, 54, 218, 0.3); }
        }
        .claude-exporter-btn:hover {
            background-color: #4520c9;
        }
        .claude-exporter-dropdown {
            position: fixed;
            right: 20px;
            bottom: 65px;
            background-color: white;
            border-radius: 4px;
            padding: 10px;
            z-index: 9999;
            box-shadow: 0 2px 10px rgba(0,0,0,0.2);
            display: none;
            flex-direction: column;
            gap: 8px;
            max-height: 300px;
            overflow-y: auto;
            width: 250px;
            color: #333; /* Ensure text is dark for contrast against white background */
        }
        .claude-exporter-option {
            padding: 8px;
            cursor: pointer;
            border-radius: 4px;
            color: #333; /* Ensure text is dark */
        }
        .claude-exporter-option:hover {
            background-color: #f5f5f5;
        }
        .claude-exporter-section {
            border-bottom: 1px solid #eee;
            padding-bottom: 8px;
            margin-bottom: 8px;
            color: #333; /* Ensure text is dark */
        }
        .claude-exporter-section-title {
            font-weight: bold;
            margin-bottom: 5px;
            color: #222; /* Even darker for titles */
        }
        .claude-exporter-checkbox {
            margin-right: 8px;
        }
        .claude-exporter-export-btn {
            background-color: #5436DA;
            color: white; /* White text on purple background */
            border: none;
            border-radius: 4px;
            padding: 8px;
            cursor: pointer;
            margin-top: 8px;
        }
        .claude-exporter-export-btn:hover {
            background-color: #4520c9;
        }
        .claude-exporter-status {
            margin-top: 8px;
            padding: 8px;
            border-radius: 4px;
            display: none;
        }
        .claude-exporter-file-count {
            font-size: 12px;
            color: #666;
            margin-top: 4px;
            padding: 4px;
            text-align: center;
        }
        .claude-exporter-status.success {
            background-color: #e6f7e6;
            color: #2d862d;
            display: block;
        }
        .claude-exporter-status.error {
            background-color: #ffebeb;
            color: #cc0000;
            display: block;
        }
        /* Ensure labels and text are visible */
        .claude-exporter-dropdown label {
            color: #333;
        }
        .claude-exporter-dropdown input[type="checkbox"],
        .claude-exporter-dropdown input[type="radio"] {
            accent-color: #5436DA; /* Match our theme color for inputs */
        }
    `);

    // Add export button to page
    function addExportButton() {
        // Check if button already exists
        if (document.querySelector('.claude-exporter-btn')) {
            return;
        }

        // Create main button
        const button = document.createElement('button');
        button.className = 'claude-exporter-btn';
        button.innerHTML = `
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M12 16L12 8" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                <path d="M9 13L12 16L15 13" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                <path d="M20 16.7428C21.2215 15.734 22 14.2079 22 12.5C22 9.46243 19.5376 7 16.5 7C16.2815 7 16.0771 6.886 15.9661 6.69774C14.6621 4.48484 12.2544 3 9.5 3C5.35786 3 2 6.35786 2 10.5C2 12.5661 2.83545 14.4371 4.18695 15.7935" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                <path d="M8 21H16" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
            Export Project Knowledge
        `;
        
        // Create dropdown
        const dropdown = document.createElement('div');
        dropdown.className = 'claude-exporter-dropdown';
        
        // Individual files section
        const filesSection = document.createElement('div');
        filesSection.className = 'claude-exporter-section';
        filesSection.innerHTML = '<div class="claude-exporter-section-title">Select Files to Export</div>';
        
        // Add "Select All" option
        const selectAllOption = document.createElement('div');
        selectAllOption.className = 'claude-exporter-option';
        selectAllOption.innerHTML = `
            <label>
                <input type="checkbox" id="select-all-files" class="claude-exporter-checkbox" checked>
                <strong>Select All</strong>
            </label>
        `;
        filesSection.appendChild(selectAllOption);
        
        // Add a placeholder message - files will be populated dynamically
        const placeholderMsg = document.createElement('div');
        placeholderMsg.id = 'files-placeholder';
        placeholderMsg.className = 'claude-exporter-option';
        placeholderMsg.textContent = 'Automatically scanning for project knowledge files...';
        filesSection.appendChild(placeholderMsg);
        
        // Handle Select All functionality
        const selectAllCheckbox = selectAllOption.querySelector('#select-all-files');
        selectAllCheckbox.addEventListener('change', (e) => {
            const isChecked = e.target.checked;
            document.querySelectorAll('.claude-exporter-checkbox[data-file]').forEach(checkbox => {
                checkbox.checked = isChecked;
            });
        });
        
        // Export options section
        const optionsSection = document.createElement('div');
        optionsSection.className = 'claude-exporter-section';
        optionsSection.innerHTML = `
            <div class="claude-exporter-section-title">Export Options</div>
            <div class="claude-exporter-option">
                <label>
                    <input type="checkbox" class="claude-exporter-checkbox" id="create-folder" checked>
                    Create cline_docs folder
                </label>
            </div>
            <div class="claude-exporter-option">
                <label>
                    <input type="radio" name="export-type" value="selected" checked>
                    Export selected files
                </label>
            </div>
            <div class="claude-exporter-option">
                <label>
                    <input type="radio" name="export-type" value="all">
                    Export all detected files
                </label>
            </div>
        `;
        
        // Export button
        const exportBtn = document.createElement('button');
        exportBtn.className = 'claude-exporter-export-btn';
        exportBtn.textContent = 'Export Files';
        
        // Status message
        const statusMsg = document.createElement('div');
        statusMsg.className = 'claude-exporter-status';
        
        // File count message
        const fileCountMsg = document.createElement('div');
        fileCountMsg.className = 'claude-exporter-file-count';
        fileCountMsg.id = 'file-count-msg';
        fileCountMsg.textContent = 'Scanning for files...';
        
        // Add everything to the dropdown
        dropdown.appendChild(filesSection);
        dropdown.appendChild(fileCountMsg);
        dropdown.appendChild(optionsSection);
        dropdown.appendChild(exportBtn);
        dropdown.appendChild(statusMsg);
        
        // Add click handlers
        button.addEventListener('click', () => {
            dropdown.style.display = dropdown.style.display === 'flex' ? 'none' : 'flex';
        });
        
        exportBtn.addEventListener('click', () => {
            exportFiles(statusMsg);
        });
        
        // Add elements to page
        document.body.appendChild(button);
        document.body.appendChild(dropdown);
        
        // Close dropdown when clicking outside
        document.addEventListener('click', (event) => {
            if (!button.contains(event.target) && !dropdown.contains(event.target)) {
                dropdown.style.display = 'none';
            }
        });
    }

    // Function to log messages when debug mode is on
    function debugLog(...args) {
        if (DEBUG) {
            console.log("[Claude Exporter]", ...args);
        }
    }
    
    // Function to find and extract knowledge files from the Project Knowledge section
    function findKnowledgeFiles(skipScroll = false) {
        const foundFiles = {};
        
        // Modern Claude interface uses a grid layout with thumbnails
        // First try the modern grid layout with thumbnails
        const modernFileFinder = () => {
            try {
                // Find the project knowledge section - look for various possible selectors
                const knowledgeSection = document.querySelector(
                    // Try various possible selectors for the modern interface
                    'div[aria-label="Project knowledge"], ' +
                    'div[role="region"][aria-label*="knowledge"], ' +
                    'h2:contains("Project knowledge"), ' +
                    'h2.text-text-200, ' + // Modern Claude has this class
                    'h2:-webkit-any-link:contains("Project knowledge")'
                );
                
                if (!knowledgeSection) {
                    debugLog("Modern knowledge section not found");
                    return false;
                }
                
                // Find the container that holds all files (might be a parent/grandparent of the heading)
                let filesContainer = knowledgeSection.closest('div.border-0.5') ||
                                   knowledgeSection.closest('section') ||
                                   knowledgeSection.parentElement ||
                                   knowledgeSection.parentElement?.parentElement;
                
                if (!filesContainer) {
                    debugLog("Files container not found in modern layout");
                    return false;
                }
                
                // Find all thumbnail elements - these are now in a grid layout
                // In modern Claude, these are often buttons with file info inside
                const fileElements = filesContainer.querySelectorAll(
                    // Target the thumbnails containing file info
                    'div[data-testid="file-thumbnail"], ' +
                    'button.rounded-lg, ' + // Thumbnail buttons
                    'div.group\\/thumbnail' // Group containers
                );
                
                debugLog(`Found ${fileElements.length} modern file thumbnails`);
                
                if (fileElements.length === 0) {
                    return false;
                }
                
                // Process each file thumbnail
                fileElements.forEach(element => {
                    try {
                        // Find file name within the thumbnail
                        const fileNameElement = element.querySelector('h3');
                        
                        if (!fileNameElement) return;
                        
                        const fileName = fileNameElement.innerText.trim();
                        
                        // Only process valid filenames (with .txt or .md extension)
                        if (fileName && (fileName.endsWith('.txt') || fileName.endsWith('.md'))) {
                            debugLog("Found modern file:", fileName);
                            
                            // Find the clickable button that contains this thumbnail
                            const clickableElement = element.closest('button') || element;
                            
                            // Try to extract the content when the file is clicked/expanded
                            clickableElement.addEventListener('click', () => {
                                debugLog("Modern file clicked:", fileName);
                                // Wait a bit for content to load
                                setTimeout(() => {
                                    // Modern Claude shows content in a modal with pre/code or prose div
                                    const contentElement = document.querySelector(
                                        'pre code, ' +
                                        'div.prose, ' +
                                        'div[role="dialog"] pre, ' +
                                        'div[role="dialog"] .prose'
                                    );
                                    
                                    if (contentElement) {
                                        const content = contentElement.innerText || contentElement.textContent;
                                        if (content) {
                                            debugLog("Content extracted for modern file:", fileName);
                                            foundFiles[fileName] = content;
                                            updateFilesList(foundFiles);
                                        }
                                    }
                                }, 500);
                            });
                            
                            // For now, just store the filename
                            foundFiles[fileName] = null; // We'll populate this when the user clicks the file
                        }
                    } catch (err) {
                        debugLog("Error processing file thumbnail:", err);
                    }
                });
                
                return fileElements.length > 0;
            } catch (err) {
                debugLog("Error in modern file finder:", err);
                return false;
            }
        };
        
        // Legacy Claude interface uses lists or links
        const legacyFileFinder = () => {
            try {
                // Look for the "Project knowledge" section in Claude
                const knowledgeSections = [...document.querySelectorAll('div[aria-label="Project knowledge"], div[role="region"][aria-label*="knowledge"]')];
                
                if (knowledgeSections.length === 0) {
                    debugLog("Legacy knowledge sections not found");
                    return false;
                }
                
                debugLog("Found legacy knowledge sections:", knowledgeSections.length);
                
                // Process each section (usually just one)
                knowledgeSections.forEach(section => {
                    // Find all the file elements - typically these are list items or links in the section
                    const fileElements = section.querySelectorAll('li, a, button[role="button"], div[role="button"]');
                    
                    debugLog("Found legacy file elements:", fileElements.length);
                    
                    fileElements.forEach(element => {
                        // Get the file name - it's typically the innerText of the element
                        const fileName = element.innerText.trim();
                        
                        // Only process valid filenames (with .txt or .md extension)
                        if (fileName && (fileName.endsWith('.txt') || fileName.endsWith('.md'))) {
                            debugLog("Found legacy file:", fileName);
                            
                            // Try to extract the content when the file is clicked/expanded
                            element.addEventListener('click', () => {
                                debugLog("Legacy file clicked:", fileName);
                                // Wait a bit for content to load
                                setTimeout(() => {
                                    // Find the expanded content - typically appears in a pre/code block or specific div
                                    const contentElement = document.querySelector(`pre code, div[role="region"] div.prose`);
                                    if (contentElement) {
                                        const content = contentElement.innerText || contentElement.textContent;
                                        if (content) {
                                            debugLog("Content extracted for legacy file:", fileName);
                                            foundFiles[fileName] = content;
                                            updateFilesList(foundFiles);
                                        }
                                    }
                                }, 500);
                            });
                            
                            // For now, just store the filename
                            foundFiles[fileName] = null; // We'll populate this when the user clicks the file
                        }
                    });
                });
                
                return true;
            } catch (err) {
                debugLog("Error in legacy file finder:", err);
                return false;
            }
        };
        
        // Try modern layout first, then fall back to legacy
        const foundModernFiles = modernFileFinder();
        if (!foundModernFiles) {
            debugLog("Falling back to legacy file detection");
            legacyFileFinder();
        }
        
        // If we should try scrolling to find more files
        if (!skipScroll) {
            // Try to find a scrollable container for the files
            const knowledgeSections = document.querySelectorAll('div[aria-label="Project knowledge"], div[role="region"][aria-label*="knowledge"]');
            const modernContainer = document.querySelector('div.border-0\\.5.rounded-lg, div.flex.flex-col.gap-5');
            
            const scrollableSection = knowledgeSections.length > 0 ? knowledgeSections[0] :
                                     modernContainer ? modernContainer : null;
            
            // If we found some files and a scrollable section, try scrolling to find more
            if (Object.keys(foundFiles).length > 0 && scrollableSection) {
                debugLog("Found scrollable section for auto-scroll");
                // Start the auto-scroll process to find more files
                autoScrollKnowledgeSection(scrollableSection, foundFiles);
            }
        }
        
        // Update the files list in the UI
        updateFilesList(foundFiles);
        
        return foundFiles;
    }
    
    // Function to automatically scroll through the knowledge section to find all files
    function autoScrollKnowledgeSection(section, foundFiles) {
        debugLog("Starting auto-scroll process to find all files");
        
        // Store the original scroll position
        const originalScrollTop = section.scrollTop;
        let attemptCount = 0;
        let lastFileCount = Object.keys(foundFiles).length;
        
        // Create a function to scroll down incrementally
        const scrollStep = () => {
            // Scroll down by the defined amount
            section.scrollTop += SCROLL_STEP;
            debugLog("Scrolled section to:", section.scrollTop);
            
            // Wait for any new content to load
            setTimeout(() => {
                // Find files at this scroll position
                const newFoundFiles = findKnowledgeFiles(true); // Skip scrolling in this call
                
                // Combine with existing found files
                Object.keys(newFoundFiles).forEach(fileName => {
                    if (!foundFiles[fileName]) {
                        foundFiles[fileName] = newFoundFiles[fileName];
                    }
                });
                
                // Update UI with newly found files
                updateFilesList(foundFiles);
                
                // Check if we found new files in this scroll step
                const currentFileCount = Object.keys(foundFiles).length;
                const foundNewFiles = currentFileCount > lastFileCount;
                lastFileCount = currentFileCount;
                
                // Check if we should continue scrolling
                attemptCount++;
                
                // If we've hit our max attempts OR we're at the bottom (scrollTop hasn't increased)
                const isAtBottom = section.scrollTop + section.clientHeight >= section.scrollHeight;
                
                if (attemptCount < MAX_SCROLL_ATTEMPTS && !isAtBottom) {
                    // Continue scrolling
                    debugLog("Continuing scroll, attempt", attemptCount, "of", MAX_SCROLL_ATTEMPTS);
                    scrollStep();
                } else {
                    // Finished scrolling
                    debugLog("Finished auto-scrolling. Found", Object.keys(foundFiles).length, "files");
                    
                    // Scroll back to the original position
                    section.scrollTop = originalScrollTop;
                    
                    // Update the file count message
                    updateFileCountMessage(foundFiles);
                }
            }, SCROLL_DELAY);
        };
        
        // Start the scrolling process
        scrollStep();
    }
    
    // Function to extract file content when a file is clicked
    function extractFileContent(fileName) {
        debugLog("Attempting to extract content for:", fileName);
        
        // Try multiple extraction strategies for different Claude interfaces
        
        // Modern Claude interface - check for the dialog shown in the screenshot
        // First look for the specific dialog structure shown in the example
        const dialogElement = document.querySelector('div[role="dialog"]');
        if (dialogElement) {
            debugLog("Found modal dialog");
            
            // Look for content in the modal - most likely in the div with whitespace-pre-wrap class
            const codeBlock = dialogElement.querySelector('.whitespace-pre-wrap, pre, .font-mono');
            if (codeBlock) {
                const content = codeBlock.innerText || codeBlock.textContent;
                if (content && content.length > 10) {
                    debugLog("Content found in dialog code block");
                    return content;
                }
            }
            
            // If not found in the code block, try other selectors
            const modalContentSelectors = [
                'div[role="dialog"] pre', // Pre-formatted code block in modal
                'div[role="dialog"] .prose', // Prose container in modal
                'div[role="dialog"] pre code', // Code inside pre in modal
                'div[role="dialog"] .bg-bg-000', // Background element that might contain the content
                'div[role="dialog"] .font-mono' // Monospaced font element
            ];
            
            for (const selector of modalContentSelectors) {
                const element = document.querySelector(selector);
                if (element) {
                    const content = element.innerText || element.textContent;
                    if (content && content.length > 10) {
                        debugLog(`Content found using selector: ${selector}`);
                        return content;
                    }
                }
            }
        }
        
        // Legacy Claude interface - expanded content
        const legacyContentSelectors = [
            '.prose pre', // Pre-formatted block
            '.prose code', // Code block
            'pre code', // Code block in pre
            'div[role="region"] div.prose' // Prose in region
        ];
        
        for (const selector of legacyContentSelectors) {
            const elements = document.querySelectorAll(selector);
            for (const element of elements) {
                const content = element.innerText || element.textContent;
                if (content && content.length > 10) {
                    debugLog(`Content found using legacy selector: ${selector}`);
                    return content;
                }
            }
        }
        
        // Last resort - try getting content from text elements
        const textElements = document.querySelectorAll('.prose p, div[role="dialog"] p');
        let combinedText = '';
        
        for (const element of textElements) {
            combinedText += (element.innerText || element.textContent) + '\n\n';
        }
        
        if (combinedText.length > 10) {
            debugLog("Content found using text elements");
            return combinedText;
        }
        
        debugLog("No content found for:", fileName);
        return null;
    }
    
    // Function to update the file count message
    function updateFileCountMessage(foundFiles) {
        const fileCountMsg = document.getElementById('file-count-msg');
        if (fileCountMsg) {
            const totalFiles = Object.keys(foundFiles).length;
            const loadedFiles = Object.values(foundFiles).filter(content => content !== null).length;
            fileCountMsg.textContent = `Detected ${totalFiles} file(s), ${loadedFiles} with loaded content`;
            debugLog(`File count updated: ${totalFiles} files, ${loadedFiles} with content`);
        }
    }
    
    // Function to update the files list in the UI
    function updateFilesList(foundFiles) {
        const filesSection = document.querySelector('.claude-exporter-section');
        const placeholder = document.getElementById('files-placeholder');
        
        // Remove placeholder if it exists
        if (placeholder) {
            placeholder.remove();
        }
        
        // Remove any existing file checkboxes
        document.querySelectorAll('.claude-exporter-option[data-file]').forEach(el => el.remove());
        
        // Add checkboxes for each found file
        const fileNames = Object.keys(foundFiles);
        
        if (fileNames.length === 0) {
            const noFilesMsg = document.createElement('div');
            noFilesMsg.className = 'claude-exporter-option';
            noFilesMsg.textContent = 'No project knowledge files detected. Automatically scanning...';
            noFilesMsg.style.color = '#333'; // Ensure text is visible
            filesSection.appendChild(noFilesMsg);
            return;
        }
        
        // Update the file count message
        updateFileCountMessage(foundFiles);
        
        fileNames.forEach(fileName => {
            const option = document.createElement('div');
            option.className = 'claude-exporter-option';
            option.setAttribute('data-file', fileName);
            option.innerHTML = `
                <label>
                    <input type="checkbox" class="claude-exporter-checkbox" data-file="${fileName}" checked>
                    ${fileName}${foundFiles[fileName] ? '' : ' (click to load content)'}
                </label>
            `;
            
            // Add click handler for files that don't have content yet
            if (!foundFiles[fileName]) {
                const checkbox = option.querySelector('input[type="checkbox"]');
                checkbox.addEventListener('change', (e) => {
                    if (e.target.checked) {
                        // If they check a file that has no content, try to load it
                        const fileElements = document.querySelectorAll('div[aria-label="Project knowledge"] li, div[aria-label="Project knowledge"] a, div[role="region"][aria-label*="knowledge"] li, div[role="region"][aria-label*="knowledge"] a');
                        
                        fileElements.forEach(el => {
                            if (el.innerText.trim() === fileName) {
                                // Click the file to expand it
                                el.click();
                            }
                        });
                    }
                });
            }
            
            filesSection.appendChild(option);
        });
        
        // Update the Select All checkbox based on current state
        const allChecked = document.querySelectorAll('.claude-exporter-checkbox[data-file]:not(:checked)').length === 0;
        document.getElementById('select-all-files').checked = allChecked && fileNames.length > 0;
    }

    // Function to export files
    function exportFiles(statusElement) {
        // Get selected files
        const selectedFiles = [];
        document.querySelectorAll('.claude-exporter-checkbox[data-file]:checked').forEach(checkbox => {
            selectedFiles.push(checkbox.getAttribute('data-file'));
        });
        
        debugLog("Starting export of selected files:", selectedFiles);
        
        // Get export type
        const exportType = document.querySelector('input[name="export-type"]:checked').value;
        
        // Get create folder option
        const createFolder = document.getElementById('create-folder').checked;
        
        // Get knowledge files
        const foundFiles = {};
        
        // For each selected file, if we don't have content, try to get it
        const promises = selectedFiles.map(fileName => {
            return new Promise(resolve => {
                // Check if we already have content
                if (window.knowledgeFileContents && window.knowledgeFileContents[fileName]) {
                    foundFiles[fileName] = window.knowledgeFileContents[fileName];
                    resolve();
                    return;
                }
                
                // Find and click the file element to expand it
                // Try both modern and legacy selectors
                const fileElements = document.querySelectorAll(`
                    div[aria-label="Project knowledge"] li,
                    div[aria-label="Project knowledge"] a,
                    div[role="region"][aria-label*="knowledge"] li,
                    div[role="region"][aria-label*="knowledge"] a,
                    button[role="button"],
                    div[data-testid="file-thumbnail"],
                    button.rounded-lg,
                    div.group\\/thumbnail
                `);
                
                let foundElement = false;
                fileElements.forEach(el => {
                    if (el.innerText.trim() === fileName) {
                        // Click the file to expand it
                        el.click();
                        foundElement = true;
                        
                        // Wait for content to load
                        setTimeout(() => {
                            const content = extractFileContent(fileName);
                            if (content) {
                                foundFiles[fileName] = content;
                                // Save for future use
                                if (!window.knowledgeFileContents) window.knowledgeFileContents = {};
                                window.knowledgeFileContents[fileName] = content;
                            }
                            resolve();
                        }, 500);
                    }
                });
                
                if (!foundElement) {
                    resolve(); // Couldn't find the element, move on
                }
            });
        });
        
        // Once all content is collected, export the files
        Promise.all(promises).then(() => {
            // Determine which files to export
            const filesToExport = exportType === 'selected' ?
                selectedFiles.filter(file => foundFiles[file]) :
                Object.keys(foundFiles);
            
            if (filesToExport.length === 0) {
                statusElement.textContent = 'No files found to export. Try clicking on each file first to load its content.';
                statusElement.className = 'claude-exporter-status error';
                debugLog("No files to export - missing content");
                return;
            }
            
            debugLog(`Exporting ${filesToExport.length} files`);
            
            // Export each file
            let exportCount = 0;
            filesToExport.forEach(fileName => {
                const content = foundFiles[fileName];
                if (!content) return;
                
                const filePath = createFolder ? `cline_docs/${fileName}` : fileName;
                const mimeType = fileName.endsWith('.md') ? 'text/markdown' : 'text/plain';
                
                try {
                    GM_download({
                        url: `data:${mimeType};charset=utf-8,` + encodeURIComponent(content),
                        name: filePath,
                        saveAs: false,
                        onload: function() {
                            exportCount++;
                            if (exportCount === filesToExport.length) {
                                statusElement.textContent = `Successfully exported ${exportCount} file(s).`;
                                statusElement.className = 'claude-exporter-status success';
                                debugLog(`Successfully exported ${exportCount} files`);
                            }
                        },
                        onerror: function(error) {
                            console.error('Failed to download file:', fileName, error);
                            statusElement.textContent = `Error exporting ${fileName}: ${error.error}`;
                            statusElement.className = 'claude-exporter-status error';
                            debugLog("Error exporting file:", fileName, error);
                        }
                    });
                } catch (error) {
                    console.error('Error during export:', error);
                    statusElement.textContent = `Error during export: ${error.message}`;
                    statusElement.className = 'claude-exporter-status error';
                }
            });
        });
    }

    // Advanced extraction function for more complex files
    function extractFileSections(text, fileName) {
        const baseName = fileName.replace('.md', '');
        
        // Look for headline patterns that match our file name
        const headingRegex = new RegExp(`^\\s*(#+)\\s*(${baseName}|${baseName.charAt(0).toUpperCase() + baseName.slice(1)})\\s*$`, 'im');
        const match = text.match(headingRegex);
        
        if (!match) return null;
        
        // Found a matching heading, now extract all content that belongs to this section
        const headingLevel = match[1].length; // Number of # characters
        const sectionStart = match.index;
        
        // Find the end of this section by looking for the next heading of same or higher level
        const nextHeadingRegex = new RegExp(`^\\s*#{1,${headingLevel}}\\s+`, 'gm');
        nextHeadingRegex.lastIndex = sectionStart + match[0].length;
        
        const nextMatch = nextHeadingRegex.exec(text);
        const sectionEnd = nextMatch ? nextMatch.index : text.length;
        
        // Extract the section content
        return text.substring(sectionStart, sectionEnd).trim();
    }
    
    // Function to scan for files periodically
    function scanForFiles() {
        // Store results globally
        window.knowledgeFileContents = window.knowledgeFileContents || {};
        
        debugLog("Starting scan for files");
        
        // Scan for files
        const foundFiles = findKnowledgeFiles();
        
        // Update the UI with found files
        updateFilesList(foundFiles);
        
        // Save found file contents
        Object.keys(foundFiles).forEach(fileName => {
            if (foundFiles[fileName]) {
                window.knowledgeFileContents[fileName] = foundFiles[fileName];
            }
        });
        
        // Start automatic content extraction if needed
        setTimeout(() => {
            autoExtractAllFileContents();
        }, 1000);
    }
    
    // Function to automatically click each file, extract content, and close dialog
    async function autoExtractAllFileContents() {
        debugLog("Starting automatic content extraction process");
        
        // Get all file thumbnails in the modern grid layout
        const fileThumbnails = document.querySelectorAll(`
            button.rounded-lg,
            div[data-testid="file-thumbnail"] button,
            div.group\\/thumbnail button,
            [data-testid="file-thumbnail"],
            div.font-styrene.transition-all,
            ul.grid button
        `);
        
        if (fileThumbnails.length === 0) {
            debugLog("No file thumbnails found for automatic extraction");
            return;
        }
        
        debugLog(`Found ${fileThumbnails.length} file thumbnails to process`);
        window.knowledgeFileContents = window.knowledgeFileContents || {};
        const extractedFiles = {};
        
        // Process each file one by one
        for (let i = 0; i < fileThumbnails.length; i++) {
            const thumbnail = fileThumbnails[i];
            
            try {
                // Get file name from thumbnail
                const fileNameElement = thumbnail.querySelector('h3');
                if (!fileNameElement) continue;
                
                const fileName = fileNameElement.innerText.trim();
                if (!fileName || !(fileName.endsWith('.txt') || fileName.endsWith('.md'))) continue;
                
                // Skip if we already have content for this file
                if (window.knowledgeFileContents[fileName] && window.knowledgeFileContents[fileName].length > 10) {
                    debugLog(`Already have content for ${fileName}, skipping`);
                    extractedFiles[fileName] = window.knowledgeFileContents[fileName];
                    continue;
                }
                
                debugLog(`Processing file ${i+1}/${fileThumbnails.length}: ${fileName}`);
                
                // Click the thumbnail to open the dialog
                thumbnail.click();
                
                // Wait for dialog to appear
                await new Promise(resolve => setTimeout(resolve, 500));
                
                // Find the dialog
                const dialog = document.querySelector('div[role="dialog"]');
                if (!dialog) {
                    debugLog(`No dialog found for ${fileName}, skipping`);
                    continue;
                }
                
                // Extract content
                const contentElement =
                    dialog.querySelector('.whitespace-pre-wrap') ||
                    dialog.querySelector('.font-mono') ||
                    dialog.querySelector('pre') ||
                    dialog.querySelector('.bg-bg-000') ||
                    dialog.querySelector('.prose');
                
                if (contentElement) {
                    const content = contentElement.innerText || contentElement.textContent;
                    if (content && content.length > 10) {
                        debugLog(`Extracted content for ${fileName} (${content.length} chars)`);
                        window.knowledgeFileContents[fileName] = content;
                        extractedFiles[fileName] = content;
                    }
                }
                
                // Find and click close button
                const closeButton = dialog.querySelector('button svg[width="20"], button svg[width="12"]');
                if (closeButton) {
                    const closeButtonElement = closeButton.closest('button');
                    if (closeButtonElement) {
                        closeButtonElement.click();
                        debugLog(`Closed dialog for ${fileName}`);
                    }
                }
                
                // Wait before processing next file
                await new Promise(resolve => setTimeout(resolve, 300));
                
            } catch (err) {
                debugLog(`Error processing thumbnail ${i}:`, err);
            }
        }
        
        // Update UI with extracted files
        debugLog(`Automatic extraction complete. Extracted ${Object.keys(extractedFiles).length} files.`);
        updateFilesList(window.knowledgeFileContents);
    }
    
    // Function to add auto-extract button
    function addAutoExtractButton() {
        if (!document.getElementById('auto-extract-button') && document.querySelector('.claude-exporter-btn')) {
            const autoExtractBtn = document.createElement('button');
            autoExtractBtn.id = 'auto-extract-button';
            autoExtractBtn.className = 'claude-exporter-btn';
            autoExtractBtn.style.bottom = '70px';
            autoExtractBtn.style.backgroundColor = '#007bff';
            autoExtractBtn.style.color = 'white';
            autoExtractBtn.innerHTML = `
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
                  <path d="M.5 9.9a.5.5 0 0 1 .5.5v2.5a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1v-2.5a.5.5 0 0 1 1 0v2.5a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2v-2.5a.5.5 0 0 1 .5-.5z"/>
                  <path d="M7.646 11.854a.5.5 0 0 0 .708 0l3-3a.5.5 0 0 0-.708-.708L8.5 10.293V1.5a.5.5 0 0 0-1 0v8.793L5.354 8.146a.5.5 0 1 0-.708.708l3 3z"/>
                </svg>
                Extract Files
            `;
            
            autoExtractBtn.addEventListener('click', () => {
                debugLog("Auto-extract button clicked");
                autoExtractAllFileContents();
            });
            
            document.body.appendChild(autoExtractBtn);
            debugLog("Added auto-extract button");
        }
    }

    // Function to register menu command
    function registerMenu() {
        GM_registerMenuCommand("Export Project Files", () => {
            // Find and show our dropdown if it exists
            const dropdown = document.querySelector('.claude-exporter-dropdown');
            if (dropdown) {
                dropdown.style.display = 'flex';
            } else {
                // Or create the button/dropdown if they don't exist yet
                addExportButton();
                document.querySelector('.claude-exporter-dropdown').style.display = 'flex';
            }
        });
    }

    // Enhanced detection of URLs to verify we're on Claude
    function isClaudeConversation() {
        // More comprehensive URL patterns for Claude
        const urlPatterns = [
            'claude.ai/chat',
            'claude.ai/conversation',
            'claude.ai/new',
            'claude.ai/console',
            'claude.anthropic.com',
            '.anthropic.com/claude',
            'claude.ai'  // Include base URL since Claude changed interface
        ];
        
        // Check if any pattern matches the current URL
        for (const pattern of urlPatterns) {
            if (window.location.href.includes(pattern)) {
                debugLog(`Claude interface detected via URL pattern: ${pattern}`);
                return true;
            }
        }
        
        // If URL check fails, try to detect by DOM structure
        // Look for elements that are likely to be present in Claude's interface
        const claudeUIIndicators = [
            // Check for specific UI identifiers by query selector
            () => !!document.querySelector('[aria-label*="claude"]'),
            () => !!document.querySelector('[aria-label*="knowledge"]'),
            () => !!document.querySelector('[data-testid*="claude"]'),
            () => !!document.querySelector('[class*="claude"]'),
            // Modern Claude UI elements
            () => !!document.querySelector('[data-testid="project-doc-upload"]'),
            () => !!document.querySelector('h2.text-text-200'), // Project knowledge heading in modern UI
            () => !!document.querySelector('div[data-testid="file-thumbnail"]'),
            // Check for Claude's logo or name in the page
            () => document.body && document.body.innerHTML.includes('Claude'),
            () => document.body && document.body.innerHTML.includes('Anthropic'),
            // Check for project knowledge mentions
            () => document.body && document.body.innerHTML.includes('Project knowledge'),
            // Check for file type indicators in modern Claude
            () => !!document.querySelector('.uppercase.truncate.font-styrene')
        ];
        
        // Try each detection method
        for (let i = 0; i < claudeUIIndicators.length; i++) {
            try {
                if (claudeUIIndicators[i]()) {
                    debugLog(`Claude interface detected via DOM indicator method #${i+1}`);
                    return true;
                }
            } catch (e) {
                // Skip if this detector fails
            }
        }
        
        debugLog("Claude interface not detected");
        return false;
    }

    // Try to find a suitable container to append our button to
    function findButtonContainer() {
        // Possible containers in priority order
        const possibleContainers = [
            // Try specific named containers first
            () => document.querySelector('div[aria-label="Project knowledge"]')?.parentElement,
            () => document.querySelector('div[role="region"][aria-label*="knowledge"]')?.parentElement,
            
            // Try general content areas that might work
            () => document.querySelector('main'),
            () => document.querySelector('[role="main"]'),
            () => document.querySelector('.chat-container'),
            () => document.querySelector('[class*="conversation"]'),
            () => document.querySelector('[class*="chat"]'),
            
            // Fallbacks
            () => document.body,
            () => document.documentElement
        ];
        
        // Try each container strategy
        for (let i = 0; i < possibleContainers.length; i++) {
            try {
                const container = possibleContainers[i]();
                if (container) {
                    debugLog(`Found suitable button container using method #${i+1}`);
                    return container;
                }
            } catch (e) {
                // Skip if this strategy fails
            }
        }
        
        debugLog("Could not find suitable container, defaulting to document.body");
        return document.body;
    }

    // Main initialization with retry mechanism
    function init() {
        initAttempts++;
        
        // Check if we're on a Claude conversation page
        if (!isClaudeConversation()) {
            if (initAttempts <= MAX_INIT_ATTEMPTS) {
                debugLog(`Not detected as a Claude conversation, will retry (attempt ${initAttempts}/${MAX_INIT_ATTEMPTS})...`);
                setTimeout(init, INIT_RETRY_DELAY);
            } else {
                debugLog("Max attempts reached. If this is a Claude page, try refreshing");
            }
            return;
        }
        
        debugLog(`Initializing Claude Project Knowledge Exporter v1.3 (attempt ${initAttempts}/${MAX_INIT_ATTEMPTS})`);
        
        // Try to add the export button
        const buttonAdded = addExportButtonWithRetry();
        
        if (!buttonAdded && initAttempts <= MAX_INIT_ATTEMPTS) {
            debugLog(`Could not add button on attempt ${initAttempts}, will retry...`);
            setTimeout(init, INIT_RETRY_DELAY);
            return;
        }
        
        // Register the menu command
        registerMenu();
        
        // Initial scan for files with a slight delay to let the page load
        setTimeout(scanForFiles, 1500);
        
        // Set up periodic scanning for files with longer interval to avoid overwhelming the UI
        const scanInterval = setInterval(scanForFiles, 15000);
        
        // Add auto-extract button with a slight delay
        setTimeout(() => {
            addAutoExtractButton();
        }, 2000);
        
        // Add mutation observer to detect when new chat messages appear or DOM changes
        const observer = new MutationObserver((mutations) => {
            let needsButtonReinsertion = false;
            
            for (const mutation of mutations) {
                // Check if relevant nodes were added
                if (mutation.addedNodes.length) {
                    needsButtonReinsertion = true;
                    break;
                }
                
                // Also check for attribute changes on project knowledge sections
                if (mutation.type === 'attributes' &&
                   (mutation.target.getAttribute('aria-label') === 'Project knowledge' ||
                    mutation.target.getAttribute('role') === 'region')) {
                    needsButtonReinsertion = true;
                    break;
                }
            }
            
            // Check if our button needs to be added/re-added
            if (needsButtonReinsertion && !document.querySelector('.claude-exporter-btn')) {
                debugLog("DOM changed significantly, attempting to re-add export button");
                addExportButtonWithRetry();
            }
        });
        
        // Start observing with a more comprehensive configuration
        observer.observe(document.body, {
            childList: true,
            subtree: true,
            attributes: true,
            attributeFilter: ['aria-label', 'role', 'class', 'id']
        });
        
        debugLog("Initialization complete");
    }
    
    // Try multiple strategies to add the export button with retries
    function addExportButtonWithRetry() {
        // Check if button already exists
        if (document.querySelector('.claude-exporter-btn')) {
            debugLog("Export button already exists");
            return true;
        }
        
        // Try appending to a suitable container
        try {
            const container = findButtonContainer();
            
            if (container) {
                // Create the button
                const button = document.createElement('button');
                button.className = 'claude-exporter-btn';
                button.innerHTML = `
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path d="M12 16L12 8" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                        <path d="M9 13L12 16L15 13" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                        <path d="M20 16.7428C21.2215 15.734 22 14.2079 22 12.5C22 9.46243 19.5376 7 16.5 7C16.2815 7 16.0771 6.886 15.9661 6.69774C14.6621 4.48484 12.2544 3 9.5 3C5.35786 3 2 6.35786 2 10.5C2 12.5661 2.83545 14.4371 4.18695 15.7935" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                        <path d="M8 21H16" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                    </svg>
                    Export Project Knowledge
                `;
                
                // Create the dropdown (reusing addExportButton function's dropdown creation)
                const dropdown = document.createElement('div');
                dropdown.className = 'claude-exporter-dropdown';
                
                // Add the rest of the dropdown content (abbreviated here)
                // ... (rest of dropdown creation code from addExportButton function)
                
                // Individual files section
                const filesSection = document.createElement('div');
                filesSection.className = 'claude-exporter-section';
                filesSection.innerHTML = '<div class="claude-exporter-section-title">Select Files to Export</div>';
                
                // Add "Select All" option
                const selectAllOption = document.createElement('div');
                selectAllOption.className = 'claude-exporter-option';
                selectAllOption.innerHTML = `
                    <label>
                        <input type="checkbox" id="select-all-files" class="claude-exporter-checkbox" checked>
                        <strong>Select All</strong>
                    </label>
                `;
                filesSection.appendChild(selectAllOption);
                
                // Add a placeholder message - files will be populated dynamically
                const placeholderMsg = document.createElement('div');
                placeholderMsg.id = 'files-placeholder';
                placeholderMsg.className = 'claude-exporter-option';
                placeholderMsg.textContent = 'Automatically scanning for project knowledge files...';
                filesSection.appendChild(placeholderMsg);
                
                // Handle Select All functionality
                const selectAllCheckbox = selectAllOption.querySelector('#select-all-files');
                selectAllCheckbox.addEventListener('change', (e) => {
                    const isChecked = e.target.checked;
                    document.querySelectorAll('.claude-exporter-checkbox[data-file]').forEach(checkbox => {
                        checkbox.checked = isChecked;
                    });
                });
                
                // Export options section
                const optionsSection = document.createElement('div');
                optionsSection.className = 'claude-exporter-section';
                optionsSection.innerHTML = `
                    <div class="claude-exporter-section-title">Export Options</div>
                    <div class="claude-exporter-option">
                        <label>
                            <input type="checkbox" class="claude-exporter-checkbox" id="create-folder" checked>
                            Create cline_docs folder
                        </label>
                    </div>
                    <div class="claude-exporter-option">
                        <label>
                            <input type="radio" name="export-type" value="selected" checked>
                            Export selected files
                        </label>
                    </div>
                    <div class="claude-exporter-option">
                        <label>
                            <input type="radio" name="export-type" value="all">
                            Export all detected files
                        </label>
                    </div>
                `;
                
                // Export button
                const exportBtn = document.createElement('button');
                exportBtn.className = 'claude-exporter-export-btn';
                exportBtn.textContent = 'Export Files';
                
                // Status message
                const statusMsg = document.createElement('div');
                statusMsg.className = 'claude-exporter-status';
                
                // File count message
                const fileCountMsg = document.createElement('div');
                fileCountMsg.className = 'claude-exporter-file-count';
                fileCountMsg.id = 'file-count-msg';
                fileCountMsg.textContent = 'Scanning for files...';
                
                // Add everything to the dropdown
                dropdown.appendChild(filesSection);
                dropdown.appendChild(fileCountMsg);
                dropdown.appendChild(optionsSection);
                dropdown.appendChild(exportBtn);
                dropdown.appendChild(statusMsg);
                
                // Add click handlers
                button.addEventListener('click', () => {
                    dropdown.style.display = dropdown.style.display === 'flex' ? 'none' : 'flex';
                });
                
                exportBtn.addEventListener('click', () => {
                    exportFiles(statusMsg);
                });
                
                // Add elements to page
                container.appendChild(button);
                container.appendChild(dropdown);
                
                // Close dropdown when clicking outside
                document.addEventListener('click', (event) => {
                    if (!button.contains(event.target) && !dropdown.contains(event.target)) {
                        dropdown.style.display = 'none';
                    }
                });
                
                debugLog("Successfully added export button to", container);
                return true;
            }
        } catch (error) {
            debugLog("Error adding export button:", error);
        }
        
        return false;
    }

    // Enhanced initialization to catch the page at various loading stages
    function initializeScript() {
        debugLog("Script starting initialization");
        
        // Try to initialize immediately
        setTimeout(init, 500);
        
        // Also try when DOM is initially loaded
        document.addEventListener('DOMContentLoaded', () => {
            debugLog("DOMContentLoaded event fired");
            setTimeout(init, 500);
        });
        
        // Also try when page is fully loaded
        window.addEventListener('load', () => {
            debugLog("Window load event fired");
            setTimeout(init, 1000);
        });
        
        // Additional check for Single Page Applications that might load content later
        // This helps with apps that load content dynamically after the initial page load
        setTimeout(() => {
            debugLog("Delayed initialization check");
            init();
        }, 3000);
    }
    
    // Start the initialization process
    initializeScript();
    
    // Add a global click handler to detect when a file is clicked and extract its content
    document.addEventListener('click', (event) => {
        // Wait a bit for content to appear
        setTimeout(() => {
            const clickedElement = event.target;
            
            // Check if we're in the Project Knowledge section or a dialog - try both modern and legacy selectors
            const isInKnowledgeSection =
                clickedElement.closest('div[aria-label="Project knowledge"]') ||
                clickedElement.closest('div[role="region"][aria-label*="knowledge"]') ||
                clickedElement.closest('div.border-0\\.5.rounded-lg') || // Modern container
                clickedElement.closest('ul.grid') ||  // Modern grid layout
                clickedElement.closest('div[role="dialog"]'); // File content dialog
                
            if (isInKnowledgeSection) {
                // Find file name - check different possibilities based on modern or legacy UI
                let fileName = null;
                
                // Modern UI: check if there's an h3 in the clicked area (thumbnail)
                const h3Element = clickedElement.querySelector('h3') ||
                                 clickedElement.closest('button')?.querySelector('h3') ||
                                 clickedElement.closest('div[data-testid="file-thumbnail"]')?.querySelector('h3');
                
                if (h3Element) {
                    fileName = h3Element.innerText.trim();
                } else {
                    // Legacy UI: use the text from the clicked element itself
                    fileName = clickedElement.innerText.trim();
                }
                
                // If it looks like a file (ends with .txt or .md)
                if (fileName && (fileName.endsWith('.txt') || fileName.endsWith('.md'))) {
                    debugLog("Global click handler detected file click:", fileName);
                    // Try to extract content
                    setTimeout(() => {
                        // For modern Claude, check for content in the dialog with specific selectors
                        const dialog = document.querySelector('div[role="dialog"]');
                        let content = null;
                        
                        if (dialog) {
                            debugLog("Found dialog, looking for content");
                            // Try multiple selectors for the content inside the dialog
                            const contentElement =
                                dialog.querySelector('.whitespace-pre-wrap') ||
                                dialog.querySelector('.font-mono') ||
                                dialog.querySelector('pre') ||
                                dialog.querySelector('.bg-bg-000') ||
                                dialog.querySelector('.prose');
                            
                            if (contentElement) {
                                content = contentElement.innerText || contentElement.textContent;
                                debugLog("Content found in modern dialog:", content.substring(0, 50) + "...");
                            }
                        }
                        
                        // If not found in dialog, try with standard content selectors
                        if (!content) {
                            const modalContent = document.querySelector('div[role="dialog"] pre') ||
                                              document.querySelector('div[role="dialog"] .prose');
                                       
                            if (modalContent) {
                                content = modalContent.innerText || modalContent.textContent;
                                debugLog("Content found in standard modal dialog");
                            }
                        } else {
                            content = extractFileContent(fileName);
                            debugLog("Content extracted via legacy method");
                        }
                    
                        if (content) {
                            debugLog("Content extracted via global handler for:", fileName);
                            if (!window.knowledgeFileContents) window.knowledgeFileContents = {};
                            window.knowledgeFileContents[fileName] = content;
                            
                            // Update UI
                            updateFilesList(window.knowledgeFileContents);
                        } else {
                            debugLog("No content found for file:", fileName);
                        }
                    }, 500);
                }
            }
        }, 100);
    });
})();