namespace AuthProvider.Authentication;

/// <summary>
/// Type of authentication that the authentication attribute will use
/// </summary>
public enum AuthType
{
    /// <summary>
    /// Authentication done by way of username and password through the X-Auth-User and X-Auth-Pass headers
    /// </summary>
    CREDENTIALS,

    /// <summary>
    /// Authentication done by way of a session cookie
    /// </summary>
    SESSION,

    /// <summary>
    /// Authentication done by way of a session cookie and password through the X-Auth-Pass header
    /// </summary>
    STRICT_SESSION,

    /// <summary>
    /// Authentication done by way of an api key through the X-Api-Key header
    /// </summary>
    API_KEY,

    /// <summary>
    /// Authentication allowing <see cref="SESSION"/> or <see cref="API_KEY"/>
    /// </summary>
    STANDARD
}
