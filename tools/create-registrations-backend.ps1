# =====================================================================
# Alakai FestivalManager - Correcciones de UX en Settings/Profile/Topbar
# PASO F
#
# Arregla, en este orden de importancia:
#   1. CRITICO: el login redirigia a /dashboard, que no existe (la
#      ruta real del dashboard es "/"). Por eso dabas 404 tras loguear
#      y solo funcionaba poniendo la URL a mano.
#   2. El engranaje del Topbar ya no abre ningun modal -- es un enlace
#      directo a /settings.
#   3. La foto de perfil se sincroniza entre /profile y el Topbar (via
#      un servicio compartido nuevo, UserProfileState) sin necesidad
#      de recargar la pagina.
#   4. Los avisos de exito/error en Settings y Profile ahora usan el
#      mismo patron ShowSuccess/ShowError de 3.5 segundos que ya usan
#      el resto de paginas (Users.razor, etc.), en vez de quedarse fijos.
#   5. Settings.razor rehecho: Create Admin User y Change My Password
#      comparten fila (grid de 2 columnas), listado de Admins debajo.
#      El boton Delete es un icono + modal de confirmacion identico al
#      que ya usa Users.razor (no un simple texto). El boton Save por
#      fila usa el mismo estilo que los botones "+ New X" del resto de
#      listados. "This is you" es un badge, no texto plano.
#
# REQUIERE los pasos A a E aplicados antes.
# USO: desde la raiz del repo -> .\tools\admin-ux-fixes-stepF.ps1
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

function Write-FullFile {
    param(
        [string]$Path,
        [string]$Content,
        [string]$IdempotencyMarker,
        [string]$Description
    )

    Require-Path (Split-Path $Path -Parent)

    if (Test-Path $Path) {
        $current = Get-Content $Path -Raw
        if ($current.Contains($IdempotencyMarker)) {
            Write-Host "  Ya actualizado: $Path ($Description)" -ForegroundColor Yellow
            return
        }
    }

    $Content | Set-Content -Path $Path -Encoding UTF8 -NoNewline
    Write-Host "  Escrito: $Path ($Description)" -ForegroundColor Green
}

function Write-NewFile {
    param(
        [string]$Path,
        [string]$Content,
        [string]$Description
    )

    $dir = Split-Path $Path -Parent
    New-Item -ItemType Directory -Force -Path $dir | Out-Null

    if (Test-Path $Path) {
        Write-Host "  Ya existe: $Path, no se toca." -ForegroundColor Yellow
        return
    }

    $Content | Set-Content -Path $Path -Encoding UTF8 -NoNewline
    Write-Host "  Creado: $Path ($Description)" -ForegroundColor Green
}

Write-Host ""
Write-Host "--- 1. Archivos actualizados ---" -ForegroundColor Cyan

$targetPath = Join-Path $adminRoot "Endpoints\AdminAuthEndpoints.cs"
@'
namespace Alakai.FestivalManager.Admin.Endpoints;

public static class AdminAuthEndpoints
{
    public static void MapAdminAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/account/login", async (
            HttpContext httpContext,
            IAuthApiClient authApiClient,
            [FromForm] string email,
            [FromForm] string password,
            [FromForm] string? returnUrl) =>
        {
            try
            {
                LoginResponse? response = await authApiClient.LoginAsync(new LoginRequest { Email = email, Password = password });

                if (response?.Auth is null)
                {
                    return Results.Redirect("/login?error=invalid");
                }

                AuthUserDto user = response.Auth.User;
                AdminUserRole role = (AdminUserRole)user.Role;

                if (role != AdminUserRole.SuperAdmin && role != AdminUserRole.Admin)
                {
                    return Results.Redirect("/login?error=forbidden");
                }

                List<Claim> claims =
                [
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.Role, role.ToString()),
                    new("access_token", response.Auth.AccessToken),
                    new("refresh_token", response.Auth.RefreshToken),
                    new("access_token_expires", response.Auth.ExpiresAt.ToString("o"))
                ];

                ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                ClaimsPrincipal principal = new(identity);

                AuthenticationProperties authProperties = new()
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                string redirectTo = string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith('/')
                    ? "/"
                    : returnUrl;

                return Results.Redirect(redirectTo);
            }
            catch (Exception)
            {
                return Results.Redirect("/login?error=invalid");
            }
        }).AllowAnonymous();

        app.MapPost("/account/logout", async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/login");
        }).AllowAnonymous();
    }
}
'@ | ForEach-Object { Write-FullFile -Path $targetPath -Content $_ -IdempotencyMarker "? "/"
                    : returnUrl" -Description "arregla redirect roto (/dashboard -> /)" }

$targetPath = Join-Path $adminRoot "Components\Layout\Topbar.razor"
@'
@implements IDisposable
@inject IJSRuntime JsRuntime
@inject UserApiClient UserApiClient
@inject UserProfileState UserProfileState

<!-- Start Topbar -->
<div class="bg-white dark:bg-darklight dark:border-darkborder flex gap-4 lg:z-10 items-center justify-between px-4 h-[60px] border-b border-black/10 detached-topbar relative">
    <div class="flex items-center flex-1 gap-2 sm:gap-4">
        <button type="button" class="flex items-center justify-center text-black dark:text-white/80" @onclick="ToggleSidebar">
            <i class="ri-menu-2-line"></i>
        </button>
    </div>

    <div class="flex items-center gap-4">
        <button type="button" class="text-black dark:text-white/80" @onclick="ToggleFullScreen">
            <i class="ri-fullscreen-line text-xl leading-none"></i>
        </button>

        <div>
            <a href="javascript:;" class="text-black dark:text-white/80" x-cloak x-show="$store.app.mode === 'light'" x-on:click="$store.app.toggleMode('dark')">
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" class="w-5 h-5">
                    <path d="M10 7C10 10.866 13.134 14 17 14C18.9584 14 20.729 13.1957 21.9995 11.8995C22 11.933 22 11.9665 22 12C22 17.5228 17.5228 22 12 22C6.47715 22 2 17.5228 2 12C2 6.47715 6.47715 2 12 2C12.0335 2 12.067 2 12.1005 2.00049C10.8043 3.27098 10 5.04157 10 7ZM4 12C4 16.4183 7.58172 20 12 20C15.0583 20 17.7158 18.2839 19.062 15.7621C18.3945 15.9187 17.7035 16 17 16C12.0294 16 8 11.9706 8 7C8 6.29648 8.08133 5.60547 8.2379 4.938C5.71611 6.28423 4 8.9417 4 12Z" fill="currentColor"></path>
                </svg>
            </a>
            <a href="javascript:;" class="text-black dark:text-white/80" x-cloak x-show="$store.app.mode === 'dark'" x-on:click="$store.app.toggleMode('light')">
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" class="w-5 h-5">
                    <path d="M12 18C8.68629 18 6 15.3137 6 12C6 8.68629 8.68629 6 12 6C15.3137 6 18 8.68629 18 12C18 15.3137 15.3137 18 12 18ZM12 16C14.2091 16 16 14.2091 16 12C16 9.79086 14.2091 8 12 8C9.79086 8 8 9.79086 8 12C8 14.2091 9.79086 16 12 16ZM11 1H13V4H11V1ZM11 20H13V23H11V20ZM3.51472 4.92893L4.92893 3.51472L7.05025 5.63604L5.63604 7.05025L3.51472 4.92893ZM16.9497 18.364L18.364 16.9497L20.4853 19.0711L19.0711 20.4853L16.9497 18.364ZM19.0711 3.51472L20.4853 4.92893L18.364 7.05025L16.9497 5.63604L19.0711 3.51472ZM5.63604 16.9497L7.05025 18.364L4.92893 20.4853L3.51472 19.0711L5.63604 16.9497ZM23 11V13H20V11H23ZM4 11V13H1V11H4Z" fill="currentColor"></path>
                </svg>
            </a>
        </div>

        <a href="/settings" class="text-black dark:text-white/80">
            <i class="ri-settings-3-line text-xl leading-none"></i>
        </a>

        <div class="relative" x-data="{ profileOpen: false }" x-on:click.outside="profileOpen = false">
            <button type="button" class="flex items-center gap-1.5 xl:gap-0 dark:text-white/80" x-on:click="profileOpen = !profileOpen">
                @if (!string.IsNullOrWhiteSpace(UserProfileState.PhotoUrl))
                {
                    <img src="@UserProfileState.PhotoUrl" alt="Header Avatar" class="object-cover rounded-full h-7 w-7 ltr:xl:mr-2 rtl:xl:ml-2" />
                }
                else
                {
                    <span class="flex items-center justify-center rounded-full h-7 w-7 ltr:xl:mr-2 rtl:xl:ml-2 bg-purple/10 text-purple"><i class="ri-user-3-fill text-base leading-none"></i></span>
                }
                <span class="hidden fw-medium xl:block dark:text-white/80">@DisplayName</span>
                <i class="ri-arrow-down-s-line hidden xl:block text-lg leading-none"></i>
            </button>

            <div x-show="profileOpen" x-cloak class="absolute right-0 top-full mt-2 w-48 bg-white dark:bg-darklight border border-black/10 dark:border-darkborder rounded-lg shadow-lg py-2 z-30">
                <a href="/profile" class="flex items-center gap-2 px-4 py-2 text-sm text-black hover:bg-light dark:text-white/80 dark:hover:bg-dark">
                    <i class="ri-user-3-line"></i>
                    Profile
                </a>
                <form method="post" action="/account/logout" data-enhance-nav="false">
                    <AntiforgeryToken />
                    <button type="submit" class="flex items-center gap-2 w-full text-left px-4 py-2 text-sm text-danger hover:bg-light dark:hover:bg-dark">
                        <i class="ri-logout-box-line"></i>
                        Logout
                    </button>
                </form>
            </div>
        </div>
    </div>
</div>
<!-- End Topbar -->
@code {
    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private string DisplayName { get; set; } = "Admin";
    private Guid _currentUserId;

    protected override async Task OnInitializedAsync()
    {
        UserProfileState.OnChange += HandleProfileChanged;

        if (AuthenticationStateTask is not null)
        {
            AuthenticationState authState = await AuthenticationStateTask;
            string? email = authState.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            string? idClaim = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrWhiteSpace(email))
            {
                DisplayName = email;
            }

            if (Guid.TryParse(idClaim, out Guid userId))
            {
                _currentUserId = userId;
            }
        }

        if (!UserProfileState.IsLoaded && _currentUserId != Guid.Empty)
        {
            try
            {
                UserDto user = await UserApiClient.GetByIdAsync(_currentUserId);
                UserProfileState.SetPhoto(user.PhotoUrl);
            }
            catch
            {
                // Non-critical: fall back to the generic icon if this fails.
            }
        }
    }

    private void HandleProfileChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    private async Task ToggleFullScreen()
    {
        await JsRuntime.InvokeVoidAsync("toggleFullScreen");
    }

    private async Task ToggleSidebar()
    {
        await JsRuntime.InvokeVoidAsync("toggleSidebar");
    }

    public void Dispose()
    {
        UserProfileState.OnChange -= HandleProfileChanged;
    }
}
'@ | ForEach-Object { Write-FullFile -Path $targetPath -Content $_ -IdempotencyMarker "UserProfileState" -Description "engranaje->/settings, foto sincronizada" }

$targetPath = Join-Path $adminRoot "Components\Pages\Profile.razor"
@'
@page "/profile"

@inject UserApiClient UserApiClient
@inject UploadsApiClient UploadsApiClient
@inject UserProfileState UserProfileState

<PageTitle>My Profile</PageTitle>

<h1 class="text-2xl font-semibold text-black dark:text-white">My Profile</h1>

<div class="card mt-4 max-w-2xl">
    @if (IsLoading)
    {
        <p class="text-black/60 dark:text-white/60">Loading...</p>
    }
    else
    {
        @if (!string.IsNullOrWhiteSpace(successMessage))
        {
            <div class="p-3 mb-4 text-sm rounded bg-success/10 text-success">@successMessage</div>
        }
        @if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            <div class="p-3 mb-4 text-sm rounded bg-danger/10 text-danger">@errorMessage</div>
        }

        <div class="flex items-center gap-4 mb-6">
            @if (!string.IsNullOrWhiteSpace(PhotoUrl))
            {
                <img src="@PhotoUrl" alt="Profile photo" class="object-cover rounded-full h-16 w-16" />
            }
            else
            {
                <span class="flex items-center justify-center rounded-full h-16 w-16 bg-purple/10 text-purple">
                    <i class="text-2xl ri-user-3-fill leading-none"></i>
                </span>
            }

            <div>
                <InputFile OnChange="OnPhotoSelected" accept="image/png,image/jpeg,image/gif,image/webp" />
                @if (IsUploadingPhoto)
                {
                    <p class="mt-1 text-xs text-black/50 dark:text-white/50">Uploading...</p>
                }
            </div>
        </div>

        <EditForm Model="@FormModel" OnValidSubmit="SaveAsync">
            <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <div>
                    <InputText @bind-Value="FormModel.FirstName" placeholder="First name" class="form-input" />
                </div>
                <div>
                    <InputText @bind-Value="FormModel.LastName" placeholder="Last name" class="form-input" />
                </div>
                <div class="sm:col-span-2">
                    <InputText @bind-Value="FormModel.Email" placeholder="Email" class="form-input" />
                </div>
            </div>
            <button type="submit" disabled="@IsSaving" class="px-4 py-2 mt-4 text-sm text-white rounded-md bg-purple disabled:opacity-70">
                @(IsSaving ? "Saving..." : "Save changes")
            </button>
        </EditForm>
    }
</div>

@code {
    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private Guid CurrentUserId { get; set; }
    private UserDto? CurrentUser { get; set; }

    private ProfileFormModel FormModel { get; set; } = new();
    private string? PhotoUrl { get; set; }

    private bool IsLoading { get; set; } = true;
    private bool IsSaving { get; set; }
    private bool IsUploadingPhoto { get; set; }
    private string? successMessage;
    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationStateTask is not null)
        {
            AuthenticationState authState = await AuthenticationStateTask;
            string? idClaim = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(idClaim, out Guid userId))
            {
                CurrentUserId = userId;
            }
        }

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;

        try
        {
            CurrentUser = await UserApiClient.GetByIdAsync(CurrentUserId);

            FormModel = new ProfileFormModel
            {
                FirstName = CurrentUser.FirstName,
                LastName = CurrentUser.LastName,
                Email = CurrentUser.Email
            };

            PhotoUrl = CurrentUser.PhotoUrl;
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OnPhotoSelected(InputFileChangeEventArgs e)
    {
        IsUploadingPhoto = true;

        try
        {
            IBrowserFile file = e.File;
            using Stream stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);

            string url = await UploadsApiClient.UploadImageAsync(stream, file.Name, file.ContentType, width: 300);

            PhotoUrl = url;

            await SaveInternalAsync(showMessage: false);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            IsUploadingPhoto = false;
        }
    }

    private async Task SaveAsync()
    {
        await SaveInternalAsync(showMessage: true);
    }

    private async Task SaveInternalAsync(bool showMessage)
    {
        if (CurrentUser is null)
        {
            return;
        }

        IsSaving = true;

        try
        {
            UpdateUserRequest request = new()
            {
                FirstName = FormModel.FirstName,
                LastName = FormModel.LastName,
                Email = FormModel.Email,
                Phone = CurrentUser.Phone,
                Country = CurrentUser.Country,
                City = CurrentUser.City,
                PhotoUrl = PhotoUrl,
                MustChangePassword = CurrentUser.MustChangePassword,
                IsActive = CurrentUser.IsActive,
                Role = CurrentUser.Role
            };

            await UserApiClient.UpdateAsync(CurrentUserId, request);

            UserProfileState.SetPhoto(PhotoUrl);

            if (showMessage)
            {
                ShowSuccess("Profile updated successfully.");
            }

            await LoadAsync();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void ShowSuccess(string message)
    {
        successMessage = message;
        errorMessage = null;
        InvokeAsync(async () =>
        {
            await Task.Delay(3500);
            successMessage = null;
            StateHasChanged();
        });
    }

    private void ShowError(string message)
    {
        errorMessage = message;
        successMessage = null;
        InvokeAsync(async () =>
        {
            await Task.Delay(3500);
            errorMessage = null;
            StateHasChanged();
        });
    }

    private class ProfileFormModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
'@ | ForEach-Object { Write-FullFile -Path $targetPath -Content $_ -IdempotencyMarker "UserProfileState.SetPhoto" -Description "auto-dismiss + notifica foto" }

$targetPath = Join-Path $adminRoot "Components\Pages\Settings.razor"
@'
@page "/settings"

@inject UserApiClient UserApiClient
@inject IAuthApiClient AuthApiClient
@inject IAdminTokenProvider AdminTokenProvider

<PageTitle>Settings</PageTitle>

<h1 class="text-2xl font-semibold text-black dark:text-white">Settings</h1>

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
        InvokeAsync(async () =>
        {
            await Task.Delay(3500);
            successMessage = null;
            StateHasChanged();
        });
    }

    private void ShowError(string message)
    {
        errorMessage = message;
        successMessage = null;
        InvokeAsync(async () =>
        {
            await Task.Delay(3500);
            errorMessage = null;
            StateHasChanged();
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
'@ | ForEach-Object { Write-FullFile -Path $targetPath -Content $_ -IdempotencyMarker "ConfirmDeleteAsync" -Description "grid 2 columnas + delete modal + estilos consistentes" }

Write-Host ""
Write-Host "--- 2. Servicio nuevo: UserProfileState ---" -ForegroundColor Cyan

$targetPath = Join-Path $adminRoot "Services\UserProfileState.cs"
@'
namespace Alakai.FestivalManager.Admin.Services;

public class UserProfileState
{
    public string? PhotoUrl { get; private set; }
    public bool IsLoaded { get; private set; }

    public event Action? OnChange;

    public void SetPhoto(string? photoUrl)
    {
        PhotoUrl = photoUrl;
        IsLoaded = true;
        OnChange?.Invoke();
    }
}
'@ | ForEach-Object { Write-NewFile -Path $targetPath -Content $_ -Description "UserProfileState" }

Write-Host ""
Write-Host "--- 3. DI: registrar UserProfileState ---" -ForegroundColor Cyan

$diPath = Join-Path $adminRoot "Extensions\ApplicationDependencyInjectionExtension.cs"
Require-Path $diPath

$diContent = Get-Content $diPath -Raw
$diOld = "services.AddScoped<ITokenStorageService, TokenStorageService>();"
$diNew = "services.AddScoped<ITokenStorageService, TokenStorageService>();`r`n`r`n        services.AddScoped<UserProfileState>();"

if ($diContent.Contains("AddScoped<UserProfileState>")) {
    Write-Host "  Ya registrado en DI." -ForegroundColor Yellow
} else {
    $count = ([regex]::Matches($diContent, [regex]::Escape($diOld))).Count
    if ($count -ne 1) {
        Write-Host "ERROR: no se encontro una unica ocurrencia de la linea de ITokenStorageService en $diPath." -ForegroundColor Red
        exit 1
    }
    $newDiContent = $diContent.Replace($diOld, $diNew)
    Set-Content -Path $diPath -Value $newDiContent -Encoding UTF8 -NoNewline
    Write-Host "  Modificado: $diPath (UserProfileState registrado)" -ForegroundColor Green
}

Write-Host ""
Write-Host "--- 4. Global.cs: using para UserProfileState ---" -ForegroundColor Cyan

$globalPath = Join-Path $adminRoot "Global.cs"
Require-Path $globalPath

$globalContent = Get-Content $globalPath -Raw
$neededUsing = "global using Alakai.FestivalManager.Admin.Services;"

if ($globalContent.Contains($neededUsing)) {
    Write-Host "  Global.cs ya tiene el using." -ForegroundColor Yellow
} else {
    Add-Content -Path $globalPath -Value ("`r`n" + $neededUsing) -Encoding UTF8
    Write-Host "  Modificado: $globalPath (using anadido)" -ForegroundColor Green
}

Write-Host ""
Write-Host "Paso F completado." -ForegroundColor Cyan
Write-Host "Reinicia el Admin (la Api no cambia en este paso)." -ForegroundColor Yellow
Write-Host "Prueba: haz login -> deberia llevarte directo al dashboard sin 404." -ForegroundColor Cyan
Write-Host "Sube una foto en /profile y comprueba que aparece en el Topbar sin recargar." -ForegroundColor Cyan
Write-Host "El engranaje del Topbar ahora navega directo a /settings." -ForegroundColor Cyan