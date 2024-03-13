using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace GoFileSharp.Model.HTTP
{
    internal class GoFileRequestBuilder
    {
        private string _route;
        private HttpMethod _method;
        private Dictionary<string, object> _params = new Dictionary<string, object>();
        private string _token = "";

        internal GoFileRequestBuilder(HttpMethod method, string route)
        {
            _route = route;
            _method = method;
        }
        
        internal GoFileRequestBuilder AddOptionalParam(string key, object? value)
        {
            if (value == null)
            {
                return this;
            }
            
            _params.Add(key, value);
            return this;
        }

        internal GoFileRequestBuilder AddRequiredParam(string key, object value)
        {
            _params.Add(key, value);
            return this;
        }

        internal GoFileRequestBuilder WithBearerToken(string token)
        {
            _token = token;
            return this;
        }

        internal HttpRequestMessage Build()
        {
            var request = new HttpRequestMessage(_method, _route);

            if (!string.IsNullOrWhiteSpace(_token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("bearer", _token);
            }

            if (_params.Count == 0)
            {
                return request;
            }

            var json = JsonConvert.SerializeObject(_params);

            request.Content = new StringContent(json);

            return request;
        }
    }
}