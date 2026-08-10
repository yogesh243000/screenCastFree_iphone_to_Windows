<#
Sets up the Python virtual environment used to drive pymobiledevice3
(device discovery, pairing, and eventually the screen-capture stream).

Requires Python 3.12 specifically - pymobiledevice3's C-extension dependencies
(lzfse/pylzss) do not currently ship prebuilt wheels for very new CPython
versions (e.g. 3.14), which forces a source build that needs a C++ compiler.
3.12 has prebuilt wheels available, so no compiler is required.

Install Python 3.12 first if needed: winget install --id Python.Python.3.12
#>

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$venvPath = Join-Path $repoRoot "scripts\pmd3-venv"

if (-not (Get-Command "py" -ErrorAction SilentlyContinue)) {
    throw "Python launcher 'py' not found. Install Python 3.12: winget install --id Python.Python.3.12"
}

py -3.12 -m venv $venvPath
& "$venvPath\Scripts\python.exe" -m pip install --upgrade pip
& "$venvPath\Scripts\python.exe" -m pip install -r (Join-Path $PSScriptRoot "requirements.txt")

Write-Host "pymobiledevice3 venv ready at $venvPath"
