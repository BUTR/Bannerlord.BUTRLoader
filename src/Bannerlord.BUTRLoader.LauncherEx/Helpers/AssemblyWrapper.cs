using System.Reflection;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal class AssemblyWrapper : Assembly
    {
        public override string Location { get; }

        public AssemblyWrapper(string location) => Location = location;
    }
}