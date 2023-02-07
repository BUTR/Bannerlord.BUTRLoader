using Bannerlord.BLSE.Features.ContinueSaveFile.Patches;

using System;

namespace Bannerlord.BLSE.Features.ContinueSaveFile
{
    internal class InformationManagerConfirmInquiryHandler : IDisposable
    {
        public InformationManagerConfirmInquiryHandler() => InformationManagerPatch.SkipChange = true;
        public void Dispose() => InformationManagerPatch.SkipChange = false;
    }
}