namespace XuanTiepApi.Src.Models.Finances;


public class DeleteFinanceRecordDto
{
	public Guid Id { get; set; }
	public required string Type { get; set; }
}
