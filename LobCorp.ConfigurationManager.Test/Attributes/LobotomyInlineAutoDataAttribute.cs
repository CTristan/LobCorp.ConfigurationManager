// SPDX-License-Identifier: LGPL-3.0-or-later

#region

using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit3;

#endregion

namespace LobCorp.ConfigurationManager.Test.Attributes
{
    public sealed class LobotomyInlineAutoDataAttribute(params object[] values)
        : InlineAutoDataAttribute(CreateFixture, values)
    {
        private static IFixture CreateFixture()
        {
            return new Fixture().Customize(new AutoMoqCustomization());
        }
    }
}
