namespace Project.Application.Exceptions
{
    public class BadRequestException : ApplicationException
    {
        public BadRequestException(string message) : base(message)
        {
        }
        public BadRequestException() : base("خطا در ثبت اطلاعات")
        {
        }
    }
}
