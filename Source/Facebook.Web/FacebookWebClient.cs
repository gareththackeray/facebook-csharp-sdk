﻿// --------------------------------
// <copyright file="FacebookWebClient.cs" company="Thuzi LLC (www.thuzi.com)">
//     Microsoft Public License (Ms-PL)
// </copyright>
// <author>Nathan Totten (ntotten.com), Jim Zimmerman (jimzimmerman.com) and Prabir Shrestha (prabir.me)</author>
// <license>Released under the terms of the Microsoft Public License (Ms-PL)</license>
// <website>http://facebooksdk.codeplex.com</website>
// ---------------------------------

namespace Facebook.Web
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the Facebook Web Client.
    /// </summary>
    public class FacebookWebClient : FacebookClient
    {
        private FacebookWebContext _request;

        private bool _isSecureConnection;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FacebookWebClient"/> class.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        public FacebookWebClient(FacebookWebContext request)
        {
            Initialize(request);
            AccessToken = request.AccessToken;
        }

        /// <summary>
        /// Initializes the FacebookWebClient from <see cref="FacebookWebContext"/>.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        private void Initialize(FacebookWebContext request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            _request = request;
            _isSecureConnection = request.IsSecureConnection;

            UseFacebookBeta = _request.Settings.UseFacebookBeta;

            if (request.HttpContext.Request.UrlReferrer != null && _request.HttpContext.Request.UrlReferrer.Host == "apps.beta.facebook.com")
            {
                UseFacebookBeta = true;
            }
        }

        internal protected override object Api(string path, IDictionary<string, object> parameters, HttpMethod httpMethod, Type resultType)
        {
            try
            {
                return base.Api(path, AddReturnSslResourceIfRequired(parameters, IsSecureConnection), httpMethod, resultType);
            }
            catch (FacebookOAuthException)
            {
                try
                {
                    _request.DeleteAuthCookie();
                }
                catch { }
                throw;
            }
        }

        protected internal override void ApiAsync(string path, IDictionary<string, object> parameters, HttpMethod httpMethod, object userToken)
        {
            base.ApiAsync(path, AddReturnSslResourceIfRequired(parameters, IsSecureConnection), httpMethod, userToken);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the scheme is secure.
        /// </summary>
        public virtual bool IsSecureConnection
        {
            get { return _isSecureConnection; }
            set { _isSecureConnection = value; }
        }

        internal static IDictionary<string, object> AddReturnSslResourceIfRequired(IDictionary<string, object> parameters, bool isSecuredConnection)
        {
            var mergedParameters = FacebookUtils.Merge(null, parameters);

            if (isSecuredConnection && !mergedParameters.ContainsKey(Facebook.Web.Properties.Resources.return_ssl_resources))
            {
                mergedParameters[Facebook.Web.Properties.Resources.return_ssl_resources] = true;
            }

            return mergedParameters;
        }

        protected override void OnGetCompleted(FacebookApiEventArgs args)
        {
            DeleteAuthCookieIfOAuthException(args);
            base.OnGetCompleted(args);
        }

        protected override void OnPostCompleted(FacebookApiEventArgs args)
        {
            DeleteAuthCookieIfOAuthException(args);
            base.OnPostCompleted(args);
        }

        protected override void OnDeleteCompleted(FacebookApiEventArgs args)
        {
            DeleteAuthCookieIfOAuthException(args);
            base.OnDeleteCompleted(args);
        }

        private void DeleteAuthCookieIfOAuthException(FacebookApiEventArgs args)
        {
            if (args.Error != null && args.Error is FacebookOAuthException)
            {
                try
                {
                    _request.DeleteAuthCookie();
                }
                catch { }
            }
        }
    }
}
