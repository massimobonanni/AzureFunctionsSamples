using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class GuidExtensions
    {
        public static string ToSmallString(this Guid guid)
        {
            return guid.ToString().Substring(0, 9);
        }
    }
}
