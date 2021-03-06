﻿using System;
using System.Linq;

namespace Dapper.Extensions
{
    public static class SqlExtensions
    {
        public static string True(this string sql, bool condition)
        {
            return condition ? sql : "";
        }

        public static string True(this string sql, Func<bool> func)
        {
            return sql.True(func());
        }

        public static string False(this string sql, bool condition)
        {
            return !condition ? sql : "";
        }

        public static string False(this string sql, Func<bool> func)
        {
            return sql.False(func());
        }

        public static string Splice(this string sql, params bool[] conditions)
        {
            var startIndex = 0;
            foreach (var condition in conditions)
            {
                var start = sql.IndexOf('{', startIndex);
                if (start < 0)
                    return sql;
                var end = sql.IndexOf('}', start);
                if (end < 0)
                    return sql;
                startIndex = end;

                var elseIndex = sql.IndexOf(':', start, end - start);
                if (elseIndex < 0)
                {
                    if (condition)
                    {
                        sql = sql.Remove(start, 1).Remove(end - 1, 1);
                        startIndex -= 2;
                    }
                    else
                    {
                        var count = end - start + 1;
                        sql = sql.Remove(start, count);
                        startIndex -= count;
                    }
                }
                else
                {
                    if (condition)
                    {
                        var count = end - elseIndex + 1;
                        sql = sql.Remove(start, 1).Remove(elseIndex - 1, count);
                        startIndex -= count;
                    }
                    else
                    {
                        var count = elseIndex - start + 1;
                        sql = sql.Remove(start, count).Remove(end - count, 1);
                        startIndex -= count;
                    }
                }
            }
            return sql;
        }
        public static string Splice(this string sql, params Func<bool>[] conditions)
        {
            return sql.Splice(conditions.Select(p => p()).ToArray());
        }
    }
}
