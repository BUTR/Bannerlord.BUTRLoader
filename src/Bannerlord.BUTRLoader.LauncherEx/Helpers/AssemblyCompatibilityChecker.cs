using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bannerlord.BUTRLoader.Helpers
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
        private AppDomain? _domain;
        private Proxy? _proxy;

        private readonly Dictionary<string, CheckResult> _checkResult = new();

        public CheckResult CheckAssembly(string assemblyPath)
        {
            if (LauncherSettings.DisableBinaryCheck)
                return CheckResult.Success;

            _domain ??= AppDomain.CreateDomain("Compatibility Checker", AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation, AppDomain.CurrentDomain.PermissionSet);
            _proxy ??= (Proxy) _domain.CreateInstanceAndUnwrap(typeof(Proxy).Assembly.FullName, typeof(Proxy).FullName);

            if (!_checkResult.TryGetValue(assemblyPath, out var result))
            {
                result = _proxy.CheckAssembly(assemblyPath);
                _checkResult[assemblyPath] = result;
            }

            return result;
        }

        public void Dispose()
        {
            if (_domain is not null)
                AppDomain.Unload(_domain);
        }
    }
}