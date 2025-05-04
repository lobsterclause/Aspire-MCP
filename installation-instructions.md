# Claude Project Knowledge Exporter - Installation Instructions

## Overview

This Tampermonkey script allows you to extract and export files from Claude's "Project knowledge" section. It supports both `.txt` and `.md` file formats and adds convenient buttons to Claude's interface for extracting and exporting files. The script automatically scans and detects all project files, even those requiring scrolling to become visible. Version 1.4 includes automatic file content extraction and significantly improved compatibility with Claude's modern interface.

## Installation

1. **Install Tampermonkey Extension**:
   - For Chrome: [Chrome Web Store - Tampermonkey](https://chrome.google.com/webstore/detail/tampermonkey/dhdgffkkebhmkfjojejmpbldmpobfkfo)
   - For Firefox: [Firefox Add-ons - Tampermonkey](https://addons.mozilla.org/en-US/firefox/addon/tampermonkey/)
   - For Edge: [Microsoft Edge Add-ons - Tampermonkey](https://microsoftedge.microsoft.com/addons/detail/tampermonkey/iikmkjmpaadaobahmlepeloendndfphd)
   - For Safari: [Mac App Store - Tampermonkey](https://apps.apple.com/app/tampermonkey/id1482490089)
   
   After installation, you should see the Tampermonkey icon in your browser toolbar. If you don't see it, check your extensions menu and make sure it's enabled.

2. **Install the Script**:
   - Click on the Tampermonkey icon in your browser toolbar
   - Select "Create a new script"
   - Delete any default code
   - Copy and paste the entire contents of `claude-project-exporter.user.js` into the editor
   - Press Ctrl+S or click File > Save to save the script

   Alternatively:
   - Click on the Tampermonkey icon in your browser toolbar
   - Select "Dashboard"
   - Click the "+" tab to create a new script
   - Delete any default code
   - Copy and paste the entire contents of `claude-project-exporter.user.js` into the editor
   - Press Ctrl+S or click File > Save to save the script
   - Make sure the script shows as "Enabled" in the Tampermonkey dashboard

3. **Verify Installation**:
   - After installing the script, check the Tampermonkey icon in your browser toolbar
   - Click the icon and verify "Claude Project Knowledge Exporter" appears in the menu list
   - If it doesn't appear, try reinstalling the script using the alternative method above

## Usage

1. **Navigate to Claude AI**:
   - Go to [Claude AI](https://claude.ai) and sign in to your account
   - Open an existing conversation or start a new one

2. **Using the Extraction Buttons**:
   - Once the script is installed and activated, you'll see **two** buttons in the bottom right corner of the Claude interface:
     1. An "Extract Files" button (blue, with a download icon)
     2. An "Export Project Knowledge" button (default color, with a cloud icon)
   - If you don't see the buttons immediately, try refreshing the page or wait a few seconds; the script includes automatic retry mechanisms
   - The blue "Extract Files" button will automatically click through each file in your Project Knowledge section, open the file dialog, extract the content, and close the dialog
   - The "Export Project Knowledge" button opens the export options panel
   - The panel will show how many files were detected and how many have loaded content

3. **Export Options**:
    - Use the "Select All" option to quickly select or deselect all files
    - Select which files you want to export (the script automatically detects all files in the Project knowledge section)
    - Choose whether to create a cline_docs folder structure
    - Choose to export only selected files or all detected files
    - Click "Export Files" to download the selected files
    
    Note: The script now includes an automatic extraction feature that will click through each file, open it, extract the content, and close it. You can also click the blue "Extract Files" button at any time to manually trigger this process if needed.

4. **Alternative Access**:
   - You can also access the export feature by clicking the Tampermonkey icon in your browser
   - Select "Claude Project Knowledge Exporter" > "Export Project Files" from the menu

## How It Works

The script automatically scans Claude's interface for the "Project knowledge" section and identifies all files listed there, including those that require scrolling to become visible. It supports:

- Both `.txt` and `.md` file formats
- Automatic scrolling to detect all files in the Project knowledge section, even those "below the fold"
- Dynamic extraction of file content when files are clicked
- Bulk export of multiple files with the "Select All" option
- Status indicators showing how many files were detected and how many have content loaded

When you click Export, the script extracts the relevant content and downloads it as properly formatted Markdown files.

## Troubleshooting

### Buttons Not Visible

If you don't see the "Extract Files" or "Export Project Knowledge" buttons in Claude's interface:

1. **Verify Script Installation**:
   - Open the Tampermonkey dashboard (click the Tampermonkey icon > Dashboard)
   - Confirm the script is listed and enabled (toggle should be green/enabled)
   - If not enabled, click the toggle to enable it
   
2. **Check Script Running Status**:
   - Open browser developer tools (F12 or right-click > Inspect)
   - Go to the Console tab
   - Look for messages starting with "[Claude Exporter]" which indicate the script is running
   - If you don't see these messages, the script might not be detecting Claude's interface correctly
   
3. **Try Multiple Solutions**:
   - **Refresh the page**: Sometimes a simple refresh will fix initialization issues
   - **Hard refresh**: Try Ctrl+F5 (Windows/Linux) or Cmd+Shift+R (Mac) for a cache-clearing refresh
   - **Navigate to a different conversation**: Try opening a new or different Claude conversation
   - **Check URL compatibility**: Ensure you're on a supported Claude URL (claude.ai or anthropic.com domain)
   - **Restart browser**: Close and reopen your browser entirely to reset extensions
   
4. **Manual Activation**:
   - Click on the Tampermonkey icon in your toolbar
   - Select "Claude Project Knowledge Exporter" > "Export Project Files" to manually trigger the UI

### Other Common Issues

- **Files Not Detected**: The script automatically scrolls to find all files. If files are still missing, try refreshing the page and waiting a few seconds for the auto-scan to complete.
- **Files With No Content**: Click on each file in the Project knowledge section to load its content before exporting. The file count indicator will show how many files have loaded content.
- **Download Issues**: Check your browser's download settings and ensure Tampermonkey has permission to download files
- **Console Logs**: If you're experiencing issues, open your browser console (F12 > Console) to see detailed debug information about what the script is detecting. The script has comprehensive logging to help diagnose issues.

## Support

If you encounter any issues or have questions about the script, you can:
- Check the Tampermonkey documentation for general userscript troubleshooting
- Examine the browser console (F12 > Console) for debug logs and error messages
- Modify the script to fit your specific needs

### Advanced Troubleshooting

If the buttons still don't appear after trying the above steps:

1. **Script Initialization**: The script implements multiple detection methods and retry mechanisms for Claude's modern interface. Check the console logs (F12 > Console) for messages like "Starting initialization" or "DOM changed significantly" that indicate the script is running.

2. **DOM Detection**: The script looks for specific elements in Claude's interface with multiple fallback strategies:
   - Checks for Claude-specific class names and attributes
   - Looks for the specific Project Knowledge section
   - Tries to detect the modern grid layout for files
   - Searches for dialog elements when files are opened
   
   Look for console messages that might indicate detection issues like "Claude interface not detected" or "Could not find suitable container".
   
3. **Manual Testing**: To test if the script can interact with Claude's files:
   - Open Claude and navigate to a conversation with project files
   - Open browser developer tools (F12)
   - Go to Console tab and type: `document.querySelector('div[data-testid="file-thumbnail"]')`
   - If it returns `null`, Claude's file elements may have changed their structure

4. **Manual Verification**: To verify if the script is running at all:
   - Open Chrome/Firefox developer tools (F12)
   - Go to the Console tab
   - Check for any messages starting with "[Claude Exporter]"
   - If you see these messages, the script is running but may be having trouble with UI detection
   - If you don't see any messages, try adding this temporary line at the top of the script:
     ```javascript
     alert('Tampermonkey script loaded');
     ```
     If you don't see this alert, there may be a problem with script loading

5. **Permissions**: Ensure Tampermonkey has the necessary permissions:
   - The script needs to interact with the page DOM
   - It needs download permissions for exporting files
   - It needs to run on Claude's domain (claude.ai and related domains)
   - Check Tampermonkey's settings in your extension manager

## License

This script is provided as-is for personal use.