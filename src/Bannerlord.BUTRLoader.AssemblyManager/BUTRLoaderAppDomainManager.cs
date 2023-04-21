using Bannerlord.BLSE.Features.AssemblyResolver;
using Bannerlord.BLSE.Features.Commands;
using Bannerlord.BLSE.Features.ContinueSaveFile;
using Bannerlord.BLSE.Features.Interceptor;
using Bannerlord.LauncherEx;

using HarmonyLib;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Bannerlord.BUTRLoader.AssemblyManager
{
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "For ReSharper")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal sealed class BUTRLoaderAppDomainManager : AppDomainManager
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private static readonly Harmony _featureHarmony = new("bannerlord.blse.features");

        public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
        {
            base.InitializeNewDomain(appDomainInfo);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();

            // delete old files
            var files = new[]
            {
                "Bannerlord.BUTRLoader.LauncherEx.dll",
                "Bannerlord.BUTRLoader.LauncherEx.pdb",
                "Bannerlord.BUTRLoader.Shared.dll",
                "Bannerlord.BUTRLoader.Shared.pdb",
            };
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception) { /* ignore */ }
            }

            Initialize();

            AppDomain.CurrentDomain.AssemblyLoad += (_, args) =>
            {
                if (args.LoadedAssembly.GetName().Name == "TaleWorlds.MountAndBlade.Launcher.Library")
                {
                    Manager.Enable();
                }
            };
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            static string GetRecursiveException(Exception ex) => new StringBuilder()
                .AppendLine()
                .AppendLine($"Type: {ex.GetType().FullName}")
                .AppendLine(!string.IsNullOrWhiteSpace(ex.Message) ? $"Message: {ex.Message}" : string.Empty)
                .AppendLine(!string.IsNullOrWhiteSpace(ex.Source) ? $"Source: {ex.Source}" : string.Empty)
                .AppendLine(!string.IsNullOrWhiteSpace(ex.StackTrace) ? $@"CallStack:{Environment.NewLine}{string.Join(Environment.NewLine, ex.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))}" : string.Empty)
                .AppendLine(ex.InnerException is not null ? $@"{Environment.NewLine}{Environment.NewLine}Inner {GetRecursiveException(ex.InnerException)}" : string.Empty)
                .ToString();

            using var fs = File.Open("BUTRLoader_lasterror.log", FileMode.OpenOrCreate, FileAccess.Write);
            fs.SetLength(0);
            using var writer = new StreamWriter(fs);
            writer.Write($@"BUTRLoader Exception:
Version: {typeof(BUTRLoaderAppDomainManager).Assembly.GetName().Version}
{(e.ExceptionObject is Exception ex ? GetRecursiveException(ex) : e.ToString())}");
        }


        private static void Initialize()
        {
            Manager.OnDisable += () => AppDomain.CurrentDomain.UnhandledException -= CurrentDomainOnUnhandledException;
            Manager.Initialize();

            InterceptorFeature.Enable(_featureHarmony);
            AssemblyResolverFeature.Enable(_featureHarmony);
            ContinueSaveFileFeature.Enable(_featureHarmony);
            CommandsFeature.Enable(_featureHarmony);

            ModuleInitializer.Disable();
        }
    }
}