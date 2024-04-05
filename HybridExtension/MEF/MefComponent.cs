using System;
using System.ComponentModel.Composition;

namespace HybridExtension.MEF
{
    public interface IMefComponent
    {
        string Id { get; }
    }

    [Export(typeof(IMefComponent))]
    internal class MefComponent : IMefComponent
    {
        public string Id { get; private set; }

        public MefComponent()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
