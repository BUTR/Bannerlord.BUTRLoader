using Bannerlord.BUTRLoader.Patches.Mixins;

using System.Collections.Generic;
using System.Linq;

using TaleWorlds.Library;

// ReSharper disable once CheckNamespace
namespace Bannerlord.BUTRLoader.Helpers
{
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

        public static void AddMixins(ViewModel launcherVM)
        {
            var wrapper = LauncherVMWrapper.Create(launcherVM);

            AddMixin(wrapper.Object!, new LauncherVMMixin(launcherVM));
            AddMixin(wrapper.ModsData.Object!, new LauncherModsVMMixin(wrapper.ModsData.Object!));
            foreach (var launcherModuleVM in wrapper.ModsData.Modules)
            {
                AddMixin(launcherModuleVM.Object, new LauncherModuleVMMixin(launcherModuleVM.Object));
            }

            if (wrapper.ModsData.Modules is IMBBindingList list)
            {
                list.ListChanged += MixinManager_ListChanged;
            }
        }

        private static void MixinManager_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (sender is not MBBindingList<ViewModel> list)
                return;

            if (e.ListChangedType == ListChangedType.Reset)
            {
                var keys = Mixins.Keys.Where(x => x.GetType() == LauncherModuleVMWrapper.LauncherModuleVMType).ToArray();
                foreach (var viewModel in keys)
                {
                    Mixins.Remove(viewModel);
                }
            }

            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                var entry = list[e.NewIndex];

                AddMixin(entry, new LauncherModuleVMMixin(entry));
            }
        }
    }
}