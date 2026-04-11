// SPDX-License-Identifier: LGPL-3.0-or-later

#region

using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit3;

#endregion

namespace LobCorp.ConfigurationManager.Test.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class LobotomyInlineAutoDataAttribute(params object[] values)
        : InlineAutoDataAttribute(CreateFixture, values)
    {
        private static IFixture CreateFixture()
        {
            return new Fixture().Customize(new AutoMoqCustomization());
        }
    }
}
