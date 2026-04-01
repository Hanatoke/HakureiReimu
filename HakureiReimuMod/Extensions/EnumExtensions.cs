using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace HakureiReimu.HakureiReimuMod.Extensions
{
    public static class EnumExtensions
    {
        public static string Name(this Enum enumValue)
        {
            return nameof(enumValue);
        }
    }
}