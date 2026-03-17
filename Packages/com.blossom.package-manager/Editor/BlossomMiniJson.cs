using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Blossom.PackageManager.Editor {
    internal static class BlossomMiniJson {

        public static object Deserialize(string json) {
            if (string.IsNullOrEmpty(json)) {
                return null;
            }

            return Parser.Parse(json);
        }

        private sealed class Parser : IDisposable {
            private const string WordBreak = "{}[],:\"";

            private enum Token {
                None,
                CurlyOpen,
                CurlyClose,
                SquaredOpen,
                SquaredClose,
                Colon,
                Comma,
                String,
                Number,
                True,
                False,
                Null
            }

            private StringReader _json;

            private Parser(string jsonString) {
                _json = new StringReader(jsonString);
            }

            public static object Parse(string jsonString) {
                using var instance = new Parser(jsonString);
                return instance.ParseValue();
            }

            public void Dispose() {
                _json.Dispose();
                _json = null;
            }

            private static bool IsWordBreak(char c) {
                return char.IsWhiteSpace(c) || WordBreak.IndexOf(c) != -1;
            }

            private Dictionary<string, object> ParseObject() {
                Dictionary<string, object> table = new();

                _json.Read();

                while (true) {
                    switch (NextToken) {
                        case Token.None:
                            return null;

                        case Token.Comma:
                            continue;

                        case Token.CurlyClose:
                            return table;

                        default:
                            string name = ParseString();
                            if (name == null) {
                                return null;
                            }

                            if (NextToken != Token.Colon) {
                                return null;
                            }

                            _json.Read();
                            table[name] = ParseValue();
                            break;
                    }
                }
            }

            private List<object> ParseArray() {
                List<object> array = new();

                _json.Read();

                bool parsing = true;
                while (parsing) {
                    Token nextToken = NextToken;
                    switch (nextToken) {
                        case Token.None:
                            return null;

                        case Token.Comma:
                            continue;

                        case Token.SquaredClose:
                            parsing = false;
                            break;

                        default:
                            object value = ParseByToken(nextToken);
                            array.Add(value);
                            break;
                    }
                }

                return array;
            }

            private object ParseValue() {
                Token nextToken = NextToken;
                return ParseByToken(nextToken);
            }

            private object ParseByToken(Token token) {
                return token switch {
                    Token.String => ParseString(),
                    Token.Number => ParseNumber(),
                    Token.CurlyOpen => ParseObject(),
                    Token.SquaredOpen => ParseArray(),
                    Token.True => true,
                    Token.False => false,
                    Token.Null => null,
                    _ => null
                };
            }

            private string ParseString() {
                StringBuilder s = new();
                _json.Read();

                bool parsing = true;
                while (parsing) {
                    if (_json.Peek() == -1) {
                        parsing = false;
                        break;
                    }

                    char c = NextChar;
                    switch (c) {
                        case '"':
                            parsing = false;
                            break;

                        case '\\':
                            if (_json.Peek() == -1) {
                                parsing = false;
                                break;
                            }

                            c = NextChar;
                            switch (c) {
                                case '"':
                                case '\\':
                                case '/':
                                    s.Append(c);
                                    break;
                                case 'b':
                                    s.Append('\b');
                                    break;
                                case 'f':
                                    s.Append('\f');
                                    break;
                                case 'n':
                                    s.Append('\n');
                                    break;
                                case 'r':
                                    s.Append('\r');
                                    break;
                                case 't':
                                    s.Append('\t');
                                    break;
                                case 'u': {
                                        char[] hex = new char[4];
                                        for (int i = 0; i < 4; i++) {
                                            hex[i] = NextChar;
                                        }

                                        s.Append((char)Convert.ToInt32(new string(hex), 16));
                                        break;
                                    }
                            }

                            break;

                        default:
                            s.Append(c);
                            break;
                    }
                }

                return s.ToString();
            }

            private object ParseNumber() {
                string number = NextWord;

                if (number.IndexOf('.') == -1) {
                    long.TryParse(number, out long parsedInt);
                    return parsedInt;
                }

                double.TryParse(number, out double parsedDouble);
                return parsedDouble;
            }

            private void EatWhitespace() {
                while (char.IsWhiteSpace(PeekChar)) {
                    _json.Read();

                    if (_json.Peek() == -1) {
                        break;
                    }
                }
            }

            private char PeekChar => Convert.ToChar(_json.Peek());

            private char NextChar => Convert.ToChar(_json.Read());

            private string NextWord {
                get {
                    StringBuilder word = new();

                    while (!IsWordBreak(PeekChar)) {
                        word.Append(NextChar);

                        if (_json.Peek() == -1) {
                            break;
                        }
                    }

                    return word.ToString();
                }
            }

            private Token NextToken {
                get {
                    EatWhitespace();

                    if (_json.Peek() == -1) {
                        return Token.None;
                    }

                    switch (PeekChar) {
                        case '{':
                            return Token.CurlyOpen;
                        case '}':
                            _json.Read();
                            return Token.CurlyClose;
                        case '[':
                            return Token.SquaredOpen;
                        case ']':
                            _json.Read();
                            return Token.SquaredClose;
                        case ',':
                            _json.Read();
                            return Token.Comma;
                        case '"':
                            return Token.String;
                        case ':':
                            return Token.Colon;
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                        case '-':
                            return Token.Number;
                    }

                    switch (NextWord) {
                        case "false":
                            return Token.False;
                        case "true":
                            return Token.True;
                        case "null":
                            return Token.Null;
                    }

                    return Token.None;
                }
            }
        }

        public static string Serialize(object obj, bool prettyPrint = false) {
            return Serializer.Serialize(obj, prettyPrint);
        }

        private sealed class Serializer {
            private readonly StringBuilder _builder;
            private readonly bool _prettyPrint;
            private int _indentLevel;

            private Serializer(bool prettyPrint) {
                _builder = new StringBuilder();
                _prettyPrint = prettyPrint;
                _indentLevel = 0;
            }

            public static string Serialize(object obj, bool prettyPrint) {
                Serializer instance = new(prettyPrint);
                instance.SerializeValue(obj);
                return instance._builder.ToString();
            }

            private void SerializeValue(object value) {
                if (value == null) {
                    _builder.Append("null");
                }
                else if (value is string str) {
                    SerializeString(str);
                }
                else if (value is bool boolean) {
                    _builder.Append(boolean ? "true" : "false");
                }
                else if (value is IList list) {
                    SerializeArray(list);
                }
                else if (value is IDictionary dict) {
                    SerializeObject(dict);
                }
                else if (value is char c) {
                    SerializeString(new string(c, 1));
                }
                else {
                    SerializeOther(value);
                }
            }

            private void SerializeObject(IDictionary obj) {
                bool first = true;

                _builder.Append('{');

                _indentLevel++;
                if (_prettyPrint) {
                    _builder.AppendLine();
                }

                foreach (object key in obj.Keys) {
                    if (!first) {
                        _builder.Append(',');
                        if (_prettyPrint) {
                            _builder.AppendLine();
                        }
                    }

                    if (_prettyPrint) {
                        _builder.Append(new string(' ', _indentLevel * 4));
                    }

                    SerializeString(key.ToString());
                    _builder.Append(':');
                    if (_prettyPrint) {
                        _builder.Append(' ');
                    }

                    SerializeValue(obj[key]);
                    first = false;
                }

                _indentLevel--;

                if (_prettyPrint) {
                    _builder.AppendLine();
                    _builder.Append(new string(' ', _indentLevel * 4));
                }

                _builder.Append('}');
            }

            private void SerializeArray(IList array) {
                _builder.Append('[');

                _indentLevel++;
                if (_prettyPrint) {
                    _builder.AppendLine();
                }

                bool first = true;
                foreach (object obj in array) {
                    if (!first) {
                        _builder.Append(',');
                        if (_prettyPrint) {
                            _builder.AppendLine();
                        }
                    }

                    if (_prettyPrint) {
                        _builder.Append(new string(' ', _indentLevel * 4));
                    }

                    SerializeValue(obj);
                    first = false;
                }

                _indentLevel--;

                if (_prettyPrint) {
                    _builder.AppendLine();
                    _builder.Append(new string(' ', _indentLevel * 4));
                }

                _builder.Append(']');
            }

            private void SerializeString(string str) {
                _builder.Append('\"');

                foreach (char c in str.ToCharArray()) {
                    switch (c) {
                        case '"':
                            _builder.Append("\\\"");
                            break;
                        case '\\':
                            _builder.Append("\\\\");
                            break;
                        case '\b':
                            _builder.Append("\\b");
                            break;
                        case '\f':
                            _builder.Append("\\f");
                            break;
                        case '\n':
                            _builder.Append("\\n");
                            break;
                        case '\r':
                            _builder.Append("\\r");
                            break;
                        case '\t':
                            _builder.Append("\\t");
                            break;
                        default:
                            int codepoint = Convert.ToInt32(c);
                            if (codepoint >= 32 && codepoint <= 126) {
                                _builder.Append(c);
                            }
                            else {
                                _builder.Append("\\u");
                                _builder.Append(codepoint.ToString("x4"));
                            }

                            break;
                    }
                }

                _builder.Append('\"');
            }

            private void SerializeOther(object value) {
                if (value is float f) {
                    _builder.Append(f.ToString("R"));
                }
                else if (value is int or uint or long or sbyte or byte or short or ushort or ulong) {
                    _builder.Append(value);
                }
                else if (value is double or decimal) {
                    _builder.Append(Convert.ToDouble(value).ToString("R"));
                }
                else {
                    SerializeString(value.ToString());
                }
            }
        }
    }
}