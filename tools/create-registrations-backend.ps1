# Script 14: Add confirmation modal before sending emails from Registration Entries
# Shows: "Send [email type] to [name] ([email])?" with Cancel / Send buttons
# Run from repo root: .\fix_14_email_confirm_modal.ps1

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Patch-File {
    param([string]$Path, [string]$OldText, [string]$NewText, [string]$Description)
    $raw = [System.IO.File]::ReadAllText($Path)
    $rawNorm = $raw.Replace("`r`n", "`n")
    $oldNorm = $OldText.Replace("`r`n", "`n")
    $newNorm = $NewText.Replace("`r`n", "`n")
    $count = ([regex]::Matches($rawNorm, [regex]::Escape($oldNorm))).Count
    if ($count -ne 1) {
        Write-Error "ABORT: '$Description' — expected 1 match in '$Path', found $count"
        exit 1
    }
    $useCRLF = $raw.Contains("`r`n")
    $patched = $rawNorm.Replace($oldNorm, $newNorm)
    if ($useCRLF) { $patched = $patched.Replace("`n", "`r`n") }
    [System.IO.File]::WriteAllText($Path, $patched, [System.Text.Encoding]::UTF8)
    Write-Host "OK: $Description"
}

$razor = "Alakai.FestivalManager.Admin\Components\Pages\Registrations.razor"

# ── 1. Change email buttons to open confirm modal instead of firing directly ──
Patch-File $razor `
    "                                            <button type=""button"" class=""text-black dark:text-white/80 hover:text-purple"" title=""Registration confirmation email"" @onclick='() => EmailAction(""registration confirmation"", registration)'><i class=""ri-mail-send-line""></i></button>
                                            <button type=""button"" class=""text-black dark:text-white/80 hover:text-purple"" title=""Payment confirmation email"" @onclick='() => EmailAction(""payment confirmation"", registration)'><i class=""ri-bank-card-line""></i></button>
                                            <button type=""button"" class=""text-black dark:text-white/80 hover:text-purple"" title=""Unpaid reminder email"" @onclick='() => EmailAction(""unpaid reminder"", registration)'><i class=""ri-error-warning-line""></i></button>
                                            <button type=""button"" class=""text-black dark:text-white/80 hover:text-purple"" title=""Missing partner email"" @onclick='() => EmailAction(""missing partner"", registration)'><i class=""ri-user-follow-line""></i></button>" `
    "                                            <button type=""button"" class=""text-black dark:text-white/80 hover:text-purple"" title=""Registration confirmation email"" @onclick='() => OpenEmailConfirmModal(""registration confirmation"", registration)'><i class=""ri-mail-send-line""></i></button>
                                            <button type=""button"" class=""text-black dark:text-white/80 hover:text-purple"" title=""Payment confirmation email"" @onclick='() => OpenEmailConfirmModal(""payment confirmation"", registration)'><i class=""ri-bank-card-line""></i></button>
                                            <button type=""button"" class=""text-black dark:text-white/80 hover:text-purple"" title=""Unpaid reminder email"" @onclick='() => OpenEmailConfirmModal(""unpaid reminder"", registration)'><i class=""ri-error-warning-line""></i></button>
                                            <button type=""button"" class=""text-black dark:text-white/80 hover:text-purple"" title=""Missing partner email"" @onclick='() => OpenEmailConfirmModal(""missing partner"", registration)'><i class=""ri-user-follow-line""></i></button>" `
    "Registrations.razor: wire email buttons to OpenEmailConfirmModal"

# ── 2. Add email confirm modal markup before the delete modal ──
Patch-File $razor `
    "@if (deleteRegistration is not null)
{
    <div class=""fixed inset-0 bg-black/60 z-[999] overflow-y-auto"">" `
    "@if (emailConfirmRegistration is not null)
{
    <div class=""fixed inset-0 bg-black/60 z-[999] overflow-y-auto"">
        <div class=""flex items-start justify-center min-h-screen px-4 py-10"">
            <div class=""relative w-[92vw] md:w-[420px] overflow-hidden bg-white border rounded-lg shadow-3xl border-black/10 dark:bg-darklight dark:border-darkborder"">
                <div class=""px-5 py-4"">
                    <h3 class=""text-lg font-semibold text-black dark:text-white"">Send Email</h3>
                    <p class=""mt-2 text-sm text-black/60 dark:text-white/50"">
                        Send <strong>@emailConfirmAction</strong> email to <strong>@emailConfirmRegistration.FullName</strong>?
                    </p>
                    <p class=""mt-1 text-xs text-black/40 dark:text-white/40"">@emailConfirmRegistration.Email</p>
                </div>
                <div class=""flex justify-end gap-3 px-5 py-4 border-t border-black/10 dark:border-darkborder"">
                    <button type=""button"" class=""btn border border-black/10 dark:border-darkborder dark:text-white/80"" disabled=""@isSaving"" @onclick=""CloseEmailConfirmModal"">Cancel</button>
                    <button type=""button"" class=""btn border border-purple text-purple hover:bg-purple hover:text-white disabled:opacity-50"" disabled=""@isSaving"" @onclick=""ConfirmEmailAsync"">@(isSaving ? ""Sending..."" : ""Send"")</button>
                </div>
            </div>
        </div>
    </div>
}

@if (deleteRegistration is not null)
{
    <div class=""fixed inset-0 bg-black/60 z-[999] overflow-y-auto"">" `
    "Registrations.razor: add email confirmation modal markup"

# ── 3. Add state vars for email confirm modal ──
Patch-File $razor `
    "    private RegistrationDto? editingRegistration;
    private RegistrationDto? deleteRegistration;" `
    "    private RegistrationDto? editingRegistration;
    private RegistrationDto? deleteRegistration;
    private RegistrationDto? emailConfirmRegistration;
    private string emailConfirmAction = string.Empty;" `
    "Registrations.razor: add email confirm modal state vars"

# ── 4. Add OpenEmailConfirmModal, CloseEmailConfirmModal, ConfirmEmailAsync methods ──
Patch-File $razor `
    "    private async Task EmailAction(string action, RegistrationDto registration)
    {
        try
        {
            switch (action)
            {" `
    "    private void OpenEmailConfirmModal(string action, RegistrationDto registration)
    {
        emailConfirmAction = action;
        emailConfirmRegistration = registration;
    }

    private void CloseEmailConfirmModal()
    {
        emailConfirmRegistration = null;
        emailConfirmAction = string.Empty;
    }

    private async Task ConfirmEmailAsync()
    {
        if (emailConfirmRegistration is null) return;
        RegistrationDto registration = emailConfirmRegistration;
        string action = emailConfirmAction;
        CloseEmailConfirmModal();
        await EmailAction(action, registration);
    }

    private async Task EmailAction(string action, RegistrationDto registration)
    {
        try
        {
            switch (action)
            {" `
    "Registrations.razor: add OpenEmailConfirmModal, CloseEmailConfirmModal, ConfirmEmailAsync"

Write-Host ""
Write-Host "Done. Email buttons now show a confirmation modal before sending."