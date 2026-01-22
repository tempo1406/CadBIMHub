namespace CadBIMHub.Models
{
    public class ExcelStructureInfoModel
    {
        public string SheetName { get; set; }
        public int HeaderRow { get; set; }
        public int StartRow { get; set; }
        public int EndRow { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
}
