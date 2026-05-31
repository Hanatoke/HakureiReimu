using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace HakureiReimu.HakureiReimuMod.Core
{
    public static class MethodHelper
    {
        private static readonly Dictionary<ushort, OpCode> Codes;
        private static readonly ConcurrentDictionary<MethodBase, HashSet<MethodBase>> CallCache = new();

        static MethodHelper()
        {
            // 缓存所有 OpCode
            Codes = typeof(OpCodes)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(OpCode))
                .Select(f => (OpCode)f.GetValue(null)!)
                .ToDictionary(op => (ushort)op.Value);
        }

        public static bool HasCall(this MethodInfo source, MethodInfo target)
        {
            try
            {
                var calls = GetCalls(source);
                return calls.Any(m => SameMethod(m, target));
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static IReadOnlyCollection<MethodBase> GetCalls(MethodInfo source)
        {
            return CallCache.GetOrAdd(source, key => BuildCallSet((MethodInfo)key));
        }

        private static HashSet<MethodBase> BuildCallSet(MethodInfo source)
        {
            var actualMethod = GetMethodToAnalyze(source);

            var result = new HashSet<MethodBase>();

            var body = actualMethod.GetMethodBody();
            if (body == null)
                return result;

            var il = body.GetILAsByteArray();
            if (il == null || il.Length == 0)
                return result;

            int pos = 0;

            while (pos < il.Length)
            {
                OpCode op;

                byte code = il[pos++];

                if (code == 0xFE)
                {
                    op = Codes[(ushort)(0xFE00 | il[pos++])];
                }
                else
                {
                    op = Codes[code];
                }

                if (op == OpCodes.Call ||
                    op == OpCodes.Callvirt ||
                    op == OpCodes.Newobj ||
                    op == OpCodes.Ldftn ||
                    op == OpCodes.Ldvirtftn)
                {
                    int token = BitConverter.ToInt32(il, pos);

                    try
                    {
                        var called = actualMethod.Module.ResolveMethod(token);

                        if (called != null)
                            result.Add(called);
                    }
                    catch
                    {
                    }
                }

                pos += OperandSize(op, il, pos);
            }

            return result;
        }
        private static MethodInfo GetMethodToAnalyze(MethodInfo method)
        {
            var asyncAttr = method.GetCustomAttribute<AsyncStateMachineAttribute>();

            if (asyncAttr == null)
                return method;

            return asyncAttr.StateMachineType.GetMethod(
                "MoveNext",
                BindingFlags.Instance | BindingFlags.NonPublic)!;
        }
        private static bool SameMethod(MethodBase a, MethodBase b)
        {
            if (a == b)
                return true;

            if (a.MetadataToken != b.MetadataToken)
                return false;

            if (a.Module != b.Module)
                return false;

            return true;
        }

        private static int OperandSize(OpCode op, byte[] il, int pos)
        {
            return op.OperandType switch
            {
                OperandType.InlineNone => 0,
                OperandType.ShortInlineBrTarget => 1,
                OperandType.ShortInlineI => 1,
                OperandType.ShortInlineVar => 1,
                OperandType.InlineVar => 2,
                OperandType.InlineI => 4,
                OperandType.InlineBrTarget => 4,
                OperandType.InlineField => 4,
                OperandType.InlineMethod => 4,
                OperandType.InlineSig => 4,
                OperandType.InlineString => 4,
                OperandType.InlineTok => 4,
                OperandType.InlineType => 4,
                OperandType.ShortInlineR => 4,
                OperandType.InlineI8 => 8,
                OperandType.InlineR => 8,
                OperandType.InlineSwitch => 4 + BitConverter.ToInt32(il, pos) * 4,
                _ => throw new NotSupportedException(op.OperandType.ToString())
            };
        }

    }
}