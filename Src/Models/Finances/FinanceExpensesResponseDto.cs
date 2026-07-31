namespace Auth.Src.Models.Finances;

public record FinanceExpensesResponseDto
{
	public decimal Value { get; set; }
	public string Currency { get; set; } = "CZK";
	public string Description { get; set; } = "None";
	public DateTime Date { get; set; }
}