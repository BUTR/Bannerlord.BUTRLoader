using Bannerlord.BUTR.Shared.Helpers;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.Features.Interceptor.Patches
{
    internal class AssemblyWrapper : Assembly
    {
        public override string Location { get; }

        public AssemblyWrapper(string location) => Location = location;
    }

    internal class TypeWrapper : Type
    {
        public override string Name { get; } = default!;
        public override Guid GUID { get; } = default!;
        public override Module Module { get; } = default!;
        public override Assembly Assembly { get; } = default!;
        public override string FullName { get; } = default!;
        public override string Namespace { get; } = default!;
        public override string AssemblyQualifiedName { get; } = default!;
        public override Type BaseType { get; } = default!;
        public override Type? UnderlyingSystemType { get; } = default!;

        public TypeWrapper(string location) => Assembly = new AssemblyWrapper(location);

        public override object[] GetCustomAttributes(bool inherit) => new object[] { };
        public override bool IsDefined(Type attributeType, bool inherit) => false;
        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) => new ConstructorInfo[] { };
        public override Type? GetInterface(string name, bool ignoreCase) => null;
        public override Type[] GetInterfaces() => new Type[] { };
        public override EventInfo? GetEvent(string name, BindingFlags bindingAttr) => null;
        public override EventInfo[] GetEvents(BindingFlags bindingAttr) => new EventInfo[] { };
        public override Type[] GetNestedTypes(BindingFlags bindingAttr) => new Type[] { };
        public override Type? GetNestedType(string name, BindingFlags bindingAttr) => null;
        public override Type? GetElementType() => null;
        protected override bool HasElementTypeImpl() => false;
        protected override PropertyInfo? GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers) => null;
        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) => new PropertyInfo[] { };
        protected override MethodInfo? GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) => null;
        public override MethodInfo[] GetMethods(BindingFlags bindingAttr) => new MethodInfo[] { };
        public override FieldInfo? GetField(string name, BindingFlags bindingAttr) => null;
        public override FieldInfo[] GetFields(BindingFlags bindingAttr) => new FieldInfo[] { };
        public override MemberInfo[] GetMembers(BindingFlags bindingAttr) => new MemberInfo[] { };
        protected override TypeAttributes GetAttributeFlagsImpl() => TypeAttributes.NotPublic;
        protected override bool IsArrayImpl() => false;
        protected override bool IsByRefImpl() => false;
        protected override bool IsPointerImpl() => false;
        protected override bool IsPrimitiveImpl() => false;
        protected override bool IsCOMObjectImpl() => false;
        public override object? InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters) => null;
        protected override ConstructorInfo? GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) => null;


        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => new object[] { };
    }

    internal static class AssemblyLoaderPatch
    {
        public static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.Method(typeof(AssemblyLoader), "LoadFrom"),
                prefix: AccessTools2.Method(typeof(AssemblyLoaderPatch), nameof(LoadFromPrefix)));
            if (!res1) return false;

            var res2 = harmony.TryPatch(
                AccessTools2.Method(typeof(AssemblyLoader), "OnAssemblyResolve"),
                prefix: AccessTools2.Method(typeof(AssemblyLoaderPatch), nameof(OnAssemblyResolvePrefix)));
            if (!res2) return false;

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool LoadFromPrefix(ref Assembly? __result, string assemblyFile)
        {
            if (assemblyFile.Contains("Modules"))
            {
                var module = ModuleInfoHelper.GetModuleByType(new TypeWrapper(Path.GetFullPath(assemblyFile)));
                if (module is not null)
                {
                    var filename = Path.GetFileName(assemblyFile);
                    var subModule = module.SubModules.FirstOrDefault(sm => sm.DLLName == filename);
                    if (subModule is not null)
                    {
                        if (subModule.Tags.TryGetValue("LoadReferencesOnLoad", out var list) && string.Equals(list.FirstOrDefault(), "false", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                __result = Assembly.LoadFrom(assemblyFile);
                            }
                            catch (Exception)
                            {
                                __result = null;
                            }
                            return false;
                        }
                    }

                }
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool OnAssemblyResolvePrefix(ref Assembly? __result, object sender, ResolveEventArgs args)
        {
            if (sender is AppDomain { FriendlyName: "Compatibility Checker" } domain)
            {
                foreach (var assembly in domain.GetAssemblies())
                {
                    if (assembly.FullName == args.Name)
                    {
                        __result = assembly;
                        return false;
                    }
                }

                var name = args.Name.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                __result = Assembly.LoadFrom(name[0]);
                return false;
            }

            return true;
        }
    }
}