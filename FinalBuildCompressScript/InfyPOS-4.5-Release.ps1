$dateString = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$RunnerWorkspace = Split-Path $env:GITHUB_WORKSPACE -Parent
$JenWorkPath = $env:GITHUB_WORKSPACE
$BuildBinPath = Join-Path $RunnerWorkspace "bin"
$RelWorkPath = Join-Path $RunnerWorkspace "release\Release-4.5"
$RelBinWorkPath = Join-Path $RelWorkPath "bin"
$FinalRelZipPath = Join-Path $RunnerWorkspace "release\ZipFiles\Release-4.5\Release-4.5-$dateString.zip"
$Clientside = Join-Path $JenWorkPath "clientside"
$RelClientWorkPath = Join-Path $RelWorkPath "clientside"
$ClientZipPath = Join-Path $RelWorkPath "clientside.zip"

# # Create Release folder
# New-Item -Path $RelWorkPath -ItemType Directory -Force
# Write-Host "Created Release folder"

# Created Clientside folder
New-Item -Path $RelWorkPath\clientside -ItemType Directory -Force
Write-Host "Created Clientside folder"

# Delete Previous Client Zip
if (Test-Path $ClientZipPath) {
    Remove-Item -Path $ClientZipPath -Recurse -Force
    Write-Host "Previous Client Zip Deleted"
} else {
    Write-Host "No previous Client Zip found, skipping."
}

# Created Bin folder
New-Item -Path $RelWorkPath\bin -ItemType Directory -Force
Write-Host "Created Bin folder"

# Copy Bin Files
$BinFiles = @('InfyPOS.dll', 'InfyPOS.pdb', 'Npgsql.dll', 'Npgsql.xml', 'System.Threading.Tasks.Extensions.dll', 'System.Threading.Tasks.Extensions.xml')
foreach ($file in $BinFiles) {
    Copy-Item -Path (Join-Path $BuildBinPath $file) -Destination $RelBinWorkPath -Recurse -Force
}
Write-Host "BinFiles Copied"

# Copy RootDirFiles
$RootDirFiles = @('index.html', 'index_full.html', 'Schema*.data', 'Catalog.html')
foreach ($file in $RootDirFiles) {
    Copy-Item -Path (Join-Path $JenWorkPath $file) -Destination $RelWorkPath -Recurse -Force
}
Write-Host "RootDirFiles Copied"

# Copy specific file types
$fileTypes = @('*.js', '*.css', '*.html', '*.ico')
foreach ($type in $fileTypes) {
    Copy-Item -Path (Join-Path $Clientside $type) -Destination $RelClientWorkPath -Recurse -Force
}
Write-Host "Client RootDirFiles Copied"

# Copy directories
$directories = @('controllers', 'grid', 'main', 'Mfg', 'Reports', 'views', 'dist', 'Feedback', 'images')
foreach ($dir in $directories) {
    Copy-Item -Path (Join-Path $Clientside $dir) -Destination $RelClientWorkPath -Recurse -Force
}
Write-Host "Client Root Dir Copied"

# Compress ClientSide files
Compress-Archive -Path $RelClientWorkPath -DestinationPath $ClientZipPath -Force
Write-Host "Compressed ClientSide"

#Delete ClientSide Folder
Remove-Item -Path  $RelWorkPath\clientside -Recurse -Force
Write-Host "ClientSide Folder Deleted"

# Compress Release files
New-Item -Path (Split-Path $FinalRelZipPath -Parent) -ItemType Directory -Force | Out-Null
Compress-Archive -Path $RelWorkPath -DestinationPath $FinalRelZipPath -Force
Write-Host "Compressed Release"

#Delete ClientSide Folder
Remove-Item -Path  $RelWorkPath\bin -Recurse -Force
Write-Host "bin Folder Deleted"

Write-Host "Script End"
