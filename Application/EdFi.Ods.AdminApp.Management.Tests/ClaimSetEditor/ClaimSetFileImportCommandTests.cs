// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using EdFi.Ods.AdminApp.Management.ClaimSetEditor;
using Shouldly;
using EdFi.Ods.AdminApp.Web.Models.ViewModels.ClaimSets;
using Moq;
using EdFi.Ods.AdminApp.Management.ClaimSetEditor.Extensions;
using EdFi.Ods.AdminApp.Management.Database.Queries;

using EdFi.Security.DataAccess.Contexts;

using static EdFi.Ods.AdminApp.Web.Models.ViewModels.ClaimSets.ClaimSetFileImportModel;
using Application = EdFi.Security.DataAccess.Models.Application;
using ClaimSet = EdFi.Security.DataAccess.Models.ClaimSet;

namespace EdFi.Ods.AdminApp.Management.Tests.ClaimSetEditor
{
    [TestFixture]
    public class ClaimSetFileImportCommandTests : SecurityDataTestBase
    {
        [Test]
        public void ShouldImportClaimSet()
        {
            var testApplication = new Application
            {
                ApplicationName = $"Test Application {DateTime.Now:O}"
            };
            Save(testApplication);

            SetupResourceClaims(testApplication, new List<string> { "TestParent1234", "TestParent9878787", "TestParent878978330" }, UniqueNameList("Child", 1));

            var testJSON = @"{
                ""title"": ""testfile"",
                ""template"": {
                    ""claimSets"": [
                      {
                        ""name"": ""Test Claimset"",
                        ""resourceClaims"": [
                          {
                            ""Name"": ""TestParent1234"",
                            ""Read"": true,
                            ""Create"": false,
                            ""Update"": false,
                            ""Delete"": false,
                            ""ReadChanges"": false,
                            ""Children"": []
                          },
                          {
                            ""Name"": ""TestParent9878787"",
                            ""Read"": true,
                            ""Create"": false,
                            ""Update"": false,
                            ""Delete"": false,
                            ""ReadChanges"": false,
                            ""Children"": []
                          },
                          {
                            ""Name"": ""TestParent878978330"",
                            ""Read"": true,
                            ""Create"": true,
                            ""Update"": true,
                            ""Delete"": true,
                            ""ReadChanges"": true,
                            ""Children"": []
                          }
                        ]
                      }
                    ]
                }
            }";

            var importModel = GetImportModel(testJSON);
            var importSharingModel = SharingModel.DeserializeToSharingModel(importModel.ImportFile.OpenReadStream());

            using var securityContext = TestContext;
            Command(securityContext).Execute(importSharingModel);

            var testClaimSet = securityContext.ClaimSets.AsEnumerable().FirstOrDefault(x => x.ClaimSetName == "Test Claimset");
            testClaimSet.ShouldNotBeNull();

            var resourcesForClaimSet = ResourceClaimsForClaimSet(testClaimSet.ClaimSetId).ToList();

            resourcesForClaimSet.Count.ShouldBeGreaterThan(0);
            var testResources = resourcesForClaimSet.Where(x => x.ParentId == 0).ToArray();
            testResources.Count().ShouldBe(3);

            var testResource1 = testResources[0];
            MatchActions(testResource1, "TestParent1234", new bool[] { false, true, false, false, false });

            var testResource2 = testResources[1];
            MatchActions(testResource2, "TestParent9878787", new bool[] { false, true, false, false, false });

            var testResource3 = testResources[2];
            MatchActions(testResource3, "TestParent878978330", new bool[] { true, true, true, true, true });

        }

        [Test]
        public void ShouldImportClaimSetResourceClaimWithAuthrozationStrategyOverrides()
        {
            var testApplication = new Application
            {
                ApplicationName = $"Test Application {DateTime.Now:O}"
            };
            Save(testApplication);

            var appAuthorizationStrategies = SetupApplicationAuthorizationStrategies(testApplication, 3).ToList();

            SetupResourceClaims(testApplication, new List<string> { "TestParentResourceClaim8789783213", "TestParentResourceClaim0989", "TestParentResourceClaim3009" }, UniqueNameList("Child", 1));

            var testJSON = @"{
    ""title"": ""testfile"",
    ""template"": {
        ""claimSets"": [
          {
            ""name"": ""Test Claimset"",
            ""resourceClaims"": [
              {
                ""Name"": ""TestParentResourceClaim8789783213"",
                ""Read"": true,
                ""Create"": true,
                ""Update"": false,
                ""Delete"": false,
                ""ReadChanges"": true,
                ""AuthStrategyOverridesForCRUD"": [
                    {
                    ""authorizationStrategies"": [{
                        ""AuthStrategyId"": {0},
                        ""AuthStrategyName"": ""{1}"",
                        ""DisplayName"": ""{1}"",
                        ""IsInheritedFromParent"": false
                    }]
                    },
                    {
                    ""authorizationStrategies"": [{
                        ""AuthStrategyId"": {2},
                        ""AuthStrategyName"": ""{3}"",
                        ""DisplayName"": ""{3}"",
                        ""IsInheritedFromParent"": false
                    }]
                    },
                    null,
                    null,
                    {
                    ""authorizationStrategies"": [{
                        ""AuthStrategyId"": {4},
                        ""AuthStrategyName"": ""{5}"",
                        ""DisplayName"": ""{5}"",
                        ""IsInheritedFromParent"": false
                    }]
                    }
                ],
                ""Children"": []
              },
              {
                ""Name"": ""TestParentResourceClaim0989"",
                ""Read"": true,
                ""Create"": false,
                ""Update"": false,
                ""Delete"": false,
                ""ReadChanges"": true,
                ""Children"": []
              },
              {
                ""Name"": ""TestParentResourceClaim3009"",
                ""Read"": true,
                ""Create"": true,
                ""Update"": true,
                ""Delete"": true,
                ""ReadChanges"": true,
                ""Children"": []
              }
            ]
          }
        ]
    }
}";


            var authStrategy1Id = appAuthorizationStrategies[0].AuthorizationStrategyId;
            var authStrategy1Name = appAuthorizationStrategies[0].AuthorizationStrategyName.ToString();

            var authStrategy2Id = appAuthorizationStrategies[1].AuthorizationStrategyId;
            var authStrategy2Name = appAuthorizationStrategies[1].AuthorizationStrategyName.ToString();

            var authStrategy3Id = appAuthorizationStrategies[2].AuthorizationStrategyId;
            var authStrategy3Name = appAuthorizationStrategies[2].AuthorizationStrategyName.ToString();

            var formattedJson = testJSON.Replace("{0}", authStrategy1Id.ToString())
                                        .Replace("{1}", authStrategy1Name)
                                        .Replace("{2}", authStrategy2Id.ToString())
                                        .Replace("{3}", authStrategy2Name)
                                        .Replace("{4}", authStrategy3Id.ToString())
                                        .Replace("{5}", authStrategy3Name);

            var importModel = GetImportModel(formattedJson);
            var importSharingModel = SharingModel.DeserializeToSharingModel(importModel.ImportFile.OpenReadStream());

            using var securityContext = TestContext;
            Command(securityContext).Execute(importSharingModel);

            var testClaimSet = securityContext.ClaimSets.AsEnumerable().FirstOrDefault(x => x.ClaimSetName == "Test Claimset");
            testClaimSet.ShouldNotBeNull();

            var resourcesForClaimSet = ResourceClaimsForClaimSet(testClaimSet.ClaimSetId).ToList();

            resourcesForClaimSet.Count.ShouldBeGreaterThan(0);
            var testResources = resourcesForClaimSet.Where(x => x.ParentId == 0).ToArray();
            testResources.Count().ShouldBe(3);

            var testResource1 = testResources[0];
            MatchActions(testResource1, "TestParentResourceClaim8789783213", new bool[] { true, true, false, false, true });

            testResource1.AuthStrategyOverridesForCRUD.ShouldNotBeNull();
            testResource1.AuthStrategyOverridesForCRUD.Length.ShouldBe(5);

            var authStrategyOverrideForCreate = testResource1.AuthStrategyOverridesForCRUD.Create();
            authStrategyOverrideForCreate.ShouldNotBeNull();
            authStrategyOverrideForCreate.AuthorizationStrategies[0].AuthStrategyId.ShouldBe(authStrategy1Id);
            authStrategyOverrideForCreate.AuthorizationStrategies[0].AuthStrategyName.ShouldBe(authStrategy1Name);

            var authStrategyOverrideForRead = testResource1.AuthStrategyOverridesForCRUD.Read();
            authStrategyOverrideForRead.ShouldNotBeNull();
            authStrategyOverrideForRead.AuthorizationStrategies[0].AuthStrategyId.ShouldBe(authStrategy2Id);
            authStrategyOverrideForRead.AuthorizationStrategies[0].AuthStrategyName.ShouldBe(authStrategy2Name);

            var authStrategyOverrideForReadChanges = testResource1.AuthStrategyOverridesForCRUD.ReadChanges();
            authStrategyOverrideForReadChanges.ShouldNotBeNull();
            authStrategyOverrideForReadChanges.AuthorizationStrategies[0].AuthStrategyId.ShouldBe(authStrategy3Id);
            authStrategyOverrideForReadChanges.AuthorizationStrategies[0].AuthStrategyName.ShouldBe(authStrategy3Name);

            var testResource2 = testResources[1];
            MatchActions(testResource2, "TestParentResourceClaim0989", new bool[] { false, true, false, false, true });

            var testResource3 = testResources[2];
            MatchActions(testResource3, "TestParentResourceClaim3009", new bool[] { true, true, true, true, true });
        }

        [Test]
        public void ShouldImportClaimSetResourceClaimWithChildrenAndAuthrozationStrategyOverrides()
        {
            var testApplication = new Application
            {
                ApplicationName = $"Test Application {DateTime.Now:O}"
            };
            Save(testApplication);

            var appAuthorizationStrategies = SetupApplicationAuthorizationStrategies(testApplication, 10).ToList();

            SetupResourceClaims(testApplication, new List<string> { "TestParentResourceClaim0981" }, new List<string> { "Child" });

            var testJSON = @"{
                ""title"": ""testfile"",
                ""template"": {
                    ""claimSets"": [
                      {
                        ""name"": ""Test Claimset"",
                        ""resourceClaims"": [
                          {
                            ""Name"": ""TestParentResourceClaim0981"",
                            ""Read"": true,
                            ""Create"": true,
                            ""Update"": false,
                            ""Delete"": false,
                            ""ReadChanges"": true,
                            ""AuthStrategyOverridesForCRUD"": [
                                  {
                                  ""authorizationStrategies"": [{
                                    ""AuthStrategyId"": {0},
                                    ""AuthStrategyName"": ""{1}"",
                                    ""DisplayName"": ""{1}"",
                                    ""IsInheritedFromParent"": false
                                  }]
                                  },
                                  {
                                  ""authorizationStrategies"": [{
                                    ""AuthStrategyId"": {2},
                                    ""AuthStrategyName"": ""{3}"",
                                    ""DisplayName"": ""{3}"",
                                    ""IsInheritedFromParent"": false
                                  }]
                                  },
                                  {
                                  ""authorizationStrategies"": [{
                                    ""AuthStrategyId"": {4},
                                    ""AuthStrategyName"": ""{5}"",
                                    ""DisplayName"": ""{5}"",
                                    ""IsInheritedFromParent"": false
                                  }]
                                  },
                                  {
                                  ""authorizationStrategies"": [{
                                    ""AuthStrategyId"": {6},
                                    ""AuthStrategyName"": ""{7}"",
                                    ""DisplayName"": ""{7}"",
                                    ""IsInheritedFromParent"": false
                                  }]
                                  },
                                  {
                                  ""authorizationStrategies"": [{
                                    ""AuthStrategyId"": {8},
                                    ""AuthStrategyName"": ""{9}"",
                                    ""DisplayName"": ""{9}"",
                                    ""IsInheritedFromParent"": false
                                  }]
                                  },
                                ],
                             ""Children"": [
                                    {
                                        ""Name"": ""Child-TestParentResourceClaim0981"",
                                        ""Read"": true,
                                        ""Create"": true,
                                        ""Update"": true,
                                        ""Delete"": true,
                                        ""ReadChanges"": true,
                                        ""AuthStrategyOverridesForCRUD"": [
                                          {
                                          ""authorizationStrategies"": [{
                                            ""AuthStrategyId"": {0},
                                            ""AuthStrategyName"": ""{1}"",
                                            ""DisplayName"": ""{1}"",
                                            ""IsInheritedFromParent"": true
                                          }]
                                          },
                                          {
                                          ""authorizationStrategies"": [{
                                            ""AuthStrategyId"": {2},
                                            ""AuthStrategyName"": ""{3}"",
                                            ""DisplayName"": ""{3}"",
                                            ""IsInheritedFromParent"": true
                                          }]
                                          },
                                          {
                                          ""authorizationStrategies"": [{
                                            ""AuthStrategyId"": {4},
                                            ""AuthStrategyName"": ""{5}"",
                                            ""DisplayName"": ""{5}"",
                                            ""IsInheritedFromParent"": true
                                          }]
                                          },
                                          {
                                          ""authorizationStrategies"": [{
                                            ""AuthStrategyId"": {6},
                                            ""AuthStrategyName"": ""{7}"",
                                            ""DisplayName"": ""{7}"",
                                            ""IsInheritedFromParent"": true
                                          }]
                                          },
                                          {
                                          ""authorizationStrategies"": [{
                                            ""AuthStrategyId"": {8},
                                            ""AuthStrategyName"": ""{9}"",
                                            ""DisplayName"": ""{9}"",
                                            ""IsInheritedFromParent"": true
                                          }]
                                          }
                                        ]
                                    }
                               ]
                          }
                        ]
                      }
                    ]
                }
            }";
            var authStrategy1Id = appAuthorizationStrategies[0].AuthorizationStrategyId;
            var authStrategy1Name = appAuthorizationStrategies[0].AuthorizationStrategyName.ToString();

            var authStrategy2Id = appAuthorizationStrategies[1].AuthorizationStrategyId;
            var authStrategy2Name = appAuthorizationStrategies[1].AuthorizationStrategyName.ToString();

            var authStrategy3Id = appAuthorizationStrategies[2].AuthorizationStrategyId;
            var authStrategy3Name = appAuthorizationStrategies[2].AuthorizationStrategyName.ToString();

            var authStrategy4Id = appAuthorizationStrategies[3].AuthorizationStrategyId;
            var authStrategy4Name = appAuthorizationStrategies[3].AuthorizationStrategyName.ToString();

            var authStrategy5Id = appAuthorizationStrategies[4].AuthorizationStrategyId;
            var authStrategy5Name = appAuthorizationStrategies[4].AuthorizationStrategyName.ToString();

            var formattedJson = testJSON.Replace("{0}", authStrategy1Id.ToString())
                                        .Replace("{1}", authStrategy1Name)
                                        .Replace("{2}", authStrategy2Id.ToString())
                                        .Replace("{3}", authStrategy2Name)
                                        .Replace("{4}", authStrategy3Id.ToString())
                                        .Replace("{5}", authStrategy3Name)
                                        .Replace("{6}", authStrategy4Id.ToString())
                                        .Replace("{7}", authStrategy4Name)
                                        .Replace("{8}", authStrategy5Id.ToString())
                                        .Replace("{9}", authStrategy5Name);

            var importModel = GetImportModel(formattedJson);
            var importSharingModel = SharingModel.DeserializeToSharingModel(importModel.ImportFile.OpenReadStream());

            using var securityContext = TestContext;
            Command(securityContext).Execute(importSharingModel);

            var testClaimSet = securityContext.ClaimSets.AsEnumerable().FirstOrDefault(x => x.ClaimSetName == "Test Claimset");
            testClaimSet.ShouldNotBeNull();

            var resourcesForClaimSet = ResourceClaimsForClaimSet(testClaimSet.ClaimSetId).ToList();

            resourcesForClaimSet.Count.ShouldBeGreaterThan(0);
            var testResources = resourcesForClaimSet.Where(x => x.ParentId == 0).ToArray();
            testResources.Count().ShouldBe(1);

            var testResource1 = testResources[0];
            MatchActions(testResource1, "TestParentResourceClaim0981", new bool[] { true, true, false, false, true });

            testResource1.Children.Count.ShouldBe(1);
            var childResource = testResource1.Children[0];
            childResource.ShouldNotBeNull();

            testResource1.AuthStrategyOverridesForCRUD.ShouldNotBeNull();
            testResource1.AuthStrategyOverridesForCRUD.Length.ShouldBe(5);

            var parentAuthStrategyOverrideForCreate = testResource1.AuthStrategyOverridesForCRUD.Create();
            parentAuthStrategyOverrideForCreate.ShouldNotBeNull();
            parentAuthStrategyOverrideForCreate.AuthorizationStrategies[0].AuthStrategyId.ShouldBe(authStrategy1Id);
            parentAuthStrategyOverrideForCreate.AuthorizationStrategies[0].AuthStrategyName.ShouldBe(authStrategy1Name);

            var parentAuthStrategyOverrideForRead = testResource1.AuthStrategyOverridesForCRUD.Read();
            parentAuthStrategyOverrideForRead.ShouldNotBeNull();
            parentAuthStrategyOverrideForRead.AuthorizationStrategies[0].AuthStrategyId.ShouldBe(authStrategy2Id);
            parentAuthStrategyOverrideForRead.AuthorizationStrategies[0].AuthStrategyName.ShouldBe(authStrategy2Name);

            var parentAuthStrategyOverrideForReadChanges = testResource1.AuthStrategyOverridesForCRUD.ReadChanges();
            parentAuthStrategyOverrideForReadChanges.ShouldNotBeNull();
            parentAuthStrategyOverrideForReadChanges.AuthorizationStrategies[0].AuthStrategyId.ShouldBe(authStrategy5Id);
            parentAuthStrategyOverrideForReadChanges.AuthorizationStrategies[0].AuthStrategyName.ShouldBe(authStrategy5Name);

            var childAuthStrategyOverrideForCreate = childResource.AuthStrategyOverridesForCRUD.Create();
            childAuthStrategyOverrideForCreate.ShouldNotBeNull();
            childAuthStrategyOverrideForCreate.AuthorizationStrategies[0].AuthStrategyId.ShouldBe(authStrategy1Id);
            childAuthStrategyOverrideForCreate.AuthorizationStrategies[0].AuthStrategyName.ShouldBe(authStrategy1Name);
            childAuthStrategyOverrideForCreate.AuthorizationStrategies[0].IsInheritedFromParent.ShouldBeTrue();

            var childAuthStrategyOverrideForRead = childResource.AuthStrategyOverridesForCRUD.Read();
            childAuthStrategyOverrideForRead.ShouldNotBeNull();
            childAuthStrategyOverrideForRead.AuthorizationStrategies[0].AuthStrategyId.ShouldBe(authStrategy2Id);
            childAuthStrategyOverrideForRead.AuthorizationStrategies[0].AuthStrategyName.ShouldBe(authStrategy2Name);
            childAuthStrategyOverrideForRead.AuthorizationStrategies[0].IsInheritedFromParent.ShouldBeTrue();

            var childAuthStrategyOverrideForUpdate = childResource.AuthStrategyOverridesForCRUD.Update();
            childAuthStrategyOverrideForUpdate.ShouldNotBeNull();
            childAuthStrategyOverrideForUpdate.AuthorizationStrategies[0].AuthStrategyId.ShouldBe(authStrategy3Id);
            childAuthStrategyOverrideForUpdate.AuthorizationStrategies[0].AuthStrategyName.ShouldBe(authStrategy3Name);
            childAuthStrategyOverrideForUpdate.AuthorizationStrategies[0].IsInheritedFromParent.ShouldBeFalse();

            var childAuthStrategyOverrideForDelete = childResource.AuthStrategyOverridesForCRUD.Delete();
            childAuthStrategyOverrideForDelete.ShouldNotBeNull();
            childAuthStrategyOverrideForDelete.AuthorizationStrategies[0].AuthStrategyId.ShouldBe(authStrategy4Id);
            childAuthStrategyOverrideForDelete.AuthorizationStrategies[0].AuthStrategyName.ShouldBe(authStrategy4Name);
            childAuthStrategyOverrideForDelete.AuthorizationStrategies[0].IsInheritedFromParent.ShouldBeFalse();

            var childAuthStrategyOverrideForReadChanges = childResource.AuthStrategyOverridesForCRUD.ReadChanges();
            childAuthStrategyOverrideForReadChanges.ShouldNotBeNull();
            childAuthStrategyOverrideForReadChanges.AuthorizationStrategies[0].AuthStrategyId.ShouldBe(authStrategy5Id);
            childAuthStrategyOverrideForReadChanges.AuthorizationStrategies[0].AuthStrategyName.ShouldBe(authStrategy5Name);
            childAuthStrategyOverrideForReadChanges.AuthorizationStrategies[0].IsInheritedFromParent.ShouldBeTrue();
        }

        private void MatchActions(ResourceClaim dbResource, string expectedResourceName, bool[] expectedCrudArray)
        {
            dbResource.Name.ShouldBe(expectedResourceName);
            dbResource.Create.ShouldBe(expectedCrudArray[0]);
            dbResource.Read.ShouldBe(expectedCrudArray[1]);
            dbResource.Update.ShouldBe(expectedCrudArray[2]);
            dbResource.Delete.ShouldBe(expectedCrudArray[3]);
            dbResource.ReadChanges.ShouldBe(expectedCrudArray[4]);
        }

        [Test]
        public void ShouldNotImportIfClaimSetNotUnique()
        {
            var testApplication = new Application
            {
                ApplicationName = $"Test Application {DateTime.Now:O}"
            };
            Save(testApplication);

            var alreadyExistingClaimSet = new ClaimSet { ClaimSetName = "Test ClaimSet", Application = testApplication };
            Save(alreadyExistingClaimSet);

            SetupResourceClaims(testApplication, new List<string> { "TestParent1234" }, new List<string>());

            var testJSON = @"{
                ""title"": ""testfile"",
                ""template"": {
                    ""claimSets"": [
                      {
                        ""name"": ""Test ClaimSet"",
                        ""resourceClaims"": [
                          {
                            ""Name"": ""TestParent1234"",
                            ""Read"": true,
                            ""Create"": false,
                            ""Update"": false,
                            ""Delete"": false,
                            ""ReadChanges"": false,
                            ""Children"": []
                          }
                        ]
                      }
                    ]
                }
            }";

            var importModel = GetImportModel(testJSON);

            using var securityContext = TestContext;
            var getAllClaimSetsQuery = new GetAllClaimSetsQuery(securityContext);
            var getResourceClaimsAsFlatListQuery = new GetResourceClaimsAsFlatListQuery(securityContext);

            var validator = new ClaimSetFileImportModelValidator(getAllClaimSetsQuery, getResourceClaimsAsFlatListQuery);
            var validationResults = validator.Validate(importModel);
            validationResults.IsValid.ShouldBe(false);
            validationResults.Errors.Select(x => x.ErrorMessage).ShouldContain(
                "This template contains a claimset with a name which already exists in the system. Please use a unique name for 'Test ClaimSet'.\n");
        }

        [Test]
        public void ShouldNotImportIfResourceDoesNotExist()
        {
            var testApplication = new Application
            {
                ApplicationName = $"Test Application {DateTime.Now:O}"
            };
            Save(testApplication);

            SetupResourceClaims(testApplication, new List<string> { "TestParentResourceClaim0981" }, new List<string>());

            var testJSON = @"{
                ""title"": ""testfile"",
                ""template"": {
                    ""claimSets"": [
                      {
                        ""name"": ""Test Claimset"",
                        ""resourceClaims"": [
                          {
                            ""Name"": ""TestParentResourceClaim-notthere"",
                            ""Read"": true,
                            ""Create"": false,
                            ""Update"": false,
                            ""Delete"": false,
                            ""ReadChanges"": false,
                            ""Children"": []
                          }
                        ]
                      }
                    ]
                }
            }";

            var importModel = GetImportModel(testJSON);

            using var securityContext = TestContext;
            var getAllClaimSetsQuery = new GetAllClaimSetsQuery(securityContext);
            var getResourceClaimsAsFlatListQuery = new GetResourceClaimsAsFlatListQuery(securityContext);

            var validator = new ClaimSetFileImportModelValidator(getAllClaimSetsQuery, getResourceClaimsAsFlatListQuery);
            var validationResults = validator.Validate(importModel);
            validationResults.IsValid.ShouldBe(false);
            validationResults.Errors.Select(x => x.ErrorMessage).ShouldContain("This template contains a resource which is not in the system. Claimset Name: Test Claimset Resource name: 'TestParentResourceClaim-notthere'.\n");

        }

        [Test]
        public void ShouldNotImportIfImportFileEmpty()
        {
            var importModel = new ClaimSetFileImportModel();

            using var securityContext = TestContext;
            var getAllClaimSetsQuery = new GetAllClaimSetsQuery(securityContext);
            var getResourceClaimsAsFlatListQuery = new GetResourceClaimsAsFlatListQuery(securityContext);

            var validator = new ClaimSetFileImportModelValidator(getAllClaimSetsQuery, getResourceClaimsAsFlatListQuery);
            var validationResults = validator.Validate(importModel);
            validationResults.IsValid.ShouldBe(false);
            validationResults.Errors.Select(x => x.ErrorMessage).ShouldContain("'Import File' must not be empty.");
        }

        [Test]
        public void ShouldNotImportIfImportFileHasAnInvalidExtension()
        {
            var importModel = new ClaimSetFileImportModel();

            var importFile = new Mock<IFormFile>();

            importFile.Setup(f => f.FileName).Returns("testfile.xml");
            importModel.ImportFile = importFile.Object;

            using var securityContext = TestContext;
            var getAllClaimSetsQuery = new GetAllClaimSetsQuery(securityContext);
            var getResourceClaimsAsFlatListQuery = new GetResourceClaimsAsFlatListQuery(securityContext);

            var validator = new ClaimSetFileImportModelValidator(getAllClaimSetsQuery, getResourceClaimsAsFlatListQuery);
            var validationResults = validator.Validate(importModel);
            validationResults.IsValid.ShouldBe(false);
            validationResults.Errors.Select(x => x.ErrorMessage).ShouldContain("Invalid file extension. Only '*.json' files are allowed.");
        }

        private static ClaimSetFileImportModel GetImportModel(string testJson)
        {
            var byteArray = Encoding.UTF8.GetBytes(testJson);
            var stream = new MemoryStream(byteArray);
            var importModel = new ClaimSetFileImportModel();

            var importFile = new Mock<IFormFile>();
            importFile.Setup(f => f.Length).Returns(stream.Capacity);
            importFile.Setup(f => f.OpenReadStream()).Returns(stream);

            importFile.Setup(f => f.FileName).Returns("testfile.json");
            importModel.ImportFile = importFile.Object;

            return importModel;
        }

        private ClaimSetFileImportCommand Command(ISecurityContext securityContext)
        {
            var editResourceCommand = new EditResourceOnClaimSetCommand(new StubOdsSecurityModelVersionResolver.V6(), null,
                new EditResourceOnClaimSetCommandV6Service(securityContext));

            var query = new GetResourceClaimsQuery(securityContext);

            var overrideAuth = new OverrideDefaultAuthorizationStrategyCommand(new StubOdsSecurityModelVersionResolver.V6(), null,
                new OverrideDefaultAuthorizationStrategyV6Service(securityContext));

            var addOrEditCommand = new AddOrEditResourcesOnClaimSetCommand(editResourceCommand, query, overrideAuth);

            return new ClaimSetFileImportCommand(new AddClaimSetCommand(new StubOdsSecurityModelVersionResolver.V6(),
                 null, new AddClaimSetCommandV6Service(securityContext)), addOrEditCommand);
        }
    }
}
