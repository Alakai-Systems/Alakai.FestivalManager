# Fix-Step45-GalleryPicker.ps1
#
# Parte 2: cliente + UI. Anade:
#   1) UploadsApiClient.UploadImageWithDetailsAsync (con FestivalId, devuelve
#      Width/Height reales del backend - ya no hace falta el JS de deteccion
#      de dimensiones del Paso 30, se simplifica).
#   2) UploadsApiClient.GetGalleryAsync (listar lo ya subido para un festival).
#   3) EmailTemplateEditor.razor: boton "Gallery" junto a "Upload image", que
#      abre un panel con miniaturas de lo ya subido - clicar una la inserta
#      directamente, con su ancho/alto reales (sin deformar).
#
# Ejecutar despues de Fix-Step44-MediaGallery.ps1.
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

    if ($normalizedContent.Contains($normalizedNew) -and -not $normalizedContent.Contains($normalizedOld)) {
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
$clientPath = "Alakai.FestivalManager.Admin/Services/Api/UploadsApiClient.cs"
$editorPath = "Alakai.FestivalManager.Admin/Components/Layout/EmailTemplateEditor.razor"

# ── 1. UploadsApiClient: nuevos metodos + DTOs ──────────────────────────────
$results += Patch-File -Path $clientPath -Description "Anadir UploadImageWithDetailsAsync + GetGalleryAsync + DTOs" -OldString @'
    public async Task<string> UploadImageAsync(Stream content, string fileName, string contentType, int? width = null, CancellationToken cancellationToken = default)
    {
'@ -NewString @'
    public async Task<UploadImageDetailResult> UploadImageWithDetailsAsync(Stream content, string fileName, string contentType, Guid? festivalId, int? width = null, CancellationToken cancellationToken = default)
    {
        using MultipartFormDataContent form = new();
        using StreamContent streamContent = new(content);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        form.Add(streamContent, "file", fileName);

        if (width.HasValue)
        {
            form.Add(new StringContent(width.Value.ToString()), "width");
        }

        if (festivalId.HasValue)
        {
            form.Add(new StringContent(festivalId.Value.ToString()), "festivalId");
        }

        HttpResponseMessage httpResponse = await _httpClient.PostAsync("api/uploads/images", form, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            string errorBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new ApiClientException($"Image upload failed: {errorBody}", null);
        }

        UploadImageDetailResult? result = await httpResponse.Content.ReadFromJsonAsync<UploadImageDetailResult>(cancellationToken: cancellationToken);

        if (result is null || string.IsNullOrWhiteSpace(result.Url))
        {
            throw new ApiClientException("Image upload returned an empty URL.", null);
        }

        return result;
    }

    public async Task<List<GalleryImageDto>> GetGalleryAsync(Guid festivalId, CancellationToken cancellationToken = default)
    {
        List<GalleryImageDto>? result = await _httpClient.GetFromJsonAsync<List<GalleryImageDto>>($"api/uploads/gallery?festivalId={festivalId}", cancellationToken);
        return result ?? [];
    }

    public async Task<string> UploadImageAsync(Stream content, string fileName, string contentType, int? width = null, CancellationToken cancellationToken = default)
    {
'@

$results += Patch-File -Path $clientPath -Description "Anadir los DTOs UploadImageDetailResult y GalleryImageDto" -OldString @'
public class UploadImageResult
{
    public string Url { get; set; } = string.Empty;
}
'@ -NewString @'
public class UploadImageResult
{
    public string Url { get; set; } = string.Empty;
}

public class UploadImageDetailResult
{
    public string Url { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
}

public class GalleryImageDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime CreatedAt { get; set; }
}
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar en UploadsApiClient.cs. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

# ── 2. EmailTemplateEditor.razor: inyectar ActiveFestivalState ──────────────
$results2 = @()

$results2 += Patch-File -Path $editorPath -Description "Inyectar ActiveFestivalState" -OldString @'
@inject UploadsApiClient UploadsApiClient
@inject IJSRuntime JsRuntime
'@ -NewString @'
@inject UploadsApiClient UploadsApiClient
@inject IJSRuntime JsRuntime
@inject ActiveFestivalState ActiveFestivalState
'@

# ── 3. Boton "Gallery" junto al de Upload ────────────────────────────────────
$results2 += Patch-File -Path $editorPath -Description "Anadir boton Gallery y el panel de miniaturas" -OldString @'
        <InputFile OnChange="@OnImageSelectedAsync" accept="image/png,image/jpeg,image/gif,image/webp" style="display:none;" id="@UploadInputId" />
        <label for="@UploadInputId" class="email-rte-upload-button">
            @(IsUploadingImage ? "Uploading..." : "📷 Upload image")
        </label>
    </div>
'@ -NewString @'
        <InputFile OnChange="@OnImageSelectedAsync" accept="image/png,image/jpeg,image/gif,image/webp" style="display:none;" id="@UploadInputId" />
        <label for="@UploadInputId" class="email-rte-upload-button">
            @(IsUploadingImage ? "Uploading..." : "📷 Upload image")
        </label>
        <button type="button" class="email-rte-upload-button" @onclick="ToggleGalleryAsync">
            🖼️ Choose from gallery
        </button>
    </div>

    @if (ShowGallery)
    {
        <div class="email-rte-gallery">
            @if (GalleryImages is null)
            {
                <p>Loading...</p>
            }
            else if (GalleryImages.Count == 0)
            {
                <p>No images uploaded yet for this festival.</p>
            }
            else
            {
                @foreach (GalleryImageDto image in GalleryImages)
                {
                    <button type="button" class="email-rte-gallery-item" @onclick="() => InsertFromGalleryAsync(image)" title="@($"{image.Width}x{image.Height}")">
                        <img src="@image.Url" alt="" />
                    </button>
                }
            }
        </div>
    }
'@

# ── 4. Estilos del panel de galeria ──────────────────────────────────────────
$results2 += Patch-File -Path $editorPath -Description "Anadir estilos del panel de galeria" -OldString @'
    .email-rte-width-input {
        width: 70px;
        padding: 4px 6px;
        border: 1px solid #d1d5db;
        border-radius: 6px;
        font-size: 0.8rem;
    }
'@ -NewString @'
    .email-rte-width-input {
        width: 70px;
        padding: 4px 6px;
        border: 1px solid #d1d5db;
        border-radius: 6px;
        font-size: 0.8rem;
    }

    .email-rte-gallery {
        margin-top: 10px;
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(90px, 1fr));
        gap: 8px;
        max-height: 260px;
        overflow-y: auto;
        padding: 10px;
        border: 1px solid #e5e7eb;
        border-radius: 8px;
    }

    .email-rte-gallery-item {
        border: 1px solid #e5e7eb;
        border-radius: 6px;
        padding: 0;
        overflow: hidden;
        background: none;
        cursor: pointer;
        aspect-ratio: 1;
    }

    .email-rte-gallery-item:hover {
        border-color: #6D28D9;
    }

    .email-rte-gallery-item img {
        width: 100%;
        height: 100%;
        object-fit: cover;
        display: block;
    }
'@

# ── 5. @code: estado de la galeria + metodos ─────────────────────────────────
$results2 += Patch-File -Path $editorPath -Description "Anadir el estado y los metodos de la galeria" -OldString @'
    protected override void OnParametersSet()
    {
        if (!_widthInitializedFromParameter)
        {
            UploadImageWidth = InitialWidth;
            _widthInitializedFromParameter = true;
        }
    }
'@ -NewString @'
    protected override void OnParametersSet()
    {
        if (!_widthInitializedFromParameter)
        {
            UploadImageWidth = InitialWidth;
            _widthInitializedFromParameter = true;
        }
    }

    private bool ShowGallery { get; set; }
    private List<GalleryImageDto>? GalleryImages { get; set; }

    private async Task ToggleGalleryAsync()
    {
        ShowGallery = !ShowGallery;

        if (ShowGallery && ActiveFestivalState.Active is not null)
        {
            GalleryImages = null;
            GalleryImages = await UploadsApiClient.GetGalleryAsync(ActiveFestivalState.Active.Id);
        }
    }

    private async Task InsertFromGalleryAsync(GalleryImageDto image)
    {
        string heightAttribute = image.Height > 0 ? $" height=\"{image.Height}\"" : string.Empty;

        await EditorRef.ExecuteCommandAsync(HtmlEditorCommands.InsertHtml,
            $"<img src=\"{image.Url}\" width=\"{image.Width}\"{heightAttribute} style=\"max-width:100%; height:auto; display:block;\" />");

        ShowGallery = false;
    }
'@

if ($results2 -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar (galeria). Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

# ── 6. OnImageSelectedAsync: usar el nuevo metodo con FestivalId, ya sin JS ──
$results3 = Patch-File -Path $editorPath -Description "Usar UploadImageWithDetailsAsync en vez del metodo simple + deteccion JS" -OldString @'
            string url = await UploadsApiClient.UploadImageAsync(stream, e.File.Name, e.File.ContentType, UploadImageWidth);

            int actualWidth = UploadImageWidth;
            int actualHeight = 0;

            try
            {
                ImageDimensions dimensions = await JsRuntime.InvokeAsync<ImageDimensions>("getImageDimensions", url);
                actualWidth = dimensions.Width;
                actualHeight = dimensions.Height;
            }
            catch
            {
                // Si por lo que sea no se pueden leer las dimensiones reales (red, formato raro),
                // seguimos adelante solo con el ancho elegido - height:auto en el CSS cubre el resto.
            }

            string heightAttribute = actualHeight > 0 ? $" height=\"{actualHeight}\"" : string.Empty;

            await EditorRef.ExecuteCommandAsync(HtmlEditorCommands.InsertHtml,
                $"<img src=\"{url}\" width=\"{actualWidth}\"{heightAttribute} style=\"max-width:100%; height:auto; display:block;\" />");

            await WidthChanged.InvokeAsync(UploadImageWidth);
'@ -NewString @'
            UploadImageDetailResult uploaded = await UploadsApiClient.UploadImageWithDetailsAsync(
                stream, e.File.Name, e.File.ContentType, ActiveFestivalState.Active?.Id, UploadImageWidth);

            string heightAttribute = uploaded.Height > 0 ? $" height=\"{uploaded.Height}\"" : string.Empty;

            await EditorRef.ExecuteCommandAsync(HtmlEditorCommands.InsertHtml,
                $"<img src=\"{uploaded.Url}\" width=\"{uploaded.Width}\"{heightAttribute} style=\"max-width:100%; height:auto; display:block;\" />");

            await WidthChanged.InvokeAsync(UploadImageWidth);
'@

if (-not $results3) {
    Write-Host "`nNo se pudo aplicar el patch de OnImageSelectedAsync." -ForegroundColor Red
    exit 1
}

Write-Host "`nGaleria anadida. dotnet build para confirmar." -ForegroundColor Green
Write-Host "Nota: el metodo getImageDimensions (JS) y la clase ImageDimensions ya no se usan aqui," -ForegroundColor Yellow
Write-Host "pero se dejan sin borrar por si algo mas los referencia - no hacen dano estando ahi." -ForegroundColor Yellow