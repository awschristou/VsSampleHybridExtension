using HybridExtension.MEF;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.Extensibility.VSSdkCompatibility;
using System.Threading;
using System.Threading.Tasks;

namespace HybridExtension
{
    [VisualStudioContribution]
    internal class VsExtMefReferenceCommand : Command
    {
        private readonly MefInjection<IMefComponent> _mefComponent;

        public override CommandConfiguration CommandConfiguration => new("%HybridExtension.VsExtHelloMefReferenceCommand.DisplayName%")
        {
            Icon = new(ImageMoniker.KnownValues.Component, IconSettings.IconAndText),
            Placements = new[] { CommandPlacement.KnownPlacements.ExtensionsMenu },
        };

        public VsExtMefReferenceCommand(MefInjection<IMefComponent> mefComponent)
        {
            _mefComponent = mefComponent;
        }

        public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
        {
            var mefComponent = await _mefComponent.GetServiceAsync();
            await Extensibility.Shell().ShowPromptAsync(
                $"VS.Ext has MEF Component with ID: {mefComponent.Id}",
                PromptOptions.OK, cancellationToken);
        }
    }
}
