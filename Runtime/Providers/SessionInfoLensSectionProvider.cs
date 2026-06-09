using System;

namespace KostasBan.Lens
{
    public sealed class SessionInfoLensSectionProvider : ILensSectionProvider
    {
        private readonly string sessionId;
        private readonly string userId;

        public SessionInfoLensSectionProvider()
        {
            sessionId = Guid.NewGuid().ToString("N");
            userId = "anonymous-user";
        }

        public string SectionTitle => "Session Info";

        public System.Collections.Generic.IEnumerable<LensEntry> GetEntries()
        {
            yield return new LensEntry("Session ID", sessionId);
            yield return new LensEntry("Environment", "Development");
            yield return new LensEntry("User ID", userId);
        }
    }
}
