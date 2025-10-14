namespace ThermoFisher.Opal.Api;

/// <summary>
/// Abstraction for a secret store for database access
/// </summary>
// TODO: Make less DB centric.
public interface ISecretStore
{
    /// <summary>
    /// Create a DB password for the given module user
    /// This is a temporary solution until secrets management (#17) is implemented.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public string CreateDbPassword(string userName);

    /// <summary>
    /// Retrieve the database password for the global admin account (opal).
    /// This is a temporary solution until secrets management (#17) is implemented.
    /// </summary>
    /// <returns></returns>
    public string GetDbAdminPassword();

    /// <summary>
    /// Get the DB password for the given module user
    /// This is a temporary solution until secrets management (#17) is implemented.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public string GetDbPassword(string userName);
}
