namespace Auth.Src.Models.Finances;


public record CreateFinanceRecordDto
{
	public decimal Value { get; set; }
	public string Description { get; set; } = "None";
}