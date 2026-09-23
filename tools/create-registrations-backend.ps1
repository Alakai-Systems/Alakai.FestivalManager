<#
    Fix-PassTypeModuleVisibility.ps1
    -----------------------------------
    Independiente de los demas scripts -- no depende de ninguno de ellos.

    (Version 2 -- la primera version que te mande tenia un bug real en el propio
    script, no era cosa de copiar y pegar: en 17 de los 21 cambios, el texto que
    cierra el bloque de PowerShell quedaba pegado a la ultima linea del codigo en
    vez de en su propia linea, así que PowerShell no reconocia el cierre. Por eso
    solo se validaban 4 cambios. Esta version lo corrige y esta re-verificada de
    principio a fin contra el repo real.)

    Nuevo campo PassType.EnabledModules (mismo enum FestivalModule que ya usa
    Festival: Competitions=1, Accommodation=2, Transport=4, Meals=8). Segun lo
    hablado:

    - Un pase NUEVO hereda los modulos que el festival tenga activos en ese
      momento, y ademas SIEMPRE tiene Competitions activo (independientemente
      del festival), para poder desactivarlo caso por caso despues.
    - En la pantalla de edicion de un Pase, solo se muestra el checkbox de
      Accommodation/Transport/Meals si el festival de esa edicion tiene ese
      modulo activo (si no, no tiene sentido poder tocarlo por pase). El
      checkbox de Competitions SIEMPRE se muestra, para poder desactivarlo.
    - En el Panel de Usuario, una seccion solo se muestra si esta activa a la
      vez a nivel de festival Y a nivel del pase concreto de esa persona
      (antes solo miraba el festival).
    - Los pases YA EXISTENTES no pierden nada: la migracion los deja con los
      4 modulos activos por defecto.

    IMPORTANTE -- este script SI necesita una migracion de base de datos (los
    anteriores no). Este script deja el codigo listo, pero la migracion hay
    que generarla tu con las herramientas de EF que ya usas (este entorno no
    tiene el SDK de .NET instalado para generarla por ti). Pasos, EN ESTE
    ORDEN:

        1) pwsh -File .\Fix-PassTypeModuleVisibility.ps1      (este script)
        2) cd Alakai.FestivalManager.Infrastructure
        3) dotnet ef migrations add AddEnabledModulesToPassTypes --startup-project ../Alakai.FestivalManager.Api
        4) Abre el .cs de la migracion recien creada y comprueba que la
           columna EnabledModules se crea con "defaultValue: 15" (eso es lo
           que hace que los pases que ya existen NO pierdan ningun modulo).
           Si no aparece "15", avisame antes de aplicarla.
        5) dotnet ef database update --startup-project ../Alakai.FestivalManager.Api
        6) dotnet build

    Uso:
        cd Alakai.FestivalManager          # raiz del repo (donde esta el .sln)
        pwsh -NoProfile -ExecutionPolicy Bypass -File .\Fix-PassTypeModuleVisibility.ps1

    Idempotente y todo-o-nada: si algun anchor no encaja porque algun archivo
    local difiere de lo esperado, no escribe nada y lista el problema.
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath 'Alakai.FestivalManager.sln')) {
    Write-Error "No se encuentra Alakai.FestivalManager.sln en el directorio actual. Ejecuta este script desde la raiz del repo (Alakai.FestivalManager/)."
    exit 1
}

# ----------------------------------------------------------------------------
# Helpers (identicos a los scripts anteriores)
# ----------------------------------------------------------------------------

function Get-NormalizedContent {
    param([string]$Path)

    $raw = [System.IO.File]::ReadAllText($Path)
    $usesCrlf = $raw.Contains("`r`n")
    $normalized = $raw.Replace("`r`n", "`n")

    return [PSCustomObject]@{
        Raw        = $raw
        Normalized = $normalized
        UsesCrlf   = $usesCrlf
    }
}

function Set-NormalizedContent {
    param(
        [string]$Path,
        [string]$NormalizedContent,
        [bool]$UsesCrlf
    )

    $final = if ($UsesCrlf) { $NormalizedContent.Replace("`n", "`r`n") } else { $NormalizedContent }
    [System.IO.File]::WriteAllText($Path, $final, [System.Text.UTF8Encoding]::new($false))
}

function Convert-ToLf {
    param([string]$Text)
    return $Text.Replace("`r`n", "`n")
}

$script:PlanErrors = @()
$script:Plan = @()

function Add-PatchOperation {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Anchor,
        [Parameter(Mandatory)][string]$Replacement,
        [Parameter(Mandatory)][string]$Description
    )

    $script:Plan += [PSCustomObject]@{
        Type        = 'Patch'
        Path        = $Path
        Anchor      = Convert-ToLf $Anchor
        Replacement = Convert-ToLf $Replacement
        Description = $Description
    }
}

function Test-PatchOperation {
    param($Op)

    if (-not (Test-Path -LiteralPath $Op.Path)) {
        return "No existe el archivo: $($Op.Path)"
    }

    $file = Get-NormalizedContent -Path $Op.Path
    $anchorCount = ([regex]::Matches($file.Normalized, [regex]::Escape($Op.Anchor))).Count

    if ($anchorCount -eq 1) {
        return $null
    }

    if ($anchorCount -eq 0) {
        $replacementCount = ([regex]::Matches($file.Normalized, [regex]::Escape($Op.Replacement))).Count
        if ($replacementCount -ge 1) {
            return $null  # ya aplicado -> idempotente
        }
        return "Anchor no encontrado en $($Op.Path) (el archivo local no coincide con lo esperado). Descripcion: $($Op.Description)"
    }

    return "Anchor encontrado $anchorCount veces en $($Op.Path) (deberia ser unico). Descripcion: $($Op.Description)"
}

function Invoke-PatchOperation {
    param($Op)

    $file = Get-NormalizedContent -Path $Op.Path
    $anchorCount = ([regex]::Matches($file.Normalized, [regex]::Escape($Op.Anchor))).Count

    if ($anchorCount -eq 0) {
        Write-Host "  = ya aplicado: $($Op.Path) -- $($Op.Description)" -ForegroundColor DarkGray
        return
    }

    $newNormalized = $file.Normalized.Replace($Op.Anchor, $Op.Replacement)
    Set-NormalizedContent -Path $Op.Path -NormalizedContent $newNormalized -UsesCrlf $file.UsesCrlf

    Write-Host "  + patched: $($Op.Path) -- $($Op.Description)" -ForegroundColor Green
}

# 1. Alakai.FestivalManager.Domain/Entities/PassType.cs -- PassType: campo EnabledModules
Add-PatchOperation -Path 'Alakai.FestivalManager.Domain/Entities/PassType.cs' `
    -Description 'PassType: campo EnabledModules' `
    -Anchor @'
public class PassType : BaseEntity
{
    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AllowsMultipleLevels { get; set; }
    public decimal? AllLevelsDiscountPercent { get; set; }
    public ICollection<Level> Levels { get; set; } = new List<Level>();
}
'@ `
    -Replacement @'
public class PassType : BaseEntity
{
    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AllowsMultipleLevels { get; set; }
    public decimal? AllLevelsDiscountPercent { get; set; }

    // Modulos visibles/activos en el Panel de Usuario para este pase en concreto (Competitions,
    // Accommodation, Transport, Meals -- mismo enum que Festival.EnabledModules). Por defecto los
    // 4 activos para no cambiar el comportamiento de pases ya existentes; los pases nuevos calculan
    // su valor a partir de los modulos del festival (ver CreatePassTypeHandler).
    public FestivalModule EnabledModules { get; set; } = FestivalModule.Competitions | FestivalModule.Accommodation | FestivalModule.Transport | FestivalModule.Meals;

    public ICollection<Level> Levels { get; set; } = new List<Level>();
}
'@

# 2. Alakai.FestivalManager.Infrastructure/Configurations/PassTypeConfiguration.cs -- PassTypeConfiguration: default value 15 para EnabledModules
Add-PatchOperation -Path 'Alakai.FestivalManager.Infrastructure/Configurations/PassTypeConfiguration.cs' `
    -Description 'PassTypeConfiguration: default value 15 para EnabledModules' `
    -Anchor @'
        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.HasOne(p => p.Edition)
'@ `
    -Replacement @'
        builder.Property(p => p.IsActive)
            .IsRequired();

        // Por defecto los 4 modulos activos (Competitions=1, Accommodation=2, Transport=4, Meals=8
        // -> 15), para que los pases ya existentes no pierdan visibilidad de ningun modulo al
        // aplicar la migracion.
        builder.Property(p => p.EnabledModules)
            .IsRequired()
            .HasDefaultValue(FestivalModule.Competitions | FestivalModule.Accommodation | FestivalModule.Transport | FestivalModule.Meals);

        builder.HasOne(p => p.Edition)
'@

# 3. Alakai.FestivalManager.Application/Features/PassTypes/Contracts/DTOs/PassTypeDto.cs -- Application PassTypeDto: campo EnabledModules
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Features/PassTypes/Contracts/DTOs/PassTypeDto.cs' `
    -Description 'Application PassTypeDto: campo EnabledModules' `
    -Anchor @'
public class PassTypeDto
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
'@ `
    -Replacement @'
public class PassTypeDto
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public FestivalModule EnabledModules { get; set; }
}
'@

# 4. Alakai.FestivalManager.Application/Features/PassTypes/Commands/UpdatePassType/UpdatePassTypeCommand.cs -- Application UpdatePassTypeCommand: campo EnabledModules
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Features/PassTypes/Commands/UpdatePassType/UpdatePassTypeCommand.cs' `
    -Description 'Application UpdatePassTypeCommand: campo EnabledModules' `
    -Anchor @'
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
'@ `
    -Replacement @'
    public int SortOrder { get; set; }
    public FestivalModule EnabledModules { get; set; }
    public bool IsActive { get; set; }
'@

# 5. Alakai.FestivalManager.Application/Features/PassTypes/Contracts/Requests/UpdatePassTypeRequest.cs -- Application UpdatePassTypeRequest: campo EnabledModules
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Features/PassTypes/Contracts/Requests/UpdatePassTypeRequest.cs' `
    -Description 'Application UpdatePassTypeRequest: campo EnabledModules' `
    -Anchor @'
public class UpdatePassTypeRequest
{
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
'@ `
    -Replacement @'
public class UpdatePassTypeRequest
{
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public FestivalModule EnabledModules { get; set; }
}
'@

# 6. Alakai.FestivalManager.Application/Features/PassTypes/Commands/CreatePassType/CreatePassTypeHandler.cs -- CreatePassTypeHandler: default de EnabledModules segun festival
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Features/PassTypes/Commands/CreatePassType/CreatePassTypeHandler.cs' `
    -Description 'CreatePassTypeHandler: default de EnabledModules segun festival' `
    -Anchor @'
public class CreatePassTypeHandler
{
    private readonly IPassTypeRepository _passTypeRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public CreatePassTypeHandler(IPassTypeRepository passTypeRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _passTypeRepository = passTypeRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<PassTypeDto> HandleAsync(CreatePassTypeCommand command, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        bool exists = await _passTypeRepository.ExistsByEditionAndNameAsync(command.EditionId, command.Name, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException($"Pass type '{command.Name}' already exists for this edition.");
        }

        PassType passType = _mapper.Map<PassType>(command);

        await _passTypeRepository.AddAsync(passType, cancellationToken);
        await _passTypeRepository.SaveChangesAsync(cancellationToken);

        PassTypeDto passTypeDto = _mapper.Map<PassTypeDto>(passType);

        return passTypeDto;
    }
'@ `
    -Replacement @'
public class CreatePassTypeHandler
{
    private readonly IPassTypeRepository _passTypeRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IFestivalRepository _festivalRepository;
    private readonly IMapper _mapper;

    public CreatePassTypeHandler(IPassTypeRepository passTypeRepository, IEditionRepository editionRepository, IFestivalRepository festivalRepository, IMapper mapper)
    {
        _passTypeRepository = passTypeRepository;
        _editionRepository = editionRepository;
        _festivalRepository = festivalRepository;
        _mapper = mapper;
    }

    public async Task<PassTypeDto> HandleAsync(CreatePassTypeCommand command, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        bool exists = await _passTypeRepository.ExistsByEditionAndNameAsync(command.EditionId, command.Name, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException($"Pass type '{command.Name}' already exists for this edition.");
        }

        PassType passType = _mapper.Map<PassType>(command);

        // Un pase nuevo hereda los modulos que el festival tenga activos en este momento.
        // Competitions se fuerza siempre activo (independientemente del festival) para poder
        // desactivarlo despues, pase por pase, desde la pantalla de edicion.
        Festival? festival = await _festivalRepository.GetByIdAsync(edition.FestivalId, cancellationToken);
        passType.EnabledModules = (festival?.EnabledModules ?? FestivalModule.None) | FestivalModule.Competitions;

        await _passTypeRepository.AddAsync(passType, cancellationToken);
        await _passTypeRepository.SaveChangesAsync(cancellationToken);

        PassTypeDto passTypeDto = _mapper.Map<PassTypeDto>(passType);

        return passTypeDto;
    }
'@

# 7. Alakai.FestivalManager.Admin/Contracts/PassTypes/DTOs/PassTypeDto.cs -- Admin PassTypeDto: campo EnabledModules
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Contracts/PassTypes/DTOs/PassTypeDto.cs' `
    -Description 'Admin PassTypeDto: campo EnabledModules' `
    -Anchor @'
public class PassTypeDto
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
'@ `
    -Replacement @'
public class PassTypeDto
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int EnabledModules { get; set; }
}
'@

# 8. Alakai.FestivalManager.Admin/Contracts/PassTypes/Requests/UpdatePassTypeRequest.cs -- Admin UpdatePassTypeRequest: campo EnabledModules
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Contracts/PassTypes/Requests/UpdatePassTypeRequest.cs' `
    -Description 'Admin UpdatePassTypeRequest: campo EnabledModules' `
    -Anchor @'
public class UpdatePassTypeRequest
{
    public Guid EditionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }
}
'@ `
    -Replacement @'
public class UpdatePassTypeRequest
{
    public Guid EditionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public int EnabledModules { get; set; }
}
'@

# 9. Alakai.FestivalManager.Admin/Components/Pages/PassTypes.razor -- PassTypes.razor: checkboxes de modulos en el modal de edicion
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/PassTypes.razor' `
    -Description 'PassTypes.razor: checkboxes de modulos en el modal de edicion' `
    -Anchor @'
                    <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                        <input type="checkbox" @bind="updateRequest.IsActive" />
                        Active
                    </label>
                </div>

                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isSaving" @onclick="CloseModals">Cancel</button>
                    <button type="button" class="btn border border-purple text-purple hover:bg-purple hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="UpdatePassTypeAsync">
                        @(isSaving ? "Saving..." : "Save")
                    </button>
                </div>
            </div>
        </div>
    </div>
}

@if (showDeleteModal && selectedPassType is not null)
'@ `
    -Replacement @'
                    <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                        <input type="checkbox" @bind="updateRequest.IsActive" />
                        Active
                    </label>

                    <div>
                        <label class="block mb-2 text-sm text-black/60 dark:text-white/60">Modules available on this pass</label>
                        <div class="flex flex-wrap gap-4">
                            <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                <input type="checkbox" @bind="UpdateHasCompetitions" />
                                Competitions
                            </label>
                            @if (UpdateFestivalHasAccommodation)
                            {
                                <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                    <input type="checkbox" @bind="UpdateHasAccommodation" />
                                    Accommodation
                                </label>
                            }
                            @if (UpdateFestivalHasTransport)
                            {
                                <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                    <input type="checkbox" @bind="UpdateHasTransport" />
                                    Transport
                                </label>
                            }
                            @if (UpdateFestivalHasMeals)
                            {
                                <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                                    <input type="checkbox" @bind="UpdateHasMeals" />
                                    Meals
                                </label>
                            }
                        </div>
                        <p class="mt-1 text-xs text-black/40 dark:text-white/60">Only modules enabled for the festival are shown here. Competitions is on by default and can be turned off for this pass.</p>
                    </div>
                </div>

                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isSaving" @onclick="CloseModals">Cancel</button>
                    <button type="button" class="btn border border-purple text-purple hover:bg-purple hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="UpdatePassTypeAsync">
                        @(isSaving ? "Saving..." : "Save")
                    </button>
                </div>
            </div>
        </div>
    </div>
}

@if (showDeleteModal && selectedPassType is not null)
'@

# 10. Alakai.FestivalManager.Admin/Components/Pages/PassTypes.razor -- PassTypes.razor: OpenEditModal copia EnabledModules
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/PassTypes.razor' `
    -Description 'PassTypes.razor: OpenEditModal copia EnabledModules' `
    -Anchor @'
    private void OpenEditModal(PassTypeDto passType)
    {
        selectedPassType = passType;
        modalErrorMessage = null;
        updateRequest = new UpdatePassTypeRequest
        {
            EditionId = passType.EditionId,
            Name = passType.Name,
            SortOrder = passType.SortOrder,
            Description = passType.Description,
            IsActive = passType.IsActive
        };
        showEditModal = true;
    }
'@ `
    -Replacement @'
    private void OpenEditModal(PassTypeDto passType)
    {
        selectedPassType = passType;
        modalErrorMessage = null;
        updateRequest = new UpdatePassTypeRequest
        {
            EditionId = passType.EditionId,
            Name = passType.Name,
            SortOrder = passType.SortOrder,
            Description = passType.Description,
            IsActive = passType.IsActive,
            EnabledModules = passType.EnabledModules
        };
        showEditModal = true;
    }
'@

# 11. Alakai.FestivalManager.Admin/Components/Pages/PassTypes.razor -- PassTypes.razor: propiedades calculadas para los checkboxes de modulos
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/PassTypes.razor' `
    -Description 'PassTypes.razor: propiedades calculadas para los checkboxes de modulos' `
    -Anchor @'
        return $"{ed.Year} - {ed.Name}";
    }

    private void SortBy(string column)
'@ `
    -Replacement @'
        return $"{ed.Year} - {ed.Name}";
    }

    private int SelectedEditionFestivalModules
    {
        get
        {
            EditionDto? edition = editions.FirstOrDefault(e => e.Id == updateRequest.EditionId);

            if (edition is null)
            {
                return 0;
            }

            FestivalDto? festival = festivals.FirstOrDefault(f => f.Id == edition.FestivalId);

            return festival?.EnabledModules ?? 0;
        }
    }

    private bool UpdateFestivalHasAccommodation => (SelectedEditionFestivalModules & 2) != 0;
    private bool UpdateFestivalHasTransport => (SelectedEditionFestivalModules & 4) != 0;
    private bool UpdateFestivalHasMeals => (SelectedEditionFestivalModules & 8) != 0;

    private bool UpdateHasCompetitions
    {
        get => (updateRequest.EnabledModules & 1) != 0;
        set => updateRequest.EnabledModules = value ? updateRequest.EnabledModules | 1 : updateRequest.EnabledModules & ~1;
    }

    private bool UpdateHasAccommodation
    {
        get => (updateRequest.EnabledModules & 2) != 0;
        set => updateRequest.EnabledModules = value ? updateRequest.EnabledModules | 2 : updateRequest.EnabledModules & ~2;
    }

    private bool UpdateHasTransport
    {
        get => (updateRequest.EnabledModules & 4) != 0;
        set => updateRequest.EnabledModules = value ? updateRequest.EnabledModules | 4 : updateRequest.EnabledModules & ~4;
    }

    private bool UpdateHasMeals
    {
        get => (updateRequest.EnabledModules & 8) != 0;
        set => updateRequest.EnabledModules = value ? updateRequest.EnabledModules | 8 : updateRequest.EnabledModules & ~8;
    }

    private void SortBy(string column)
'@

# 12. Alakai.FestivalManager.Application/Features/Festivals/Contracts/DTOs/RegistrationFestivalInfoDto.cs -- Application RegistrationFestivalInfoDto: campo PassTypeEnabledModules
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Features/Festivals/Contracts/DTOs/RegistrationFestivalInfoDto.cs' `
    -Description 'Application RegistrationFestivalInfoDto: campo PassTypeEnabledModules' `
    -Anchor @'
    public int EnabledModules { get; set; }
    public string? TermsUrl { get; set; }
'@ `
    -Replacement @'
    public int EnabledModules { get; set; }
    public int PassTypeEnabledModules { get; set; }
    public string? TermsUrl { get; set; }
'@

# 13. Alakai.FestivalManager.Admin/Contracts/Festivals/DTOs/RegistrationFestivalInfoDto.cs -- Admin RegistrationFestivalInfoDto: campo PassTypeEnabledModules
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Contracts/Festivals/DTOs/RegistrationFestivalInfoDto.cs' `
    -Description 'Admin RegistrationFestivalInfoDto: campo PassTypeEnabledModules' `
    -Anchor @'
    public int EnabledModules { get; set; }
    public string? TermsUrl { get; set; }
'@ `
    -Replacement @'
    public int EnabledModules { get; set; }
    public int PassTypeEnabledModules { get; set; }
    public string? TermsUrl { get; set; }
'@

# 14. Alakai.FestivalManager.Application/Features/Festivals/Services/RegistrationFestivalInfoService.cs -- RegistrationFestivalInfoService: incluye PassTypeEnabledModules
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Features/Festivals/Services/RegistrationFestivalInfoService.cs' `
    -Description 'RegistrationFestivalInfoService: incluye PassTypeEnabledModules' `
    -Anchor @'
        return new ApiResponse<RegistrationFestivalInfoDto>
        {
            Success = true,
            Data = new RegistrationFestivalInfoDto { EnabledModules = (int)festival.EnabledModules, TermsUrl = festival.TermsUrl },
            Errors = [],
            Message = "Festival info loaded."
        };
'@ `
    -Replacement @'
        // registration.PassType ya viene cargado (Include en RegistrationRepository.GetByIdAsync).
        return new ApiResponse<RegistrationFestivalInfoDto>
        {
            Success = true,
            Data = new RegistrationFestivalInfoDto
            {
                EnabledModules = (int)festival.EnabledModules,
                PassTypeEnabledModules = (int)registration.PassType.EnabledModules,
                TermsUrl = festival.TermsUrl
            },
            Errors = [],
            Message = "Festival info loaded."
        };
'@

# 15. Alakai.FestivalManager.Admin/Services/Api/UserPanelApiClient.cs -- UserPanelApiClient: nuevo metodo GetEnabledPassTypeModulesAsync
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Services/Api/UserPanelApiClient.cs' `
    -Description 'UserPanelApiClient: nuevo metodo GetEnabledPassTypeModulesAsync' `
    -Anchor @'
        return response.Data.EnabledModules;
    }

    public async Task<IReadOnlyList<BusReservationDto>> GetBusReservationsAsync(string? domain = null, CancellationToken cancellationToken = default)
    {
'@ `
    -Replacement @'
        return response.Data.EnabledModules;
    }

    public async Task<int> GetEnabledPassTypeModulesAsync(string? domain = null, CancellationToken cancellationToken = default)
    {
        string? token = await _tokenStorageService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return 0;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string url = string.IsNullOrWhiteSpace(domain)
            ? "api/user-panel/festival-modules"
            : $"api/user-panel/festival-modules?domain={Uri.EscapeDataString(domain)}";

        ApiResponse<RegistrationFestivalInfoDto>? response = await _httpClient.GetFromJsonAsync<ApiResponse<RegistrationFestivalInfoDto>>(url, cancellationToken);

        if (response?.Success is not true || response.Data is null)
        {
            return 0;
        }

        return response.Data.PassTypeEnabledModules;
    }

    public async Task<IReadOnlyList<BusReservationDto>> GetBusReservationsAsync(string? domain = null, CancellationToken cancellationToken = default)
    {
'@

# 16. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: envolver tarjeta resumen de Competitions
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: envolver tarjeta resumen de Competitions' `
    -Anchor @'
        </div>

        <div class="card shadow-sm">
            <div class="flex items-center gap-4">
                <div class="flex items-center justify-center w-12 h-12 rounded bg-success/10 text-success shrink-0">
                    <i class="ri-trophy-line text-2xl"></i>
                </div>
                <div class="overflow-hidden">
                    <MudText Class="text-muted dark:text-darkmuted">@T.Get("up_competitions")</MudText>
                    <MudText Typo="Typo.h5" Class="text-lg font-bold dark:text-white truncate">
                        @Competitions.Count
                        @if (!string.IsNullOrWhiteSpace(CompetitionsCardSuffix))
                        {
                            <span class="ml-1 text-xs font-normal text-muted dark:text-darkmuted">&middot; @CompetitionsCardSuffix</span>
                        }
                    </MudText>
                </div>
            </div>
        </div>

        <div class="card shadow-sm">
'@ `
    -Replacement @'
        </div>

@if (HasCompetitionsModule)
{
        <div class="card shadow-sm">
            <div class="flex items-center gap-4">
                <div class="flex items-center justify-center w-12 h-12 rounded bg-success/10 text-success shrink-0">
                    <i class="ri-trophy-line text-2xl"></i>
                </div>
                <div class="overflow-hidden">
                    <MudText Class="text-muted dark:text-darkmuted">@T.Get("up_competitions")</MudText>
                    <MudText Typo="Typo.h5" Class="text-lg font-bold dark:text-white truncate">
                        @Competitions.Count
                        @if (!string.IsNullOrWhiteSpace(CompetitionsCardSuffix))
                        {
                            <span class="ml-1 text-xs font-normal text-muted dark:text-darkmuted">&middot; @CompetitionsCardSuffix</span>
                        }
                    </MudText>
                </div>
            </div>
        </div>
}

        <div class="card shadow-sm">
'@

# 17. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: envolver seccion principal de Competitions
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: envolver seccion principal de Competitions' `
    -Anchor @'
        <div class="card shadow-sm">
            <div class="flex items-center justify-between gap-4 mb-4">
                <div class="mb-5" id="competitions">
                    <h2 class="text-lg font-bold text-black dark:text-white">@T.Get("up_competitions")</h2>
                </div>
                <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85]" @onclick="OpenCreateCompetitionModal">
                    @T.Get("up_register_competition")
                </button>
            </div>

            @if (!string.IsNullOrWhiteSpace(CompetitionSuccessMessage))
            {
                <div class="p-3 mb-4 text-sm rounded bg-success/10 text-success">
                    @CompetitionSuccessMessage
                </div>
            }

            @if (!string.IsNullOrWhiteSpace(CompetitionErrorMessage))
            {
                <div class="p-3 mb-4 text-sm rounded bg-danger/10 text-danger">
                    @CompetitionErrorMessage
                </div>
            }

            <div class="overflow-x-auto">
                <table class="w-full table-hover">
                    <thead class="bg-gray-50 dark:bg-dark">
                        <tr class="text-left">
                            <th class="px-4 py-3 font-semibold">@T.Get("up_competition")</th>
                            <th class="px-4 py-3 font-semibold">@T.Get("up_level")</th>
                            <th class="px-4 py-3 font-semibold">@T.Get("up_role")</th>
                            <th class="px-4 py-3 font-semibold">@T.Get("up_status")</th>
                            <th class="px-4 py-3 font-semibold text-right">@T.Get("up_actions")</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (CompetitionEntryDto competition in Competitions)
                        {
                            <tr class="border-b border-black/10 dark:border-darkborder">
                                <td class="px-4 py-3">@GetCompetitionName(competition.CompetitionId)</td>
                                <td class="px-4 py-3">@GetCompetitionLevel(competition)</td>
                                <td class="px-4 py-3">@competition.DanceRole.ToString()</td>
                                <td class="px-4 py-3">
                                    <span class="@GetCompetitionStatusClass(@competition.Status.ToString())">
                                        @competition.Status
                                    </span>
                                </td>
                                <td class="px-4 py-3 text-right">
                                    <button type="button" class="text-black dark:text-white/80" 
                                        @onclick="() => OpenEditCompetitionModal(competition)">
                                        <i class="ri-pencil-line text-lg"></i>
                                    </button>
                                    <button type="button" class="text-danger"
                                        @onclick="() => OpenDeleteCompetitionModal(competition)">
                                        <i class="ri-delete-bin-line text-lg"></i>
                                    </button>
                                </td>
                            </tr>
                        }

                        @if (Competitions.Count == 0)
                        {
                            <tr>
                                <td colspan="4" class="px-4 py-6 text-center text-black/50 dark:text-white/60">@T.Get("up_no_competition")</td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        </div>

@if (HasAccommodationModule)
'@ `
    -Replacement @'
@if (HasCompetitionsModule)
{
        <div class="card shadow-sm">
            <div class="flex items-center justify-between gap-4 mb-4">
                <div class="mb-5" id="competitions">
                    <h2 class="text-lg font-bold text-black dark:text-white">@T.Get("up_competitions")</h2>
                </div>
                <button type="button" class="btn bg-purple border-purple text-white hover:bg-purple/[0.85] hover:border-purple/[0.85]" @onclick="OpenCreateCompetitionModal">
                    @T.Get("up_register_competition")
                </button>
            </div>

            @if (!string.IsNullOrWhiteSpace(CompetitionSuccessMessage))
            {
                <div class="p-3 mb-4 text-sm rounded bg-success/10 text-success">
                    @CompetitionSuccessMessage
                </div>
            }

            @if (!string.IsNullOrWhiteSpace(CompetitionErrorMessage))
            {
                <div class="p-3 mb-4 text-sm rounded bg-danger/10 text-danger">
                    @CompetitionErrorMessage
                </div>
            }

            <div class="overflow-x-auto">
                <table class="w-full table-hover">
                    <thead class="bg-gray-50 dark:bg-dark">
                        <tr class="text-left">
                            <th class="px-4 py-3 font-semibold">@T.Get("up_competition")</th>
                            <th class="px-4 py-3 font-semibold">@T.Get("up_level")</th>
                            <th class="px-4 py-3 font-semibold">@T.Get("up_role")</th>
                            <th class="px-4 py-3 font-semibold">@T.Get("up_status")</th>
                            <th class="px-4 py-3 font-semibold text-right">@T.Get("up_actions")</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (CompetitionEntryDto competition in Competitions)
                        {
                            <tr class="border-b border-black/10 dark:border-darkborder">
                                <td class="px-4 py-3">@GetCompetitionName(competition.CompetitionId)</td>
                                <td class="px-4 py-3">@GetCompetitionLevel(competition)</td>
                                <td class="px-4 py-3">@competition.DanceRole.ToString()</td>
                                <td class="px-4 py-3">
                                    <span class="@GetCompetitionStatusClass(@competition.Status.ToString())">
                                        @competition.Status
                                    </span>
                                </td>
                                <td class="px-4 py-3 text-right">
                                    <button type="button" class="text-black dark:text-white/80" 
                                        @onclick="() => OpenEditCompetitionModal(competition)">
                                        <i class="ri-pencil-line text-lg"></i>
                                    </button>
                                    <button type="button" class="text-danger"
                                        @onclick="() => OpenDeleteCompetitionModal(competition)">
                                        <i class="ri-delete-bin-line text-lg"></i>
                                    </button>
                                </td>
                            </tr>
                        }

                        @if (Competitions.Count == 0)
                        {
                            <tr>
                                <td colspan="4" class="px-4 py-6 text-center text-black/50 dark:text-white/60">@T.Get("up_no_competition")</td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        </div>
}

@if (HasAccommodationModule)
'@

# 18. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: EnabledPassTypeModules + HasCompetitionsModule + AND en Accommodation
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: EnabledPassTypeModules + HasCompetitionsModule + AND en Accommodation' `
    -Anchor @'
    // ==================== Accommodation ====================
    private const int AccommodationModuleFlag = 2;
    private int EnabledFestivalModules { get; set; }
    private bool HasAccommodationModule => (EnabledFestivalModules & AccommodationModuleFlag) != 0;
'@ `
    -Replacement @'
    // ==================== Accommodation ====================
    private const int AccommodationModuleFlag = 2;
    private const int CompetitionsModuleFlag = 1;
    private int EnabledFestivalModules { get; set; }
    private int EnabledPassTypeModules { get; set; }
    private bool HasAccommodationModule => (EnabledFestivalModules & AccommodationModuleFlag) != 0 && (EnabledPassTypeModules & AccommodationModuleFlag) != 0;
    private bool HasCompetitionsModule => (EnabledFestivalModules & CompetitionsModuleFlag) != 0 && (EnabledPassTypeModules & CompetitionsModuleFlag) != 0;
'@

# 19. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: AND en HasMealsModule
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: AND en HasMealsModule' `
    -Anchor @'
    // ==================== Meals ====================
    private const int MealsModuleFlag = 8;
    private bool HasMealsModule => (EnabledFestivalModules & MealsModuleFlag) != 0;
'@ `
    -Replacement @'
    // ==================== Meals ====================
    private const int MealsModuleFlag = 8;
    private bool HasMealsModule => (EnabledFestivalModules & MealsModuleFlag) != 0 && (EnabledPassTypeModules & MealsModuleFlag) != 0;
'@

# 20. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: AND en HasTransportModule
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: AND en HasTransportModule' `
    -Anchor @'
    // ==================== Buses ====================
    private const int TransportModuleFlag = 4;
    private bool HasTransportModule => (EnabledFestivalModules & TransportModuleFlag) != 0;
'@ `
    -Replacement @'
    // ==================== Buses ====================
    private const int TransportModuleFlag = 4;
    private bool HasTransportModule => (EnabledFestivalModules & TransportModuleFlag) != 0 && (EnabledPassTypeModules & TransportModuleFlag) != 0;
'@

# 21. Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor -- UserPanel.razor: LoadFestivalModulesAsync tambien carga EnabledPassTypeModules
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/UserPanelDashboard/UserPanel.razor' `
    -Description 'UserPanel.razor: LoadFestivalModulesAsync tambien carga EnabledPassTypeModules' `
    -Anchor @'
            EnabledFestivalModules = await UserPanelApiClient.GetEnabledFestivalModulesAsync(CurrentDomain);
        }
'@ `
    -Replacement @'
            EnabledFestivalModules = await UserPanelApiClient.GetEnabledFestivalModulesAsync(CurrentDomain);
            EnabledPassTypeModules = await UserPanelApiClient.GetEnabledPassTypeModulesAsync(CurrentDomain);
        }
'@


# ============================================================================
# EJECUCION: validar TODO primero (todo o nada), luego aplicar
# ============================================================================

Write-Host ""
Write-Host "Validando $($script:Plan.Count) cambios contra los archivos locales..." -ForegroundColor Cyan

foreach ($op in $script:Plan) {
    $err = Test-PatchOperation -Op $op
    if ($err) {
        $script:PlanErrors += $err
    }
}

if ($script:PlanErrors.Count -gt 0) {
    Write-Host ""
    Write-Host "NO se ha escrito nada. $($script:PlanErrors.Count) problema(s) encontrados:" -ForegroundColor Red
    foreach ($e in $script:PlanErrors) {
        Write-Host "  - $e" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "Si alguno de estos archivos se ha editado desde que se genero este script," -ForegroundColor Yellow
    Write-Host "dimelo y ajusto el script." -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

Write-Host "Todo valida OK. Aplicando cambios..." -ForegroundColor Cyan
Write-Host ""

foreach ($op in $script:Plan) {
    Invoke-PatchOperation -Op $op
}

Write-Host ""
Write-Host "Codigo listo. AHORA FALTA LA MIGRACION -- pasos 2 a 6 del cabecero de este" -ForegroundColor Cyan
Write-Host "script (dotnet ef migrations add / database update / dotnet build)." -ForegroundColor Cyan
Write-Host "No lo olvides: sin la migracion la columna EnabledModules no existe en la" -ForegroundColor Yellow
Write-Host "base de datos y la app fallara al consultar Pass Types." -ForegroundColor Yellow
Write-Host ""