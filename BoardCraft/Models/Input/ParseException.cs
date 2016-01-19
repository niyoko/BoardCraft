﻿namespace BoardCraft.Input
{
    using System;

    public class ParseException : FormatException
    {
        public ParseException(string message) : base(message)
        {
        }

        public ParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}