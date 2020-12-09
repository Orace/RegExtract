using System;

using Xunit;

using RegExtract;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace RegExtract.Test
{
    public class Usage
    {
        const string data = "123456789";
        const string pattern = "(.)(.)(.)(.)(.)(.)(.)(.)(.)";
        const string pattern_nested = "(((.)(.)(.)(.)(.)(.)(.)(.)(.)))";
        const string pattern_named = "(?<n>(?<s>(?<a>.)(?<b>.)(?<c>.)(?<d>.)(?<e>.)(?<f>.)(?<g>.)(?<h>.)(?<i>.)))";

        [Fact]
        public void can_extract_to_tuple()
        {
            var (a, b, c, d, e, f, g, h, i) = data.Extract<(int, char, string, int, char, string, int, char, string)>(pattern);

            Assert.IsType<int>(a);
            Assert.IsType<char>(b);
            Assert.IsType<string>(c);
            Assert.IsType<int>(d);
            Assert.IsType<char>(e);
            Assert.IsType<string>(f);
            Assert.IsType<int>(g);
            Assert.IsType<char>(h);
            Assert.IsType<string>(i);

            Assert.Equal(1, a);
            Assert.Equal('2', b);
            Assert.Equal("3", c);
            Assert.Equal(4, d);
            Assert.Equal('5', e);
            Assert.Equal("6", f);
            Assert.Equal(7, g);
            Assert.Equal('8', h);
            Assert.Equal("9", i);
        }

        [Fact]
        public void can_extract_to_tuple_nested()
        {
            var (n, s, a, b, c, d, e, f, g, h, i) = data.Extract<(long, string, int, char, string, int, char, string, int, char, string)>(pattern_nested);

            Assert.IsType<long>(n);
            Assert.IsType<string>(s);

            Assert.IsType<int>(a);
            Assert.IsType<char>(b);
            Assert.IsType<string>(c);
            Assert.IsType<int>(d);
            Assert.IsType<char>(e);
            Assert.IsType<string>(f);
            Assert.IsType<int>(g);
            Assert.IsType<char>(h);
            Assert.IsType<string>(i);

            Assert.Equal(123456789, n);
            Assert.Equal("123456789", s);

            Assert.Equal(1, a);
            Assert.Equal('2', b);
            Assert.Equal("3", c);
            Assert.Equal(4, d);
            Assert.Equal('5', e);
            Assert.Equal("6", f);
            Assert.Equal(7, g);
            Assert.Equal('8', h);
            Assert.Equal("9", i);
        }

        [Fact]
        public void fails_when_tuple_is_wrong_arity()
        {
            Assert.Throws<ArgumentException>(() => data.Extract<(int, char, string, int, char, string, int, char, string)>(pattern_nested));
        }

        record PositionalRecord(int a, char b, string c, int d, char e, string f, int g, char h, string i);

        [Fact]
        public void can_extract_to_positional_record()
        {
            PositionalRecord record = data.Extract<PositionalRecord>(pattern);

            var (a, b, c, d, e, f, g, h, i) = record;

            Assert.IsType<int>(a);
            Assert.IsType<char>(b);
            Assert.IsType<string>(c);
            Assert.IsType<int>(d);
            Assert.IsType<char>(e);
            Assert.IsType<string>(f);
            Assert.IsType<int>(g);
            Assert.IsType<char>(h);
            Assert.IsType<string>(i);

            Assert.Equal(1, a);
            Assert.Equal('2', b);
            Assert.Equal("3", c);
            Assert.Equal(4, d);
            Assert.Equal('5', e);
            Assert.Equal("6", f);
            Assert.Equal(7, g);
            Assert.Equal('8', h);
            Assert.Equal("9", i);
        }

        [Fact]
        public void fails_when_positional_record_is_wrong_arity()
        {
            Assert.Throws<ArgumentException>(() => data.Extract<PositionalRecord>(pattern_nested));
        }

        record PropertiesRecord
        {
            public string s { get; init; }
            public long n { get; init; }
            public int a { get; init; }
            public char b { get; init; }
            public string c { get; init; }
            public int? d { get; init; }
            public char? e { get; init; }
            public string? f { get; init; }
            public int g { get; init; }
            public char h { get; init; }
            public string i { get; init; }
        }

        [Fact]
        public void can_extract_named_capture_groups_to_properties()
        {
            PropertiesRecord record = data.Extract<PropertiesRecord>(pattern_named);
        }

        record Passport
        {
            public int? byr { get; set; }
            public int? iyr { get; set; }
            public int? eyr { get; set; }
            public string? hgt { get; set; }
            public string? hcl { get; set; }
            public string? ecl { get; set; }
            public string? pid { get; set; }
        }

        [Fact]
        public void can_extract_mondo_conditional_regex()
        {
            var mondoString = @"
^(\b
( (byr: ((?<byr>19[2-9][0-9]|200[0-2])                           |.*?) )
| (iyr: ((?<iyr>20(1[0-9]|20))                                   |.*?) )
| (eyr: ((?<eyr>20(2[0-9]|30))                                   |.*?) )
| (hgt: ((?<hgt>((59|6[0-9]|7[0-6])in)|(1([5-8][0-9]|9[0-3])cm)) |.*?) )
| (hcl: ((?<hcl>\#[0-9a-f]{6})                                   |.*?) )
| (ecl: ((?<ecl>amb|blu|brn|gry|grn|hzl|oth)                     |.*?) )
| (pid: ((?<pid>[0-9]{9})                                        |.*?) )
| (cid: (.*?)                                                          )
)
\b\s*)+
$
";
            var mondo = new Regex(mondoString, RegexOptions.IgnorePatternWhitespace);

            "hgt:61in iyr:2014 pid:916315544 hcl:#733820 ecl:oth".Extract<Passport>(mondoString,RegexOptions.IgnorePatternWhitespace);

            //TODO: this was the only test using the `this Match` extension for Extract. Should re-add one.
        }

        record Container
        {
            public string container { get; init; }
            public List<int> count { get; init; }
            public List<string> bag { get; init; }
            public string? none { get; init; }
        }

        [Fact]
        public void can_extract_capture_collections_to_lists()
        {
            var line = "faded yellow bags contain 4 mirrored fuchsia bags, 4 dotted indigo bags, 3 faded orange bags, 5 plaid crimson bags.";
            var regex = @"^(?<container>.+) bags contain( (?<none>no more bags\.)| (?<count>\d+) (?<bag>[^,.]*) bag[s]?[,.])+$";

            var output = line.Extract<Container>(regex);
        }

        [Fact]
        public void can_extract_single_item()
        {
            var output = "asdf".Extract<string>("(.*)");
            Assert.Equal("asdf", output);

            var n = "2023".Extract<int>(@"(\d+)");
            Assert.Equal(2023, n);
        }

        [Fact]
        public void can_extract_multimatch_to_tuple()
        {
            var result = "123 456 789".Extract <List<int>> (@"(?:(\d+) ?)+");
        }

        [Fact]
        public void can_extract_alternation_to_tuple()
        {
            var result = "asdf".Extract<(int?, string)>(@"(\d+)|(.*)");
        }

        record Alternation(int? n, string s);

        record NamedAlternation
        {
            public int? n { get; init; }
            public string s { get; init; }
        }

        [Fact]
        public void can_extract_alternation_to_record()
        {
            var result = "asdf".Extract<Alternation>(@"(\d+)|(.*)");
            var result_named = "asdf".Extract<NamedAlternation>(@"(?<n>\d+)|(?<s>.*)");
        }

        [Fact]
        public void can_extract_enum()
        {
            var result = "Asynchronous,Encrypted".Extract<System.IO.FileOptions>(@".*");
        }

        record WithTemplate(string op, int arg)
        {
            public const string REGEXTRACT_REGEX_PATTERN = @"(\S+) ([+-]?\d+)";
            public const RegexOptions REGEXTRACT_REGEX_OPTIONS = RegexOptions.None;
        }

        [Fact]
        public void can_extract_with_template()
        {
            var result = "acc +7".Extract<WithTemplate>();
        }

        const RegexOptions opts = RegexOptions.IgnoreCase|RegexOptions.Multiline;

        [Fact]
        public void group_to_type_list_of_int()
        {
            var match = Regex.Match("123 456 789", @"(?:(\d+) ?)+");
            RegExtractExtensions.GroupToType(match.Groups[1], typeof(List<int>));

            match = Regex.Match("", @"(\d+)?");
            RegExtractExtensions.GroupToType(match.Groups[1], typeof(int?));

            Assert.Throws<InvalidCastException>(() => RegExtractExtensions.GroupToType(match.Groups[1], typeof(int)));
        }

        [Fact]
        public void string_to_type_tests()
        {
            RegExtractExtensions.StringToType("123", typeof(int?));
        }

        [Fact]
        public void can_extract_to_string_constructor()
        {
            "https://www.google.com/".Extract<Uri>(".*");
        }

        [Fact]
        public void regex_does_not_match()
        {
            Assert.Throws<ArgumentException>(()=>"https://www.google.com/".Extract<Uri>(@"\d+"));
        }
    }
}

// This is here to enable use of record types in .NET 3.1.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
