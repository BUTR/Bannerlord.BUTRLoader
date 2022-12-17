using Bannerlord.BUTRLoader.Patches.Mixins;

using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Launcher.Library;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal abstract class ViewModelMixin<TViewModel> where TViewModel : ViewModel
    {
        private readonly WeakReference<TViewModel> _vm;

        protected TViewModel? ViewModel => _vm.TryGetTarget(out var vm) ? vm : null;

        protected ViewModelMixin(TViewModel vm)
        {
            _vm = new WeakReference<TViewModel>(vm);
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            ViewModel?.OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChangedWithValue(object value, [CallerMemberName] string? propertyName = null)
        {
            ViewModel?.OnPropertyChangedWithValue(value, propertyName);
        }

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    internal static class MixinManager
    {
        public static readonly Dictionary<ViewModel, List<object>> Mixins = new();

        private static readonly Type? LauncherModuleVMType = AccessTools2.TypeByName("TaleWorlds.MountAndBlade.Launcher.Library.LauncherModuleVM");

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
            AddMixin(launcherVM, new LauncherVMMixin(launcherVM));
            AddMixin(launcherVM.News, new LauncherNewsVMMixin(launcherVM.News));
            AddMixin(launcherVM.ModsData, new LauncherModsVMMixin(launcherVM.ModsData));
            foreach (var launcherModuleVM in launcherVM.ModsData.Modules)
            {
                AddMixin(launcherModuleVM, new LauncherModuleVMMixin(launcherModuleVM));
            }

            launcherVM.ModsData.Modules.ListChanged += MixinManager_ListChanged;
        }

        private static void MixinManager_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (sender is not IList<LauncherModuleVM> list)
                return;

            if (e.ListChangedType == ListChangedType.Reset)
            {
                var keys = Mixins.Keys.Where(x => x.GetType() == LauncherModuleVMType).ToArray();
                foreach (var viewModel in keys)
                {
                    Mixins.Remove(viewModel);
                }
            }

            if (e.ListChangedType == ListChangedType.ItemAdded && list[e.NewIndex] is var entry)
            {
                AddMixin(entry, new LauncherModuleVMMixin(entry));
            }
        }
    }
}