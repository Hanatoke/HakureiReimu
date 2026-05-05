using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.ValueProps;

namespace HakureiReimu.HakureiReimuMod.Extensions
{
    public static class AttackCommandExtensions
    {
        private static readonly PropertyInfo DamagePropsProperty=AccessTools.Property(typeof(AttackCommand),nameof(AttackCommand.DamageProps));

        public static AttackCommand AddDamageProps(this AttackCommand command, ValueProp prop)
        {
            ValueProp value = command.DamageProps | prop;
            DamagePropsProperty.SetValue(command, value);
            return command;
        }
        public static AttackCommand RemoveDamageProps(this AttackCommand command, ValueProp prop)
        {
            ValueProp value = command.DamageProps & ~prop;
            DamagePropsProperty.SetValue(command, value);
            return command;
        }
    }
}