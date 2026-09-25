using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Application.Exceptions
{
    public class NullException : ApplicationException
    {
        public NullException(string item) : base($" به برنامه نویس اطلاع دهید : {item}")
        {
        }
        public NullException    () : base("مشکلی در سیستم رخ داده است")
        {
        }
    }
}
