# Fix-Step66-EyeIconInlineStyle.ps1
#
# BUG REAL, mismo patron que el problema de dark mode de antes: use clases de
# Tailwind (-translate-y-1/2, top-1/2, right-3, pr-11) que NO se usan en
# ningun otro sitio del proyecto, asi que no tienen regla CSS en el bundle
# precompilado - el boton del ojo quedaba sin ningun posicionamiento real.
#
# Fix: usar CSS en linea (style="...") en vez de clases de Tailwind para el
# posicionamiento - el CSS en linea siempre funciona, no depende de que
# Tailwind lo haya compilado de antemano.
#
# Ejecutar DESPUES de Fix-Step63/64.
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"

function Patch-File {
    param(
        [string]$Path,
        [string]$OldString,
        [string]$NewString,
        [string]$Description
    )

    if (-not (Test-Path $Path)) {
        Write-Host "SKIP (archivo no encontrado): $Path" -ForegroundColor Yellow
        return $false
    }

    $rawContent = Get-Content -Path $Path -Raw
    $usesCrlf = $rawContent.Contains("`r`n")

    $normalizedContent = $rawContent -replace "`r`n", "`n"
    $normalizedOld = $OldString -replace "`r`n", "`n"
    $normalizedNew = $NewString -replace "`r`n", "`n"

    if ($normalizedContent.Contains($normalizedNew)) {
        Write-Host "SKIP (ya aplicado): $Description" -ForegroundColor Cyan
        return $true
    }

    if (-not $normalizedContent.Contains($normalizedOld)) {
        Write-Host "SKIP (anchor no encontrado): $Description" -ForegroundColor Yellow
        return $false
    }

    $updatedNormalized = $normalizedContent.Replace($normalizedOld, $normalizedNew)

    if ($usesCrlf) {
        $updatedFinal = $updatedNormalized -replace "`n", "`r`n"
    } else {
        $updatedFinal = $updatedNormalized
    }

    Set-Content -Path $Path -Value $updatedFinal -NoNewline
    Write-Host "OK: $Description" -ForegroundColor Green
    return $true
}

$authLoginPath = "Alakai.FestivalManager.Admin/Components/Pages/Auth/Login.razor"
$adminLoginPath = "Alakai.FestivalManager.Admin/Components/Pages/AdminAuth/Login.razor"

$results = @()

$results += Patch-File -Path $authLoginPath -Description "Auth: posicionar el icono del ojo con CSS en linea" -OldString @'
                <div class="relative">
                    <InputText id="userPanelPassword" @bind-Value="LoginModel.Password" placeholder="Password" type="password" class="form-input pr-11" />
                    <button type="button" onclick="const i=document.getElementById('userPanelPassword'); const isPw=i.type==='password'; i.type=isPw?'text':'password'; this.querySelector('i').className=isPw?'ri-eye-line':'ri-eye-off-line';" class="absolute -translate-y-1/2 right-3 top-1/2 text-black/50 dark:text-white/60">
                        <i class="ri-eye-off-line"></i>
                    </button>
                </div>
'@ -NewString @'
                <div style="position:relative;">
                    <InputText id="userPanelPassword" @bind-Value="LoginModel.Password" placeholder="Password" type="password" class="form-input" style="padding-right:44px;" />
                    <button type="button" onclick="const i=document.getElementById('userPanelPassword'); const isPw=i.type==='password'; i.type=isPw?'text':'password'; this.querySelector('i').className=isPw?'ri-eye-line':'ri-eye-off-line';" class="text-black/50 dark:text-white/60" style="position:absolute; top:50%; right:12px; transform:translateY(-50%); background:none; border:none; padding:0; cursor:pointer;">
                        <i class="ri-eye-off-line"></i>
                    </button>
                </div>
'@

$results += Patch-File -Path $adminLoginPath -Description "AdminAuth: posicionar el icono del ojo con CSS en linea" -OldString @'
            <div class="relative">
                <input id="adminPassword" name="password" type="password" placeholder="Password" class="form-input pr-11" required />
                <button type="button" onclick="const i=document.getElementById('adminPassword'); const isPw=i.type==='password'; i.type=isPw?'text':'password'; this.querySelector('i').className=isPw?'ri-eye-line':'ri-eye-off-line';" class="absolute -translate-y-1/2 right-3 top-1/2 text-black/50 dark:text-white/60">
                    <i class="ri-eye-off-line"></i>
                </button>
            </div>
'@ -NewString @'
            <div style="position:relative;">
                <input id="adminPassword" name="password" type="password" placeholder="Password" class="form-input" style="padding-right:44px;" required />
                <button type="button" onclick="const i=document.getElementById('adminPassword'); const isPw=i.type==='password'; i.type=isPw?'text':'password'; this.querySelector('i').className=isPw?'ri-eye-line':'ri-eye-off-line';" class="text-black/50 dark:text-white/60" style="position:absolute; top:50%; right:12px; transform:translateY(-50%); background:none; border:none; padding:0; cursor:pointer;">
                    <i class="ri-eye-off-line"></i>
                </button>
            </div>
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nCorregido con CSS en linea (siempre funciona, no depende del bundle de Tailwind). dotnet build para confirmar." -ForegroundColor Green