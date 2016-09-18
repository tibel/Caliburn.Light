using System.Collections.Generic;

namespace Caliburn.Light
{
    internal sealed class ViewTypeLookupKeyComparer : IEqualityComparer<ViewTypeLookupKey>
    {
        public bool Equals(ViewTypeLookupKey x, ViewTypeLookupKey y)
        {
            return x.ModelType.Equals(y.ModelType) && x.Context.Equals(y.Context);
        }

        public int GetHashCode(ViewTypeLookupKey obj)
        {
            var h1 = obj.ModelType.GetHashCode();
            var h2 = obj.Context.GetHashCode();
            return (((h1 << 5) + h1) ^ h2);
        }
    }
}
