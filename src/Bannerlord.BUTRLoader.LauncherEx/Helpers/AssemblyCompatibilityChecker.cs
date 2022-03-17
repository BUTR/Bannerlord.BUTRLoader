using System;
using System.Collections.Generic;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.LauncherEx.Helpers
{
    public enum CheckResult
    {
        Success,
        TypeLoadException,
        ReflectionTypeLoadException,
        GenericException
    }

    public class Proxy : MarshalByRefObject
    {
        public CheckResult CheckAssembly(string assemblyPath)
        {
            try
            {
                var asm = Assembly.Load(assemblyPath);
                var types = asm.GetTypes();
                return CheckResult.Success;
            }
            catch (TypeLoadException e)
            {
                return CheckResult.TypeLoadException;
            }
            catch (ReflectionTypeLoadException e)
            {
                return CheckResult.ReflectionTypeLoadException;
            }
            catch (Exception e)
            {
                return CheckResult.GenericException;
            }
        }
    }

    internal sealed class AssemblyCompatibilityChecker : IDisposable
    {
        private readonly AppDomain _domain;
        private readonly Proxy _proxy;
        private readonly Dictionary<string, CheckResult> _checkResult = new();

        public AssemblyCompatibilityChecker()
        {
            var current = AppDomain.CurrentDomain;
            _domain = AppDomain.CreateDomain("Compatibility Checker", current.Evidence, current.SetupInformation, current.PermissionSet);
            _proxy = (Proxy)_domain.CreateInstanceAndUnwrap(typeof(Proxy).Assembly.FullName, typeof(Proxy).FullName);
        }

        public CheckResult CheckAssembly(string assemblyPath)
        {
            if (!_checkResult.TryGetValue(assemblyPath, out var result))
            {
                result = _proxy.CheckAssembly(assemblyPath);
                _checkResult[assemblyPath] = result;
            }

            return result;
        }

        public void Dispose()
        {
            AppDomain.Unload(_domain);
        }
    }
}
