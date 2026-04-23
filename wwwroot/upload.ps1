# upload.ps1
$ftp = "ftp://asp-hotel.somee.com/www.asp-hotel.somee.com/wwwroot"
$user = "Awshn"
$pass = "TYfghxcvbn76@."  # ЗАМЕНИТЕ НА ВАШ ПАРОЛЬ
$localPath = "C:\Users\daniel\Desktop\rab\projects\c#\HotelUyutClean\wwwroot"

$webclient = New-Object System.Net.WebClient
$webclient.Credentials = New-Object System.Net.NetworkCredential($user, $pass)

function UploadFolder($folderPath, $ftpPath) {
    try {
        $webclient.UploadFile("$ftpPath/", "MKCOL", $null)
        Write-Host "Created: $ftpPath" -ForegroundColor Green
    } catch {}

    Get-ChildItem -Path $folderPath -File | ForEach-Object {
        $localFile = $_.FullName
        $ftpFile = "$ftpPath/$($_.Name)"
        try {
            $webclient.UploadFile($ftpFile, "STOR", $localFile)
            Write-Host "Uploaded: $($_.Name)" -ForegroundColor Green
        } catch {
            Write-Host "Failed: $($_.Name)" -ForegroundColor Red
        }
    }

    Get-ChildItem -Path $folderPath -Directory | ForEach-Object {
        UploadFolder $_.FullName "$ftpPath/$($_.Name)"
    }
}

UploadFolder $localPath $ftp
Write-Host "Upload complete!" -ForegroundColor Yellow