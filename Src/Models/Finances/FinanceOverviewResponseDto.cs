using Auth.Src.Models.Finances;

namespace Auth.Src.Models.Finances;


public record FinanceOverviewResponseDto
{
	public required List<FinanceIncomeResponseDto>? Income { get; set; }
	public required List<FinanceExpensesResponseDto>? Expenses { get; set; }
	public required FinanceSummaryResponseDto Summary { get; set; }
}