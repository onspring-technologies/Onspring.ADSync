using System.Configuration;
using Onspring.API.SDK.Models;

namespace Onspring.ADSync.Models
{
    internal sealed class OnspringMapper
    {
        public OnspringMapper()
        {
            UsersAppId = int.Parse(ConfigurationManager.AppSettings[nameof(UsersAppId)]);
            UsernameFieldId = int.Parse(ConfigurationManager.AppSettings[nameof(UsernameFieldId)]);
            FirstNameFieldId = int.Parse(ConfigurationManager.AppSettings[nameof(FirstNameFieldId)]);
            LastNameFieldId = int.Parse(ConfigurationManager.AppSettings[nameof(LastNameFieldId)]);
            EmailFieldId = int.Parse(ConfigurationManager.AppSettings[nameof(EmailFieldId)]);
        }

        public int UsersAppId { get; }
        public int UsernameFieldId { get; }
        public int FirstNameFieldId { get; }
        public int LastNameFieldId { get; }
        public int EmailFieldId { get; }

        public OnspringUser LoadUser(ResultRecord record)
        {
            return new OnspringUser
            {
                Username = record.Values[UsernameFieldId]?.AsString,
                FirstName = record.Values[FirstNameFieldId]?.AsString,
                LastName = record.Values[LastNameFieldId]?.AsString,
                Email = record.Values[EmailFieldId]?.AsString,
            };
        }

        public FieldAddEditContainer GetAddEditValues(OnspringUser user)
        {
            var values = new FieldAddEditContainer();
            values.Add(UsernameFieldId, user.Username);
            values.Add(FirstNameFieldId, user.FirstName);
            values.Add(LastNameFieldId, user.LastName);
            values.Add(EmailFieldId, user.Email);
            return values;
        }

    }
}
