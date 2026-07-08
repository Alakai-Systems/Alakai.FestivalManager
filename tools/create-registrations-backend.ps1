# patch-invoice-error-wording-and-timing.ps1
# Two fixes to the invoice payment gate:
#   1. Wording: uses invoice-specific text ("...before requesting an
#      invoice") instead of the generic reservation message, since
#      requesting an invoice is not a reservation.
#   2. Timing: the banner above "Create invoice" now auto-dismisses
#      after 3.5 seconds via ShowInvoiceListError, matching the same
#      InvokeAsync + Task.Delay(3500) + StateHasChanged pattern used by
#      every other Show*Error method in this file (ShowMealError,
#      ShowBusError, ShowAccommodationError), instead of staying on
#      screen permanently.
$ErrorActionPreference = "Stop"
$up = "Alakai.FestivalManager.Admin\Components\Pages\UserPanelDashboard\UserPanel.razor"
$rawUp = [System.IO.File]::ReadAllBytes($up)
$hadBomUp = ($rawUp.Length -ge 3 -and $rawUp[0] -eq 0xEF -and $rawUp[1] -eq 0xBB -and $rawUp[2] -eq 0xBF)
$rawUpText = [System.IO.File]::ReadAllText($up)
$hadCrlfUp = $rawUpText.Contains("`r`n")
$upContent = ($rawUpText -replace "`r`n","`n")

$o1 = @'
    private string? InvoiceListError { get; set; }

    private void OpenInvoiceModal()
    {
        InvoiceListError = null;

        if (!HasSufficientPayment)
        {
            InvoiceListError = PaymentRequiredMessage;
            return;
        }

        InvoiceModalError = null;
        InvoiceFiscalName = $"{FirstName} {LastName}".Trim();
        InvoiceTaxId = DocumentNumber ?? string.Empty;
        InvoiceAddress = string.Empty;
        InvoiceCity = City ?? string.Empty;
        InvoicePostalCode = string.Empty;
        InvoiceCountry = Country ?? string.Empty;
        ShowInvoiceModal = true;
    }
'@
$n1 = @'
    private string? InvoiceListError { get; set; }

    private void ShowInvoiceListError(string message)
    {
        InvoiceListError = message;
        InvokeAsync(async () =>
        {
            await Task.Delay(3500);
            InvoiceListError = null;
            StateHasChanged();
        });
    }

    private void OpenInvoiceModal()
    {
        if (!HasSufficientPayment)
        {
            ShowInvoiceListError("You must complete at least a partial payment for your registration before requesting an invoice.");
            return;
        }

        InvoiceListError = null;
        InvoiceModalError = null;
        InvoiceFiscalName = $"{FirstName} {LastName}".Trim();
        InvoiceTaxId = DocumentNumber ?? string.Empty;
        InvoiceAddress = string.Empty;
        InvoiceCity = City ?? string.Empty;
        InvoicePostalCode = string.Empty;
        InvoiceCountry = Country ?? string.Empty;
        ShowInvoiceModal = true;
    }
'@
$o1 = $o1 -replace "`r`n","`n"
$n1 = $n1 -replace "`r`n","`n"
$c1 = ([regex]::Matches($upContent,[regex]::Escape($o1))).Count
if ($c1 -ne 1) { Write-Host "OpenInvoiceModal anchor not found ($c1) - already patched or file differs. Skipping that part." -ForegroundColor Yellow }
else { $upContent = $upContent.Replace($o1,$n1) }

$o2 = @'
InvoiceModalError = PaymentRequiredMessage;
'@
$n2 = @'
InvoiceModalError = "You must complete at least a partial payment for your registration before requesting an invoice.";
'@
$c2 = ([regex]::Matches($upContent,[regex]::Escape($o2))).Count
if ($c2 -ne 1) { Write-Host "CreateInvoiceAsync message anchor not found ($c2) - already patched or file differs. Skipping that part." -ForegroundColor Yellow }
else { $upContent = $upContent.Replace($o2,$n2) }

if ($hadCrlfUp) { $upContent = $upContent -replace "`n","`r`n" }
[System.IO.File]::WriteAllText($up, $upContent, (New-Object System.Text.UTF8Encoding($hadBomUp)))
Write-Host "OK: UserPanel.razor updated. dotnet build" -ForegroundColor Green