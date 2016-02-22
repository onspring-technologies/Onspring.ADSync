using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;

namespace Onspring.ADSync
{
    class AdUser
    {
        public static readonly string[] Properties = {"distinguishedName", "sAMAccountName", "cn", "memberOf", "displayName", "givenName", "sn", "name", "whenChanged", "lockoutTime", "userPrincipalName", "mail"};
        public AdUser(SearchResult result)
        {
            Dn = GetSinglePropString(result, "distinguishedName");
            Upn = GetSinglePropString(result, "userPrincipalName");
            SamAccountName = GetSinglePropString(result, "sAMAccountName");
            Cn = GetSinglePropString(result, "cn");
            DisplayName = GetSinglePropString(result, "displayName");
            GivenName = GetSinglePropString(result, "givenName");
            SurName = GetSinglePropString(result, "sn");
            FullName = GetSinglePropString(result, "name");
            Mail = GetSinglePropString(result, "mail");
            MemberOf = result.Properties["memberOf"].OfType<string>().ToList();

            if (result.Properties.Contains("lockoutTime") && result.Properties["lockoutTime"][0] != null)
                LockoutTime = (long) result.Properties["lockoutTime"][0];
            else
                LockoutTime = 0;

            IsLocked = (LockoutTime >= 1);

            if (result.Properties["whenChanged"][0] != null)
                WhenChanged = (DateTime)result.Properties["whenChanged"][0];
        }

        private static string GetSinglePropString(SearchResult result, string key)
        {
            return (result.Properties.Contains(key)) ? result.Properties[key][0] as string : null;
        }

        public string Dn { get; private set; }
        public string Upn { get; private set; }
        public string SamAccountName { get; private set; }
        public string Cn { get; private set; }
        public List<string> MemberOf { get; private set; }
        public string DisplayName { get; private set; }
        public string GivenName { get; private set; }
        public string SurName { get; private set; }
        public string FullName { get; private set; }
        public string Mail { get; private set; }
        public DateTime WhenChanged { get; private set; }
        public bool IsLocked { get; private set; }
        public long LockoutTime { get; }
    }

    class AdUserCollection : List<AdUser>
    {
        public AdUserCollection(SearchResultCollection results)
        {
            foreach (SearchResult result in results)
            {
                Add(new AdUser(result));
            }
        }
    }
}
