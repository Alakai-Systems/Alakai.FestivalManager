# Fix-Step33c-WireLayoutWidthEditor.ps1
#
# Parte 2: engancha el editor compartido (EmailTemplateEditor) con dos
# parametros nuevos - InitialWidth (para precargar el ancho guardado) y
# WidthChanged (para avisar al padre de cual eligio el usuario, y que lo
# guarde). EmailLayoutSettings.razor pasa/recibe estos valores desde/hacia
# HeaderImageWidth/FooterImageWidth del propio EmailLayout.
#
# NOTA: en Emails.razor (Email Templates) el componente se sigue usando SIN
# estos parametros - simplemente usa el valor por defecto (600), ya que ahi
# no hay un campo natural donde guardar "el ancho preferido" (un template
# puede llevar varias imagenes distintas).
#
# Ejecutar despues de Fix-Step33b-HeaderFooterWidthOnLayout.ps1 (y en vez de,
# no ademas de, Fix-Step33-RememberImageWidth.ps1 si llegaste a generarlo).
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
$editorPath = "Alakai.FestivalManager.Admin/Components/Layout/EmailTemplateEditor.razor"
$layoutPath = "Alakai.FestivalManager.Admin/Components/Pages/EmailLayoutSettings.razor"

# ── EmailTemplateEditor.razor ───────────────────────────────────────────────

$results += Patch-File -Path $editorPath -Description "Anadir parametros InitialWidth / WidthChanged" -OldString @'
    [Parameter] public List<string>? Variables { get; set; }

    private RadzenHtmlEditor EditorRef { get; set; } = default!;
'@ -NewString @'
    [Parameter] public List<string>? Variables { get; set; }
    [Parameter] public int InitialWidth { get; set; } = 600;
    [Parameter] public EventCallback<int> WidthChanged { get; set; }

    private RadzenHtmlEditor EditorRef { get; set; } = default!;
'@

$results += Patch-File -Path $editorPath -Description "Anadir campo de control + OnParametersSet para precargar InitialWidth" -OldString @'
    public int UploadImageWidth { get; set; } = 600;

    private async Task OnEditorChange(string html)
    {
'@ -NewString @'
    public int UploadImageWidth { get; set; } = 600;
    private bool _widthInitializedFromParameter;

    protected override void OnParametersSet()
    {
        if (!_widthInitializedFromParameter)
        {
            UploadImageWidth = InitialWidth;
            _widthInitializedFromParameter = true;
        }
    }

    private async Task OnEditorChange(string html)
    {
'@

$results += Patch-File -Path $editorPath -Description "Avisar al padre (WidthChanged) tras insertar la imagen" -OldString @'
            await EditorRef.ExecuteCommandAsync(HtmlEditorCommands.InsertHtml,
                $"<img src=\"{url}\" width=\"{actualWidth}\"{heightAttribute} style=\"max-width:100%; height:auto; display:block;\" />");
'@ -NewString @'
            await EditorRef.ExecuteCommandAsync(HtmlEditorCommands.InsertHtml,
                $"<img src=\"{url}\" width=\"{actualWidth}\"{heightAttribute} style=\"max-width:100%; height:auto; display:block;\" />");

            await WidthChanged.InvokeAsync(UploadImageWidth);
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar en EmailTemplateEditor.razor. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

# ── EmailLayoutSettings.razor ────────────────────────────────────────────────

$results2 = @()

$results2 += Patch-File -Path $layoutPath -Description "Enganchar InitialWidth/WidthChanged en el editor del Header" -OldString @'
                            <EmailTemplateEditor @bind-Value="formModel.HeaderHtml" />
'@ -NewString @'
                            <EmailTemplateEditor @bind-Value="formModel.HeaderHtml"
                                                  InitialWidth="@(formModel.HeaderImageWidth ?? 600)"
                                                  WidthChanged="@(w => formModel.HeaderImageWidth = w)" />
'@

$results2 += Patch-File -Path $layoutPath -Description "Enganchar InitialWidth/WidthChanged en el editor del Footer" -OldString @'
                            <EmailTemplateEditor @bind-Value="formModel.FooterHtml" />
'@ -NewString @'
                            <EmailTemplateEditor @bind-Value="formModel.FooterHtml"
                                                  InitialWidth="@(formModel.FooterImageWidth ?? 600)"
                                                  WidthChanged="@(w => formModel.FooterImageWidth = w)" />
'@

$results2 += Patch-File -Path $layoutPath -Description "OpenEditModal: precargar los anchos guardados" -OldString @'
            HeaderHtml = layout.HeaderHtml,
            HeaderText = layout.HeaderText,
            FooterHtml = layout.FooterHtml,
            FooterText = layout.FooterText,
            IsActive = layout.IsActive
        };
'@ -NewString @'
            HeaderHtml = layout.HeaderHtml,
            HeaderText = layout.HeaderText,
            FooterHtml = layout.FooterHtml,
            FooterText = layout.FooterText,
            HeaderImageWidth = layout.HeaderImageWidth,
            FooterImageWidth = layout.FooterImageWidth,
            IsActive = layout.IsActive
        };
'@

$results2 += Patch-File -Path $layoutPath -Description "SaveAsync (Create): incluir los anchos en el request" -OldString @'
                    HeaderHtml = formModel.HeaderHtml,
                    HeaderText = formModel.HeaderText,
                    FooterHtml = formModel.FooterHtml,
                    FooterText = formModel.FooterText,
                    IsActive = formModel.IsActive
                };

                await EmailLayoutApiClient.CreateAsync(request);
'@ -NewString @'
                    HeaderHtml = formModel.HeaderHtml,
                    HeaderText = formModel.HeaderText,
                    FooterHtml = formModel.FooterHtml,
                    FooterText = formModel.FooterText,
                    HeaderImageWidth = formModel.HeaderImageWidth,
                    FooterImageWidth = formModel.FooterImageWidth,
                    IsActive = formModel.IsActive
                };

                await EmailLayoutApiClient.CreateAsync(request);
'@

$results2 += Patch-File -Path $layoutPath -Description "SaveAsync (Update): incluir los anchos en el request" -OldString @'
                    HeaderHtml = formModel.HeaderHtml,
                    HeaderText = formModel.HeaderText,
                    FooterHtml = formModel.FooterHtml,
                    FooterText = formModel.FooterText,
                    IsActive = formModel.IsActive
                };

                await EmailLayoutApiClient.UpdateAsync(editingLayout.Id, request);
'@ -NewString @'
                    HeaderHtml = formModel.HeaderHtml,
                    HeaderText = formModel.HeaderText,
                    FooterHtml = formModel.FooterHtml,
                    FooterText = formModel.FooterText,
                    HeaderImageWidth = formModel.HeaderImageWidth,
                    FooterImageWidth = formModel.FooterImageWidth,
                    IsActive = formModel.IsActive
                };

                await EmailLayoutApiClient.UpdateAsync(editingLayout.Id, request);
'@

$results2 += Patch-File -Path $layoutPath -Description "FormModel: anadir campos de ancho" -OldString @'
        public string FooterHtml { get; set; } = string.Empty;
'@ -NewString @'
        public string FooterHtml { get; set; } = string.Empty;
        public int? HeaderImageWidth { get; set; }
        public int? FooterImageWidth { get; set; }
'@

if ($results2 -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar en EmailLayoutSettings.razor. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nEditor enganchado. dotnet build para confirmar." -ForegroundColor Green
Write-Host "Ahora el ancho de Header/Footer se guarda con el propio layout - persiste pase lo que pase con el navegador." -ForegroundColor Green