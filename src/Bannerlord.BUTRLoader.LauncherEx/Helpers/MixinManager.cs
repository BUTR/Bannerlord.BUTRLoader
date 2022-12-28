﻿using Bannerlord.BUTR.Shared.Utils;
using Bannerlord.BUTRLoader.Extensions;
using Bannerlord.BUTRLoader.Mixins;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.BUTRLoader.Helpers
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal class BUTRDataSourcePropertyAttribute : Attribute
    {
        public string? OverrideName { get; set; }
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal class BUTRDataSourceMethodAttribute : Attribute
    {
        public string? OverrideName { get; set; }
    }

    internal abstract class BUTRViewModel : ViewModel
    {
        protected BUTRViewModel()
        {
            var properties = GetType().GetProperties(AccessTools.all);
            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.GetCustomAttribute<BUTRDataSourcePropertyAttribute>() is { } attribute)
                {
                    if (propertyInfo.GetMethod?.IsPrivate == true || propertyInfo.SetMethod?.IsPrivate == true) throw new Exception();

                    this.AddProperty(attribute.OverrideName ?? propertyInfo.Name, propertyInfo);
                }
            }

            var methods = GetType().GetMethods(AccessTools.all);
            foreach (var methodInfo in methods)
            {
                if (methodInfo.GetCustomAttribute<BUTRDataSourceMethodAttribute>() is { } attribute)
                {
                    if (methodInfo.IsPrivate) throw new Exception();

                    this.AddMethod(attribute.OverrideName ?? methodInfo.Name, methodInfo);
                }
            }
        }
    }

    internal abstract class ViewModelMixin<TViewModelMixin, TViewModel>
        where TViewModelMixin : ViewModelMixin<TViewModelMixin, TViewModel>
        where TViewModel : ViewModel
    {
        private readonly WeakReference<TViewModel> _vm;

        protected TViewModel? ViewModel => _vm.TryGetTarget(out var vm) ? vm : null;

        public TViewModelMixin Mixin => (TViewModelMixin) this;

        protected ViewModelMixin(TViewModel vm)
        {
            _vm = new WeakReference<TViewModel>(vm);

            SetVMProperty(nameof(Mixin), GetType().Name);
            foreach (var propertyInfo in GetType().GetProperties(AccessTools.all))
            {
                if (propertyInfo.GetCustomAttribute<BUTRDataSourcePropertyAttribute>() is { } attribute)
                {
                    if (propertyInfo.GetMethod?.IsPrivate == true || propertyInfo.SetMethod?.IsPrivate == true) throw new Exception();

                    var wrappedPropertyInfo = new WrappedPropertyInfo(propertyInfo, this);
                    vm.AddProperty(attribute.OverrideName ?? propertyInfo.Name, wrappedPropertyInfo);
                    wrappedPropertyInfo.PropertyChanged += (_, e) => ViewModel?.OnPropertyChanged(e.PropertyName);
                }
            }
            foreach (var methodInfo in GetType().GetMethods(AccessTools.all))
            {
                if (methodInfo.GetCustomAttribute<BUTRDataSourceMethodAttribute>() is { } attribute)
                {
                    if (methodInfo.IsPrivate) throw new Exception();

                    var wrappedMethodInfo = new WrappedMethodInfo(methodInfo, this);
                    vm.AddMethod(attribute.OverrideName ?? methodInfo.Name, wrappedMethodInfo);
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            ViewModel?.OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChangedWithValue(object? value, [CallerMemberName] string? propertyName = null)
        {
            ViewModel?.OnPropertyChangedWithValue(value, propertyName);
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            field = value;
            OnPropertyChangedWithValue(value, propertyName);
            return true;
        }

        protected void SetVMProperty(string property, string? overrideName = null)
        {
            var propertyInfo = new WrappedPropertyInfo(AccessTools2.Property(GetType(), property)!, this);
            ViewModel?.AddProperty(overrideName ?? property, propertyInfo);
            propertyInfo.PropertyChanged += (_, e) => ViewModel?.OnPropertyChanged(e.PropertyName);
        }

        protected void SetVMMethod(string method, string? overrideName = null)
        {
            var methodInfo = new WrappedMethodInfo(AccessTools2.Method(GetType(), method)!, this);
            ViewModel?.AddMethod(overrideName ?? method, methodInfo);
        }
    }

    internal static class MixinManager
    {
        public static readonly Dictionary<ViewModel, List<object>> Mixins = new();

        private static void AddMixin(ViewModel viewModel, object mixin)
        {
            if (Mixins.TryGetValue(viewModel, out var list))
            {
                list.Add(mixin);
            }
            else
            {
                Mixins.Add(viewModel, new List<object> { mixin });
            }
        }

        public static void AddMixins(LauncherVM launcherVM)
        {
            AddMixin(launcherVM.News, new LauncherNewsVMMixin(launcherVM.News));
            AddMixin(launcherVM.ModsData, new LauncherModsVMMixin(launcherVM.ModsData));
            AddMixin(launcherVM, new LauncherVMMixin(launcherVM));
        }
    }
}