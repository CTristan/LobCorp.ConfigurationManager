// SPDX-License-Identifier: MIT

#region

using System;
using System.Linq;
using System.Reflection;
using AwesomeAssertions;
using JetBrains.Annotations;

#endregion

namespace LobCorp.ConfigurationManager.Test.Extensions
{
    public static class TestExtensions
    {
        internal static void ValidateHarmonyPatch(
            [NotNull] this MemberInfo patchClass,
            Type originalClass,
            string methodName
        )
        {
            // Use reflection to access HarmonyPatch attributes since we can't reference 0Harmony.dll in net10.0
            var declaringAssembly =
                patchClass.DeclaringType?.Assembly ?? (patchClass is Type t ? t.Assembly : null);

            _ = declaringAssembly
                .Should()
                .NotBeNull("Unable to determine assembly from MemberInfo");

            // Search all loaded assemblies to find HarmonyPatch attribute type
            var harmonyPatchAttributeType = AppDomain
                .CurrentDomain.GetAssemblies()
                .Select(a =>
                    a.GetType("HarmonyLib.HarmonyPatch") ?? a.GetType("Harmony.HarmonyPatch")
                )
                .FirstOrDefault(t => t != null);

            _ = harmonyPatchAttributeType
                .Should()
                .NotBeNull("HarmonyPatch attribute type not found in loaded assemblies");

            var attribute = Attribute.GetCustomAttribute(patchClass, harmonyPatchAttributeType);
            _ = attribute.Should().NotBeNull("HarmonyPatch attribute not found on member");

            // Harmony 1.x stores patch info in a public 'info' field of type HarmonyMethod
            var infoField = harmonyPatchAttributeType?.GetField(
                "info",
                BindingFlags.Public | BindingFlags.Instance
            );
            _ = infoField.Should().NotBeNull("HarmonyPatch info field not found");

            var info = infoField?.GetValue(attribute);
            _ = info.Should().NotBeNull("HarmonyPatch info is null");

            // HarmonyMethod has public originalType and methodName fields
            var originalTypeField = info
                ?.GetType()
                .GetField("originalType", BindingFlags.Public | BindingFlags.Instance);
            var methodNameField = info
                ?.GetType()
                .GetField("methodName", BindingFlags.Public | BindingFlags.Instance);

            _ = originalTypeField.Should().NotBeNull("HarmonyMethod originalType field not found");
            _ = methodNameField.Should().NotBeNull("HarmonyMethod methodName field not found");

            var originalTypeValue = originalTypeField?.GetValue(info) as Type;
            var methodNameValue = methodNameField?.GetValue(info) as string;

            _ = originalTypeValue.Should().Be(originalClass);
            _ = methodNameValue.Should().Be(methodName);
        }
    }
}
