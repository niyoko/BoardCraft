using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardCraft.Input
{
    public class InvalidNameException : FormatException
    {
        public InvalidNameException(string message): base(message) { }
    }
}
