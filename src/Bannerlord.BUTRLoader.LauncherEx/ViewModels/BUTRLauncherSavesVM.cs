using Bannerlord.BUTRLoader.Helpers;
using Bannerlord.ModuleManager;

using System;
using System.Linq;

using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.BUTRLoader.ViewModels
{
    internal sealed class BUTRLauncherSavesVM : BUTRViewModel
    {
        private readonly Func<string, ModuleInfoExtended?> _getModuleById;
        private readonly Func<string, ModuleInfoExtended?> _getModuleByName;

        [BUTRDataSourceProperty]
        public string NameCategoryText { get => _nameCategoryText; set => SetField(ref _nameCategoryText, value, nameof(NameCategoryText)); }
        private string _nameCategoryText = "Name";

        [BUTRDataSourceProperty]
        public string VersionCategoryText { get => _versionCategoryText; set => SetField(ref _versionCategoryText, value, nameof(VersionCategoryText)); }
        private string _versionCategoryText = "Version";

        [BUTRDataSourceProperty]
        public string CharacterNameCategoryText { get => _characterNameCategoryText; set => SetField(ref _characterNameCategoryText, value, nameof(CharacterNameCategoryText)); }
        private string _characterNameCategoryText = "Character";

        [BUTRDataSourceProperty]
        public string LevelCategoryText { get => _levelCategoryText; set => SetField(ref _levelCategoryText, value, nameof(LevelCategoryText)); }
        private string _levelCategoryText = "Level";

        [BUTRDataSourceProperty]
        public string DaysCategoryText { get => _daysCategoryText; set => SetField(ref _daysCategoryText, value, nameof(DaysCategoryText)); }
        private string _daysCategoryText = "Days";

        [BUTRDataSourceProperty]
        public string CreatedAtCategoryText { get => _createdAtCategoryText; set => SetField(ref _createdAtCategoryText, value, nameof(CreatedAtCategoryText)); }
        private string _createdAtCategoryText = "CreatedAt";

        [BUTRDataSourceProperty]
        public MBBindingList<BUTRLauncherSaveVM> Saves { get; } = new();

        [BUTRDataSourceProperty]
        public bool IsDisabled { get => _isDisabled; set => SetField(ref _isDisabled, value, nameof(IsDisabled)); }
        private bool _isDisabled;

        public BUTRLauncherSaveVM? Selected => Saves.FirstOrDefault(x => x.IsSelected);

        public BUTRLauncherSavesVM(Func<string, ModuleInfoExtended?> getModuleById, Func<string, ModuleInfoExtended?> getModuleByName)
        {
            _getModuleById = getModuleById;
            _getModuleByName = getModuleByName;

            ExecuteRefresh();
        }

        private void SelectSave(BUTRLauncherSaveVM saveVM)
        {
            foreach (var save in Saves)
            {
                save.IsSelected = false;
            }
            saveVM.IsSelected = true;
        }

        [BUTRDataSourceMethod]
        public void ExecuteRefresh()
        {
            Saves.Clear();
            foreach (var saveFile in MBSaveLoad.GetSaveFiles())
            {
                if (saveFile.MetaData is null) continue;
                Saves.Add(new BUTRLauncherSaveVM(saveFile, SelectSave, _getModuleById, _getModuleByName));
            }
        }
    }
}