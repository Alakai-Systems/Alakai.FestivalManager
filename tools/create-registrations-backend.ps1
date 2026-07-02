# =====================================================================
# Alakai FestivalManager - Arreglo definitivo: Settings.razor + EmailLogs.razor
#
# Este script SOBRESCRIBE estos dos archivos por completo, sin
# comprobacion de "ya esta actualizado" -- dado que los ultimos
# parches se aplicaron sobre una version base incorrecta, prefiero
# forzar el estado correcto de una vez que seguir arriesgando con
# otro parche a ciegas.
#
# Settings.razor: grid de 2 columnas (Create Admin User / Change My
# Password), listado de Admins con boton Save (estilo "+ New X") +
# Delete (icono + modal), badge "This is you", PageHeader, avisos de
# exito/error que desaparecen a los 3.5s.
#
# EmailLogs.razor: arreglo real de la paginacion -- el archivo
# original nunca aplicaba Skip/Take, asi que "rows" era solo
# cosmetico. Anadido selector de filas por pagina + PagedLogs.
#
# USO: desde la raiz del repo -> .\tools\fix-settings-and-emaillogs.ps1
# =====================================================================

$ErrorActionPreference = "Stop"
Write-Host "Trabajando en: $(Get-Location)" -ForegroundColor Cyan

$adminRoot = "Alakai.FestivalManager.Admin"

if (-not (Test-Path $adminRoot)) {
    Write-Host "ERROR: no se encontro la carpeta '$adminRoot'. Ejecuta este script desde la raiz del repo." -ForegroundColor Red
    exit 1
}

function Require-Path {
    param([string]$Path)
    if (-not (Test-Path $Path)) {
        Write-Host "ERROR: no se encontro $Path" -ForegroundColor Red
        exit 1
    }
}

function Write-ForceFile {
    param(
        [string]$Path,
        [string]$Content,
        [string]$Description
    )

    Require-Path (Split-Path $Path -Parent)
    $Content | Set-Content -Path $Path -Encoding UTF8 -NoNewline
    Write-Host "  Sobrescrito: $Path ($Description)" -ForegroundColor Green
}

Write-Host ""
$settingsPath = Join-Path $adminRoot "Components\Pages\Settings.razor"
@'
@page "/settings"
@using Alakai.FestivalManager.Admin.Components.Layout

@inject UserApiClient UserApiClient
@inject IAuthApiClient AuthApiClient
@inject IAdminTokenProvider AdminTokenProvider

<PageTitle>Settings</PageTitle>

<PageHeader Title="Settings" pTitle="Admin Users"></PageHeader>

@if (!string.IsNullOrWhiteSpace(successMessage))
{
    <div class="p-3 mt-4 text-sm rounded bg-success/10 text-success">@successMessage</div>
}
@if (!string.IsNullOrWhiteSpace(errorMessage))
{
    <div class="p-3 mt-4 text-sm rounded bg-danger/10 text-danger">@errorMessage</div>
}

@if (IsSuperAdmin)
{
    <div class="grid grid-cols-1 gap-4 mt-4 lg:grid-cols-2">
        <div class="card">
            <h2 class="mb-4 text-lg font-semibold text-black dark:text-white">Create Admin User</h2>

            <EditForm Model="@CreateModel" OnValidSubmit="CreateAdminAsync">
                <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
                    <div>
                        <InputText @bind-Value="CreateModel.FirstName" placeholder="First name" class="form-input" />
                    </div>
                    <div>
                        <InputText @bind-Value="CreateModel.LastName" placeholder="Last name" class="form-input" />
                    </div>
                    <div class="sm:col-span-2">
                        <InputText @bind-Value="CreateModel.Email" placeholder="Email" class="form-input" />
                    </div>
                    <div>
                        <InputText @bind-Value="CreateModel.Password" type="password" placeholder="Password" class="form-input" />
                    </div>
                    <div>
                        <select class="form-select" @bind="CreateModel.Role">
                            <option value="2">Admin</option>
                            <option value="1">SuperAdmin</option>
                        </select>
                    </div>
                </div>
                <button type="submit" disabled="@IsCreating" class="px-4 py-2 mt-4 text-sm text-white rounded-md bg-purple disabled:opacity-70">
                    @(IsCreating ? "Creating..." : "Create Admin")
                </button>
            </EditForm>
        </div>

        <div class="card">
            <h2 class="mb-4 text-lg font-semibold text-black dark:text-white">Change My Password</h2>

            <EditForm Model="@PasswordModel" OnValidSubmit="ChangePasswordAsync">
                <div class="grid grid-cols-1 gap-4">
                    <div>
                        <InputText @bind-Value="PasswordModel.CurrentPassword" type="password" placeholder="Current password" class="form-input" />
                    </div>
                    <div>
                        <InputText @bind-Value="PasswordModel.NewPassword" type="password" placeholder="New password" class="form-input" />
                    </div>
                    <div>
                        <InputText @bind-Value="PasswordModel.ConfirmPassword" type="password" placeholder="Confirm new password" class="form-input" />
                    </div>
                </div>
                <button type="submit" disabled="@IsChangingPassword" class="px-4 py-2 mt-4 text-sm text-white rounded-md bg-purple disabled:opacity-70">
                    @(IsChangingPassword ? "Saving..." : "Change Password")
                </button>
            </EditForm>
        </div>
    </div>

    <div class="card mt-4">
        <h2 class="mb-4 text-lg font-semibold text-black dark:text-white">Admin Users</h2>

        @if (IsLoadingUsers)
        {
            <p class="text-black/60 dark:text-white/60">Loading...</p>
        }
        else if (AdminUsers.Count == 0)
        {
            <p class="text-black/60 dark:text-white/60">No admin users found.</p>
        }
        else
        {
            <div class="overflow-x-auto">
                <table class="w-full table-hover">
                    <thead class="bg-gray-50 dark:bg-dark">
                        <tr class="text-left">
                            <th class="px-3 py-3 font-semibold">Name</th>
                            <th class="px-3 py-3 font-semibold">Email</th>
                            <th class="px-3 py-3 font-semibold">Role</th>
                            <th class="px-3 py-3 font-semibold">Active</th>
                            <th class="px-3 py-3 font-semibold text-right">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (AdminUserRow row in AdminUsers)
                        {
                            bool isSelf = row.User.Email.Equals(CurrentUserEmail, StringComparison.OrdinalIgnoreCase);

                            <tr class="border-b border-black/10 dark:border-darkborder">
                                <td class="px-4 py-3">@row.User.FullName</td>
                                <td class="px-3 py-3">@row.User.Email</td>
                                <td class="px-3 py-3">
                                    <select class="form-select" @bind="row.SelectedRole" disabled="@isSelf">
                                        <option value="2">Admin</option>
                                        <option value="1">SuperAdmin</option>
                                    </select>
                                </td>
                                <td class="px-3 py-3">
                                    <input type="checkbox" class="form-checkbox" @bind="row.IsActive" disabled="@isSelf" />
                                </td>
                                <td class="px-3 py-3">
                                    <div class="flex items-center justify-end gap-2">
                                        @if (isSelf)
                                        {
                                            <span class="inline-flex items-center gap-1 px-2.5 py-1 text-xs font-medium rounded-full bg-purple/10 text-purple">
                                                <i class="ri-star-smile-line"></i>
                                                This is you
                                            </span>
                                        }
                                        else
                                        {
                                            <button type="button" class="transition-all duration-300 border rounded-md btn text-purple border-purple hover:bg-purple hover:text-white whitespace-nowrap" disabled="@row.IsSaving" @onclick="() => SaveUserAsync(row)">
                                                @(row.IsSaving ? "Saving..." : "Save")
                                            </button>
                                            <button type="button" class="text-danger" title="Delete" @onclick="() => OpenDeleteModal(row.User)"><i class="ri-delete-bin-line text-lg"></i></button>
                                        }
                                    </div>
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        }
    </div>
}
else
{
    <div class="card mt-4 max-w-2xl">
        <h2 class="mb-4 text-lg font-semibold text-black dark:text-white">Change My Password</h2>

        <EditForm Model="@PasswordModel" OnValidSubmit="ChangePasswordAsync">
            <div class="grid grid-cols-1 gap-4 sm:grid-cols-3">
                <div>
                    <InputText @bind-Value="PasswordModel.CurrentPassword" type="password" placeholder="Current password" class="form-input" />
                </div>
                <div>
                    <InputText @bind-Value="PasswordModel.NewPassword" type="password" placeholder="New password" class="form-input" />
                </div>
                <div>
                    <InputText @bind-Value="PasswordModel.ConfirmPassword" type="password" placeholder="Confirm new password" class="form-input" />
                </div>
            </div>
            <button type="submit" disabled="@IsChangingPassword" class="px-4 py-2 mt-4 text-sm text-white rounded-md bg-purple disabled:opacity-70">
                @(IsChangingPassword ? "Saving..." : "Change Password")
            </button>
        </EditForm>
    </div>
}

@if (deletingUser is not null)
{
    <div class="fixed inset-0 bg-black/60 z-[999] overflow-y-auto">
        <div class="flex items-start justify-center min-h-screen px-4 py-10">
            <div class="relative w-[92vw] md:w-[380px] overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="px-5 py-4">
                    <h3 class="text-lg font-semibold text-black dark:text-white">Delete Admin</h3>
                    <p class="mt-2 text-sm text-black/60 dark:text-white/60">Delete @deletingUser.FullName?</p>
                    <p class="mt-2 text-sm text-danger">This cannot be undone.</p>
                </div>
                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isDeleting" @onclick="CloseDeleteModal">Cancel</button>
                    <button type="button" class="btn border border-danger text-danger hover:bg-danger hover:text-white disabled:opacity-50" disabled="@isDeleting" @onclick="ConfirmDeleteAsync">@(isDeleting ? "Deleting..." : "Delete")</button>
                </div>
            </div>
        </div>
    </div>
}

@code {
    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private string? CurrentUserEmail { get; set; }
    private bool IsSuperAdmin { get; set; }

    private ChangePasswordFormModel PasswordModel { get; set; } = new();
    private bool IsChangingPassword { get; set; }

    private List<AdminUserRow> AdminUsers { get; set; } = new();
    private bool IsLoadingUsers { get; set; } = true;

    private CreateAdminFormModel CreateModel { get; set; } = new();
    private bool IsCreating { get; set; }

    private UserDto? deletingUser;
    private bool isDeleting;

    private string? successMessage;
    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationStateTask is not null)
        {
            AuthenticationState authState = await AuthenticationStateTask;
            CurrentUserEmail = authState.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            IsSuperAdmin = authState.User.IsInRole("SuperAdmin");
        }

        if (IsSuperAdmin)
        {
            await LoadAdminUsersAsync();
        }
        else
        {
            IsLoadingUsers = false;
        }
    }

    private async Task LoadAdminUsersAsync()
    {
        IsLoadingUsers = true;

        try
        {
            IReadOnlyList<UserDto> allUsers = await UserApiClient.GetAllAsync();

            AdminUsers = allUsers
                .Where(u => u.Role == 1 || u.Role == 2)
                .Select(u => new AdminUserRow
                {
                    User = u,
                    SelectedRole = u.Role,
                    IsActive = u.IsActive
                })
                .ToList();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            IsLoadingUsers = false;
        }
    }

    private async Task SaveUserAsync(AdminUserRow row)
    {
        row.IsSaving = true;

        try
        {
            UpdateUserRequest request = new()
            {
                FirstName = row.User.FirstName,
                LastName = row.User.LastName,
                Email = row.User.Email,
                Phone = row.User.Phone,
                Country = row.User.Country,
                City = row.User.City,
                PhotoUrl = row.User.PhotoUrl,
                MustChangePassword = row.User.MustChangePassword,
                IsActive = row.IsActive,
                Role = row.SelectedRole
            };

            await UserApiClient.UpdateAsync(row.User.Id, request);
            ShowSuccess($"{row.User.Email} updated successfully.");
            await LoadAdminUsersAsync();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            row.IsSaving = false;
        }
    }

    private void OpenDeleteModal(UserDto user)
    {
        deletingUser = user;
    }

    private void CloseDeleteModal()
    {
        deletingUser = null;
    }

    private async Task ConfirmDeleteAsync()
    {
        if (deletingUser is null)
        {
            return;
        }

        isDeleting = true;

        try
        {
            await UserApiClient.DeleteAsync(deletingUser.Id);
            ShowSuccess($"{deletingUser.Email} deleted successfully.");
            deletingUser = null;
            await LoadAdminUsersAsync();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            isDeleting = false;
        }
    }

    private async Task ChangePasswordAsync()
    {
        if (PasswordModel.NewPassword != PasswordModel.ConfirmPassword)
        {
            ShowError("New password and confirmation do not match.");
            return;
        }

        IsChangingPassword = true;

        try
        {
            string? accessToken = await AdminTokenProvider.GetValidAccessTokenAsync();
            bool success = await AuthApiClient.ChangePasswordAsync(PasswordModel.CurrentPassword, PasswordModel.NewPassword, accessToken);

            if (success)
            {
                ShowSuccess("Password changed successfully.");
                PasswordModel = new();
            }
            else
            {
                ShowError("Current password is incorrect, or your session token has expired (try logging out and back in).");
            }
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            IsChangingPassword = false;
        }
    }

    private async Task CreateAdminAsync()
    {
        IsCreating = true;

        try
        {
            CreateAdminUserRequest request = new()
            {
                FirstName = CreateModel.FirstName,
                LastName = CreateModel.LastName,
                Email = CreateModel.Email,
                Password = CreateModel.Password,
                Role = CreateModel.Role
            };

            await UserApiClient.CreateAdminAsync(request);

            ShowSuccess($"{CreateModel.Email} created successfully.");
            CreateModel = new();
            await LoadAdminUsersAsync();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            IsCreating = false;
        }
    }

    private void ShowSuccess(string message)
    {
        successMessage = message;
        errorMessage = null;
        StateHasChanged();
        InvokeAsync(async () =>
        {
            await Task.Delay(3500);
            successMessage = null;
            await InvokeAsync(StateHasChanged);
        });
    }

    private void ShowError(string message)
    {
        errorMessage = message;
        successMessage = null;
        StateHasChanged();
        InvokeAsync(async () =>
        {
            await Task.Delay(3500);
            errorMessage = null;
            await InvokeAsync(StateHasChanged);
        });
    }

    private class AdminUserRow
    {
        public UserDto User { get; set; } = default!;
        public int SelectedRole { get; set; }
        public bool IsActive { get; set; }
        public bool IsSaving { get; set; }
    }

    private class ChangePasswordFormModel
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    private class CreateAdminFormModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Role { get; set; } = 2;
    }
}
'@ | ForEach-Object { Write-ForceFile -Path $settingsPath -Content $_ -Description "grid 2 columnas + delete icono/modal + PageHeader" }

$emailLogsPath = Join-Path $adminRoot "Components\Pages\EmailLogs.razor"
@'
@page "/email-logs"
@using Alakai.FestivalManager.Admin.Components.Layout
@inject EmailLogApiClient EmailLogApiClient

<PageHeader Title="Communication" pTitle="Email Logs"></PageHeader>

<div class="flex flex-col gap-4 min-h-[calc(100vh-212px)]">
    <div class="card">
        <div class="flex flex-col gap-4 mb-5 xl:flex-row xl:items-center xl:justify-between">

            <div class="flex flex-col gap-3 md:flex-row md:items-center">
                <input class="form-input md:w-72" placeholder="Search recipient or subject..." @bind="searchText" @bind:event="oninput" @bind:after="ResetPage" />
                <select class="form-select md:w-40" @bind="statusFilter" @bind:after="ResetPage">
                    <option value="">All statuses</option>
                    <option value="@EmailLogStatus.Pending">Pending</option>
                    <option value="@EmailLogStatus.Sent">Sent</option>
                    <option value="@EmailLogStatus.Failed">Failed</option>
                    <option value="@EmailLogStatus.Skipped">Skipped</option>
                </select>
                <select class="form-select md:w-32" @bind="pageSize" @bind:after="ResetPage">
                    <option value="10">10 rows</option>
                    <option value="25">25 rows</option>
                    <option value="50">50 rows</option>
                </select>
            </div>
        </div>

        @if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            <div class="p-3 mb-4 text-sm rounded bg-danger/10 text-danger">@errorMessage</div>
        }

        @if (isLoading)
        {
            <p class="text-sm text-black/50 dark:text-white/40">Loading email logs...</p>
        }
        else
        {
            <div class="overflow-x-auto">
                <table class="w-full table-hover">
                    <thead class="bg-gray-50 dark:bg-dark">
                        <tr class="text-left">
                            <th class="px-4 py-3 font-semibold">Recipient</th>
                            <th class="px-4 py-3 font-semibold">Template</th>
                            <th class="px-4 py-3 font-semibold">Status</th>
                            <th class="px-4 py-3 font-semibold">Sent At</th>
                            <th class="px-4 py-3 font-semibold">Error</th>
                            <th class="px-4 py-3 font-semibold text-right">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        @if (PagedLogs.Count == 0)
                        {
                            <tr>
                                <td colspan="6" class="px-4 py-6 text-center text-black/50 dark:text-white/40">No email logs found.</td>
                            </tr>
                        }
                        else
                        {
                            @foreach (EmailLogDto log in PagedLogs)
                            {
                                <tr class="border-b border-black/10 dark:border-darkborder">
                                    <td class="px-4 py-3">
                                        <div class="font-medium">@log.RecipientEmail</div>
                                        <div class="text-xs text-black/50">@log.RecipientName</div>
                                    </td>
                                    <td class="px-4 py-3">@log.TemplateKey</td>
                                    <td class="px-4 py-3"><span class="@GetStatusClass(log.Status)">@log.Status</span></td>
                                    <td class="px-4 py-3">@(log.SentAt.HasValue ? log.SentAt.Value.ToString("dd/MM/yyyy HH:mm") : "-")</td>
                                    <td class="px-4 py-3">@log.ErrorMessage</td>
                                    <td class="px-4 py-3 text-right">
                                        <button type="button" class="text-black dark:text-white/80" title="Preview" @onclick="() => OpenPreview(log)">
                                            <i class="ri-eye-line text-lg"></i>
                                        </button>
                                    </td>
                                </tr>
                            }
                        }
                    </tbody>
                </table>
            </div>

            <div class="flex items-center justify-between mt-4">
                <p class="text-sm text-black/50 dark:text-white/40">
                    Showing @ShowingFrom to @ShowingTo of @FilteredLogs.Count logs
                </p>

                <div class="flex items-center gap-2">
                    <button type="button" class="px-4 py-2 text-sm border rounded-md border-black/10 disabled:opacity-50" disabled="@(currentPage == 1)" @onclick="PreviousPage">Previous</button>
                    <span class="flex items-center justify-center w-10 h-10 text-sm rounded-md bg-purple/10 text-purple">@currentPage</span>
                    <button type="button" class="px-4 py-2 text-sm border rounded-md border-black/10 disabled:opacity-50" disabled="@(currentPage >= TotalPages)" @onclick="NextPage">Next</button>
                </div>
            </div>
        }
    </div>
</div>

@if (previewingLog is not null)
{
    <div class="fixed inset-0 bg-black/60 z-[999] overflow-y-auto">
        <div class="flex items-start justify-center min-h-screen px-4 py-10">
            <div class="relative mx-auto overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder" style="width:760px; max-width:95vw;">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <div>
                        <h3 class="text-lg font-semibold text-black dark:text-white">@previewingLog.Subject</h3>
                        <p class="text-xs text-black/50 dark:text-white/40">@previewingLog.RecipientEmail - @previewingLog.TemplateKey</p>
                    </div>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="ClosePreview"><i class="ri-close-line text-2xl"></i></button>
                </div>

                <div class="p-2 flex justify-center bg-gray-100">
                    <iframe srcdoc="@previewingLog.BodyHtml" style="width:680px; max-width:100%; height:70vh; border:1px solid #e5e7eb; border-radius:6px; background:#fff;"></iframe>
                </div>
            </div>
        </div>
    </div>
}

@code {
    private List<EmailLogDto> logs = [];
    private EmailLogDto? previewingLog;
    private string searchText = string.Empty;
    private string statusFilter = string.Empty;
    private bool isLoading = true;
    private string? errorMessage;
    private int pageSize = 10;
    private int currentPage = 1;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            logs = (await EmailLogApiClient.GetAllAsync()).OrderByDescending(l => l.SentAt).ToList();
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            isLoading = false;
        }
    }

    private List<EmailLogDto> FilteredLogs
    {
        get
        {
            IEnumerable<EmailLogDto> query = logs;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(l => l.RecipientEmail.Contains(searchText, StringComparison.OrdinalIgnoreCase) || l.Subject.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            if (Enum.TryParse(statusFilter, out EmailLogStatus status))
            {
                query = query.Where(l => l.Status == status);
            }

            return query.ToList();
        }
    }

    private List<EmailLogDto> PagedLogs => FilteredLogs.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

    private int TotalPages => Math.Max(1, (int)Math.Ceiling((double)FilteredLogs.Count / pageSize));
    private int ShowingFrom => FilteredLogs.Count == 0 ? 0 : ((currentPage - 1) * pageSize) + 1;
    private int ShowingTo => Math.Min(currentPage * pageSize, FilteredLogs.Count);

    private void ResetPage()
    {
        currentPage = 1;
        NormalizePage();
    }

    private void NormalizePage()
    {
        if (currentPage > TotalPages)
        {
            currentPage = TotalPages;
        }

        if (currentPage < 1)
        {
            currentPage = 1;
        }
    }

    private void PreviousPage()
    {
        if (currentPage > 1) currentPage--;
    }

    private void NextPage()
    {
        if (currentPage < TotalPages) currentPage++;
    }

    private static string GetStatusClass(EmailLogStatus status)
    {
        return status switch
        {
            EmailLogStatus.Sent => "inline-block rounded text-xs px-2 py-1 bg-success/10 text-success",
            EmailLogStatus.Pending => "inline-block rounded text-xs px-2 py-1 bg-warning/10 text-warning",
            EmailLogStatus.Failed => "inline-block rounded text-xs px-2 py-1 bg-danger/10 text-danger",
            _ => "inline-block rounded text-xs px-2 py-1 bg-black/10 text-black"
        };
    }

    private void OpenPreview(EmailLogDto log)
    {
        previewingLog = log;
    }

    private void ClosePreview()
    {
        previewingLog = null;
    }
}
'@ | ForEach-Object { Write-ForceFile -Path $emailLogsPath -Content $_ -Description "paginacion real (Skip/Take) + filas por pagina" }

Write-Host ""
Write-Host "Completado." -ForegroundColor Cyan
Write-Host "Reinicia el Admin. Comprueba: Settings con el grid de 2 columnas, delete con modal," -ForegroundColor Cyan
Write-Host "y en Email Logs que cambiar 'rows' realmente recorta las filas visibles." -ForegroundColor Cyan