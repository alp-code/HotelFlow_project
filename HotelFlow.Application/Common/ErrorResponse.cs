namespace HotelFlow.Application.Common;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = default!;
    public string? Details { get; set; }
}
