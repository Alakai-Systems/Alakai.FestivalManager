# =====================================================================
# Alakai FestivalManager - Edit Competition Entry modal completo
# Anade Competition, Level, Role y Notes al modal de edicion del admin,
# igual que el modal del usuario. Status e Internal Notes se mantienen.
# USO: desde la raiz del repo -> .\competition-entry-edit-modal.ps1
# =====================================================================

$ErrorActionPreference = "Stop"
Write-Host "Trabajando en: $(Get-Location)" -ForegroundColor Cyan

$path = "Alakai.FestivalManager.Admin/Components/Pages/CompetitionEntries.razor"

if (-not (Test-Path $path)) {
    Write-Host "ERROR: no se encontro $path" -ForegroundColor Red
    exit 1
}

$content = Get-Content $path -Raw

# Sustituir el cuerpo del modal (solo el <div class="p-5 space-y-4"> hasta su cierre)
$old = @'
                <div class="p-5 space-y-4">
                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Status</label>
                        <select class="form-select" @bind="editModel.Status">
                            <option value="@CompetitionEntryStatus.Registered">Registered</option>
                            <option value="@CompetitionEntryStatus.Confirmed">Confirmed</option>
                            <option value="@CompetitionEntryStatus.WaitingPartner">Waiting Partner</option>
                            <option value="@CompetitionEntryStatus.Cancelled">Cancelled</option>
                        </select>
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Internal Notes</label>
                        <InputTextArea class="form-input" rows="3" @bind-Value="editModel.InternalNotes" />
                    </div>

                </div>
'@

$new = @'
                <div class="p-5 space-y-4">
                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Competition</label>
                        <select class="form-select" @bind="editModel.CompetitionId" @bind:after="OnEditCompetitionChanged">
                            @foreach (CompetitionDto competition in competitions)
                            {
                                <option value="@competition.Id">@competition.Name</option>
                            }
                        </select>
                    </div>

                    @if (EditSelectedCompetitionHasLevels)
                    {
                        <div>
                            <label class="block text-sm text-black/60 dark:text-white/60">Level</label>
                            <select class="form-select" @bind="editModel.SelectedLevelId">
                                <option value="@Guid.Empty">Select level</option>
                                @foreach (CompetitionLevelDto level in EditSelectedCompetitionLevels)
                                {
                                    <option value="@level.Id">@level.Name</option>
                                }
                            </select>
                        </div>
                    }

                    @if (EditSelectedCompetitionRequiresRole)
                    {
                        <div>
                            <label class="block text-sm text-black/60 dark:text-white/60">Role</label>
                            <select class="form-select" @bind="editModel.DanceRole">
                                <option value="@DanceRole.Leader">Leader</option>
                                <option value="@DanceRole.Follower">Follower</option>
                                <option value="@DanceRole.Individual">Individual</option>
                            </select>
                        </div>
                    }

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Status</label>
                        <select class="form-select" @bind="editModel.Status">
                            <option value="@CompetitionEntryStatus.Registered">Registered</option>
                            <option value="@CompetitionEntryStatus.Confirmed">Confirmed</option>
                            <option value="@CompetitionEntryStatus.WaitingPartner">Waiting Partner</option>
                            <option value="@CompetitionEntryStatus.Cancelled">Cancelled</option>
                        </select>
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Notes</label>
                        <InputTextArea class="form-input" rows="2" @bind-Value="editModel.Notes" />
                    </div>

                    <div>
                        <label class="block text-sm text-black/60 dark:text-white/60">Internal Notes</label>
                        <InputTextArea class="form-input" rows="2" @bind-Value="editModel.InternalNotes" />
                    </div>
                </div>
'@

if ($content.Contains($old)) {
    $content = $content.Replace($old, $new)
    Write-Host "  OK: modal HTML expandido" -ForegroundColor Green
} else {
    Write-Host "  FALLO: no se encontro el bloque del modal. Revisa manualmente." -ForegroundColor Red
    exit 1
}

# Anadir SelectedLevelId al CompetitionEntryEditModel
$oldModel = @'
    private class CompetitionEntryEditModel
    {
        public Guid CompetitionId { get; set; }
        public Guid RegistrationId { get; set; }
        public Guid? PartnerRegistrationId { get; set; }
        public Guid CompetitionCapacityId { get; set; }
        public DanceRole? DanceRole { get; set; }
        public string? Notes { get; set; }
        public string? InternalNotes { get; set; }
        public CompetitionEntryStatus Status { get; set; }
        public DateTime? CancelledAt { get; set; }
        public bool IsActive { get; set; }
    }
'@

$newModel = @'
    private class CompetitionEntryEditModel
    {
        public Guid CompetitionId { get; set; }
        public Guid RegistrationId { get; set; }
        public Guid? PartnerRegistrationId { get; set; }
        public Guid CompetitionCapacityId { get; set; }
        public Guid SelectedLevelId { get; set; }
        public DanceRole? DanceRole { get; set; }
        public string? Notes { get; set; }
        public string? InternalNotes { get; set; }
        public CompetitionEntryStatus Status { get; set; }
        public DateTime? CancelledAt { get; set; }
        public bool IsActive { get; set; }
    }
'@

if ($content.Contains($oldModel)) {
    $content = $content.Replace($oldModel, $newModel)
    Write-Host "  OK: SelectedLevelId anadido al model" -ForegroundColor Green
} else {
    Write-Host "  FALLO: no se encontro el bloque del model class." -ForegroundColor Red
    exit 1
}

# Anadir propiedades computed y OnEditCompetitionChanged antes de OpenEditModal
$oldOpen = @'
    private void OpenEditModal(CompetitionEntryDto entry)
    {
        editingEntry = entry;
        editModel = new CompetitionEntryEditModel
        {
            CompetitionId = entry.CompetitionId,
            RegistrationId = entry.RegistrationId,
            PartnerRegistrationId = entry.PartnerRegistrationId,
            CompetitionCapacityId = entry.CompetitionCapacityId,
            DanceRole = entry.DanceRole,
            Notes = entry.Notes,
            InternalNotes = entry.InternalNotes,
            Status = entry.Status,
            CancelledAt = entry.CancelledAt,
            IsActive = entry.IsActive
        };
    }
'@

$newOpen = @'
    private CompetitionDto? EditSelectedCompetition =>
        competitions.FirstOrDefault(c => c.Id == editModel.CompetitionId);

    private bool EditSelectedCompetitionRequiresRole =>
        EditSelectedCompetition?.RequiresRole ?? false;

    private IReadOnlyList<CompetitionLevelDto> EditSelectedCompetitionLevels =>
        EditSelectedCompetition?.Levels.OrderBy(l => l.SortOrder).ToList() ?? [];

    private bool EditSelectedCompetitionHasLevels =>
        EditSelectedCompetitionLevels.Count > 0;

    private void OnEditCompetitionChanged()
    {
        editModel.SelectedLevelId = Guid.Empty;
        editModel.DanceRole = null;
        ResolveEditCapacityId();
    }

    private void ResolveEditCapacityId()
    {
        CompetitionDto? comp = EditSelectedCompetition;

        if (comp is null)
        {
            editModel.CompetitionCapacityId = Guid.Empty;
            return;
        }

        Guid? levelId = editModel.SelectedLevelId != Guid.Empty ? editModel.SelectedLevelId : null;

        CompetitionCapacityDto? cap = comp.Capacities.FirstOrDefault(c =>
            c.IsActive &&
            c.CompetitionLevelId == levelId &&
            (!comp.RequiresRole || c.DanceRole == editModel.DanceRole));

        editModel.CompetitionCapacityId = cap?.Id ?? Guid.Empty;
    }

    private void OpenEditModal(CompetitionEntryDto entry)
    {
        editingEntry = entry;

        CompetitionDto? comp = competitions.FirstOrDefault(c => c.Id == entry.CompetitionId);
        CompetitionCapacityDto? cap = comp?.Capacities.FirstOrDefault(c => c.Id == entry.CompetitionCapacityId);
        Guid levelId = cap?.CompetitionLevelId ?? Guid.Empty;

        editModel = new CompetitionEntryEditModel
        {
            CompetitionId = entry.CompetitionId,
            RegistrationId = entry.RegistrationId,
            PartnerRegistrationId = entry.PartnerRegistrationId,
            CompetitionCapacityId = entry.CompetitionCapacityId,
            SelectedLevelId = levelId,
            DanceRole = entry.DanceRole,
            Notes = entry.Notes,
            InternalNotes = entry.InternalNotes,
            Status = entry.Status,
            CancelledAt = entry.CancelledAt,
            IsActive = entry.IsActive
        };
    }
'@

if ($content.Contains($oldOpen)) {
    $content = $content.Replace($oldOpen, $newOpen)
    Write-Host "  OK: propiedades computed y OpenEditModal actualizados" -ForegroundColor Green
} else {
    Write-Host "  FALLO: no se encontro OpenEditModal." -ForegroundColor Red
    exit 1
}

# Actualizar SaveEditAsync para resolver la capacidad antes de guardar
$oldSave = @'
        try
        {
            UpdateCompetitionEntryRequest request = new()
            {
                CompetitionId = editModel.CompetitionId,
                RegistrationId = editModel.RegistrationId,
                PartnerRegistrationId = editModel.PartnerRegistrationId,
                CompetitionCapacityId = editModel.CompetitionCapacityId,
                DanceRole = editModel.DanceRole,
                Notes = editModel.Notes,
                InternalNotes = editModel.InternalNotes,
                Status = editModel.Status,
                CancelledAt = editModel.CancelledAt,
                IsActive = editModel.IsActive
            };

            await CompetitionEntryApiClient.UpdateAsync(editingEntry.Id, request);
'@

$newSave = @'
        try
        {
            // Resolve the correct capacity from the selected level + role before saving
            ResolveEditCapacityId();

            if (editModel.CompetitionCapacityId == Guid.Empty)
            {
                ShowError("Could not find a matching capacity for the selected level/role. Please check the competition configuration.");
                return;
            }

            UpdateCompetitionEntryRequest request = new()
            {
                CompetitionId = editModel.CompetitionId,
                RegistrationId = editModel.RegistrationId,
                PartnerRegistrationId = editModel.PartnerRegistrationId,
                CompetitionCapacityId = editModel.CompetitionCapacityId,
                DanceRole = editModel.DanceRole,
                Notes = editModel.Notes,
                InternalNotes = editModel.InternalNotes,
                Status = editModel.Status,
                CancelledAt = editModel.CancelledAt,
                IsActive = editModel.IsActive
            };

            await CompetitionEntryApiClient.UpdateAsync(editingEntry.Id, request);
'@

if ($content.Contains($oldSave)) {
    $content = $content.Replace($oldSave, $newSave)
    Write-Host "  OK: SaveEditAsync resuelve capacidad antes de guardar" -ForegroundColor Green
} else {
    Write-Host "  FALLO: no se encontro el bloque de SaveEditAsync." -ForegroundColor Red
    exit 1
}

Set-Content $path -Value $content -Encoding UTF8
Write-Host ""
Write-Host "=====================================================================" -ForegroundColor Cyan
Write-Host "Listo. El modal de edicion ahora incluye:" -ForegroundColor Cyan
Write-Host "  - Selector de Competition" -ForegroundColor Yellow
Write-Host "  - Selector de Level (si la competicion tiene niveles)" -ForegroundColor Yellow
Write-Host "  - Selector de Role (si RequiresRole)" -ForegroundColor Yellow
Write-Host "  - Notes + Internal Notes + Status (como antes)" -ForegroundColor Yellow
Write-Host "Compila y prueba." -ForegroundColor Yellow
Write-Host "=====================================================================" -ForegroundColor Cyan