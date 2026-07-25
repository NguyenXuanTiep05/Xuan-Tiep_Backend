namespace Auth.Src.Models.Finances;


public record CreateFinanceExpenseDto
{
	public decimal Value { get; set; }
	public string Currency { get; set; } = "CZK";
	public string Description { get; set; } = "None";
	public int UserId { get; set; }
}