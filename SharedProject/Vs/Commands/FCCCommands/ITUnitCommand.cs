using FineCodeCoverage.Vs.Commands.CommandInitializer;

namespace FineCodeCoverage.Vs.Commands.FCCCommands
{
    internal interface ITUnitCommand : ICommandInitializer
    {
        void SetVisible(bool isVisible);
    }
}
