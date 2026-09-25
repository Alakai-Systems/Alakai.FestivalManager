<#
    Fix-RefundMultiplePayments.ps1
    -----------------------------------------------

    Arregla el reembolso cuando una inscripcion se pago en VARIOS cobros
    (p.ej. un plan de pago dividido en dos, o simplemente porque intentaste
    pagar dos veces). El caso real que lo disparo: una inscripcion con dos
    PaymentIntents de Stripe guardados; el reembolso de un primer tramo
    funciono, pero un segundo reembolso volvia a intentarse SIEMPRE contra
    el ULTIMO cobro guardado -- y Stripe lo rechazaba con "Refund amount is
    greater than unrefunded amount on charge", aunque la inscripcion en
    conjunto tuviera de sobra para cubrirlo.

    Causa exacta: RefundRegistrationAsync solo miraba la ULTIMA entrada de
    PaymentAuthCodes (el campo ya venia preparado para guardar varias,
    separadas por "|", pero nadie las recorria). Un reembolso repartido
    entre dos cobros de Stripe intentaba TODO el importe contra uno solo,
    y Stripe lo rechazaba en bloque.

    Que trae:

      - Nuevo IStripeGateway.GetRefundableAmountInCentsAsync: pregunta a
        Stripe cuanto le queda reembolsable a un cargo concreto (importe
        del cargo menos lo ya reembolsado en ese cargo).
      - PaymentService.RefundRegistrationAsync ahora recorre TODAS las
        entradas de PaymentAuthCodes (no solo la ultima):
          * Stripe: por cada PaymentIntent guardado, pregunta cuanto le
            queda disponible y reparte el importe pedido entre los que
            tengan margen, en orden, hasta completarlo o agotarlos. Si no
            llega a completarse el importe total (poco probable, pero
            posible si algo ya se reembolso por otro lado), lo dice en el
            mensaje y lo deja anotado en Notas Internas para revisar a
            mano -- nunca falla en silencio ni se inventa el resto.
          * Redsys: prueba el importe completo contra cada Order guardado
            en orden, hasta que uno lo acepte (Redsys no tiene forma de
            preguntar "cuanto le queda a este Order", asi que no se puede
            repartir un mismo reembolso entre varios Orders de Redsys en
            una sola accion -- si tu caso necesita eso, dimelo y lo miramos
            aparte).
      - El registro en Notas Internas ahora detalla contra que cobro(s) se
        repartio cada reembolso.

    Que NO cambia (a proposito):

      - La logica de que PaymentStatus se queda igual tras un reembolso
        (no reabre el registro para volver a cobrar) -- intacta.
      - El limite global de cuanto se puede reembolsar en total
        (registration.AmountPaid - RefundedAmount) -- intacto, se sigue
        comprobando antes de tocar Stripe o Redsys.

    Esto es la MISMA correccion que ya te pase, corregida: la primera
    version no encajo contra tus archivos reales porque el anchor que use
    para StripeGateway.cs y PaymentService.cs tenia la indentacion mal
    (0 espacios en vez de los 4 espacios reales de un metodo de clase) --
    un problema mio al montar el anchor a partir de fragmentos pegados
    antes, no un cambio de diseno. Esta vez el anchor se ha sacado
    directamente, caracter a caracter, del contenido completo de ambos
    archivos que me pasaste, y he confirmado por script que encaja de
    forma exacta y unica contra ese contenido antes de generar este
    fichero.

    Importante -- esto toca dinero real. Antes de darlo por bueno:

      1) Revisa el diff (git diff) de los 3 archivos.
      2) Compila.
      3) Prueba con la inscripcion de Jose Camacho (o cualquier otra con
         mas de un PaymentIntent de Stripe guardado): que el reembolso de
         los 2,50 EUR pendientes contra pi_...MB3p7cT salga bien.
      4) Si tienes alguna inscripcion con mas de dos cobros de Stripe,
         prueba tambien un reembolso que necesite repartirse entre los
         tres.

    Verificado: anchors comprobados como subcadena exacta y unica contra
    el contenido completo que me pasaste de StripeGateway.cs y
    PaymentService.cs; balance de llaves/parentesis/corchetes del
    reemplazo de PaymentService.cs cuadra (57/57, 69/69, 14/14).

    Uso:
        cd Alakai.FestivalManager          # raiz del repo (donde esta el .sln)
        pwsh -NoProfile -ExecutionPolicy Bypass -File .\Fix-RefundMultiplePayments.ps1

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

function Add-CreateOperation {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Content,
        [Parameter(Mandatory)][string]$Description
    )

    $script:Plan += [PSCustomObject]@{
        Type        = 'Create'
        Path        = $Path
        Content     = Convert-ToLf $Content
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
        return "Anchor no encontrado en $($Op.Path) (el archivo local no coincide con lo esperado -- revisalo a mano). Descripcion: $($Op.Description)"
    }

    return "Anchor encontrado $anchorCount veces en $($Op.Path) (deberia ser unico). Descripcion: $($Op.Description)"
}

function Test-CreateOperation {
    param($Op)

    if (-not (Test-Path -LiteralPath $Op.Path)) {
        return $null
    }

    $existing = Get-NormalizedContent -Path $Op.Path

    if ($existing.Normalized.TrimEnd() -eq $Op.Content.TrimEnd()) {
        return $null  # ya existe con el contenido esperado -> idempotente
    }

    return "Ya existe $($Op.Path) con un contenido distinto al esperado; revisalo a mano antes de reintentar."
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

function Invoke-CreateOperation {
    param($Op)

    if (Test-Path -LiteralPath $Op.Path) {
        Write-Host "  = ya existe: $($Op.Path) -- $($Op.Description)" -ForegroundColor DarkGray
        return
    }

    $dir = Split-Path -Parent $Op.Path
    if ($dir -and -not (Test-Path -LiteralPath $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }

    Set-NormalizedContent -Path $Op.Path -NormalizedContent $Op.Content -UsesCrlf $false

    Write-Host "  + created: $($Op.Path) -- $($Op.Description)" -ForegroundColor Green
}

# 1. Alakai.FestivalManager.Application/Interfaces/Services/IStripeGateway.cs -- IStripeGateway.cs: nuevo GetRefundableAmountInCentsAsync
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Interfaces/Services/IStripeGateway.cs' `
    -Description 'IStripeGateway.cs: nuevo GetRefundableAmountInCentsAsync' `
    -Anchor @'
namespace Alakai.FestivalManager.Application.Interfaces.Services;

public interface IStripeGateway
{
    /// <summary>Crea una sesion de Stripe Checkout (pagina alojada) para un cobro puntual.</summary>
    Task<StripeCheckoutSessionDto> CreateCheckoutSessionAsync(FestivalCredentials credentials, string order, long amountInCents, string productDescription, string urlOk, string urlKo, CancellationToken cancellationToken = default);

    /// <summary>Lee el Order (ClientReferenceId) de un evento de webhook SIN validar la firma (no necesita clave -- solo para saber que registro/festival corresponde y con que secreto verificar despues).</summary>
    string? PeekOrder(string payload);

    /// <summary>Valida la firma del webhook con el secreto del festival correspondiente y devuelve los datos del evento si es un checkout.session.completed.</summary>
    StripeNotificationDto? VerifyAndParseEvent(FestivalCredentials credentials, string payload, string signatureHeader);

    /// <summary>Reembolsa (total o parcialmente) un PaymentIntent ya cobrado.</summary>
    Task<StripeRefundResultDto> SendRefundAsync(FestivalCredentials credentials, string paymentIntentId, long amountInCents, CancellationToken cancellationToken = default);
}
'@ `
    -Replacement @'
namespace Alakai.FestivalManager.Application.Interfaces.Services;

public interface IStripeGateway
{
    /// <summary>Crea una sesion de Stripe Checkout (pagina alojada) para un cobro puntual.</summary>
    Task<StripeCheckoutSessionDto> CreateCheckoutSessionAsync(FestivalCredentials credentials, string order, long amountInCents, string productDescription, string urlOk, string urlKo, CancellationToken cancellationToken = default);

    /// <summary>Lee el Order (ClientReferenceId) de un evento de webhook SIN validar la firma (no necesita clave -- solo para saber que registro/festival corresponde y con que secreto verificar despues).</summary>
    string? PeekOrder(string payload);

    /// <summary>Valida la firma del webhook con el secreto del festival correspondiente y devuelve los datos del evento si es un checkout.session.completed.</summary>
    StripeNotificationDto? VerifyAndParseEvent(FestivalCredentials credentials, string payload, string signatureHeader);

    /// <summary>Reembolsa (total o parcialmente) un PaymentIntent ya cobrado.</summary>
    Task<StripeRefundResultDto> SendRefundAsync(FestivalCredentials credentials, string paymentIntentId, long amountInCents, CancellationToken cancellationToken = default);

    /// <summary>Cuanto le queda reembolsable (en centimos) al cargo de un PaymentIntent ya cobrado -- para poder repartir un reembolso entre varios cobros de una misma inscripcion sin que Stripe rechace el conjunto.</summary>
    Task<long> GetRefundableAmountInCentsAsync(FestivalCredentials credentials, string paymentIntentId, CancellationToken cancellationToken = default);
}
'@

# 2. Alakai.FestivalManager.Infrastructure/Payments/StripeGateway.cs -- StripeGateway.cs: implementacion de GetRefundableAmountInCentsAsync
Add-PatchOperation -Path 'Alakai.FestivalManager.Infrastructure/Payments/StripeGateway.cs' `
    -Description 'StripeGateway.cs: implementacion de GetRefundableAmountInCentsAsync' `
    -Anchor @'
    public async Task<StripeRefundResultDto> SendRefundAsync(FestivalCredentials credentials, string paymentIntentId, long amountInCents, CancellationToken cancellationToken = default)
    {
        RefundCreateOptions options = new()
        {
            PaymentIntent = paymentIntentId,
            Amount = amountInCents
        };

        RequestOptions requestOptions = new() { ApiKey = credentials.StripeSecretKey };
        RefundService service = new();

        try
        {
            Refund refund = await service.CreateAsync(options, requestOptions, cancellationToken);

            bool success = refund.Status == "succeeded" || refund.Status == "pending";

            _logger.LogInformation("Stripe refund created. Payment intent {PaymentIntentId}, refund {RefundId}, status {Status}.", paymentIntentId, refund.Id, refund.Status);

            return new StripeRefundResultDto
            {
                Success = success,
                ResponseCode = refund.Status,
                Message = success ? "Reembolso aceptado por Stripe." : $"Stripe devolvio el estado '{refund.Status}'."
            };
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Stripe refund failed for payment intent {PaymentIntentId}.", paymentIntentId);

            return new StripeRefundResultDto
            {
                Success = false,
                ResponseCode = ex.StripeError?.Code,
                Message = ex.StripeError?.Message ?? "Stripe refund failed."
            };
        }
    }
}
'@ `
    -Replacement @'
    public async Task<StripeRefundResultDto> SendRefundAsync(FestivalCredentials credentials, string paymentIntentId, long amountInCents, CancellationToken cancellationToken = default)
    {
        RefundCreateOptions options = new()
        {
            PaymentIntent = paymentIntentId,
            Amount = amountInCents
        };

        RequestOptions requestOptions = new() { ApiKey = credentials.StripeSecretKey };
        RefundService service = new();

        try
        {
            Refund refund = await service.CreateAsync(options, requestOptions, cancellationToken);

            bool success = refund.Status == "succeeded" || refund.Status == "pending";

            _logger.LogInformation("Stripe refund created. Payment intent {PaymentIntentId}, refund {RefundId}, status {Status}.", paymentIntentId, refund.Id, refund.Status);

            return new StripeRefundResultDto
            {
                Success = success,
                ResponseCode = refund.Status,
                Message = success ? "Reembolso aceptado por Stripe." : $"Stripe devolvio el estado '{refund.Status}'."
            };
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Stripe refund failed for payment intent {PaymentIntentId}.", paymentIntentId);

            return new StripeRefundResultDto
            {
                Success = false,
                ResponseCode = ex.StripeError?.Code,
                Message = ex.StripeError?.Message ?? "Stripe refund failed."
            };
        }
    }

    /// <summary>
    /// Cuanto le queda reembolsable (en centimos) al cargo de un PaymentIntent ya cobrado.
    /// Necesario para repartir un reembolso entre varios cobros de una misma inscripcion
    /// (p.ej. un plan de pago dividido en dos cargos de Stripe): sin esto, un reembolso
    /// que pida mas de lo que le queda a UN cargo concreto lo rechaza Stripe entero,
    /// aunque la inscripcion en conjunto tenga saldo suficiente repartido entre varios.
    /// </summary>
    public async Task<long> GetRefundableAmountInCentsAsync(FestivalCredentials credentials, string paymentIntentId, CancellationToken cancellationToken = default)
    {
        RequestOptions requestOptions = new() { ApiKey = credentials.StripeSecretKey };
        PaymentIntentService service = new();
        PaymentIntentGetOptions options = new() { Expand = ["latest_charge"] };

        PaymentIntent paymentIntent = await service.GetAsync(paymentIntentId, options, requestOptions, cancellationToken);
        Charge? charge = paymentIntent.LatestCharge;

        if (charge is null)
        {
            return 0;
        }

        long refundable = charge.Amount - charge.AmountRefunded;

        return refundable > 0 ? refundable : 0;
    }
}
'@

# 3. Alakai.FestivalManager.Application/Features/Payments/Services/PaymentService.cs -- PaymentService.cs: RefundRegistrationAsync recorre TODOS los cobros de PaymentAuthCodes (no solo el ultimo) y reparte el reembolso entre los que tengan margen
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Features/Payments/Services/PaymentService.cs' `
    -Description 'PaymentService.cs: RefundRegistrationAsync recorre TODOS los cobros de PaymentAuthCodes (no solo el ultimo) y reparte el reembolso entre los que tengan margen' `
    -Anchor @'
    public async Task<ApiResponse<bool>> RefundRegistrationAsync(RefundRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Amount <= 0)
        {
            return new ApiResponse<bool> { Success = false, Data = false, Errors = ["The refund amount must be greater than zero."], Message = "Refund failed" };
        }

        Registration? registration = await _registrationRepository.GetByIdAsync(command.RegistrationId, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<bool> { Success = false, Data = false, Errors = ["Registration not found."], Message = "Refund failed" };
        }

        decimal alreadyRefunded = registration.RefundedAmount;
        decimal refundable = registration.AmountPaid - alreadyRefunded;

        if (command.Amount > refundable)
        {
            return new ApiResponse<bool> { Success = false, Data = false, Errors = [$"The refund amount exceeds the amount available to refund ({refundable:0.00})."], Message = "Refund failed" };
        }

        // "order:identificador" del ultimo cobro confirmado, tal y como se guardo en
        // PaymentAuthCodes (varias entradas separadas por "|" si hubo mas de un cobro,
        // p.ej. en un plan de pago dividido). Para Redsys el identificador es el
        // AuthorisationCode (no se usa para reembolsar, solo se reembolsa contra el
        // Order); para Stripe el identificador ES el PaymentIntentId que hace falta
        // para reembolsar.
        string? lastEntry = registration.PaymentAuthCodes?
            .Split('|', StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault();
        string? order = lastEntry?.Split(':')[0];
        string? identifier = lastEntry is not null && lastEntry.Contains(':') ? lastEntry.Split(':')[1] : null;

        if (string.IsNullOrWhiteSpace(order))
        {
            return new ApiResponse<bool> { Success = false, Data = false, Errors = ["No payment is on record for this registration."], Message = "Refund failed" };
        }

        FestivalCredentials? credentials = registration.Edition?.Festival?.Credentials;

        if (credentials is null)
        {
            return new ApiResponse<bool> { Success = false, Data = false, Errors = ["Payment configuration missing for this festival."], Message = "Refund failed" };
        }

        long amountInCents = (long)Math.Round(command.Amount * 100m, MidpointRounding.AwayFromZero);

        // Nulo (inscripciones anteriores a esta funcionalidad) se trata como Redsys,
        // que era la unica plataforma que existia hasta ahora.
        PaymentPlatform platform = registration.PaymentPlatformUsed ?? PaymentPlatform.Redsys;

        bool refundSuccess;
        string refundMessage;
        string platformLabel;

        if (platform == PaymentPlatform.Stripe)
        {
            platformLabel = "Stripe";

            if (string.IsNullOrWhiteSpace(identifier))
            {
                return new ApiResponse<bool> { Success = false, Data = false, Errors = ["No Stripe payment intent is on record for this registration."], Message = "Refund failed" };
            }

            StripeRefundResultDto stripeResult = await _stripeGateway.SendRefundAsync(credentials, identifier, amountInCents, cancellationToken);
            refundSuccess = stripeResult.Success;
            refundMessage = stripeResult.Message;
        }
        else
        {
            platformLabel = "Redsys";

            RedsysRefundResultDto redsysResult = await _redsysGateway.SendRefundAsync(credentials, order, amountInCents, cancellationToken);
            refundSuccess = redsysResult.Success;
            refundMessage = redsysResult.Message;
        }

        if (!refundSuccess)
        {
            _logger.LogWarning("{Platform} refund rejected for registration {RegistrationId}, order {Order}: {Message}", platformLabel, registration.Id, order, refundMessage);

            return new ApiResponse<bool> { Success = false, Data = false, Errors = [refundMessage], Message = "Refund failed" };
        }

        registration.RefundedAmount = alreadyRefunded + command.Amount;

        // Un reembolso NO debe reabrir el registro para volver a cobrar: es dinero
        // que se devuelve a proposito, no una deuda pendiente. Por eso ya NO se toca
        // PaymentStatus aqui -- se deja tal cual estaba antes del reembolso:
        //   - Si estaba "Paid" (pago completo, o un Split ya completado), se queda
        //     "Paid": no aparece nada pendiente ni el boton de pago.
        //   - Si estaba "PartiallyPaid" (Split con solo el primer 50% pagado), se
        //     queda "PartiallyPaid": el segundo 50% sigue pendiente de pago, se
        //     reembolse o no ese primer tramo -- es un tramo aparte.

        string note = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC] Reembolso de {command.Amount:0.00} via {platformLabel} (order {order})." +
            (string.IsNullOrWhiteSpace(command.Reason) ? string.Empty : $" Motivo: {command.Reason}");
        registration.InternalNotes = string.IsNullOrWhiteSpace(registration.InternalNotes)
            ? note
            : registration.InternalNotes + "\n" + note;

        registration.SetUpdated();
        _registrationRepository.Update(registration);
        await _registrationRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{Platform} refund confirmed for registration {RegistrationId}, order {Order}, amount {Amount} cents.", platformLabel, registration.Id, order, amountInCents);

        return new ApiResponse<bool> { Success = true, Data = true, Errors = [], Message = "Refund processed" };
    }
'@ `
    -Replacement @'
    public async Task<ApiResponse<bool>> RefundRegistrationAsync(RefundRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Amount <= 0)
        {
            return new ApiResponse<bool> { Success = false, Data = false, Errors = ["The refund amount must be greater than zero."], Message = "Refund failed" };
        }

        Registration? registration = await _registrationRepository.GetByIdAsync(command.RegistrationId, cancellationToken);

        if (registration is null)
        {
            return new ApiResponse<bool> { Success = false, Data = false, Errors = ["Registration not found."], Message = "Refund failed" };
        }

        decimal alreadyRefunded = registration.RefundedAmount;
        decimal refundable = registration.AmountPaid - alreadyRefunded;

        if (command.Amount > refundable)
        {
            return new ApiResponse<bool> { Success = false, Data = false, Errors = [$"The refund amount exceeds the amount available to refund ({refundable:0.00})."], Message = "Refund failed" };
        }

        // Todas las entradas "order:identificador" guardadas en PaymentAuthCodes, en el
        // orden en que se cobraron (puede haber mas de una, p.ej. en un plan de pago
        // dividido en dos cobros). Para Redsys el identificador es el AuthorisationCode
        // (no se usa para reembolsar, solo se reembolsa contra el Order); para Stripe el
        // identificador ES el PaymentIntentId que hace falta para reembolsar.
        //
        // Antes esto solo miraba la ULTIMA entrada, asi que un reembolso que no cupiera
        // en el cobro mas reciente fallaba aunque la inscripcion, en conjunto, tuviera
        // saldo de sobra repartido entre varios cobros. Ahora se recorren todas: si el
        // importe pedido no cabe en un cobro (porque ya se reembolso parte de el, o
        // porque el cobro es mas pequeno que lo pedido), se sigue con el siguiente hasta
        // completar el importe o agotar los cobros disponibles.
        List<(string Order, string? Identifier)> paymentEntries = (registration.PaymentAuthCodes ?? string.Empty)
            .Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(entry =>
            {
                string[] parts = entry.Split(':');
                return (Order: parts[0], Identifier: parts.Length > 1 ? parts[1] : (string?)null);
            })
            .ToList();

        if (paymentEntries.Count == 0)
        {
            return new ApiResponse<bool> { Success = false, Data = false, Errors = ["No payment is on record for this registration."], Message = "Refund failed" };
        }

        FestivalCredentials? credentials = registration.Edition?.Festival?.Credentials;

        if (credentials is null)
        {
            return new ApiResponse<bool> { Success = false, Data = false, Errors = ["Payment configuration missing for this festival."], Message = "Refund failed" };
        }

        long amountInCents = (long)Math.Round(command.Amount * 100m, MidpointRounding.AwayFromZero);

        // Nulo (inscripciones anteriores a esta funcionalidad) se trata como Redsys,
        // que era la unica plataforma que existia hasta ahora.
        PaymentPlatform platform = registration.PaymentPlatformUsed ?? PaymentPlatform.Redsys;

        bool refundSuccess;
        string refundMessage;
        string platformLabel;
        decimal actuallyRefundedNow;
        List<string> touchedPayments = [];

        if (platform == PaymentPlatform.Stripe)
        {
            platformLabel = "Stripe";

            List<(string Order, string? Identifier)> stripeEntries = paymentEntries
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Identifier))
                .ToList();

            if (stripeEntries.Count == 0)
            {
                return new ApiResponse<bool> { Success = false, Data = false, Errors = ["No Stripe payment intent is on record for this registration."], Message = "Refund failed" };
            }

            long remainingCents = amountInCents;
            List<string> failureMessages = [];

            foreach ((string order, string? identifier) in stripeEntries)
            {
                if (remainingCents <= 0)
                {
                    break;
                }

                long refundableHereCents;

                try
                {
                    refundableHereCents = await _stripeGateway.GetRefundableAmountInCentsAsync(credentials, identifier!, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not read the refundable amount for Stripe payment intent {PaymentIntentId} (registration {RegistrationId}).", identifier, registration.Id);
                    failureMessages.Add($"No se pudo consultar Stripe para {order}: {ex.Message}");
                    continue;
                }

                long amountHereCents = Math.Min(remainingCents, refundableHereCents);

                if (amountHereCents <= 0)
                {
                    continue;
                }

                StripeRefundResultDto stripeResult = await _stripeGateway.SendRefundAsync(credentials, identifier!, amountHereCents, cancellationToken);

                if (!stripeResult.Success)
                {
                    _logger.LogWarning("Stripe refund rejected for registration {RegistrationId}, payment intent {PaymentIntentId}: {Message}", registration.Id, identifier, stripeResult.Message);
                    failureMessages.Add(stripeResult.Message);
                    continue;
                }

                remainingCents -= amountHereCents;
                touchedPayments.Add($"{amountHereCents / 100m:0.00} contra {order}");
            }

            actuallyRefundedNow = touchedPayments.Count == 0
                ? 0
                : (amountInCents - remainingCents) / 100m;

            refundSuccess = actuallyRefundedNow > 0;
            refundMessage = !refundSuccess
                ? (failureMessages.Count > 0 ? string.Join(" ", failureMessages) : "No queda importe reembolsable en los pagos de Stripe registrados para esta inscripcion.")
                : remainingCents > 0
                    ? $"Reembolsados {actuallyRefundedNow:0.00} de los {command.Amount:0.00} solicitados (no quedaba mas disponible en los pagos registrados)."
                    : "Reembolso confirmado por Stripe.";
        }
        else
        {
            platformLabel = "Redsys";

            string? lastError = null;

            foreach ((string order, string? _) in paymentEntries)
            {
                RedsysRefundResultDto redsysResult = await _redsysGateway.SendRefundAsync(credentials, order, amountInCents, cancellationToken);

                if (redsysResult.Success)
                {
                    touchedPayments.Add($"{command.Amount:0.00} contra {order}");
                    lastError = null;
                    break;
                }

                lastError = redsysResult.Message;
            }

            actuallyRefundedNow = touchedPayments.Count > 0 ? command.Amount : 0;
            refundSuccess = actuallyRefundedNow > 0;
            refundMessage = refundSuccess
                ? "Reembolso confirmado por Redsys."
                : (paymentEntries.Count > 1
                    ? $"Redsys rechazo el reembolso contra los {paymentEntries.Count} pagos registrados. Ultimo motivo: {lastError}"
                    : lastError ?? "Redsys rejected the refund.");
        }

        if (!refundSuccess)
        {
            _logger.LogWarning("{Platform} refund rejected for registration {RegistrationId}: {Message}", platformLabel, registration.Id, refundMessage);

            return new ApiResponse<bool> { Success = false, Data = false, Errors = [refundMessage], Message = "Refund failed" };
        }

        registration.RefundedAmount = alreadyRefunded + actuallyRefundedNow;

        // Un reembolso NO debe reabrir el registro para volver a cobrar: es dinero
        // que se devuelve a proposito, no una deuda pendiente. Por eso ya NO se toca
        // PaymentStatus aqui -- se deja tal cual estaba antes del reembolso:
        //   - Si estaba "Paid" (pago completo, o un Split ya completado), se queda
        //     "Paid": no aparece nada pendiente ni el boton de pago.
        //   - Si estaba "PartiallyPaid" (Split con solo el primer 50% pagado), se
        //     queda "PartiallyPaid": el segundo 50% sigue pendiente de pago, se
        //     reembolse o no ese primer tramo -- es un tramo aparte.

        string note = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC] Reembolso de {actuallyRefundedNow:0.00} via {platformLabel}: {string.Join(", ", touchedPayments)}." +
            (string.IsNullOrWhiteSpace(command.Reason) ? string.Empty : $" Motivo: {command.Reason}") +
            (actuallyRefundedNow < command.Amount ? $" ATENCION: solo se pudo reembolsar {actuallyRefundedNow:0.00} de los {command.Amount:0.00} solicitados -- revisar manualmente el resto." : string.Empty);
        registration.InternalNotes = string.IsNullOrWhiteSpace(registration.InternalNotes)
            ? note
            : registration.InternalNotes + "\n" + note;

        registration.SetUpdated();
        _registrationRepository.Update(registration);
        await _registrationRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{Platform} refund confirmed for registration {RegistrationId}, amount {Amount} cents: {Details}.", platformLabel, registration.Id, (long)Math.Round(actuallyRefundedNow * 100m, MidpointRounding.AwayFromZero), string.Join(", ", touchedPayments));

        return new ApiResponse<bool> { Success = true, Data = true, Errors = [], Message = refundMessage };
    }
'@


# ============================================================================
# EJECUCION: validar TODO primero (todo o nada), luego aplicar
# ============================================================================

Write-Host ""
Write-Host "Validando $($script:Plan.Count) cambios contra los archivos locales..." -ForegroundColor Cyan

foreach ($op in $script:Plan) {
    $err = if ($op.Type -eq 'Patch') { Test-PatchOperation -Op $op } else { Test-CreateOperation -Op $op }
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
    Write-Host "Revisa esos archivos a mano, o dime que ha cambiado para regenerar el script." -ForegroundColor Yellow
    exit 1
}

Write-Host "Todo valida OK. Aplicando cambios..." -ForegroundColor Cyan
Write-Host ""

foreach ($op in $script:Plan) {
    if ($op.Type -eq 'Patch') {
        Invoke-PatchOperation -Op $op
    }
    else {
        Invoke-CreateOperation -Op $op
    }
}

Write-Host ""
Write-Host "Listo. Siguientes pasos:" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1) Revisa el diff (git diff) de los 3 archivos." -ForegroundColor White
Write-Host "  2) Compila." -ForegroundColor White
Write-Host "  3) Prueba el reembolso pendiente de Jose Camacho (o cualquier" -ForegroundColor White
Write-Host "     inscripcion con mas de un PaymentIntent guardado)." -ForegroundColor White
Write-Host ""