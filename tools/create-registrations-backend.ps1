param(
    [string]$Root = (Get-Location).Path
)

function Write-FileIfNotExists([string]$path, [string]$content)
{
    $dir = Split-Path $path -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    if (-not (Test-Path $path))
    {
        $content | Out-File -FilePath $path -Encoding utf8
        Write-Host "Created $path"
    }
    else
    {
        Write-Host "Skipped existing $path"
    }
}

$base = Join-Path $Root "Alakai.FestivalManager.Application\Features\Registrations\Contracts"
$dtoDir = Join-Path $base "DTOs"
$reqDir = Join-Path $base "Requests"
$respDir = Join-Path $base "Responses"

# DTO
$dtoPath = Join-Path $dtoDir "RegistrationDto.cs"
$dtoContent = @'
using System;
using Alakai.FestivalManager.Domain.Enums;

namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.DTOs;

public class RegistrationDto
{
    public Guid Id { get; set; }

    public Guid EditionId { get; set; }

    public Guid PassTypeId { get; set; }

    public Guid? LevelId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public DanceRole? DanceRole { get; set; }

    public string? PartnerEmail { get; set; }

    public Guid? PartnerRegistrationId { get; set; }

    public RegistrationStatus Status { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public decimal BasePrice { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal FinalPrice { get; set; }

    public string? DiscountCode { get; set; }

    public string? PaymentReference { get; set; }

    public DateTime? PaidAt { get; set; }

    public string? Notes { get; set; }

    public string? InternalNotes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public bool IsActive { get; set; }
}
'@

# Requests
$createReqPath = Join-Path $reqDir "CreateRegistrationRequest.cs"
$createReqContent = @'
using System;
using Alakai.FestivalManager.Domain.Enums;

namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.Requests;

public class CreateRegistrationRequest
{
    public Guid EditionId { get; set; }

    public Guid PassTypeId { get; set; }

    public Guid? LevelId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public DanceRole? DanceRole { get; set; }

    public string? PartnerEmail { get; set; }

    public decimal BasePrice { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal FinalPrice { get; set; }

    public string? DiscountCode { get; set; }

    public string? Notes { get; set; }

    public string? InternalNotes { get; set; }
}
'@

$updateReqPath = Join-Path $reqDir "UpdateRegistrationRequest.cs"
$updateReqContent = @'
using System;
using Alakai.FestivalManager.Domain.Enums;

namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.Requests;

public class UpdateRegistrationRequest
{
    public Guid Id { get; set; }

    public Guid EditionId { get; set; }

    public Guid PassTypeId { get; set; }

    public Guid? LevelId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public DanceRole? DanceRole { get; set; }

    public string? PartnerEmail { get; set; }

    public Guid? PartnerRegistrationId { get; set; }

    public RegistrationStatus Status { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public decimal BasePrice { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal FinalPrice { get; set; }

    public string? DiscountCode { get; set; }

    public string? PaymentReference { get; set; }

    public DateTime? PaidAt { get; set; }

    public string? Notes { get; set; }

    public string? InternalNotes { get; set; }

    public DateTime? CancelledAt { get; set; }

    public bool IsActive { get; set; }
}
'@

# Responses
$getAllRespPath = Join-Path $respDir "GetRegistrationsResponse.cs"
$getAllRespContent = @'
using System.Collections.Generic;
using Alakai.FestivalManager.Application.Features.Registrations.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.Responses;

public class GetRegistrationsResponse
{
    public IReadOnlyList<RegistrationDto> Registrations { get; set; } = new List<RegistrationDto>();
}
'@

$createRespPath = Join-Path $respDir "CreateRegistrationResponse.cs"
$createRespContent = @'
using Alakai.FestivalManager.Application.Features.Registrations.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.Responses;

public class CreateRegistrationResponse
{
    public RegistrationDto Registration { get; set; } = new RegistrationDto();
}
'@

$updateRespPath = Join-Path $respDir "UpdateRegistrationResponse.cs"
$updateRespContent = @'
using Alakai.FestivalManager.Application.Features.Registrations.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.Responses;

public class UpdateRegistrationResponse
{
    public RegistrationDto Registration { get; set; } = new RegistrationDto();
}
'@

$deleteRespPath = Join-Path $respDir "DeleteRegistrationResponse.cs"
$deleteRespContent = @'
using System;

namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.Responses;

public class DeleteRegistrationResponse
{
    public Guid Id { get; set; }

    public bool Deleted { get; set; }
}
'@

$getByIdRespPath = Join-Path $respDir "GetRegistrationByIdResponse.cs"
$getByIdRespContent = @'
using Alakai.FestivalManager.Application.Features.Registrations.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.Responses;

public class GetRegistrationByIdResponse
{
    public RegistrationDto Registration { get; set; } = new RegistrationDto();
}
'@

$getByEditionRespPath = Join-Path $respDir "GetRegistrationsByEditionIdResponse.cs"
$getByEditionRespContent = @'
using System.Collections.Generic;
using Alakai.FestivalManager.Application.Features.Registrations.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.Responses;

public class GetRegistrationsByEditionIdResponse
{
    public IReadOnlyList<RegistrationDto> Registrations { get; set; } = new List<RegistrationDto>();
}
'@

# Create folders and files
Write-FileIfNotExists $dtoPath $dtoContent
Write-FileIfNotExists $createReqPath $createReqContent
Write-FileIfNotExists $updateReqPath $updateReqContent
Write-FileIfNotExists $getAllRespPath $getAllRespContent
Write-FileIfNotExists $createRespPath $createRespContent
Write-FileIfNotExists $updateRespPath $updateRespContent
Write-FileIfNotExists $deleteRespPath $deleteRespContent
Write-FileIfNotExists $getByIdRespPath $getByIdRespContent
Write-FileIfNotExists $getByEditionRespPath $getByEditionRespContent

Write-Host "Created application contracts under: $base"
Write-Host "Review and adjust namespaces if your project layout differs."