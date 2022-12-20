﻿using Bannerlord.BUTR.Shared.Helpers;
using Bannerlord.ModuleManager;

using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.BUTRLoader.Tests
{
    internal sealed record ModuleInfoExtendedWithSelected : ModuleInfoExtended
    {
        public bool IsSelected { get; init; }

        public ModuleInfoExtendedWithSelected() { }
        public ModuleInfoExtendedWithSelected(ModuleInfoExtended module, bool isSelected) : base(module)
        {
            IsSelected = isSelected;
        }
    }

    internal static class ModuleTemplates
    {
        public static ModuleInfoExtendedWithSelected NativeModuleBase => new()
        {
            Id = "NativeModule",
            Name = "Native Module Module",
            Version = ApplicationVersion.Empty,
            IsSelected = true,
            IsOfficial = true,
            IsSingleplayerModule = true,
            IsMultiplayerModule = false,
        };
        public static ModuleInfoExtendedWithSelected Native2ModuleBase => new()
        {
            Id = "Native2Module",
            Name = "Native2 Module Module",
            Version = ApplicationVersion.Empty,
            IsSelected = true,
            IsOfficial = true,
            IsSingleplayerModule = true,
            IsMultiplayerModule = false,
            DependentModuleMetadatas = new List<DependentModuleMetadata>
            {
                new()
                {
                    Id = "NativeModule",
                    IsOptional = false,
                    LoadType = LoadType.LoadBeforeThis,
                    Version = ApplicationVersion.Empty
                }
            }
        };

        public static ModuleInfoExtendedWithSelected CommunityFrameworkModuleBase => new()
        {
            Id = "CommunityFrameworkModule",
            Name = "Community Framework Module",
            Version = ApplicationVersion.Empty,
            IsSelected = true,
            IsOfficial = false,
            IsSingleplayerModule = true,
            IsMultiplayerModule = false,
            DependentModuleMetadatas = new List<DependentModuleMetadata>
            {
                new()
                {
                    Id = "NativeModule",
                    IsOptional = false,
                    LoadType = LoadType.LoadAfterThis,
                    Version = ApplicationVersion.Empty
                }
            }
        };

        public static ModuleInfoExtendedWithSelected CustomModuleBase => new()
        {
            Id = "CustomModule",
            Name = "Custom Module",
            Version = ApplicationVersion.Empty,
            IsSelected = true,
            IsOfficial = false,
            IsSingleplayerModule = true,
            IsMultiplayerModule = false,
        };

        public static ModuleInfoExtendedWithSelected Custom2ModuleBase => new()
        {
            Id = "Custom2Module",
            Name = "Custom2 Module",
            Version = ApplicationVersion.Empty,
            IsSelected = true,
            IsOfficial = false,
            IsSingleplayerModule = true,
            IsMultiplayerModule = false,
        };


        public static ModuleInfoExtendedWithSelected CustomModuleWithNativeDM => CustomModuleBase with
        {
            DependentModuleMetadatas = new List<DependentModuleMetadata>
            {
                new()
                {
                    Id = "NativeModule",
                    IsOptional = false,
                    LoadType = LoadType.LoadBeforeThis,
                    Version = ApplicationVersion.Empty
                }
            }
        };
        public static ModuleInfoExtendedWithSelected CustomModuleWithNativeOptional => CustomModuleBase with
        {
            DependentModuleMetadatas = new List<DependentModuleMetadata>
            {
                new()
                {
                    Id = "NativeModule",
                    IsOptional = true,
                    LoadType = LoadType.LoadBeforeThis,
                    Version = ApplicationVersion.Empty
                }
            }
        };

        public static ModuleInfoExtendedWithSelected CustomModule2WithCustomModuleDM => Custom2ModuleBase with
        {
            DependentModuleMetadatas = new List<DependentModuleMetadata>
            {
                new()
                {
                    Id = "CustomModule",
                    IsOptional = false,
                    LoadType = LoadType.LoadBeforeThis,
                    Version = ApplicationVersion.Empty
                }
            }
        };
        public static ModuleInfoExtendedWithSelected CustomModule2WithCustomModuleDMVersion => Custom2ModuleBase with
        {
            DependentModuleMetadatas = new List<DependentModuleMetadata>
            {
                new()
                {
                    Id = "CustomModule",
                    IsOptional = false,
                    LoadType = LoadType.LoadBeforeThis,
                    Version = new ApplicationVersion(ApplicationVersionType.EarlyAccess, 1, 0, 0, 0)
                }
            }
        };
        public static ModuleInfoExtendedWithSelected CustomModule2WithCustomModuleOptional => Custom2ModuleBase with
        {
            DependentModuleMetadatas = new List<DependentModuleMetadata>
            {
                new()
                {
                    Id = "CustomModule",
                    IsOptional = true,
                    LoadType = LoadType.LoadBeforeThis,
                    Version = ApplicationVersion.Empty
                }
            }
        };
        public static ModuleInfoExtendedWithSelected CustomModule2WithCustomModuleIncompatible => Custom2ModuleBase with
        {
            DependentModuleMetadatas = new List<DependentModuleMetadata>
            {
                new()
                {
                    Id = "CustomModule",
                    IsIncompatible = true
                }
            }
        };

        public static ModuleInfoExtendedWithSelected CommunityFrameworkModuleBeforeNative => CommunityFrameworkModuleBase with
        {
            DependentModuleMetadatas = new List<DependentModuleMetadata>
            {
                new()
                {
                    Id = "NativeModule",
                    IsOptional = false,
                    LoadType = LoadType.LoadAfterThis,
                    Version = ApplicationVersion.Empty
                }
            }
        };
    }

    public enum ModuleListTemplates
    {
        /// <summary>
        /// One module depends on another module using the BUTR's approach
        /// </summary>
        SimpleBUTR,
        /// <summary>
        /// One module depends on another module using the BUTR's approach
        /// Two times
        /// </summary>
        SimpleBUTR2,

        /// <summary>
        /// One module depends on another module as an optional
        /// </summary>
        Optional,
        /// <summary>
        /// One module depends on another module as an optional
        /// Two times
        /// </summary>
        Optional2,
        /// <summary>
        /// One module depends on another module as an optional, but that module doesn't exist
        /// </summary>
        OptionalNonExisting,

        /// <summary>
        /// One module declares another module as incompatible
        /// </summary>
        Incompatible0,
        /// <summary>
        /// One module declares another module as incompatible
        /// </summary>
        Incompatible1,

        /// <summary>
        /// Framework - load before Native
        /// Native    - nothing
        /// Native2   - after Native
        /// Custom    - after Native
        /// </summary>
        Complex1,
        /// <summary>
        /// Framework - load before Native
        /// Native    - nothing
        /// Custom    - after Native
        /// Custom2   - incompatible with Custom
        /// </summary>
        Complex2,

        /// <summary>
        /// Native    - nothing
        /// Custom2   - load after with Custom
        /// </summary>
        MissingModule1,
        /// <summary>
        /// Native    - nothing
        /// Custom2   - load after with Custom with version check
        /// </summary>
        MissingModule2,
    }

    internal class ModuleStorage
    {
        public readonly ModuleListTemplates ModuleListTemplate;

        public ModuleStorage(ModuleListTemplates moduleListTemplate)
        {
            ModuleListTemplate = moduleListTemplate;
        }

        private (List<ModuleInfoExtendedWithSelected> ModuleInfoModels, List<string> ExpectedIdOrder) Tuple => ModuleListTemplate switch
        {
            ModuleListTemplates.SimpleBUTR => (
                new()
                {
                    ModuleTemplates.CustomModuleWithNativeDM,
                    ModuleTemplates.NativeModuleBase,
                },
                new()
                {
                    ModuleTemplates.NativeModuleBase.Id,
                    ModuleTemplates.CustomModuleWithNativeDM.Id,
                }),
            ModuleListTemplates.SimpleBUTR2 => (
                new()
                {
                    ModuleTemplates.CustomModule2WithCustomModuleDM,
                    ModuleTemplates.CustomModuleWithNativeDM,
                    ModuleTemplates.NativeModuleBase,
                },
                new()
                {
                    ModuleTemplates.NativeModuleBase.Id,
                    ModuleTemplates.CustomModuleWithNativeDM.Id,
                    ModuleTemplates.CustomModule2WithCustomModuleDM.Id,
                }),

            ModuleListTemplates.Optional => (
                new()
                {
                    ModuleTemplates.CustomModule2WithCustomModuleOptional,
                    ModuleTemplates.NativeModuleBase,
                    ModuleTemplates.CustomModuleWithNativeDM,
                },
                new()
                {
                    ModuleTemplates.NativeModuleBase.Id,
                    ModuleTemplates.CustomModuleWithNativeDM.Id,
                    ModuleTemplates.CustomModule2WithCustomModuleOptional.Id,
                }),
            ModuleListTemplates.Optional2 => (
                new()
                {
                    ModuleTemplates.CustomModule2WithCustomModuleOptional,
                    ModuleTemplates.NativeModuleBase,
                    ModuleTemplates.CustomModuleWithNativeOptional,
                },
                new()
                {
                    ModuleTemplates.NativeModuleBase.Id,
                    ModuleTemplates.CustomModuleWithNativeOptional.Id,
                    ModuleTemplates.CustomModule2WithCustomModuleOptional.Id,
                }),
            ModuleListTemplates.OptionalNonExisting => (
                new()
                {
                    ModuleTemplates.Custom2ModuleBase,
                    ModuleTemplates.NativeModuleBase,
                },
                new()
                {
                    ModuleTemplates.NativeModuleBase.Id,
                    ModuleTemplates.Custom2ModuleBase.Id,
                }),
            ModuleListTemplates.Incompatible0 => (
                new()
                {
                    ModuleTemplates.CustomModuleWithNativeDM,
                    ModuleTemplates.NativeModuleBase,
                    ModuleTemplates.CustomModule2WithCustomModuleIncompatible,
                },
                new()
                {
                    ModuleTemplates.NativeModuleBase.Id,
                    ModuleTemplates.CustomModuleWithNativeDM.Id,
                }),
            ModuleListTemplates.Incompatible1 => (
                new()
                {
                    ModuleTemplates.CustomModule2WithCustomModuleIncompatible,
                    ModuleTemplates.NativeModuleBase,
                    ModuleTemplates.CustomModuleWithNativeDM with { IsSelected = false },
                },
                new()
                {
                    ModuleTemplates.NativeModuleBase.Id,
                    ModuleTemplates.CustomModule2WithCustomModuleIncompatible.Id,
                }),

            ModuleListTemplates.Complex1 => (
                new()
                {
                    ModuleTemplates.CustomModuleWithNativeDM,
                    ModuleTemplates.NativeModuleBase,
                    ModuleTemplates.Native2ModuleBase,
                    ModuleTemplates.CommunityFrameworkModuleBeforeNative,
                },
                new()
                {
                    ModuleTemplates.CommunityFrameworkModuleBeforeNative.Id,
                    ModuleTemplates.NativeModuleBase.Id,
                    ModuleTemplates.Native2ModuleBase.Id,
                    ModuleTemplates.CustomModuleWithNativeDM.Id,
                }),
            ModuleListTemplates.Complex2 => (
                new()
                {
                    ModuleTemplates.CustomModuleWithNativeDM,
                    ModuleTemplates.NativeModuleBase,
                    ModuleTemplates.CustomModule2WithCustomModuleIncompatible,
                    ModuleTemplates.CommunityFrameworkModuleBeforeNative,
                },
                new()
                {
                    ModuleTemplates.CommunityFrameworkModuleBeforeNative.Id,
                    ModuleTemplates.NativeModuleBase.Id,
                    ModuleTemplates.CustomModuleWithNativeDM.Id,
                }),

            ModuleListTemplates.MissingModule1 => (
                new()
                {
                    ModuleTemplates.CustomModule2WithCustomModuleDM,
                    ModuleTemplates.NativeModuleBase,
                },
                new()
                {
                    ModuleTemplates.NativeModuleBase.Id
                }),
            ModuleListTemplates.MissingModule2 => (
                new()
                {
                    ModuleTemplates.CustomModule2WithCustomModuleDMVersion,
                    ModuleTemplates.NativeModuleBase,
                },
                new()
                {
                    ModuleTemplates.NativeModuleBase.Id
                }),

            _ => (new(), new())
        };

        public List<ModuleInfoExtendedWithSelected> ModuleInfoModels => Tuple.ModuleInfoModels;
        public List<string> ExpectedIdOrder => Tuple.ExpectedIdOrder;

        public List<ModuleInfoExtendedWithMetadata> GetModuleInfos() => PreSort(ModuleInfoModels).Select(x => new ModuleInfoExtendedWithMetadata(x, false, string.Empty)).ToList();

        private static IEnumerable<ModuleInfoExtendedWithSelected> PreSort(IEnumerable<ModuleInfoExtendedWithSelected> source) => source
            .OrderByDescending(mim => mim.Id, new AlphanumComparatorFast())
            .ThenByDescending(mim => mim.IsOfficial);
    }
}