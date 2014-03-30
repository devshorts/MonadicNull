using System;

namespace Devshorts.MonadicNull
{
    public class NoValueException : Exception
    {
        public NoValueException(string message, params object[] properties) : base(String.Format(message, properties))
        {
            
        }
    }
}