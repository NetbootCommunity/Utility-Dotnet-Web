namespace MicroAutomation.Web.Models;

/// <summary>
/// Authentication type
/// This enum allow to configure kestrel in order to setup "Client Certificate"
/// </summary>
public enum AuthenticationType
{
    Default = 0,
    Certificate = 1,
}