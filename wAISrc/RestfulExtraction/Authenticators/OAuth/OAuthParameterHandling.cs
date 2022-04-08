using System.Runtime.Serialization;

namespace RestfulExtraction.Authenticators.OAuth
{
    [DataContract]
    public enum OAuthParameterHandling
    {
        HttpAuthorizationHeader,
        UrlOrPostParameters
    }
}
