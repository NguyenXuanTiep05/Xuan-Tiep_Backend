using Auth.Src.Models.Finances;

namespace Auth.Src.Models.Finances;


public record FinanceOverviewResponseDto
{
	public required List<FinanceRecordResponseDto>? Income { get; set; }
	public required List<FinanceRecordResponseDto>? Expenses { get; set; }
	public required FinanceSummaryResponseDto Summary { get; set; }
}