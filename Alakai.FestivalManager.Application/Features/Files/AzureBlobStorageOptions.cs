namespace Alakai.FestivalManager.Infrastructure.Files;

public class AzureBlobStorageOptions
{
    /// <summary>Connection string de la Storage Account de Azure. Vacio = Azure Blob Storage
    /// deshabilitado y se sigue usando LocalFileStorageService (disco local).
    /// Se llama "ConnString" y no "ConnectionString" a proposito: Azure App Service
    /// bloquea cualquier Application Setting cuyo nombre termine en "ConnectionString".</summary>
    public string ConnString { get; set; } = string.Empty;

    /// <summary>Nombre del contenedor de blobs donde se guardan los ficheros.</summary>
    public string ContainerName { get; set; } = "uploads";
}