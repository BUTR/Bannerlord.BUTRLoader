﻿using System.Reflection;

namespace Bannerlord.BUTRLoader.Utils
{
    public class AssemblyWrapper : Assembly
    {
        public override string Location { get; }

        public AssemblyWrapper(string location) => Location = location;
    }
}