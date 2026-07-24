namespace Alakai.FestivalManager.Admin.Contracts.ProductionReservations.DTOs;

public class ReservationDto
{
    public Guid Id { get; set; }
    public Guid ProductionAccommodationBuildingId { get; set; }
    public int? RoomType { get; set; }
    public string? BuildingName { get; set; }
    public Guid? ResponsibleProductionPersonId { get; set; }
    public string? ResponsibleName { get; set; }
    public List<ReservationOccupantDto> Occupants { get; set; } = new List<ReservationOccupantDto>();
}