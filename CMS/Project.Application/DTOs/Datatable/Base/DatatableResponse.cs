namespace Project.Application.DTOs.Datatable.Base
{
    public class DatatableResponse<T>
    {
        public object Data { get; set; }
        public int RecordsFiltered { get; set; }
        public int RecordsTotal { get; set; }
        public string SEcho { get; set; }
    }
}