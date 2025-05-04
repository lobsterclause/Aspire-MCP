# Setting Python MCP Credentials

To connect the Python MCP server (`AspireMCP_Python`) to the Aspire API, you need to set the following environment variables with your Aspire API credentials:

*   `ASPIRE_CLIENT_ID`
*   `ASPIRE_CLIENT_SECRET`
*   `ASPIRE_SUBSCRIPTION_KEY`

Replace `"your_value"` in the examples below with your actual credentials obtained from Aspire.

**Important:** Do **NOT** paste your actual secret values back into the chat.

## Instructions for Common Shells

### Bash / Zsh

Open your terminal and run the following commands:

```bash
export ASPIRE_CLIENT_ID="d50f2e74-ab5b-4142-af7a-8c3effb3e893"
export ASPIRE_CLIENT_SECRET="htJoXsmjkV07Jsb1fdioaccGg7cWG1Hy"
export ASPIRE_SUBSCRIPTION_KEY="htJoXsmjkY07Jsb1fdioaccGg7cWG1Hy"
```

To make these variables persistent across new terminal sessions, add these lines to your shell's profile file (e.g., `~/.zshrc`, `~/.bash_profile`, or `~/.bashrc`) and then restart your terminal or run `source ~/.your_profile_file`.

### PowerShell

Open PowerShell and run the following commands:

```powershell
$env:ASPIRE_CLIENT_ID="your_client_id"
$env:ASPIRE_CLIENT_SECRET="your_client_secret"
$env:ASPIRE_SUBSCRIPTION_KEY="your_subscription_key"
```

To make these variables persistent, you can add these lines to your PowerShell profile script. You can find the path to your profile by typing `$PROFILE` in PowerShell.

### Command Prompt (CMD)

Open Command Prompt and run the following commands:

```cmd
set ASPIRE_CLIENT_ID=your_client_id
set ASPIRE_CLIENT_SECRET=your_client_secret
set ASPIRE_SUBSCRIPTION_KEY=your_subscription_key
```

Note that variables set with `set` in CMD are only valid for the current session. For persistent variables, you would typically use the system's environment variable settings (System Properties -> Advanced -> Environment Variables).

## Verification

After setting the variables, you can verify they are set correctly in your current terminal session by echoing them:

### Bash / Zsh
```bash
echo $ASPIRE_CLIENT_ID
echo $ASPIRE_CLIENT_SECRET
echo $ASPIRE_SUBSCRIPTION_KEY
```

### PowerShell
```powershell
echo $env:ASPIRE_CLIENT_ID
echo $env:ASPIRE_CLIENT_SECRET
echo $env:ASPIRE_SUBSCRIPTION_KEY
```

### Command Prompt (CMD)
```cmd
echo %ASPIRE_CLIENT_ID%
echo %ASPIRE_CLIENT_SECRET%
echo %ASPIRE_SUBSCRIPTION_KEY%