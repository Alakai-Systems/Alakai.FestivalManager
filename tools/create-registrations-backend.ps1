# fix_program_cs.ps1
# 1. Añade migraciones automáticas al arrancar
# 2. Corrige CORS para producción
# Ejecutar desde la raiz del repo: .\tools\fix_program_cs.ps1

$ErrorActionPreference = "Stop"

$file = "Alakai.FestivalManager.Api\Program.cs"

if (-not (Test-Path $file)) {
    Write-Error "ABORT: No se encuentra $file"
    exit 1
}

function Patch-File {
    param([string]$Path, [string]$OldText, [string]$NewText, [string]$Description)
    $raw = [System.IO.File]::ReadAllText($Path)
    $rawNorm = $raw.Replace("`r`n", "`n")
    $oldNorm = $OldText.Replace("`r`n", "`n")
    $newNorm = $NewText.Replace("`r`n", "`n")
    $count = ([regex]::Matches($rawNorm, [regex]::Escape($oldNorm))).Count
    if ($count -ne 1) {
        Write-Error "ABORT: '$Description' — esperaba 1 en '$Path', encontradas $count"
        exit 1
    }
    $useCRLF = $raw.Contains("`r`n")
    $patched = $rawNorm.Replace($oldNorm, $newNorm)
    if ($useCRLF) { $patched = $patched.Replace("`n", "`r`n") }
    [System.IO.File]::WriteAllText($Path, $patched, [System.Text.Encoding]::UTF8)
    Write-Host "OK: $Description"
}

# 1. Corregir CORS — añadir producción y arreglar nombre de policy
Patch-File $file `
    'builder.Services.AddCors(options =>
{
    options.AddPolicy("Admin",
        policy =>
        {
            policy
                .WithOrigins("https://localhost:7033")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});' `
    'builder.Services.AddCors(options =>
{
    options.AddPolicy("Admin",
        policy =>
        {
            policy
                .WithOrigins(
                    "https://localhost:7033",
                    "https://app-alakai-swimout-admin-bpdebfdvbgdacyda.westus2-01.azurewebsites.net",
                    "https://app-alakai-lajam-admin.azurewebsites.net"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});' `
    "Program.cs: CORS añadir producción"

# 2. Corregir nombre de policy UseCors
Patch-File $file `
    'app.UseCors("AdminCors");' `
    'app.UseCors("Admin");' `
    "Program.cs: UseCors nombre correcto"

# 3. Añadir migraciones automáticas
Patch-File $file `
    'app.UseMiddleware<GlobalExceptionMiddleware>();' `
    'using (IServiceScope scope = app.Services.CreateScope())
{
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<GlobalExceptionMiddleware>();' `
    "Program.cs: migraciones automáticas al arrancar"

Write-Host "Listo. Haz commit y push."