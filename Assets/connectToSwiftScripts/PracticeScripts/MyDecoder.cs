using System;
using System.Collections.Generic;

namespace PythonConnection
{
    public class MyDecoder : DataDecoder
    {
        protected override Dictionary<string, Type> DataToType()
        {
            return new Dictionary<string, Type>() { { "subjugation", typeof(SubjugationData) }, };
        }
    }
}
