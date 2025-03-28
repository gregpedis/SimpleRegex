﻿namespace SimpleRegex.Scanning;

internal class Scanner(string source)
{
	private readonly string source = source;
	private readonly List<Token> tokens = [];

	private static readonly Dictionary<string, TokenType> RESERVED_KEYWORDS = new()
	{
		// Single character tokens.
		{ "OR", TokenType.OR },							// |
		{ "ANY", TokenType.ANY },						// .
		{ "START", TokenType.START },					// ^
		{ "END", TokenType.END },                       // $

		// Escaped characters.
		{ "WS" , TokenType.WHITESPACE },				// \s
		{ "WHITESPACE" , TokenType.WHITESPACE },		// \s
		{ "DIGIT" , TokenType.DIGIT },					// \d
		{ "NOTDIGIT" , TokenType.NOT_DIGIT},			// \D
		{ "WORD" , TokenType.WORD },					// \w
		{ "NOTWORD" , TokenType.NOT_WORD },				// \W
		{ "BOUNDARY" , TokenType.BOUNDARY },			// \b
		{ "NOTBOUNDARY" , TokenType.NOT_BOUNDARY},		// \B
		{ "NEWLINE" , TokenType.NEWLINE },				// \n
		{ "NL" , TokenType.NEWLINE },					// \n
		{ "CR" , TokenType.CR },						// \r
		{ "TAB" , TokenType.TAB },						// \t
		{ "NULL" , TokenType.NULL },					// \0
		{ "QUOTE" , TokenType.QUOTE},					// ""

		// simple quantifiers.
		{ "MAYBE", TokenType.MAYBE },					// a?
		{ "MAYBEMANY", TokenType.MAYBE_MANY },			// a*
		{ "MANY", TokenType.MANY },						// a+
		{ "LAZY", TokenType.LAZY },						// a{quantifier}?

		// precise quantifiers.
		{ "EXACTLY", TokenType.EXACTLY },				// a{3}
		{ "ATLEAST", TokenType.AT_LEAST },				// a{3,}
		{ "BETWEEN", TokenType.BETWEEN },				// a{3,6}

		// character classes.
		{ "RANGE", TokenType.RANGE },					// a-z
		{ "ANYOF", TokenType.ANY_OF },					// [a-zA]
		{ "NOTANYOF", TokenType.NOT_ANY_OF },			// [^a-zA]

		// group constructs.
		{ "CAPTURE", TokenType.CAPTURE },				// (abc) or (?<name>abc)
		{ "MATCH", TokenType.MATCH},					// (?:abc)
		{ "NOTMATCH", TokenType.NOT_MATCH },			// (?!abc)
	};

	private int start = 0;
	private int current = 0;
	private int line = 1;

	public List<Token> ScanTokens()
	{
		while (!IsAtEnd())
		{
			// We are at the beginning of the next lexeme.
			start = current;
			ScanToken();
		}

		tokens.Add(new Token(TokenType.EOF, "", null, line));
		return tokens;
	}

	private void ScanToken()
	{
		char c = Advance();
		switch (c)
		{
			case '(': AddToken(TokenType.LEFT_PAREN); break;
			case ')': AddToken(TokenType.RIGHT_PAREN); break;
			case ',': AddToken(TokenType.COMMA); break;
			case '+': AddToken(TokenType.CONCAT); break;
			case '=': AddToken(TokenType.EQUALS); break;

			case '"': AddString(); break;
			case var x when char.IsDigit(x): AddNumber(); break;
			case var x when IsAlpha(x): AddIdentifier(); break;

			// Ignore whitespace.
			case ' ':
			case '\r':
			case '\t':
				break;

			case '\n':
				line++;
				break;

			case '/':
				SkipComment();
				break;

			default: throw Error($"Unexpected character '{c}'");
		}
	}

	private void AddString()
	{
		while (Peek() != '"' && !IsAtEnd())
		{
			if (Peek() == '\n')
			{
				line++;
			}
			Advance();
		}

		if (IsAtEnd())
		{
			throw Error("Unterminated string");
		}

		// The closing ".
		Advance();

		AddToken(TokenType.LITERAL);
	}

	private void AddNumber()
	{
		while (char.IsDigit(Peek()))
		{
			Advance();
		}
		AddToken(TokenType.NUMBER, int.Parse(source[start..current]));
	}

	private void AddIdentifier()
	{
		while (IsAlphanumeric(Peek()))
		{
			Advance();
		}

		var text = source[start..current];

		var type = RESERVED_KEYWORDS.TryGetValue(text.ToUpperInvariant(), out var reserved)
			? reserved
			: TokenType.IDENTIFIER;
		AddToken(type);
	}

	private void SkipComment()
	{
		if (Peek() == '/')
		{
			Advance();
			while (!IsAtEnd() && Peek() != '\n')
			{
				Advance();
			}
		}
		else
		{
			throw Error($"Expected second '/'");
		}
	}

	private char Advance() =>
		source[current++];

	private bool IsAtEnd() =>
		current >= source.Length;

	private char Peek() =>
		IsAtEnd() ? '\0' : source[current];

	private void AddToken(TokenType type, int? literal = null)
	{
		var text = source[start..current];
		tokens.Add(new Token(type, text, literal, line));
	}

	private static bool IsAlphanumeric(char c) =>
		IsAlpha(c) || char.IsAsciiDigit(c);

	private static bool IsAlpha(char c) =>
		char.IsAsciiLetter(c) || c == '_';

	private ScanningException Error(string message) =>
		new(line, message);
}