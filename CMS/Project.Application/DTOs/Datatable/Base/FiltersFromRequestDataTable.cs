namespace Project.Application.DTOs.Datatable.Base
{
    public class FiltersFromRequestDataTable
    {
        public int Length { get; set; }
        public int Start { get; set; }
        public string SortColumn { get; set; }
        public string SortColumnDirection { get; set; }
        public string SortColumnIndex { get; set; }
        public string Draw { get; set; }
        public string SearchValue { get; set; }
    }
}