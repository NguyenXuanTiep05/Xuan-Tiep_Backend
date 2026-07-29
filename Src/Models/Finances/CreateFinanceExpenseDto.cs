namespace Auth.Src.Models.Finances;


public record CreateFinanceExpenseDto
{
	public decimal Value { get; set; }
	public string Description { get; set; } = "None";
}