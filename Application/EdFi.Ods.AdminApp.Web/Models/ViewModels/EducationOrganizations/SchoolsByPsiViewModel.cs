// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using EdFi.Ods.AdminApp.Management.Api.Models;

namespace EdFi.Ods.AdminApp.Web.Models.ViewModels.EducationOrganizations
{
    public class SchoolsByPsiViewModel
    {
        public int PsiId { get; set; }
        public List<PsiSchool> Schools { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int TotalSchools { get; set; }
    }
}
