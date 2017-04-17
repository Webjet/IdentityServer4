﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Default resource owner password validator (not implementation == not supported)
    /// </summary>
    /// <seealso cref="IdentityServer4.Validation.IResourceOwnerPasswordValidator" />
    public class NotSupportedResouceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly ILogger<NotSupportedResouceOwnerPasswordValidator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotSupportedResouceOwnerPasswordValidator"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public NotSupportedResouceOwnerPasswordValidator(ILogger<NotSupportedResouceOwnerPasswordValidator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validates the resource owner password credential
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            context.Result = new GrantValidationResult(TokenRequestErrors.UnsupportedGrantType);

            _logger.LogWarning("Resource owner password credential type not supported. Configure an IResourceOwnerPasswordValidator.");
            return Task.FromResult(0);
        }
    }
}