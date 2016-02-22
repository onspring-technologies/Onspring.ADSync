# Onspring ActiveDirectory Sync

Onspring.ADSync is a small sample console application that helps you learn the recommended approach for creating an integration to an Onspring instance using the [Onspring .NET SDK](https://github.com/onspring-technologies/onspring-api-sdk).

## Prerequisites

- .NET 4.5 or later
- Visual Studio 2015 or later (for C# 6)

## Application Overview

Although this application is intended primarily as a learning tool, it can actually add users to your Onspring instance from ActiveDirectory.  The application queries ActiveDirectory to obtain the users in the group specified in a command line parameter.  For each user, it checks to see if the Username already exists in the configured Onspring instance.  If it does not, it asks if the user should be added.  **Users will be added to your Onspring instance if you answer this question with "y"**, so please be aware of this if you run this application.

## Configure for your Onspring instance

This section describes how to configure the sample project for use with your Onspring instance.  In doing so, you will be learning the recommended way to configure your own integration with Onspring.

### Configure ApiKey

- Obtain an API key as described in the **API Key** section of [the SDK's README](https://github.com/onspring-technologies/onspring-api-sdk)
- In the sample project's App.config, replace the current `ApiKey` setting with your API key:

```
      <appSettings>
        ...
        <add key="ApiKey" value="000000ffffff000000ffffff/00000000-ffff-0000-ffff-000000000000"/>
        ...
      </appSettings>
```

### Configure Users AppId

- Obtain the AppId for the Users app in your Onspring instance.  One easy way to do this is by using the [Onspring.ApiDemo](https://github.com/onspring-technologies/Onspring.ApiDemo) sample project.  If you are using it, click the **Get App List** button, then inspect the **Results** at the bottom to locate the Id of the Users app.
- In the sample project's App.config, replace the current `UsersAppId` setting with the Users AppId for your instance:

```
      <appSettings>
        ...
        <add key="UsersAppId" value="1"/>
        ...
      </appSettings>
```

### Configure User FieldIds

- Obtain the FieldId for the following fields in the Users app in your Onspring instance:
  * Username
  * First Name
  * Last Name
  * Email Address

- One easy way to do this is by using the [Onspring.ApiDemo](https://github.com/onspring-technologies/Onspring.ApiDemo) sample project.  If you are using it, enter your Users AppId into the field next to the **Get Field List** button, then click the button.  Inspect the **Results** at the bottom to locate the Ids of the fields listed above.

- In the sample project's App.config, replace the current `UsernameFieldId`, `FirstNameFieldId`, `LastNameFieldId`, and `EmailFieldId` settings with the appropriate values for your instance:

```
      <appSettings>
        ...
        <add key="UsernameFieldId" value="4"/>
        <add key="FirstNameFieldId" value="5"/>
        <add key="LastNameFieldId" value="7"/>
        <add key="EmailFieldId" value="12"/>
        ...
      </appSettings>
```

## Code Walkthrough

This section describes key aspects of the code in the sample project to help you learn the recommended approach for creating effective and efficient integrations with Onspring.

### OnspringUser.cs

- A strongly-typed business object - you will probably want to create similar business objects for your own integration

```
    public class OnspringUser
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
```

### OnspringMapper.cs

- Create a mapper using the settings configured in the App.config

```
    public OnspringMapper()
    {
        UsersAppId = int.Parse(ConfigurationManager.AppSettings[nameof(UsersAppId)]);
        UsernameFieldId = int.Parse(ConfigurationManager.AppSettings[nameof(UsernameFieldId)]);
        FirstNameFieldId = int.Parse(ConfigurationManager.AppSettings[nameof(FirstNameFieldId)]);
        LastNameFieldId = int.Parse(ConfigurationManager.AppSettings[nameof(LastNameFieldId)]);
        EmailFieldId = int.Parse(ConfigurationManager.AppSettings[nameof(EmailFieldId)]);
    }
```

- Load a business object using a generic result record obtained from calling the API using the SDK

```
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
```

- Pass in a business object to prepare the value container used to add and update records using the SDK

```
    public FieldAddEditContainer GetAddEditValues(OnspringUser user)
    {
        var values = new FieldAddEditContainer();
        values.Add(UsernameFieldId, user.Username);
        values.Add(FirstNameFieldId, user.FirstName);
        values.Add(LastNameFieldId, user.LastName);
        values.Add(EmailFieldId, user.Email);
        return values;
    }
```

### OnspringHelper.cs

#### GetUserByUsername(string userName)

- Construct a filter used to query the API
```
    var filter = $"{_mapper.UsernameFieldId} eq '{userName}'";
```

- Limit the fields in the result to the ones we care about
```
    var fieldIds = new[]
    {
        _mapper.UsernameFieldId,
        _mapper.FirstNameFieldId,
        _mapper.LastNameFieldId,
        _mapper.EmailFieldId,
    };
```

- Retrieve the matching record information by calling the API using the SDK
```
    var records = _httpHelper.GetAppRecords(_mapper.UsersAppId, filter, fieldIds: fieldIds);
```

- Use the `OnspringMapper` method mentioned earlier to create the strongly-typed business object
```
    return _mapper.LoadUser(records[0]);
```

#### AddNewUser(OnspringUser user)

- Use a strongly-typed object to create and load a value container, then add the record and return the new record's Id

```
    public int? AddNewUser(OnspringUser user)
    {
        var fieldValues = _mapper.GetAddEditValues(user);
        var createResult = _httpHelper.CreateAppRecord(_mapper.UsersAppId, fieldValues);
        return createResult.CreatedId;
    }
```

### Program.cs

- Create an instance of the project-specific `OnspringHelper` used to make calls to the API
```
    var apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
    var apiKey = ConfigurationManager.AppSettings["ApiKey"];
    var onspringApi = new OnspringHelper(apiBaseUrl, apiKey);
```

- Call the project-specific helper method to search for a user by username
```
    var onUser = onspringApi.GetUserByUsername(adUser.SamAccountName);
```

- Call the project-specific helper method to add a new user
```
    var recordId = onspringApi.AddNewUser(onUser);
```
