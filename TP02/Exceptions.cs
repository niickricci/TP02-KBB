using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionTP01
{
    internal class Exceptions
    {
        public class PositionIllégaleException : Exception
        {
            public PositionIllégaleException() : base()
            {
            }

            public PositionIllégaleException(string message) : base(message)
            {
            }

            public PositionIllégaleException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }
    }
}
