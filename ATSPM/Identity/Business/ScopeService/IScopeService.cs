namespace Identity.Business.ScopeService
{
    public interface IScopeService
    {
        IEnumerable<string> GetScopesForClient(string clientId);
    }
}
