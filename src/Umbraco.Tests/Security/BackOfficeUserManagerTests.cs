﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Owin.Security.DataProtection;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Membership;
using Umbraco.Net;
using Umbraco.Web.Models.Identity;
using Umbraco.Web.Security;

namespace Umbraco.Tests.Security
{
    public class BackOfficeUserManagerTests
    {
        [Test]
        public async Task CheckPasswordAsync_When_Default_Password_Hasher_Validates_Umbraco7_Hash_Expect_Valid_Password()
        {
            const string v7Hash = "7Uob6fMTTxDIhWGebYiSxg==P+hgvWlXLbDd4cFLADn811KOaVI/9pg1PNvTuG5NklY=";
            const string plaintext = "4XxzH3s3&J";

            var mockPasswordConfiguration = new Mock<IPasswordConfiguration>();
            var mockIpResolver = new Mock<IIpResolver>();
            var mockUserStore = new Mock<IUserPasswordStore<BackOfficeIdentityUser>>();
            var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();

            mockDataProtectionProvider.Setup(x => x.Create(It.IsAny<string>()))
                .Returns(new Mock<IDataProtector>().Object);
            mockPasswordConfiguration.Setup(x => x.HashAlgorithmType)
                .Returns("HMACSHA256");

            var userManager = BackOfficeUserManager.Create(
                mockPasswordConfiguration.Object,
                mockIpResolver.Object,
                mockUserStore.Object,
                null,
                mockDataProtectionProvider.Object,
                new NullLogger<UserManager<BackOfficeIdentityUser>>());

            var mockGlobalSettings = new Mock<IGlobalSettings>();
            mockGlobalSettings.Setup(x => x.DefaultUILanguage).Returns("test");

            var user = new BackOfficeIdentityUser(mockGlobalSettings.Object, 2, new List<IReadOnlyUserGroup>())
            {
                UserName = "alice",
                Name = "Alice",
                Email = "alice@umbraco.test",
                PasswordHash = v7Hash
            };

            mockUserStore.Setup(x => x.GetPasswordHashAsync(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync(v7Hash);

            var isValidPassword = await userManager.CheckPasswordAsync(user, plaintext);

            Assert.True(isValidPassword);
        }
    }
}