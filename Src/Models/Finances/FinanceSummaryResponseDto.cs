namespace XuanTiepApi.Src.Models.Finances;

public record FinanceSummaryResponseDto
{
    public int UserId { get; set; }
    public string Currency { get; set; } = "";
    public double TotalIncome { get; set; }
    public double TotalExpenses { get; set; }
    public double Net { get; set; }
}
