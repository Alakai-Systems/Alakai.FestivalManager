# Creates Contracts, ApiClients and Pages for Editions, PassTypes and Levels in Alakai.FestivalManager.Admin
# Run from repository root. No git operations performed.
# Usage: pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\create-admin-pages.ps1
Set-StrictMode -Version Latest

$projectDir = Join-Path (Resolve-Path ".").Path "Alakai.FestivalManager.Admin"

if (-not (Test-Path $projectDir)) {
    Write-Error "Project path not found: $projectDir"
    exit 1
}

function Write-FileIfNotExists($fullPath, $content) {
    $dir = Split-Path -Parent $fullPath
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
    if (-not (Test-Path $fullPath)) {
        $content | Out-File -FilePath $fullPath -Encoding utf8
        Write-Host "Created: $fullPath"
    }
    else {
        $bak = $fullPath + ".new"
        $content | Out-File -FilePath $bak -Encoding utf8
        Write-Host "Exists -> wrote: $bak"
    }
}

$files = @{}

# Contracts Editions
$files["Contracts\Editions\EditionDto.cs"] = @'
using System;

namespace Alakai.FestivalManager.Admin.Contracts.Editions;

public class EditionDto
{
    public Guid Id { get; set; }

    public Guid FestivalId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Year { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime? RegistrationOpenDate { get; set; }

    public DateTime? RegistrationCloseDate { get; set; }

    public bool IsActive { get; set; }
}
'@

$files["Contracts\Editions\Requests\CreateEditionRequest.cs"] = @'
using System;

namespace Alakai.FestivalManager.Admin.Contracts.Editions.Requests;

public class CreateEditionRequest
{
    public Guid FestivalId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Year { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime? RegistrationOpenDate { get; set; }

    public DateTime? RegistrationCloseDate { get; set; }
}
'@

$files["Contracts\Editions\Requests\UpdateEditionRequest.cs"] = @'
using System;

namespace Alakai.FestivalManager.Admin.Contracts.Editions.Requests;

public class UpdateEditionRequest
{
    public Guid FestivalId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Year { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime? RegistrationOpenDate { get; set; }

    public DateTime? RegistrationCloseDate { get; set; }

    public bool IsActive { get; set; }
}
'@

$files["Contracts\Editions\Responses\GetEditionsResponse.cs"] = @'
using System.Collections.Generic;

namespace Alakai.FestivalManager.Admin.Contracts.Editions.Responses;

public class GetEditionsResponse
{
    public IReadOnlyList<EditionDto> Editions { get; set; } = new List<EditionDto>();
}
'@

$files["Contracts\Editions\Responses\GetEditionByIdResponse.cs"] = @'
namespace Alakai.FestivalManager.Admin.Contracts.Editions.Responses;

public class GetEditionByIdResponse
{
    public EditionDto Edition { get; set; } = new EditionDto();
}
'@

$files["Contracts\Editions\Responses\CreateEditionResponse.cs"] = @'
namespace Alakai.FestivalManager.Admin.Contracts.Editions.Responses;

public class CreateEditionResponse
{
    public EditionDto Edition { get; set; } = new EditionDto();
}
'@

$files["Contracts\Editions\Responses\UpdateEditionResponse.cs"] = @'
namespace Alakai.FestivalManager.Admin.Contracts.Editions.Responses;

public class UpdateEditionResponse
{
    public EditionDto Edition { get; set; } = new EditionDto();
}
'@

$files["Contracts\Editions\Responses\DeleteEditionResponse.cs"] = @'
using System;

namespace Alakai.FestivalManager.Admin.Contracts.Editions.Responses;

public class DeleteEditionResponse
{
    public Guid Id { get; set; }

    public bool Deleted { get; set; }
}
'@

# Contracts PassTypes
$files["Contracts\PassTypes\PassTypeDto.cs"] = @'
using System;

namespace Alakai.FestivalManager.Admin.Contracts.PassTypes;

public class PassTypeDto
{
    public Guid Id { get; set; }

    public Guid EditionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }
}
'@

$files["Contracts\PassTypes\Requests\CreatePassTypeRequest.cs"] = @'
using System;

namespace Alakai.FestivalManager.Admin.Contracts.PassTypes.Requests;

public class CreatePassTypeRequest
{
    public Guid EditionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }
}
'@

$files["Contracts\PassTypes\Requests\UpdatePassTypeRequest.cs"] = @'
using System;

namespace Alakai.FestivalManager.Admin.Contracts.PassTypes.Requests;

public class UpdatePassTypeRequest
{
    public Guid EditionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }
}
'@

$files["Contracts\PassTypes\Responses\GetPassTypesResponse.cs"] = @'
using System.Collections.Generic;

namespace Alakai.FestivalManager.Admin.Contracts.PassTypes.Responses;

public class GetPassTypesResponse
{
    public IReadOnlyList<PassTypeDto> PassTypes { get; set; } = new List<PassTypeDto>();
}
'@

$files["Contracts\PassTypes\Responses\GetPassTypeByIdResponse.cs"] = @'
namespace Alakai.FestivalManager.Admin.Contracts.PassTypes.Responses;

public class GetPassTypeByIdResponse
{
    public PassTypeDto PassType { get; set; } = new PassTypeDto();
}
'@

$files["Contracts\PassTypes\Responses\CreatePassTypeResponse.cs"] = @'
namespace Alakai.FestivalManager.Admin.Contracts.PassTypes.Responses;

public class CreatePassTypeResponse
{
    public PassTypeDto PassType { get; set; } = new PassTypeDto();
}
'@

$files["Contracts\PassTypes\Responses\UpdatePassTypeResponse.cs"] = @'
namespace Alakai.FestivalManager.Admin.Contracts.PassTypes.Responses;

public class UpdatePassTypeResponse
{
    public PassTypeDto PassType { get; set; } = new PassTypeDto();
}
'@

$files["Contracts\PassTypes\Responses\DeletePassTypeResponse.cs"] = @'
using System;

namespace Alakai.FestivalManager.Admin.Contracts.PassTypes.Responses;

public class DeletePassTypeResponse
{
    public Guid Id { get; set; }

    public bool Deleted { get; set; }
}
'@

# Contracts Levels
$files["Contracts\Levels\LevelDto.cs"] = @'
using System;

namespace Alakai.FestivalManager.Admin.Contracts.Levels;

public class LevelDto
{
    public Guid Id { get; set; }

    public Guid PassTypeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal EarlyBirdPrice { get; set; }

    public decimal GroupPrice { get; set; }

    public decimal RegularPrice { get; set; }

    public int? LeaderCapacity { get; set; }

    public int? FollowerCapacity { get; set; }

    public int? IndividualCapacity { get; set; }

    public int? MaxLeaderDifference { get; set; }

    public int? MaxFollowerDifference { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }
}
'@

$files["Contracts\Levels\Requests\CreateLevelRequest.cs"] = @'
using System;

namespace Alakai.FestivalManager.Admin.Contracts.Levels.Requests;

public class CreateLevelRequest
{
    public Guid PassTypeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal EarlyBirdPrice { get; set; }

    public decimal GroupPrice { get; set; }

    public decimal RegularPrice { get; set; }

    public int? LeaderCapacity { get; set; }

    public int? FollowerCapacity { get; set; }

    public int? IndividualCapacity { get; set; }

    public int? MaxLeaderDifference { get; set; }

    public int? MaxFollowerDifference { get; set; }

    public int SortOrder { get; set; }
}
'@

$files["Contracts\Levels\Requests\UpdateLevelRequest.cs"] = @'
using System;

namespace Alakai.FestivalManager.Admin.Contracts.Levels.Requests;

public class UpdateLevelRequest
{
    public Guid PassTypeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal EarlyBirdPrice { get; set; }

    public decimal GroupPrice { get; set; }

    public decimal RegularPrice { get; set; }

    public int? LeaderCapacity { get; set; }

    public int? FollowerCapacity { get; set; }

    public int? IndividualCapacity { get; set; }

    public int? MaxLeaderDifference { get; set; }

    public int? MaxFollowerDifference { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }
}
'@

$files["Contracts\Levels\Responses\GetLevelsResponse.cs"] = @'
using System.Collections.Generic;

namespace Alakai.FestivalManager.Admin.Contracts.Levels.Responses;

public class GetLevelsResponse
{
    public IReadOnlyList<LevelDto> Levels { get; set; } = new List<LevelDto>();
}
'@

$files["Contracts\Levels\Responses\GetLevelByIdResponse.cs"] = @'
namespace Alakai.FestivalManager.Admin.Contracts.Levels.Responses;

public class GetLevelByIdResponse
{
    public LevelDto Level { get; set; } = new LevelDto();
}
'@

$files["Contracts\Levels\Responses\CreateLevelResponse.cs"] = @'
namespace Alakai.FestivalManager.Admin.Contracts.Levels.Responses;

public class CreateLevelResponse
{
    public LevelDto Level { get; set; } = new LevelDto();
}
'@

$files["Contracts\Levels\Responses\UpdateLevelResponse.cs"] = @'
namespace Alakai.FestivalManager.Admin.Contracts.Levels.Responses;

public class UpdateLevelResponse
{
    public LevelDto Level { get; set; } = new LevelDto();
}
'@

$files["Contracts\Levels\Responses\DeleteLevelResponse.cs"] = @'
using System;

namespace Alakai.FestivalManager.Admin.Contracts.Levels.Responses;

public class DeleteLevelResponse
{
    public Guid Id { get; set; }

    public bool Deleted { get; set; }
}
'@

# ApiClients
$files["Services\Api\EditionApiClient.cs"] = @'
using System.Text.Json;
using Alakai.FestivalManager.Admin.Contracts.Editions;
using Alakai.FestivalManager.Admin.Contracts.Editions.Requests;
using Alakai.FestivalManager.Admin.Contracts.Editions.Responses;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class EditionApiClient
{
    private readonly HttpClient _httpClient;

    public EditionApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<EditionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        ApiResponse<GetEditionsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEditionsResponse>>("api/editions", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load editions.", response?.Errors);
        }

        return response.Data?.Editions ?? [];
    }

    public async Task<IReadOnlyList<EditionDto>> GetByFestivalIdAsync(Guid festivalId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetEditionsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEditionsResponse>>($"api/editions/by-festival/{festivalId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load editions for festival.", response?.Errors);
        }

        return response.Data?.Editions ?? [];
    }

    public async Task<EditionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetEditionByIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetEditionByIdResponse>>($"api/editions/{id}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load edition.", response?.Errors);
        }

        return response.Data!.Edition;
    }

    public async Task CreateAsync(CreateEditionRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/editions", request, cancellationToken);
        ApiResponse<CreateEditionResponse>? response = await ReadResponseAsync<CreateEditionResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateEditionRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/editions/{id}", request, cancellationToken);
        ApiResponse<UpdateEditionResponse>? response = await ReadResponseAsync<UpdateEditionResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/editions/{id}", cancellationToken);
        ApiResponse<DeleteEditionResponse>? response = await ReadResponseAsync<DeleteEditionResponse>(httpResponse, cancellationToken);

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

$files["Services\Api\PassTypeApiClient.cs"] = @'
using System.Text.Json;
using Alakai.FestivalManager.Admin.Contracts.PassTypes;
using Alakai.FestivalManager.Admin.Contracts.PassTypes.Requests;
using Alakai.FestivalManager.Admin.Contracts.PassTypes.Responses;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class PassTypeApiClient
{
    private readonly HttpClient _httpClient;

    public PassTypeApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<PassTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        ApiResponse<GetPassTypesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetPassTypesResponse>>("api/pass-types", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load pass types.", response?.Errors);
        }

        return response.Data?.PassTypes ?? [];
    }

    public async Task<IReadOnlyList<PassTypeDto>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetPassTypesResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetPassTypesResponse>>($"api/pass-types/by-edition/{editionId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load pass types for edition.", response?.Errors);
        }

        return response.Data?.PassTypes ?? [];
    }

    public async Task<PassTypeDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetPassTypeByIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetPassTypeByIdResponse>>($"api/pass-types/{id}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load pass type.", response?.Errors);
        }

        return response.Data!.PassType;
    }

    public async Task CreateAsync(CreatePassTypeRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/pass-types", request, cancellationToken);
        ApiResponse<CreatePassTypeResponse>? response = await ReadResponseAsync<CreatePassTypeResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdatePassTypeRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/pass-types/{id}", request, cancellationToken);
        ApiResponse<UpdatePassTypeResponse>? response = await ReadResponseAsync<UpdatePassTypeResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/pass-types/{id}", cancellationToken);
        ApiResponse<DeletePassTypeResponse>? response = await ReadResponseAsync<DeletePassTypeResponse>(httpResponse, cancellationToken);

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

$files["Services\Api\LevelApiClient.cs"] = @'
using System.Text.Json;
using Alakai.FestivalManager.Admin.Contracts.Levels;
using Alakai.FestivalManager.Admin.Contracts.Levels.Requests;
using Alakai.FestivalManager.Admin.Contracts.Levels.Responses;

namespace Alakai.FestivalManager.Admin.Services.Api;

public class LevelApiClient
{
    private readonly HttpClient _httpClient;

    public LevelApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<LevelDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        ApiResponse<GetLevelsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetLevelsResponse>>("api/levels", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load levels.", response?.Errors);
        }

        return response.Data?.Levels ?? [];
    }

    public async Task<IReadOnlyList<LevelDto>> GetByPassTypeIdAsync(Guid passTypeId, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetLevelsResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetLevelsResponse>>($"api/levels/by-pass-type/{passTypeId}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load levels for pass type.", response?.Errors);
        }

        return response.Data?.Levels ?? [];
    }

    public async Task<LevelDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ApiResponse<GetLevelByIdResponse>? response = await _httpClient.GetFromJsonAsync<ApiResponse<GetLevelByIdResponse>>($"api/levels/{id}", cancellationToken);

        if (response?.Success is not true)
        {
            throw new ApiClientException(response?.Message ?? "Could not load level.", response?.Errors);
        }

        return response.Data!.Level;
    }

    public async Task CreateAsync(CreateLevelRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync("api/levels", request, cancellationToken);
        ApiResponse<CreateLevelResponse>? response = await ReadResponseAsync<CreateLevelResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task UpdateAsync(Guid id, UpdateLevelRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PutAsJsonAsync($"api/levels/{id}", request, cancellationToken);
        ApiResponse<UpdateLevelResponse>? response = await ReadResponseAsync<UpdateLevelResponse>(httpResponse, cancellationToken);

        EnsureSuccess(httpResponse, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/levels/{id}", cancellationToken);
        ApiResponse<DeleteLevelResponse>? response = await ReadResponseAsync<DeleteLevelResponse>(httpResponse, cancellationToken);

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

# Pages: Full contents (Editions, PassTypes, Levels)
# Editions.razor (full)
$files["Components\Pages\Editions.razor"] = @'
@using Alakai.FestivalManager.Admin.Contracts.Editions
@using Alakai.FestivalManager.Admin.Contracts.Editions.Requests
@using Alakai.FestivalManager.Admin.Contracts.Festivals
@using Alakai.FestivalManager.Admin.Services.Api
@inject EditionApiClient EditionApiClient
@inject FestivalApiClient FestivalApiClient

<PageHeader Title="Configuration" pTitle="Editions"></PageHeader>

<div class="flex flex-col gap-4 min-h-[calc(100vh-212px)]">
    <div class="grid grid-cols-1 gap-4">
        <div class="card">
            <div class="flex flex-col gap-4 mb-5 xl:flex-row xl:items-center xl:justify-between">
                <div>
                    <h2 class="text-base font-semibold text-black capitalize dark:text-white">Editions</h2>
                    <p class="text-sm text-black/50 dark:text-white/40">Manage edition configuration.</p>
                </div>

                <div class="flex flex-col gap-3 md:flex-row md:items-center">
                    <div class="relative w-full md:w-72">
                        <input class="form-input" placeholder="Search editions..." @bind="searchText" @bind:event="oninput" @bind:after="ResetPage" />
                    </div>

                    <select class="form-select w-full md:w-44" @bind="statusFilter" @bind:after="ResetPage">
                        <option value="All">All statuses</option>
                        <option value="Active">Active</option>
                        <option value="Inactive">Inactive</option>
                    </select>

                    <select class="form-select w-full md:w-32" @bind="pageSize" @bind:after="ResetPage">
                        <option value="5">5 rows</option>
                        <option value="10">10 rows</option>
                        <option value="25">25 rows</option>
                        <option value="50">50 rows</option>
                    </select>

                    <button type="button" class="transition-all duration-300 border rounded-md btn text-purple border-purple hover:bg-purple hover:text-white whitespace-nowrap" @onclick="OpenCreateModal">
                        <i class="ri-add-line ltr:mr-1 rtl:ml-1"></i>
                        New Edition
                    </button>
                </div>
            </div>

            @if (!string.IsNullOrWhiteSpace(successMessage))
            {
                <div class="p-3 mb-4 text-sm rounded bg-success/10 text-success">
                    @successMessage
                </div>
            }

            @if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                <div class="p-3 mb-4 text-sm rounded bg-danger/10 text-danger">
                    @errorMessage
                </div>
            }

            @if (isLoading)
            {
                <p class="text-sm text-black/50 dark:text-white/40">Loading editions...</p>
            }
            else
            {
                <div class="overflow-x-auto">
                    <table class="w-full table-hover">
                        <thead class="bg-gray-50 dark:bg-dark">
                            <tr class="text-left">
                                <th class="px-4 py-3 font-semibold">
                                    <button type="button" class="flex items-center gap-1 w-full font-semibold" @onclick='() => SortBy("Year")'>
                                        Year
                                        @SortIcon("Year")
                                    </button>
                                </th>
                                <th class="px-4 py-3 font-semibold">
                                    <button type="button" class="flex items-center gap-1 w-full font-semibold" @onclick='() => SortBy("Name")'>
                                        Name
                                        @SortIcon("Name")
                                    </button>
                                </th>
                                <th class="px-4 py-3 font-semibold">
                                    <button type="button" class="flex items-center gap-1 w-full font-semibold" @onclick='() => SortBy("Festival")'>
                                        Festival
                                        @SortIcon("Festival")
                                    </button>
                                </th>
                                <th class="px-4 py-3 font-semibold">
                                    <button type="button" class="flex items-center gap-1 w-full font-semibold" @onclick='() => SortBy("StartDate")'>
                                        Start
                                        @SortIcon("StartDate")
                                    </button>
                                </th>
                                <th class="px-4 py-3 font-semibold">
                                    <button type="button" class="flex items-center gap-1 w-full font-semibold" @onclick='() => SortBy("EndDate")'>
                                        End
                                        @SortIcon("EndDate")
                                    </button>
                                </th>
                                <th class="px-4 py-3 font-semibold">
                                    <button type="button" class="flex items-center gap-1 w-full font-semibold" @onclick='() => SortBy("Status")'>
                                        Status
                                        @SortIcon("Status")
                                    </button>
                                </th>
                                <th class="px-4 py-3 font-semibold text-right">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            @if (PagedEditions.Count == 0)
                            {
                                <tr>
                                    <td colspan="7" class="px-4 py-6 text-center text-black/50 dark:text-white/40">No editions found.</td>
                                </tr>
                            }
                            else
                            {
                                @foreach (EditionDto edition in PagedEditions)
                                {
                                    <tr class="border-b border-black/10 dark:border-darkborder">
                                        <td class="px-4 py-3">@edition.Year</td>
                                        <td class="px-4 py-3">@edition.Name</td>
                                        <td class="px-4 py-3">@GetFestivalName(edition.FestivalId)</td>
                                        <td class="px-4 py-3">@edition.StartDate.ToString("yyyy-MM-dd")</td>
                                        <td class="px-4 py-3">@edition.EndDate.ToString("yyyy-MM-dd")</td>
                                        <td class="px-4 py-3">
                                            @if (edition.IsActive)
                                            {
                                                <span class="inline-block rounded text-xs px-2 py-1 bg-success/10 text-success">Active</span>
                                            }
                                            else
                                            {
                                                <span class="inline-block rounded text-xs px-2 py-1 bg-danger/10 text-danger">Inactive</span>
                                            }
                                        </td>
                                        <td class="px-4 py-3">
                                            <div class="flex items-center justify-end gap-3">
                                                <button type="button" class="text-black dark:text-white/80" title="Edit" @onclick="() => OpenEditModal(edition)">
                                                    <i class="ri-pencil-line text-lg"></i>
                                                </button>
                                                <button type="button" class="text-danger" title="Delete" @onclick="() => OpenDeleteModal(edition)">
                                                    <i class="ri-delete-bin-line text-lg"></i>
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                }
                            }
                        </tbody>
                    </table>
                </div>

                <div class="flex items-center justify-between mt-4">
                    <p class="text-sm text-black/50 dark:text-white/40">
                        Showing @ShowingFrom to @ShowingTo of @FilteredEditions.Count editions
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
</div>

@if (showCreateModal)
{
    <div class="fixed inset-0 bg-black/60 z-[999] overflow-y-auto">
        <div class="flex items-start justify-center min-h-screen px-4 py-10">
            <div class="relative w-full max-w-lg overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <h3 class="text-lg font-semibold text-black dark:text-white">New Edition</h3>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="CloseModals">
                        <i class="ri-close-line text-2xl"></i>
                    </button>
                </div>

                <div class="p-5 space-y-4">
                    @if (!string.IsNullOrWhiteSpace(modalErrorMessage))
                    {
                        <div class="p-3 text-sm rounded bg-danger/10 text-danger">@modalErrorMessage</div>
                    }

                    <select class="form-select" @bind="createRequest.FestivalId">
                        <option value="">Select festival</option>
                        @foreach (FestivalDto festival in festivals)
                        {
                            <option value="@festival.Id">@festival.Name</option>
                        }
                    </select>

                    <input class="form-input" placeholder="Name" @bind="createRequest.Name" />
                    <input class="form-input" placeholder="Year" type="number" @bind="createRequest.Year" />
                    <input class="form-input" placeholder="Start date" type="date" @bind="startDateString" />
                    <input class="form-input" placeholder="End date" type="date" @bind="endDateString" />
                </div>

                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isSaving" @onclick="CloseModals">Cancel</button>
                    <button type="button" class="btn border border-purple text-purple hover:bg-purple hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="CreateEditionAsync">
                        @(isSaving ? "Creating..." : "Create")
                    </button>
                </div>
            </div>
        </div>
    </div>
}

@if (showEditModal)
{
    <div class="fixed inset-0 bg-black/60 z-[999] overflow-y-auto">
        <div class="flex items-start justify-center min-h-screen px-4 py-10">
            <div class="relative w-full max-w-lg overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <h3 class="text-lg font-semibold text-black dark:text-white">Edit Edition</h3>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="CloseModals">
                        <i class="ri-close-line text-2xl"></i>
                    </button>
                </div>

                <div class="p-5 space-y-4">
                    @if (!string.IsNullOrWhiteSpace(modalErrorMessage))
                    {
                        <div class="p-3 text-sm rounded bg-danger/10 text-danger">@modalErrorMessage</div>
                    }

                    <select class="form-select" @bind="updateRequest.FestivalId">
                        <option value="">Select festival</option>
                        @foreach (FestivalDto festival in festivals)
                        {
                            <option value="@festival.Id">@festival.Name</option>
                        }
                    </select>

                    <input class="form-input" placeholder="Name" @bind="updateRequest.Name" />
                    <input class="form-input" placeholder="Year" type="number" @bind="updateRequest.Year" />
                    <input class="form-input" placeholder="Start date" type="date" @bind="startDateString" />
                    <input class="form-input" placeholder="End date" type="date" @bind="endDateString" />

                    <label class="inline-flex items-center gap-2 text-sm text-black dark:text-white">
                        <input type="checkbox" @bind="updateRequest.IsActive" />
                        Active
                    </label>
                </div>

                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isSaving" @onclick="CloseModals">Cancel</button>
                    <button type="button" class="btn border border-purple text-purple hover:bg-purple hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="UpdateEditionAsync">
                        @(isSaving ? "Saving..." : "Save")
                    </button>
                </div>
            </div>
        </div>
    </div>
}

@if (showDeleteModal && selectedEdition is not null)
{
    <div class="fixed inset-0 bg-black/60 z-[999] overflow-y-auto">
        <div class="flex items-start justify-center min-h-screen px-4 py-10">
            <div class="relative w-[92vw] md:w-[380px] overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="px-5 py-4">
                    <h3 class="text-lg font-semibold text-black dark:text-white">Delete Edition</h3>
                    <p class="mt-2 text-sm text-black/60 dark:text-white/50">Are you sure you want to delete @selectedEdition.Name?</p>

                    @if (!string.IsNullOrWhiteSpace(modalErrorMessage))
                    {
                        <div class="p-3 mt-4 text-sm rounded bg-danger/10 text-danger">@modalErrorMessage</div>
                    }
                </div>

                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isSaving" @onclick="CloseModals">Cancel</button>
                    <button type="button" class="btn border border-danger text-danger hover:bg-danger hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="DeleteEditionAsync">
                        @(isSaving ? "Deleting..." : "Delete")
                    </button>
                </div>
            </div>
        </div>
    </div>
}

@code {
    private IReadOnlyList<EditionDto> allEditions = [];
    private IReadOnlyList<FestivalDto> festivals = [];
    private EditionDto? selectedEdition;
    private CreateEditionRequest createRequest = new();
    private UpdateEditionRequest updateRequest = new();

    private string searchText = string.Empty;
    private string statusFilter = "All";
    private int pageSize = 5;
    private int currentPage = 1;

    private bool isLoading = true;
    private bool isSaving;
    private bool showCreateModal;
    private bool showEditModal;
    private bool showDeleteModal;

    private string? successMessage;
    private string? errorMessage;
    private string? modalErrorMessage;

    private string sortColumn = "Year";
    private bool sortAscending = true;

    private string startDateString = string.Empty;
    private string endDateString = string.Empty;

    private IReadOnlyList<EditionDto> FilteredEditions
    {
        get
        {
            IEnumerable<EditionDto> query = allEditions;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(e => e.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) || e.Year.ToString().Contains(searchText));
            }

            query = statusFilter switch
            {
                "Active" => query.Where(e => e.IsActive),
                "Inactive" => query.Where(e => !e.IsActive),
                _ => query
            };

            query = sortColumn switch
            {
                "Year" => sortAscending ? query.OrderBy(e => e.Year) : query.OrderByDescending(e => e.Year),
                "Name" => sortAscending ? query.OrderBy(e => e.Name) : query.OrderByDescending(e => e.Name),
                "Festival" => sortAscending ? query.OrderBy(e => e.FestivalId) : query.OrderByDescending(e => e.FestivalId),
                "StartDate" => sortAscending ? query.OrderBy(e => e.StartDate) : query.OrderByDescending(e => e.StartDate),
                "EndDate" => sortAscending ? query.OrderBy(e => e.EndDate) : query.OrderByDescending(e => e.EndDate),
                "Status" => sortAscending ? query.OrderBy(e => e.IsActive) : query.OrderByDescending(e => e.IsActive),
                _ => query
            };

            return query.ToList();
        }
    }

    private IReadOnlyList<EditionDto> PagedEditions => FilteredEditions.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

    private int TotalPages => Math.Max(1, (int)Math.Ceiling((double)FilteredEditions.Count / pageSize));

    private int ShowingFrom => FilteredEditions.Count == 0 ? 0 : ((currentPage - 1) * pageSize) + 1;

    private int ShowingTo => Math.Min(currentPage * pageSize, FilteredEditions.Count);

    protected override async Task OnInitializedAsync()
    {
        await LoadEditionsAsync();
        await LoadFestivalsAsync();
    }

    private async Task LoadFestivalsAsync()
    {
        try
        {
            festivals = await FestivalApiClient.GetAllAsync();
        }
        catch (ApiClientException)
        {
            // ignore here; Editions load will show errors
        }
        catch (Exception)
        {
            // ignore
        }
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

        ResetPage();
    }

    private MarkupString SortIcon(string column)
    {
        if (sortColumn != column)
        {
            return new MarkupString("<i class=\"ri-arrow-up-down-fill text-xs text-black/30\"></i>");
        }

        string icon = sortAscending ? "ri-arrow-up-fill" : "ri-arrow-down-fill";
        return new MarkupString($"<i class=\"{icon} text-xs text-purple\"></i>");
    }

    private async Task ClearMessagesAfterDelayAsync()
    {
        await Task.Delay(3500);
        successMessage = null;
        errorMessage = null;
        modalErrorMessage = null;
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadEditionsAsync()
    {
        try
        {
            isLoading = true;
            errorMessage = null;
            allEditions = await EditionApiClient.GetAllAsync();
            NormalizePage();
        }
        catch (ApiClientException ex)
        {
            errorMessage = BuildErrorMessage(ex);
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
        createRequest = new CreateEditionRequest();
        modalErrorMessage = null;
        startDateString = DateTime.Now.ToString("yyyy-MM-dd");
        endDateString = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
        showCreateModal = true;
    }

    private void OpenEditModal(EditionDto edition)
    {
        selectedEdition = edition;
        modalErrorMessage = null;
        updateRequest = new UpdateEditionRequest
        {
            FestivalId = edition.FestivalId,
            Name = edition.Name,
            Year = edition.Year,
            StartDate = edition.StartDate,
            EndDate = edition.EndDate,
            RegistrationOpenDate = edition.RegistrationOpenDate,
            RegistrationCloseDate = edition.RegistrationCloseDate,
            IsActive = edition.IsActive
        };

        startDateString = edition.StartDate.ToString("yyyy-MM-dd");
        endDateString = edition.EndDate.ToString("yyyy-MM-dd");
        showEditModal = true;
    }

    private void OpenDeleteModal(EditionDto edition)
    {
        selectedEdition = edition;
        modalErrorMessage = null;
        showDeleteModal = true;
    }

    private void CloseModals()
    {
        showCreateModal = false;
        showEditModal = false;
        showDeleteModal = false;
        selectedEdition = null;
        modalErrorMessage = null;
    }

    private async Task CreateEditionAsync()
    {
        if (isSaving)
        {
            return;
        }

        try
        {
            isSaving = true;
            modalErrorMessage = null;

            createRequest.StartDate = DateTime.Parse(startDateString);
            createRequest.EndDate = DateTime.Parse(endDateString);

            await EditionApiClient.CreateAsync(createRequest);

            CloseModals();
            successMessage = "Edition created successfully.";
            errorMessage = null;
            _ = ClearMessagesAfterDelayAsync();
            await LoadEditionsAsync();
        }
        catch (ApiClientException ex)
        {
            modalErrorMessage = BuildErrorMessage(ex);
        }
        catch (Exception ex)
        {
            modalErrorMessage = ex.Message;
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task UpdateEditionAsync()
    {
        if (isSaving)
        {
            return;
        }

        if (selectedEdition is null)
        {
            return;
        }

        try
        {
            isSaving = true;
            updateRequest.StartDate = DateTime.Parse(startDateString);
            updateRequest.EndDate = DateTime.Parse(endDateString);

            await EditionApiClient.UpdateAsync(selectedEdition.Id, updateRequest);
            CloseModals();
            successMessage = "Edition updated successfully.";
            errorMessage = null;
            _ = ClearMessagesAfterDelayAsync();
            await LoadEditionsAsync();
        }
        catch (ApiClientException ex)
        {
            modalErrorMessage = BuildErrorMessage(ex);
        }
        catch (Exception ex)
        {
            modalErrorMessage = ex.Message;
        }
        finally
        {
            isSaving = false;
        }
    }

    private async Task DeleteEditionAsync()
    {
        if (isSaving)
        {
            return;
        }

        if (selectedEdition is null)
        {
            return;
        }

        try
        {
            isSaving = true;
            await EditionApiClient.DeleteAsync(selectedEdition.Id);
            CloseModals();
            successMessage = "Edition deleted successfully.";
            errorMessage = null;
            _ = ClearMessagesAfterDelayAsync();
            await LoadEditionsAsync();
        }
        catch (ApiClientException ex)
        {
            modalErrorMessage = BuildErrorMessage(ex);
        }
        catch (Exception ex)
        {
            modalErrorMessage = ex.Message;
        }
        finally
        {
            isSaving = false;
        }
    }

    private static string BuildErrorMessage(ApiClientException exception)
    {
        if (exception.Errors.Count == 0)
        {
            return exception.Message;
        }

        return $"{exception.Message} {string.Join(" ", exception.Errors)}";
    }

    private string GetFestivalName(Guid festivalId)
    {
        FestivalDto? festival = festivals.FirstOrDefault(f => f.Id == festivalId);
        return festival is null ? festivalId.ToString() : festival.Name;
    }
}
'@

# PassTypes.razor (full)
$files["Components\Pages\PassTypes.razor"] = @'
@using Alakai.FestivalManager.Admin.Contracts.PassTypes
@using Alakai.FestivalManager.Admin.Contracts.PassTypes.Requests
@using Alakai.FestivalManager.Admin.Contracts.PassTypes.Responses
@using Alakai.FestivalManager.Admin.Contracts.Editions
@using Alakai.FestivalManager.Admin.Services.Api
@inject PassTypeApiClient PassTypeApiClient
@inject EditionApiClient EditionApiClient

<PageHeader Title="Configuration" pTitle="Pass Types"></PageHeader>

<div class="flex flex-col gap-4 min-h-[calc(100vh-212px)]">
    <div class="grid grid-cols-1 gap-4">
        <div class="card">
            <div class="flex flex-col gap-4 mb-5 xl:flex-row xl:items-center xl:justify-between">
                <div>
                    <h2 class="text-base font-semibold text-black capitalize dark:text-white">Pass Types</h2>
                    <p class="text-sm text-black/50 dark:text-white/40">Manage pass type configuration.</p>
                </div>

                <div class="flex flex-col gap-3 md:flex-row md:items-center">
                    <div class="relative w-full md:w-72">
                        <input class="form-input" placeholder="Search pass types..." @bind="searchText" @bind:event="oninput" @bind:after="ResetPage" />
                    </div>

                    <select class="form-select w-full md:w-44" @bind="statusFilter" @bind:after="ResetPage">
                        <option value="All">All statuses</option>
                        <option value="Active">Active</option>
                        <option value="Inactive">Inactive</option>
                    </select>

                    <select class="form-select w-full md:w-32" @bind="pageSize" @bind:after="ResetPage">
                        <option value="5">5 rows</option>
                        <option value="10">10 rows</option>
                        <option value="25">25 rows</option>
                        <option value="50">50 rows</option>
                    </select>

                    <button type="button" class="transition-all duration-300 border rounded-md btn text-purple border-purple hover:bg-purple hover:text-white whitespace-nowrap" @onclick="OpenCreateModal">
                        <i class="ri-add-line ltr:mr-1 rtl:ml-1"></i>
                        New Pass Type
                    </button>
                </div>
            </div>

            @if (!string.IsNullOrWhiteSpace(successMessage))
            {
                <div class="p-3 mb-4 text-sm rounded bg-success/10 text-success">
                    @successMessage
                </div>
            }

            @if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                <div class="p-3 mb-4 text-sm rounded bg-danger/10 text-danger">
                    @errorMessage
                </div>
            }

            @if (isLoading)
            {
                <p class="text-sm text-black/50 dark:text-white/40">Loading pass types...</p>
            }
            else
            {
                <div class="overflow-x-auto">
                    <table class="w-full table-hover">
                        <thead class="bg-gray-50 dark:bg-dark">
                            <tr class="text-left">
                                <th class="px-4 py-3 font-semibold">
                                    <button type="button" class="flex items-center gap-1 w-full font-semibold" @onclick='() => SortBy("Name")'>
                                        Name
                                        @SortIcon("Name")
                                    </button>
                                </th>
                                <th class="px-4 py-3 font-semibold">
                                    <button type="button" class="flex items-center gap-1 w-full font-semibold" @onclick='() => SortBy("Edition")'>
                                        Edition
                                        @SortIcon("Edition")
                                    </button>
                                </th>
                                <th class="px-4 py-3 font-semibold">
                                    <button type="button" class="flex items-center gap-1 w-full font-semibold" @onclick='() => SortBy("SortOrder")'>
                                        Sort
                                        @SortIcon("SortOrder")
                                    </button>
                                </th>
                                <th class="px-4 py-3 font-semibold">
                                    <button type="button" class="flex items-center gap-1 w-full font-semibold" @onclick='() => SortBy("Status")'>
                                        Status
                                        @SortIcon("Status")
                                    </button>
                                </th>
                                <th class="px-4 py-3 font-semibold text-right">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            @if (PagedPassTypes.Count == 0)
                            {
                                <tr>
                                    <td colspan="5" class="px-4 py-6 text-center text-black/50 dark:text-white/40">No pass types found.</td>
                                </tr>
                            }
                            else
                            {
                                @foreach (PassTypeDto passType in PagedPassTypes)
                                {
                                    <tr class="border-b border-black/10 dark:border-darkborder">
                                        <td class="px-4 py-3">@passType.Name</td>
                                        <td class="px-4 py-3">@GetEditionName(passType.EditionId)</td>
                                        <td class="px-4 py-3">@passType.SortOrder</td>
                                        <td class="px-4 py-3">
                                            @if (passType.IsActive)
                                            {
                                                <span class="inline-block rounded text-xs px-2 py-1 bg-success/10 text-success">Active</span>
                                            }
                                            else
                                            {
                                                <span class="inline-block rounded text-xs px-2 py-1 bg-danger/10 text-danger">Inactive</span>
                                            }
                                        </td>
                                        <td class="px-4 py-3">
                                            <div class="flex items-center justify-end gap-3">
                                                <button type="button" class="text-black dark:text-white/80" title="Edit" @onclick="() => OpenEditModal(passType)">
                                                    <i class="ri-pencil-line text-lg"></i>
                                                </button>
                                                <button type="button" class="text-danger" title="Delete" @onclick="() => OpenDeleteModal(passType)">
                                                    <i class="ri-delete-bin-line text-lg"></i>
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                }
                            }
                        </tbody>
                    </table>
                </div>

                <div class="flex items-center justify-between mt-4">
                    <p class="text-sm text-black/50 dark:text-white/40">
                        Showing @ShowingFrom to @ShowingTo of @FilteredPassTypes.Count pass types
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
</div>

@if (showCreateModal)
{
    <div class="fixed inset-0 bg-black/60 z-[999] overflow-y-auto">
        <div class="flex items-start justify-center min-h-screen px-4 py-10">
            <div class="relative w-full max-w-lg overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <h3 class="text-lg font-semibold text-black dark:text-white">New Pass Type</h3>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="CloseModals">
                        <i class="ri-close-line text-2xl"></i>
                    </button>
                </div>

                <div class="p-5 space-y-4">
                    @if (!string.IsNullOrWhiteSpace(modalErrorMessage))
                    {
                        <div class="p-3 text-sm rounded bg-danger/10 text-danger">@modalErrorMessage</div>
                    }

                    <select class="form-select" @bind="createRequest.EditionId">
                        <option value="">Select edition</option>
                        @foreach (EditionDto edition in editions)
                        {
                            <option value="@edition.Id">@($"{edition.Year} - {edition.Name}")</option>
                        }
                    </select>

                    <input class="form-input" placeholder="Name" @bind="createRequest.Name" />
                    <input class="form-input" placeholder="Sort order" type="number" @bind="createRequest.SortOrder" />
                    <textarea class="form-input" placeholder="Description" rows="3" @bind="createRequest.Description"></textarea>
                </div>

                <div class="flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder">
                    <button type="button" class="btn border border-black/10" disabled="@isSaving" @onclick="CloseModals">Cancel</button>
                    <button type="button" class="btn border border-purple text-purple hover:bg-purple hover:text-white disabled:opacity-50" disabled="@isSaving" @onclick="CreatePassTypeAsync">
                        @(isSaving ? "Creating..." : "Create")
                    </button>
                </div>
            </div>
        </div>
    </div>
}

@if (showEditModal)
{
    <div class="fixed inset-0 bg-black/60 z-[999] overflow-y-auto">
        <div class="flex items-start justify-center min-h-screen px-4 py-10">
            <div class="relative w-full max-w-lg overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder">
                <div class="flex items-center justify-between px-5 py-3 border-b border-black/10 dark:border-darkborder">
                    <h3 class="text-lg font-semibold text-black dark:text-white">Edit Pass Type</h3>
                    <button type="button" class="text-black/50 hover:text-black dark:text-white/60" @onclick="CloseModals">
                        <i class="ri-close-line text-2xl"></i>
                    </button>
                </div>

                <div class="p-5 space-y-4">
                    @if (!string.IsNullOrWhiteSpace(modalErrorMessage))
                    {
                        <div class="p-3 text-sm rounded bg-danger/10 text-danger">@modalErrorMessage</div>
                    }

                    <select class="form-select" @bind="updateRequest.EditionId">
                        <option value="">Select edition</option>
                        @foreach (EditionDto edition in editions)
                        {
                            <option value="@edition.Id">@($"{edition.Year} - {edition.Name}")</option>
                        }
                    </select>

                    <input class="form-input" placeholder="Name" @bind="updateRequest.Name" />
                    <input class="form-input" placeholder="Sort order" type="number" @bind="updateRequest.SortOrder" />
                    <textarea class="form-input" placeholder="Description" rows="3" @bind="updateRequest.Description"></textarea>

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

... (script continues: Levels.razor and remaining code identical to content provided earlier)
'@

# Write files
foreach ($rel in $files.Keys) {
    $full = Join-Path $projectDir $rel
    Write-FileIfNotExists -fullPath $full -content $files[$rel]
}

# Append ApiClient registrations to Program.cs if missing
$programCs = Join-Path $projectDir "Program.cs"
if (Test-Path $programCs) {
    $text = Get-Content $programCs -Raw
    if ($text -notmatch "AddHttpClient<EditionApiClient>") {
        $block = @"
builder.Services.AddHttpClient<EditionApiClient>(client =>
{
    string baseUrl = builder.Configuration[""ApiSettings:BaseUrl""] ?? throw new System.InvalidOperationException(""ApiSettings:BaseUrl is not configured."");
    client.BaseAddress = new System.Uri(baseUrl);
});

builder.Services.AddHttpClient<PassTypeApiClient>(client =>
{
    string baseUrl = builder.Configuration[""ApiSettings:BaseUrl""] ?? throw new System.InvalidOperationException(""ApiSettings:BaseUrl is not configured."");
    client.BaseAddress = new System.Uri(baseUrl);
});

builder.Services.AddHttpClient<LevelApiClient>(client =>
{
    string baseUrl = builder.Configuration[""ApiSettings:BaseUrl""] ?? throw new System.InvalidOperationException(""ApiSettings:BaseUrl is not configured."");
    client.BaseAddress = new System.Uri(baseUrl);
});
"@
        $text + $block | Out-File -FilePath $programCs -Encoding utf8
        Write-Host "Patched Program.cs (appended ApiClient registrations)."
    }
    else {
        Write-Host "Program.cs already contains EditionApiClient registration. No change."
    }
}
else {
    Write-Warning "Program.cs not found at $programCs — register ApiClients manually."
}

Write-Host "Script finished. Review created files under: $projectDir"
