using Bannerlord.BUTRLoader.Features.ContinueSaveFile.Patches;

using System;

namespace Bannerlord.BUTRLoader.Features.ContinueSaveFile
{
    internal class InformationManagerConfirmInquiryHandler : IDisposable
    {
        public InformationManagerConfirmInquiryHandler() => InformationManagerPatch.SkipChange = true;
        public void Dispose() => InformationManagerPatch.SkipChange = false;
    }
}