param(
    [string]$SolutionRoot = "."
)

$ErrorActionPreference = "Stop"

function Write-File {
    param([string]$Path, [string]$Content)

    $directory = Split-Path $Path -Parent

    if (-not (Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    Set-Content -Path $Path -Value $Content -Encoding UTF8
    Write-Host "Written: $Path"
}

function Add-GlobalUsing {
    param([string]$Path, [string]$Line)

    if (-not (Test-Path $Path)) {
        Write-Host "Skipped global using. File not found: $Path"
        return
    }

    $content = Get-Content -Path $Path -Raw

    if ($content.Contains($Line)) {
        Write-Host "Already present: $Line"
        return
    }

    Add-Content -Path $Path -Value $Line -Encoding UTF8
    Write-Host "Added global using: $Line"
}

$root = Resolve-Path $SolutionRoot
$adminRoot = Join-Path $root "Alakai.FestivalManager.Admin"
$globalPath = Join-Path $adminRoot "Global.cs"

if (-not (Test-Path $adminRoot)) {
    throw "Admin project not found at $adminRoot"
}

Add-GlobalUsing $globalPath "global using Alakai.FestivalManager.Admin.Contracts.Users.DTOs;"
Add-GlobalUsing $globalPath "global using Alakai.FestivalManager.Admin.Contracts.Users.Requests;"
Add-GlobalUsing $globalPath "global using Alakai.FestivalManager.Admin.Contracts.Users.Responses;"

Write-File "$adminRoot\Contracts\Users\DTOs\UserDto.cs" @'
namespace Alakai.FestivalManager.Admin.Contracts.Users.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool MustChangePassword { get; set; }
    public bool IsActive { get; set; }
}
'@

Write-File "$adminRoot\Contracts\Users\Requests\CreateUserRequest.cs" @'
namespace Alakai.FestivalManager.Admin.Contracts.Users.Requests;

public class CreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Password { get; set; }
}
'@

Write-File "$adminRoot\Contracts\Users\Requests\UpdateUserRequest.cs" @'
namespace Alakai.FestivalManager.Admin.Contracts.Users.Requests;

public class UpdateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public bool MustChangePassword { get; set; }
    public bool IsActive { get; set; }
}
'@

Write-File "$adminRoot\Contracts\Users\Responses\GetUsersResponse.cs" @'
namespace Alakai.FestivalManager.Admin.Contracts.Users.Responses;

public class GetUsersResponse
{
    public IReadOnlyList<UserDto> Users { get; set; } = [];
}
'@

Write-File "$adminRoot\Contracts\Users\Responses\GetUserByIdResponse.cs" @'
namespace Alakai.FestivalManager.Admin.Contracts.Users.Responses;

public class GetUserByIdResponse
{
    public UserDto User { get; set; } = default!;
}
'@

Write-File "$adminRoot\Contracts\Users\Responses\CreateUserResponse.cs" @'
namespace Alakai.FestivalManager.Admin.Contracts.Users.Responses;

public class CreateUserResponse
{
    public UserDto User { get; set; } = default!;
}
'@

Write-File "$adminRoot\Contracts\Users\Responses\UpdateUserResponse.cs" @'
namespace Alakai.FestivalManager.Admin.Contracts.Users.Responses;

public class UpdateUserResponse
{
    public UserDto User { get; set; } = default!;
}
'@

Write-File "$adminRoot\Contracts\Users\Responses\DeleteUserResponse.cs" @'
namespace Alakai.FestivalManager.Admin.Contracts.Users.Responses;

public class DeleteUserResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}
'@

Write-File "$adminRoot\Services\Api\UserApiClient.cs" @'
using System.Text.Json;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class UserApiClient
{
    private readonly HttpClient _httpClient;

    public UserApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        ApiResponse<GetUsersResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetUsersResponse>>("api/users", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load users.", response?.Errors);
        }

        return response.Data?.Users ?? [];
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetUserByIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetUserByIdResponse>>($"api/users/{id}", cancellationToken);

        if (response?.Success is not true || response.Data?.User is null)
        {
            throw new ApiClientException(response?.Message ?? "Could not load user.", response?.Errors);
        }

        return response.Data.User;
    }

    public async Task CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/users", request, cancellationToken);
        ApiResponse<CreateUserResponse>? response = await ReadResponseAsync<CreateUserResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/users/{id}", request, cancellationToken);
        ApiResponse<UpdateUserResponse>? response = await ReadResponseAsync<UpdateUserResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/users/{id}", cancellationToken);
        ApiResponse<DeleteUserResponse>? response = await ReadResponseAsync<DeleteUserResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    private static async Task<ApiResponse<T>?> ReadResponseAsync<T>(HttpResponseMessage httpResponse, CancellationToken cancellationToken)
    {
        try
        {
            return await httpResponse.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken);
        }
        catch (JsonException)
        {
            string content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            string message = string.IsNullOrWhiteSpace(content) ? $"Request failed with status code {(int)httpResponse.StatusCode}." : content;

            throw new ApiClientException(message);
        }
    }

    private static void EnsureSuccess<T>(HttpResponseMessage httpResponse, ApiResponse<T>? response)
    {
        if (httpResponse.IsSuccessStatusCode && response?.Success == true)
        {
            return;
        }

        string message = response?.Message ?? $"Request failed with status code {(int)httpResponse.StatusCode}.";
        IReadOnlyList<string> errors = response?.Errors ?? [];

        throw new ApiClientException(message, errors);
    }
}
'@

Write-File "$adminRoot\Components\Pages\Users.razor" @'
@page "/users"
@inject UserApiClient UserApiClient

<PageHeader Title="Operations" pTitle="Users"></PageHeader>

<div class="flex flex-col gap-4 min-h-[calc(100vh-212px)]">
    <div class="card">
        <div class="flex flex-col gap-4 mb-5 xl:flex-row xl:items-center xl:justify-between">
            <div>
                <h2 class="text-base font-semibold text-black capitalize dark:text-white">Users</h2>
                <p class="text-sm text-black/50 dark:text-white/40">Manage users created from registrations and admin entries.</p>
            </div>

            <div class="flex flex-col gap-3 md:flex-row md:items-center">
                <input class="form-input md:w-72" placeholder="Search users..." @bind="searchText" @bind:event="oninput" @bind:after="ResetPage" />

                <select class="form-select md:w-40" @bind="statusFilter" @bind:after="ResetPage">
                    <option value="">All statuses</option>
                    <option value="active">Active</option>
                    <option value="inactive">Inactive</option>
                    <option value="mustchange">Must change password</option>
                </select>

                <select class="form-select md:w-32" @bind="pageSize" @bind:after="ResetPage">
                    <option value="10">10 rows</option>
                    <option value="25">25 rows</option>
                    <option value="50">50 rows</option>
                </select>

                <button type="button" class="transition-all duration-300 border rounded-md btn text-purple border-purple hover:bg-purple hover:text-white whitespace-nowrap" @onclick="OpenCreateModal">
                    <i class="ri-add-line ltr:mr-1 rtl:ml-1"></i>
                    New User
                </button>
            </div>
        </div>

        @if (!string.IsNullOrWhiteSpace(successMessage))
        {
            <div class="p-3 mb-4 text-sm rounded bg-success/10 text-success">@successMessage</div>
        }

        @if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            <div class="p-3 mb-4 text-sm rounded bg-danger/10 text-danger">@errorMessage</div>
        }

        @if (isLoading)
        {
            <p class="text-sm text-black/50 dark:text-white/40">Loading users...</p>
        }
        else
        {
            <div class="overflow-x-auto">
                <table class="w-full table-hover">
                    <thead class="bg-gray-50 dark:bg-dark">
                        <tr class="text-left">
                            <th class="px-4 py-3 font-semibold"><button type="button" class="flex items-center gap-1 w-full font-semibold" @onclick='() => SortBy("Name")'>Name @SortIcon("Name")</button></th>
                            <th class="px-4 py-3 font-semibold"><button type="button" class="flex items-center gap-1 w-full font-semibold" @onclick='() => SortBy("Email")'>Email @SortIcon("Email")</button></th>
                            <th class="px-4 py-3 font-semibold">Phone</th>
                            <th class="px-4 py-3 font-semibold"><button type="button" class="flex items-center gap-1 w-full font-semibold" @onclick='() => SortBy("Country")'>Country @SortIcon("Country")</button></th>
                            <th class="px-4 py-3 font-semibold">City</th>
                            <th class="px-4 py-3 font-semibold"><button type="button" class="flex items-center gap-1 w-full font-semibold" @onclick='() => SortBy("CreatedAt")'>Created @SortIcon("CreatedAt")</button></th>
                            <th class="px-4 py-3 font-semibold">Login</th>
                            <th class="px-4 py-3 font-semibold">Status</th>
                            <th class="px-4 py-3 font-semibold text-right">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        @if (PagedUsers.Count == 0)
                        {
                            <tr>
                                <td colspan="9" class="px-4 py-6 text-center text-black/50 dark:text-white/40">No users found.</td>
                            </tr>
                        }
                        else
                        {
                            @foreach (UserDto user in PagedUsers)
                            {
                                <tr class="border-b border-black/10 dark:border-darkborder">
                                    <td class="px-4 py-3">
                                        <div class="font-medium">@user.FullName</div>
                                        @if (user.MustChangePassword)
                                        {
                                            <div class="text-xs text-warning">Must change password</div>
                                        }
                                    </td>
                                    <td class="px-4 py-3">@user.Email</td>
                                    <td class="px-4 py-3">@user.Phone</td>
                                    <td class="px-4 py-3">@user.Country</td>
                                    <td class="px-4 py-3">@user.City</td>
                                    <td class="px-4 py-3">@FormatDate(user.CreatedAt)</td>
                                    <td class="px-4 py-3">@FormatNullableDate(user.LastLoginAt)</td>
                                    <td class="px-4 py-3"><span class="@GetStatusClass(user.IsActive)">@(user.IsActive ? "Active" : "Inactive")</span></td>
                                    <td class="px-4 py-3">
                                        <div class="flex items-center justify-end gap-3">
                                            <button type="button" class="text-black dark:text-white/80" title="Edit" @onclick="() => OpenEditModal(user)"><i class="ri-pencil-line text-lg"></i></button>
                                            <button type="button" class="text-danger" title="Delete" @onclick="() => OpenDeleteModal(user)"><i class="ri-delete-bin-line text-lg"></i></button>
                                        </div>
                                    </td>
                                </tr>
                            }
                        }
                    </tbody>
                </table>
            </div>

            <div class="flex flex-col gap-3 mt-4 md:flex-row md:items-center md:justify-between">
                <p class="text-sm text-black/50 dark:text-white/40">Showing @StartRow to @EndRow of @FilteredUsers.Count users</p>

                <div class="flex items-center gap-2">
                    <button type="button" class="btn border" disabled="@(currentPage == 1)" @onclick="PreviousPage">Previous</button>
                    <span class="px-4 py-2 text-sm rounded bg-purple/10 text-purple">@currentPage</span>
                    <button type="button" class="btn border" disabled="@(currentPage >= TotalPages)" @onclick="NextPage">Next</button>
                </div>
            </div>
        }
    </div>
</div>

@if (showModal)
{
    <div class="fixed inset-0 bg-black/60 z-[999] overflow-y-auto">
        <div class="flex items-start justify-center min-h-screen px-4 py-10">
            <div class="relative mx-auto overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder" style="width: min(95vw, 760px);">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <h3 class="text-lg font-semibold text-black dark:text-white">@(editingUser is null ? "New User" : "Edit User")</h3>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="CloseModal"><i class="ri-close-line text-2xl"></i></button>
                </div>

                <EditForm Model="formModel" OnValidSubmit="SaveAsync">
                    <div class="p-5 space-y-4">
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">First Name</label>
                                <InputText class="form-input" @bind-Value="formModel.FirstName" />
                            </div>

                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">Last Name</label>
                                <InputText class="form-input" @bind-Value="formModel.LastName" />
                            </div>

                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">Email</label>
                                <InputText class="form-input" @bind-Value="formModel.Email" />
                            </div>

                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">Phone</label>
                                <InputText class="form-input" @bind-Value="formModel.Phone" />
                            </div>

                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">Country</label>
                                <InputText class="form-input" @bind-Value="formModel.Country" />
                            </div>

                            <div>
                                <label class="block text-sm text-black/60 dark:text-white/60">City</label>
                                <InputText class="form-input" @bind-Value="formModel.City" />
                            </div>

                            @if (editingUser is null)
                            {
                                <div class="md:col-span-2">
                                    <label class="block text-sm text-black/60 dark:text-white/60">Initial Password</label>
                                    <InputText class="form-input" type="password" @bind-Value="formModel.Password" />
                                </div>
                            }

                            @if (editingUser is not null)
                            {
                                <div class="flex items-center gap-4 md:col-span-2">
                                    <label class="inline-flex items-center gap-2"><InputCheckbox @bind-Value="formModel.MustChangePassword" />Must change password</label>
                                    <label class="inline-flex items-center gap-2"><InputCheckbox @bind-Value="formModel.IsActive" />Active</label>
                                </div>
                            }
                        </div>
                    </div>

                    <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                        <button type="button" class="px-4 py-2 text-sm border rounded-md border-black/10" @onclick="CloseModal">Cancel</button>
                        <button type="submit" class="px-4 py-2 text-sm text-white rounded-md bg-purple">Save</button>
                    </div>
                </EditForm>
            </div>
        </div>
    </div>
}

@if (deletingUser is not null)
{
    <div class="fixed inset-0 bg-black/60 z-[999] overflow-y-auto">
        <div class="flex items-start justify-center min-h-screen px-4 py-10">
            <div class="relative w-full max-w-md mx-auto overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="px-5 py-4">
                    <h3 class="text-lg font-semibold text-black dark:text-white">Delete User</h3>
                    <p class="mt-2 text-sm text-black/60 dark:text-white/60">Delete @deletingUser.FullName?</p>
                    <p class="mt-2 text-sm text-danger">If this user has registrations, backend constraints may block the delete.</p>
                </div>

                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="px-4 py-2 text-sm border rounded-md border-black/10" @onclick="CloseDeleteModal">Cancel</button>
                    <button type="button" class="px-4 py-2 text-sm text-white rounded-md bg-danger" @onclick="DeleteAsync">Delete</button>
                </div>
            </div>
        </div>
    </div>
}

@code {
    private List<UserDto> users = [];
    private UserDto? editingUser;
    private UserDto? deletingUser;
    private UserFormModel formModel = new();
    private string searchText = string.Empty;
    private string statusFilter = string.Empty;
    private string sortColumn = "Name";
    private bool sortAscending = true;
    private bool isLoading = true;
    private bool showModal;
    private string? successMessage;
    private string? errorMessage;
    private int currentPage = 1;
    private int pageSize = 10;

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
            users = (await UserApiClient.GetAllAsync()).ToList();
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

    private List<UserDto> FilteredUsers
    {
        get
        {
            IEnumerable<UserDto> query = users;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(u => u.FullName.Contains(searchText, StringComparison.OrdinalIgnoreCase) || u.Email.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            if (statusFilter == "active")
            {
                query = query.Where(u => u.IsActive);
            }
            else if (statusFilter == "inactive")
            {
                query = query.Where(u => !u.IsActive);
            }
            else if (statusFilter == "mustchange")
            {
                query = query.Where(u => u.MustChangePassword);
            }

            query = sortColumn switch
            {
                "Email" => sortAscending ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
                "Country" => sortAscending ? query.OrderBy(u => u.Country) : query.OrderByDescending(u => u.Country),
                "CreatedAt" => sortAscending ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt),
                _ => sortAscending ? query.OrderBy(u => u.FullName) : query.OrderByDescending(u => u.FullName)
            };

            return query.ToList();
        }
    }

    private List<UserDto> PagedUsers => FilteredUsers.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(FilteredUsers.Count / (double)pageSize));
    private int StartRow => FilteredUsers.Count == 0 ? 0 : ((currentPage - 1) * pageSize) + 1;
    private int EndRow => Math.Min(currentPage * pageSize, FilteredUsers.Count);

    private void ResetPage()
    {
        currentPage = 1;
    }

    private void SortBy(string column)
    {
        if (sortColumn == column)
        {
            sortAscending = !sortAscending;
        }
        else
        {
            sortColumn = column;
            sortAscending = true;
        }
    }

    private MarkupString SortIcon(string column)
    {
        if (sortColumn != column)
        {
            return new MarkupString("<i class=\"ri-arrow-up-down-line text-xs opacity-40\"></i>");
        }

        return new MarkupString(sortAscending ? "<i class=\"ri-arrow-up-s-fill text-xs text-purple\"></i>" : "<i class=\"ri-arrow-down-s-fill text-xs text-purple\"></i>");
    }

    private void PreviousPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
        }
    }

    private void NextPage()
    {
        if (currentPage < TotalPages)
        {
            currentPage++;
        }
    }

    private void OpenCreateModal()
    {
        editingUser = null;
        formModel = new UserFormModel
        {
            IsActive = true
        };
        showModal = true;
    }

    private void OpenEditModal(UserDto user)
    {
        editingUser = user;
        formModel = new UserFormModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Country = user.Country,
            City = user.City,
            MustChangePassword = user.MustChangePassword,
            IsActive = user.IsActive
        };
        showModal = true;
    }

    private void CloseModal()
    {
        showModal = false;
    }

    private async Task SaveAsync()
    {
        try
        {
            if (editingUser is null)
            {
                CreateUserRequest request = new()
                {
                    FirstName = formModel.FirstName,
                    LastName = formModel.LastName,
                    Email = formModel.Email,
                    Phone = formModel.Phone,
                    Country = formModel.Country,
                    City = formModel.City,
                    Password = formModel.Password
                };

                await UserApiClient.CreateAsync(request);
                successMessage = "User created successfully.";
            }
            else
            {
                UpdateUserRequest request = new()
                {
                    FirstName = formModel.FirstName,
                    LastName = formModel.LastName,
                    Email = formModel.Email,
                    Phone = formModel.Phone,
                    Country = formModel.Country,
                    City = formModel.City,
                    MustChangePassword = formModel.MustChangePassword,
                    IsActive = formModel.IsActive
                };

                await UserApiClient.UpdateAsync(editingUser.Id, request);
                successMessage = "User updated successfully.";
            }

            showModal = false;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
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

    private async Task DeleteAsync()
    {
        if (deletingUser is null)
        {
            return;
        }

        try
        {
            await UserApiClient.DeleteAsync(deletingUser.Id);
            successMessage = "User deleted successfully.";
            deletingUser = null;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private static string FormatDate(DateTime date)
    {
        if (date == DateTime.MinValue)
        {
            return "-";
        }

        return date.ToString("dd/MM/yyyy");
    }

    private static string FormatNullableDate(DateTime? date)
    {
        return date.HasValue ? date.Value.ToString("dd/MM/yyyy") : "-";
    }

    private static string GetStatusClass(bool isActive)
    {
        return isActive ? "inline-block rounded text-xs px-2 py-1 bg-success/10 text-success" : "inline-block rounded text-xs px-2 py-1 bg-danger/10 text-danger";
    }

    private class UserFormModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Password { get; set; }
        public bool MustChangePassword { get; set; }
        public bool IsActive { get; set; }
    }
}
'@

Write-Host "Users admin files created."
Write-Host "No Sidebar, Program.cs, App.razor, csproj, CSS or assets were modified."
