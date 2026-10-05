using System.Text.Json;
using System.Text.Json.Serialization;

public partial class AppShellViewModel
{

    private User _user;

    public User CurrentUser
    {
        get { return _user; }
        set { _user = value; }
    }

    // The token of POST /session, sent as "Authorization: Bearer <token>"
    // (BearerTokenHandler); null when signed out
    private string? _token;

    public string? Token
    {
        get { return _token; }
        set { _token = value; }
    }

    private JsonSerializerOptions _jsonOptions;

    public JsonSerializerOptions JSONOptions
    {
        get { return _jsonOptions; }
        set { _jsonOptions = value; }
    }

    public AppShellViewModel()
    {
        CurrentUser = new User();

        JSONOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };
    }
}
