// Eclipse Public License - v 1.0, http://www.eclipse.org/legal/epl-v10.html
// Copyright (c) 2013, Christian Wulf (chwchw@gmx.de)
// Copyright (c) 2016-2017, Ivan Kochurkin (kvanttt@gmail.com), Positive Technologies.

parser grammar LanguageParser;

options { tokenVocab=LanguageLexer; superClass = LanguageParserBase; }

// entry point
compilation_unit
	: statement_list? template? EOF
	;

template
    : EOF
	;

statement_list
	: statement+
	;

statement
    : block
    | declarationStatement
	| 
    ;

block
	: OPEN_BRACE statement_list? CLOSE_BRACE
	;

declarationStatement
	: local_variable_declaration
	;

local_variable_declaration
	: (USING)? local_variable_type local_variable_declarator ( ','  local_variable_declarator { this.IsLocalVariableDeclaration() }? )*
	;

identifier
	: IDENTIFIER
	//| ADD
	//| ALIAS
	//| ARGLIST
	//| ASCENDING
	| ASYNC
	| AWAIT
	//| BY
	//| DESCENDING
	//| DYNAMIC
	//| EQUALS
	//| FROM
	//| GET
	//| GROUP
	//| INTO
	//| JOIN
	//| LET
	//| NAMEOF
	//| ON
	//| ORDERBY
	//| PARTIAL
	//| REMOVE
	//| SELECT
	//| SET
	//| UNMANAGED
	| VAR
	//| WHEN
	//| WHERE
	//| YIELD
	;

local_variable_declarator
	: identifier ('=' local_variable_initializer )?
	;

local_variable_initializer
	: expression
	;

expression
	: assignment
	| non_assignment_expression
	// | REF non_assignment_expression
	;

local_variable_type
	: VAR
	| type_
	;

type_
	: base_type ('?' | '*')*
	;

base_type
	: simple_type
	| class_type  // represents types: enum, class, interface, delegate, type_parameter
	| VOID '*'
	| tuple_type
	;

tuple_type
    : '(' tuple_element (',' tuple_element)+ ')'
    ;

tuple_element
    : type_ identifier?
    ;

simple_type
	: numeric_type
	| BOOL
	;

numeric_type
	: integral_type
	| floating_point_type
	// | DECIMAL
	;

integral_type
	: INT
    // | SBYTE
	// | BYTE
	// | SHORT
	// | USHORT	
	// | UINT
	// | LONG
	// | ULONG
	// | CHAR
	;

floating_point_type
	: FLOAT
	// | DOUBLE
	;

/** namespace_or_type_name, OBJECT, STRING */
class_type
	: namespace_or_type_name
	| OBJECT
	// | DYNAMIC
	| STRING
	;

type_argument_list
	: '<' type_ ( ',' type_)* '>'
	;