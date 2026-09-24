namespace Project.Application.Exceptions
{
    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string item) : base($" خطا : {item}")
        {
        }
        public NotFoundException() : base("اطلاعات مورد نظر پیدا نشد")
        {
        }
    }
}
