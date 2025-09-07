using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class AppShellViewModel
{

    private bool _isVisible;
    public bool IsVisible
    {
        get { return _isVisible; }
        set { _isVisible = value; }
    }

    private User _user;
    public User CurrentUser
    {
        get { return _user; }
        set { _user = value; }
    }

    private int _sessionID;
    public int SessionID
    {
        get { return _sessionID; }
        set { _sessionID = value; }
    }

    private JsonSerializerOptions _jsonOptions;
    public JsonSerializerOptions JSONOptions
    {
        get { return _jsonOptions; }
        set { _jsonOptions = value; }
    }
public AppShellViewModel()
    {
        _isVisible = false;

        CurrentUser = new User();

        JSONOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };
    }
}
