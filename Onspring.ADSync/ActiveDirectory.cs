using System.Collections.Generic;
using System.DirectoryServices;

namespace Onspring.ADSync
{
    class ActiveDirectory
    {
        public string BaseDn { get; }
        public string Server { get; }
        public DirectoryEntry BaseDe { get; }
        public ActiveDirectory(string server, string baseDn)
        {
            Server = server;
            BaseDn = baseDn;
            BaseDe = new DirectoryEntry($"LDAP://{Server}/{BaseDn}");
        }

        public AdUserCollection GetUsersByGroupMembership(string groupName)
        {
            // get group Dn
            var groupDn = GetGroupDn(groupName);
            if (groupDn == null) return null;
            var results = SearchFilter($"(&(objectClass=user)(memberOf={groupDn}))", AdUser.Properties);
            return new AdUserCollection(results);
        }

        private string GetGroupDn(string groupName)
        {
            var results = SearchFilter($"(&(objectClass=group)(cn={groupName}))", new[] {"distinguishedName"});
            return (results.Count < 1) ? null : results[0].Properties["distinguishedName"][0].ToString();
        }

        private SearchResultCollection SearchFilter(string filter, IEnumerable<string> properties)
        {
            var searcher = new DirectorySearcher(BaseDe) {SearchScope = SearchScope.Subtree, Filter = filter};

            foreach (var property in properties)
            {
                searcher.PropertiesToLoad.Add(property);
            }

            return searcher.FindAll();
        }
    }
}
