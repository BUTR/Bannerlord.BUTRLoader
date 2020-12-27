namespace Bannerlord.BUTRLoader.ModuleInfoExtended
{
    /// <summary>
    /// https://github.com/BUTR/Bannerlord.ButterLib/blob/dev/src/Bannerlord.ButterLib/Helpers/ModuleInfo/LoadType.cs
    /// </summary>
    internal enum LoadType
    {
        NONE           = 0,
        LoadAfterThis  = 1,
        LoadBeforeThis = 2
    }

    internal enum LoadTypeParse
    {
        LoadAfterThis  = 1,
        LoadBeforeThis = 2
    }
}