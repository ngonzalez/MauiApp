using System;

public class AppShellViewModel
{

    private bool _signInIsVisible;
    public bool SignInIsVisible
    {
        get { return _signInIsVisible; }
        set { _signInIsVisible = value; }
    }
    private User _user;
    public User CurrentUser
    {
        get { return _user; }
        set { _user = value; }
    }
    public AppShellViewModel()
    {
        _signInIsVisible = false;
    }
}
