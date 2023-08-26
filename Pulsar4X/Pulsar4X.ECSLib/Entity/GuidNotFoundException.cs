using System;

namespace Pulsar4X.ECSLib
{
    [PublicAPI]
    public class GuidNotFoundException : Exception
    {
        [PublicAPI]
        public StringIdentifier MissingGuid { get; private set; }

        [PublicAPI]
        public GuidNotFoundException(StringIdentifier missingGuid)
        {
            MissingGuid = missingGuid;
        }
    }
}