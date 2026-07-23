# Fix-ActiveFestivalStateDoubleRender.ps1
#
# BUG REAL CONFIRMADO: Dashboard.razor y Topbar.razor cargan festivales,
# ediciones y (en Dashboard) todas las stats/analytics/revenue dentro de
# OnInitializedAsync. Blazor Server ejecuta OnInitializedAsync DOS VECES por
# cada carga de pagina interactiva: una en el prerender (sin acceso fiable a
# localStorage, asi que "adivina" el festival) y otra al conectar el circuito
# interactivo (repite todo el fetch). Dashboard.razor ademas corrige una
# TERCERA vez en OnAfterRenderAsync cuando por fin puede leer el festival
# guardado en localStorage. Resultado: hasta 4 llamadas a "listar festivales",
# 2 a "listar ediciones" y hasta 3 cargas completas de LoadAllAsync en una
# sola carga de pagina - el "triple render" con festivales cambiando y buena
# parte de la lentitud general.
#
# Fix: un guard "if (!RendererInfo.IsInteractive) return;" al inicio de
# OnInitializedAsync en ambos componentes, para que el prerender no ejecute
# ningun fetch ni adivinanza. El trabajo pesado pasa de ejecutarse hasta 3
# veces a ejecutarse como mucho 2 (la carga real + la correccion de
# OnAfterRenderAsync si el valor de localStorage difiere). No cambia el
# concepto de ActiveFestivalState ni como se propaga el filtro entre paginas.
#
# Efecto secundario menor y esperado: en Topbar.razor, el HTML del prerender
# mostrara "Admin" generico un instante en vez del email/foto real (ese fetch
# tambien se movia al prerender). No es una regresion funcional.
#
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
$results = @()
# Dashboard.razor -> no hacer fetch ni carga pesada durante el prerender
$dashboardPath = "Alakai.FestivalManager.Admin/Components/Pages/Dashboard.razor"
$results += Patch-File -Path $dashboardPath -Description "Dashboard.razor: OnInitializedAsync se salta el prerender" -OldString @'
    protected override async Task OnInitializedAsync()
    {
        try
        {
            festivals = (await FestivalApiClient.GetAllAsync()).Where(f => f.IsActive).ToList();
'@ -NewString @'
    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive)
        {
            return;
        }

        try
        {
            festivals = (await FestivalApiClient.GetAllAsync()).Where(f => f.IsActive).ToList();
'@
if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar (Dashboard.razor). Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}
# Topbar.razor -> no hacer fetch durante el prerender
$topbarPath = "Alakai.FestivalManager.Admin/Components/Layout/Topbar.razor"
$results2 = @()
$results2 += Patch-File -Path $topbarPath -Description "Topbar.razor: OnInitializedAsync se salta el prerender" -OldString @'
    protected override async Task OnInitializedAsync()
    {
        UserProfileState.OnChange += HandleProfileChanged;
        ActiveFestivalState.OnChange += HandleProfileChanged;
'@ -NewString @'
    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive)
        {
            return;
        }

        UserProfileState.OnChange += HandleProfileChanged;
        ActiveFestivalState.OnChange += HandleProfileChanged;
'@
if ($results2 -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar (Topbar.razor). Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}
Write-Host "`nCorregido - el prerender ya no dispara fetch ni adivinanza de festival. dotnet build para confirmar." -ForegroundColor Green