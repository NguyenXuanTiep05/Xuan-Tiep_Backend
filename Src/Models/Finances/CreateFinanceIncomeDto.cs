namespace Auth.Src.Models.Finances;


public record CreateFinanceIncomeDto
{
	public decimal Value { get; set; }
	public string Description { get; set; } = "None";
}