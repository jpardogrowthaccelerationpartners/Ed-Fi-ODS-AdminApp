// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.
extern alias SecurityCompatiblity53;

using System.Linq;
using SecurityCompatiblity53::EdFi.SecurityCompatiblity53.DataAccess.Contexts;

namespace EdFi.Ods.AdminApp.Management.ClaimSetEditor
{
    public class UpdateResourcesOnClaimSetCommandV53Service
    {
        private readonly ISecurityContext _context;
        private readonly AddOrEditResourcesOnClaimSetCommand _addOrEditResourcesOnClaimSetCommand;

        public UpdateResourcesOnClaimSetCommandV53Service(ISecurityContext context,
            AddOrEditResourcesOnClaimSetCommand addOrEditResourcesOnClaimSetCommand)
        {
            _context = context;
            _addOrEditResourcesOnClaimSetCommand = addOrEditResourcesOnClaimSetCommand;
        }

        public void Execute(IUpdateResourcesOnClaimSetModel model)
        {
            var resourceClaimsForClaimSet =
                _context.ClaimSetResourceClaims.Where(x => x.ClaimSet.ClaimSetId == model.ClaimSetId).ToList();
            _context.ClaimSetResourceClaims.RemoveRange(resourceClaimsForClaimSet);
            _context.SaveChanges();

            _addOrEditResourcesOnClaimSetCommand.Execute(model.ClaimSetId, model.ResourceClaims);
        }
    }
}
