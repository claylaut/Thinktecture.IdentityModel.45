﻿using System;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Security;
using Thinktecture.IdentityModel.Constants;
using Thinktecture.IdentityModel.Extensions;
using Thinktecture.IdentityModel.WSTrust;
using Thinktecture.Samples;

namespace Thinktecture.Samples
{
    class Program
    {
        static Uri _baseAddress = new Uri(Constants.WebHostBaseAddress);
        //static Uri _baseAddress = new Uri(Constants.SelfHostBaseAddress);

        static void Main(string[] args)
        {
            var token = GetSamlToken();
            CallService(token);
        }

        private static void CallService(string token)
        {
            var client = new HttpClient
            {
                BaseAddress = _baseAddress
            };

            client.SetToken("SAML", token);

            while (true)
            {
                Helper.Timer(() =>
                {
                    var response = client.GetAsync("identity").Result;
                    response.EnsureSuccessStatusCode();

                    var claims = response.Content.ReadAsAsync<ViewClaims>().Result;
                    Helper.ShowConsole(claims);
                });

                Console.ReadLine();
            }            
        }

        private static string GetSamlToken()
        {
            "Requesting identity token".ConsoleYellow();

            var factory = new WSTrustChannelFactory(
                new UserNameWSTrustBinding(SecurityMode.TransportWithMessageCredential),
                Constants.IdSrv.WSTrustEndpoint);
            factory.TrustVersion = TrustVersion.WSTrust13;

            factory.Credentials.UserName.UserName = "bob";
            factory.Credentials.UserName.Password = "abc!123";

            var rst = new RequestSecurityToken
            {
                RequestType = RequestTypes.Issue,
                KeyType = KeyTypes.Bearer,
                TokenType = TokenTypes.Saml2TokenProfile11,
                AppliesTo = new EndpointReference(Constants.Realm)
            };

            var token = factory.CreateChannel().Issue(rst) as GenericXmlSecurityToken;
            return token.TokenXml.OuterXml;
        }
    }
}
