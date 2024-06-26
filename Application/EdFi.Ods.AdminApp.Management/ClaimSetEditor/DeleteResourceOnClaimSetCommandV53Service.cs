// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.
extern alias SecurityCompatiblity53;

using System.Linq;
using SecurityCompatiblity53::EdFi.SecurityCompatiblity53.DataAccess.Contexts;

namespace EdFi.Ods.AdminApp.Management.ClaimSetEditor
{
    public class DeleteResourceOnClaimSetCommandV53Service
    {
        private readonly ISecurityContext _context;

        public DeleteResourceOnClaimSetCommandV53Service(ISecurityContext context)
        {
            _context = context;
        }

        public void Execute(IDeleteResourceOnClaimSetModel model)
        {
            var resourceClaimsToRemove = _context.ClaimSetResourceClaims.Where(x =>
                x.ResourceClaim.ResourceClaimId == model.ResourceClaimId && x.ClaimSet.ClaimSetId == model.ClaimSetId).ToList();

            _context.ClaimSetResourceClaims.RemoveRange(resourceClaimsToRemove);
            _context.SaveChanges();
        }
    }
}
