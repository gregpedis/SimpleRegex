﻿namespace SimpleRegex.Parsing.Nodes;

public class StaticExpr<T> : Expr where T : StaticExpr<T>, new()
{
	public static T Instance { get; } = new();

	public override string ToString() =>
		GetType().SimpleName();
}

public class Anchor<T> : StaticExpr<T> where T : StaticExpr<T>, new();

public class Any : StaticExpr<Any>;
public class Whitespace : StaticExpr<Whitespace>;
public class Digit : StaticExpr<Digit>;
public class NotDigit : StaticExpr<NotDigit>;
public class Word : StaticExpr<Word>;
public class NotWord : StaticExpr<NotWord>;
public class NewLine : StaticExpr<NewLine>;
public class Cr : StaticExpr<Cr>;
public class Tab : StaticExpr<Tab>;
public class Null : StaticExpr<Null>;
public class Quote : StaticExpr<Quote>;

public class Start : Anchor<Start>;
public class End : Anchor<End>;
public class Boundary : Anchor<Boundary>;

