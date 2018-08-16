using System.Collections.Generic;

namespace ToastyNetworksUpdateAnnouncer
{
    public class Modpack
    {
        public string Name { get; private set; }
        public int Id { get; private set; }
        public string Version { get; private set; }
        public string VersionType { get; private set; }
        public string ChangelogUrl { get; private set; }
        public List<Modpack> ModpackList = new List<Modpack>();

        public List<int> ModpackIds = new List<int>()
        {
            227724,
            256183,
            227724,
            233384,
            283861,
            281999,
            246578,
            243560,
            261783,
            235223,
            293960
        };

        public Modpack(string modpackName, int modpackId, string version, string versionType, string changelogUrl)
        {
            Name = modpackName;
            Id = modpackId;
            Version = version;
            VersionType = versionType;
            ChangelogUrl = changelogUrl;
        }

        public Modpack()
        {
        }
    }
}