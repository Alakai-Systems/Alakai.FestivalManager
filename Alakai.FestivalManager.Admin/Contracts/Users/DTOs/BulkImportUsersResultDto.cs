namespace Alakai.FestivalManager.Admin.Contracts.Users.DTOs;

public class BulkImportUsersResultDto
{
    public int TotalRows { get; set; }
    public int Created { get; set; }
    public List<string> SkippedDuplicateEmails { get; set; } = [];
    public List<BulkImportRowErrorDto> Errors { get; set; } = [];
}

public class BulkImportRowErrorDto
{
    public int Row { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}