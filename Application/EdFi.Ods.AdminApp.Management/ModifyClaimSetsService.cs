// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.
extern alias SecurityCompatiblity53;

using Microsoft.EntityFrameworkCore;
using System.Linq;
using EdFi.Ods.AdminApp.Management.Configuration.Claims;
using SecurityCompatiblity53::EdFi.SecurityCompatiblity53.DataAccess.Contexts;

namespace EdFi.Ods.AdminApp.Management
{
    public interface IModifyClaimSetsService
    {
        void SetNoFurtherAuthorizationRequiredOverrideOnResouceClaim(string resoureName, string actionType);
    }

    public class ModifyClaimSetsService : IModifyClaimSetsService
    {
        private readonly ISecurityContext _securityContext;

        public ModifyClaimSetsService(ISecurityContext securityContext)
        {
            _securityContext = securityContext;
        }

        public void SetNoFurtherAuthorizationRequiredOverrideOnResouceClaim(string resourceName, string actionType)
        {
            var claimAuthMetadata = _securityContext.ResourceClaimAuthorizationMetadatas
                .Include(x => x.Action)
                .Include(x => x.ResourceClaim)
                .Include(x => x.AuthorizationStrategy)
                .AsEnumerable().FirstOrDefault(x =>
                    x.Action.ActionName == actionType && x.ResourceClaim.ResourceName == resourceName);

            if (claimAuthMetadata == null) return;

            var authStrategy = _securityContext.AuthorizationStrategies.FirstOrDefault(x =>
                x.AuthorizationStrategyName == CloudOdsClaimAuthorizationStrategy.NoFurtherAuthorizationRequired.StrategyName);

            if (authStrategy == null) return;

            claimAuthMetadata.AuthorizationStrategy = authStrategy;
        }
    }
}
