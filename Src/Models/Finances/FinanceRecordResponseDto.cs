namespace Auth.Src.Models.Finances;

public record FinanceRecordResponseDto
{
	public int RecordId { get; set; }
	public decimal Value { get; set; }
	public string Currency { get; set; } = "CZK";
	public string Description { get; set; } = "None";
	public DateTime Date { get; set; }
	public required string Type { get; set; }
}