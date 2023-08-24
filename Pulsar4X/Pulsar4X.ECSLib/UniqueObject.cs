using System;

namespace Pulsar4X.ECSLib
{
    public record UniqueObject(StringIdentifier DescriptiveId, Guid InstanceId)
    {
        public UniqueObject(StringIdentifier descriptiveId) : this(descriptiveId, Guid.NewGuid())
        {
            if (descriptiveId == null)
            {
                throw new ArgumentNullException(nameof(descriptiveId));
            }
        }

        public override string ToString() => $"{DescriptiveId} - {InstanceId}";
    }
}