﻿namespace SimpleRegex.Parsing.Nodes;

public class VariadicExpr(List<Expr> operands) : Expr
{
	public List<Expr> Operands { get; } = operands;

	public override string ToString() =>
		$"{GetType().SimpleName()} ({string.Join(", ", Operands)})";
}

public class Or(List<Expr> operands) : VariadicExpr(operands);
public class Concat(List<Expr> operands) : VariadicExpr(operands);

public class AnyOf(List<Expr> operands) : VariadicExpr(operands);
public class NotAnyOf(List<Expr> operands) : VariadicExpr(operands);
