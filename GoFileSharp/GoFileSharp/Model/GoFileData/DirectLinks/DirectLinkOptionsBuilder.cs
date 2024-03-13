using System;
using System.Collections.Generic;
using System.Net;

namespace GoFileSharp.Model.GoFileData
{
    public class DirectLinkOptionsBuilder
    {
        internal DateTimeOffset? _expireTime { get; set; }
        internal List<string> _sourceIpsAllowed = new List<string>();
        internal List<string> _domainsAllowed = new List<string>();
        internal List<string> _auth = new List<string>();

        public DirectLinkOptionsBuilder WithExpireTime(DateTimeOffset expireTime)
        {
            _expireTime = expireTime;
            return this;
        }

        public DirectLinkOptionsBuilder AddAllowedSourceIp(IPAddress ip)
        {
            _sourceIpsAllowed.Add(ip.ToString());
            return this;
        }

        public DirectLinkOptionsBuilder AddAllowedDomain(string domain)
        {
            _domainsAllowed.Add(domain);
            return this;
        }

        public DirectLinkOptionsBuilder AddAuth(string name, string password)
        {
            _auth.Add($"{name}:{password}");
            return this;
        }

        public DirectLinkOptions Build()
        {
            string[]? sourceIPs = _sourceIpsAllowed.Count > 0 ? _sourceIpsAllowed.ToArray() : null;
            string[]? domains = _domainsAllowed.Count > 0 ? _domainsAllowed.ToArray() : null;
            string[]? auth = _auth.Count > 0 ? _auth.ToArray() : null;
            
            return new DirectLinkOptions()
            {
                ExpireTime = _expireTime,
                SourceIpsAllowed = sourceIPs,
                DomainsAllowed = domains,
                Auth = auth
            };
        }
    }
}