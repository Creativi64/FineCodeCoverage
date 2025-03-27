using System;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal static class CommandLineParser
    {
        /// <summary>
        /// Options parser support:
        ///     * Only - and -- prefix for options https://learn.microsoft.com/dotnet/standard/commandline/syntax#options
        ///     * Multiple option arguments https://learn.microsoft.com/dotnet/standard/commandline/syntax#multiple-arguments
        ///     * Use a space, '=', or ':' as the delimiter between an option name and its argument
        ///     * escape with \
        ///     * surrounding with ""
        ///     * surrounding with ''
        ///
        /// Options parser doesn't support
        ///     * Default argument/commands/verb
        ///     * Alias https://learn.microsoft.com/dotnet/standard/commandline/syntax#aliases
        ///     * Case sensitivity https://learn.microsoft.com/dotnet/standard/commandline/syntax#case-sensitivity
        ///     * -- token https://learn.microsoft.com/dotnet/standard/commandline/syntax#the----token
        ///
        /// https://pubs.opengroup.org/onlinepubs/9699919799/utilities/V3_chap02.html#tag_18_02_03
        /// https://learn.microsoft.com/cpp/c-language/parsing-c-command-line-arguments?view=msvc-170
        /// Double-Quotes: we don't support for now
        ///     * The dollar-sign shall retain its special meaning introducing parameter expansion
        ///     * The backquote shall retain its special meaning introducing the other form of command substitution
        ///     * A POSIX convention lets you omit the delimiter when you are specifying a single-character option alias, i.e. myapp -vquiet.
        /// </summary>
        public static CommandLineParseResult Parse(string[] args)
            => Parse(args.ToList());

        private static CommandLineParseResult Parse(List<string> args)
        {
            List<CommandLineParseOption> options = new List<CommandLineParseOption>();
            List<string> errors = new List<string>();

            string currentOption = null;
            string currentArg = null;
            string toolName = null;
            List<string> currentOptionArguments = new List<string>();
            for (int i = 0; i < args.Count; i++)
            {
                if (args[i].StartsWith("@") && ResponseFileHelper.TryReadResponseFile(args[i].Substring(1), errors, out string[] newArguments))
                {
                    args.InsertRange(i + 1, newArguments);
                    continue;
                }
                bool argumentHandled = false;
                currentArg = args[i];

                while (!argumentHandled)
                {
                    if (currentArg is null)
                    {
                        //errors.Add(string.Format(CultureInfo.InvariantCulture, PlatformResources.CommandLineParserUnexpectedNullArgument, i));
                        errors.Add($"UnexpectedNullArgument {i}");
                        break;
                    }

                    // we accept as start for options -- and - all the rest are arguments to the previous option
                    if ((args[i].Length > 1 && currentArg[0].Equals('-') && !currentArg[1].Equals('-')) ||
                        (args[i].Length > 2 && currentArg[0].Equals('-') && currentArg[1].Equals('-') && !currentArg[2].Equals('-')))
                    {
                        if (currentOption is null)
                        {
                            ParseOptionAndSeparators(args[i], out currentOption, out currentArg);
                            argumentHandled = currentArg is null;
                        }
                        else
                        {
                            options.Add(new CommandLineParseOption(currentOption, currentOptionArguments.ToArray()));
                            currentOptionArguments.Clear();
                            ParseOptionAndSeparators(args[i], out currentOption, out currentArg);
                            argumentHandled = true;
                        }
                    }
                    else
                    {
                        // If it's the first argument and it doesn't start with - then it's the tool name
                        if (i == 0 && !args[0][0].Equals('-'))
                        {
                            toolName = currentArg;
                        }
                        else if (currentOption is null)
                        {
                            errors.Add($"UnexpectedArgument {args[i]}");
                            //errors.Add(string.Format(CultureInfo.InvariantCulture, PlatformResources.CommandLineParserUnexpectedArgument, args[i]));
                        }
                        else
                        {
                            currentOptionArguments.Add(currentArg.Trim());
                            //if (TryUnescape(currentArg.Trim(), currentOption, out string unescapedArg, out string error))
                            //{
                            //    currentOptionArguments.Add(unescapedArg);
                            //}
                            //else
                            //{
                            //    errors.Add(error);
                            //}

                            currentArg = null;
                        }

                        argumentHandled = true;
                    }
                }
            }

            if (currentOption != null)
            {
                if (currentArg != null)
                {
                    currentOptionArguments.Add(currentArg.Trim());
                    //if (TryUnescape(currentArg.Trim(), currentOption, out string unescapedArg, out string error))
                    //{
                    //    currentOptionArguments.Add(unescapedArg);
                    //}
                    //else
                    //{
                    //    errors.Add(error);
                    //}
                }

                options.Add(new CommandLineParseOption(currentOption, currentOptionArguments.ToArray()));
            }

            return new CommandLineParseResult(options, errors);
        }

        private static bool TryUnescape(string input, string option, out string unescapedArg, out string error)
        {
            unescapedArg = input;
            error = null;

            // Enclosing characters in single-quotes ( '' ) shall preserve the literal value of each character within the single-quotes.
            // A single-quote cannot occur within single-quotes.
            if (input.StartsWith(@"'", StringComparison.OrdinalIgnoreCase) && input.EndsWith(@"'", StringComparison.OrdinalIgnoreCase))
            {
                if (input.IndexOf('\'', 1, input.Length - 2) != -1)
                {
                    //error = option is null
                    //    ? string.Format(CultureInfo.InvariantCulture, PlatformResources.CommandLineParserUnexpectedSingleQuoteInArgument, input)
                    //    : string.Format(CultureInfo.InvariantCulture, PlatformResources.CommandLineParserUnexpectedSingleQuoteInArgumentForOption, input, option);
                    error = option is null
                        ? $"UnexpectedSingleQuoteInArgument {input}"
                        :  $"UnexpectedSingleQuoteInArgumentForOption {input} {option}";
                    return false;
                }

                unescapedArg = input.Substring(1, input.Length - 2);
                return true;
            }

            // Enclosing characters in double-quotes("") shall preserve the literal value of all characters within the double-quotes,
            // with the exception of the characters backquote, <dollar-sign>, and <backslash>, as follows:
            //  * The backquote shall retain its special meaning introducing the other form of command substitution. [NOT SUPPORTED]
            //  * The <dollar-sign> shall retain its special meaning introducing parameter expansion. [NOT SUPPORTED]
            //  * The backslash shall retain its special meaning as an escape character only when followed by one of the following characters when considered special:
            //    $   `   "   \   <newline>
            if (input.StartsWith("\"", StringComparison.OrdinalIgnoreCase) && input.EndsWith("\"", StringComparison.OrdinalIgnoreCase))
            {
                unescapedArg = input.Substring(1, input.Length - 2).Replace(@"\\", "\\")
                    .Replace(@"\""", "\"")
                    .Replace(@"\$", "$")
                    .Replace(@"\`", "`")
                    .Replace($@"\{Environment.NewLine}", Environment.NewLine);
                return true;
            }

            return true;

        }

        private static void ParseOptionAndSeparators(string arg, out string currentOption, out string currentArg)
        {
            var delimiterIndex = arg.IndexOfAny(new char[] { ':', '=', ' ' });
            if (delimiterIndex == -1)
            {
                currentOption = arg;
                currentArg = null;
            }
            else
            {
                currentOption = arg.Substring(0, delimiterIndex);
                currentArg = arg.Substring(++delimiterIndex);
            }
            currentOption = currentOption.TrimStart('-');
        }
    }
}
