// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using EdFi.Admin.DataAccess.Contexts;
using EdFi.Ods.AdminApp.Management.ClaimSetEditor;
using EdFi.Ods.AdminApp.Management.Configuration.Claims;
using EdFi.Ods.AdminApp.Management.Database.Models;
using EdFi.Ods.AdminApp.Management.Instances;
using EdFi.Ods.AdminApp.Management.OdsInstanceServices;
using EdFi.Security.DataAccess.Contexts;
using Action = System.Action;

namespace EdFi.Ods.AdminApp.Management.OnPrem
{
    public class CompleteOnPremFirstTimeSetupCommand : ICompleteOdsFirstTimeSetupCommand
    {
        private readonly IUsersContext _usersContext;
        private readonly ISecurityContext _securityContext;
        private readonly ICloudOdsClaimSetConfigurator _cloudOdsClaimSetConfigurator;
        private readonly IOdsInstanceFirstTimeSetupService _firstTimeSetupService;
        private readonly IAssessmentVendorAdjustment _assessmentVendorAdjustment;
        private readonly IClaimSetCheckService _claimSetCheckService;
        private readonly IInferInstanceService _instanceService;

        public Action ExtraDatabaseInitializationAction { get; set; }

        public CompleteOnPremFirstTimeSetupCommand(
            IUsersContext usersContext,
            ISecurityContext securityContext,
            ICloudOdsClaimSetConfigurator cloudOdsClaimSetConfigurator,
            IOdsInstanceFirstTimeSetupService firstTimeSetupService,
            IAssessmentVendorAdjustment assessmentVendorAdjustment,
            IClaimSetCheckService claimSetCheckService,
            IInferInstanceService instanceService)
        {
            _assessmentVendorAdjustment = assessmentVendorAdjustment;
            _claimSetCheckService = claimSetCheckService;
            _instanceService = instanceService;
            _usersContext = usersContext;
            _securityContext = securityContext;
            _cloudOdsClaimSetConfigurator = cloudOdsClaimSetConfigurator;
            _firstTimeSetupService = firstTimeSetupService;
        }

        public async Task<bool> Execute(string odsInstanceName, CloudOdsClaimSet claimSet, ApiMode apiMode)
        {
            CancellationToken _cancellationToken = new CancellationToken();
            ExtraDatabaseInitializationAction?.Invoke();
            var restartRequired = false;

            if (apiMode.SupportsSingleInstance)
            {
                var defaultOdsInstance = new OdsInstanceRegistration
                {
                    Name = odsInstanceName,
                    DatabaseName = _instanceService.DatabaseName(0, apiMode),
                    Description = "Default single ods instance"
                };
                await _firstTimeSetupService.CompleteSetup(defaultOdsInstance, claimSet, apiMode);
            }

            if (!_claimSetCheckService.RequiredClaimSetsExist())
            {
                CreateClaimSetForAdminApp(claimSet);

                ApplyAdditionalClaimSetModifications();

                restartRequired = true;
            }

            await _usersContext.SaveChangesAsync(_cancellationToken);
            await _securityContext.SaveChangesAsync();

            return restartRequired;
        }

        private void CreateClaimSetForAdminApp(CloudOdsClaimSet cloudOdsClaimSet)
        {
            _cloudOdsClaimSetConfigurator.ApplyConfiguration(cloudOdsClaimSet);
        }

        private void ApplyAdditionalClaimSetModifications()
        {

            _assessmentVendorAdjustment.ReadAndCreatePerformanceLevelDescriptors();
        }
    }
}
