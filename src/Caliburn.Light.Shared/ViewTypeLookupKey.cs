using System;
using System.Diagnostics;

namespace Caliburn.Light
{
    [DebuggerDisplay("ModelType = {ModelType.Name} Context = {Context}")]
    internal readonly struct ViewTypeLookupKey
    {
        public ViewTypeLookupKey(Type modelType, string context)
        {
            ModelType = modelType;
            Context = context;
        }

        public Type ModelType { get; }

        public string Context { get; }
    }
}
