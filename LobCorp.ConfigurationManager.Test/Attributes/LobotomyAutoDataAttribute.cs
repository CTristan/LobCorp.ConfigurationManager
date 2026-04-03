// SPDX-License-Identifier: MIT

#region

using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit3;

#endregion

namespace LobCorp.ConfigurationManager.Test.Attributes
{
    public sealed class LobotomyAutoDataAttribute : AutoDataAttribute
    {
        public LobotomyAutoDataAttribute()
            : base(CreateFixture) { }

        private static IFixture CreateFixture()
        {
            return new Fixture().Customize(new AutoMoqCustomization());
        }
    }
}
