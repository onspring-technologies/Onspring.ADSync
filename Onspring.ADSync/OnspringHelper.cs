using System;
using Onspring.ADSync.Models;
using Onspring.API.SDK.Helpers;

namespace Onspring.ADSync
{
    internal sealed class OnspringHelper
    {
        private readonly OnspringMapper _mapper = new OnspringMapper();
        private readonly HttpHelper _httpHelper;

        public OnspringHelper(string apiUrl, string apiKey)
        {
            _httpHelper = new HttpHelper(apiUrl, apiKey);
        }

        public OnspringUser GetUserByUsername(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return null;
            }
            var filter = $"{_mapper.UsernameFieldId} eq '{userName}'";
            var fieldIds = new[]
            {
                _mapper.UsernameFieldId,
                _mapper.FirstNameFieldId,
                _mapper.LastNameFieldId,
                _mapper.EmailFieldId,
            };
            var records = _httpHelper.GetAppRecords(_mapper.UsersAppId, filter, fieldIds: fieldIds);
            switch (records.Count)
            {
                case 0:
                    return null;
                case 1:
                    return _mapper.LoadUser(records[0]);
                default:
                    throw new ApplicationException("More than one user with Username: " + userName);
            }
        }

        public int? AddNewUser(OnspringUser user)
        {
            var fieldValues = _mapper.GetAddEditValues(user);
            var createResult = _httpHelper.CreateAppRecord(_mapper.UsersAppId, fieldValues);
            return createResult.CreatedId;
        }
    }
}
